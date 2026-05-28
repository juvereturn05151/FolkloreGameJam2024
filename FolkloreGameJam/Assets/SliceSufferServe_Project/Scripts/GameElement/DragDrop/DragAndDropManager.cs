using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    public static DragAndDropManager Instance;

    [Header("Cursor")]
    [SerializeField] private Texture2D cursorKnife;
    [SerializeField] private Texture2D cursorHand;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private bool dragging;

    public bool isDragging
    {
        get => dragging;
        set
        {
            if (dragging == value)
            {
                return;
            }

            dragging = value;
            ApplyCursor(dragging ? cursorHand : cursorKnife);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UseKnifeCursor();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
        }
    }

    private void OnEnable()
    {
        UseKnifeCursor();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursor(dragging ? cursorHand : cursorKnife);
        }
    }

    public void UseKnifeCursor()
    {
        dragging = false;
        ApplyCursor(cursorKnife);
    }

    public void UseHandCursor()
    {
        dragging = true;
        ApplyCursor(cursorHand);
    }

    private void ApplyCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
    }
}
