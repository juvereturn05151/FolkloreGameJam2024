using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("Settings")]
    public float updateInterval = 0.5f;

    [Header("Display")]
    public int fontSize = 24;
    public int paddingX = 10;
    public int paddingY = 10;

    private float timer;
    private int frameCount;
    private float fps;
    private GUIStyle style;

    void Start()
    {
        style = new GUIStyle();
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
    }

    void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            fps = frameCount / timer;
            frameCount = 0;
            timer = 0f;
        }
    }

    void OnGUI()
    {
        int roundedFPS = Mathf.RoundToInt(fps);

        // Color based on FPS range
        if (roundedFPS >= 60)
            style.normal.textColor = Color.green;
        else if (roundedFPS >= 30)
            style.normal.textColor = Color.yellow;
        else
            style.normal.textColor = Color.red;

        // Draw background box
        GUI.Box(new Rect(paddingX - 5, paddingY - 5, 120, 40), GUIContent.none);

        // Draw FPS label
        GUI.Label(
            new Rect(paddingX, paddingY, 150, 50),
            $"FPS: {roundedFPS}",
            style
        );
    }
}