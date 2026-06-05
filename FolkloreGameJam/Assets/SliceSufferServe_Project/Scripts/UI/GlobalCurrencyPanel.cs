using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalCurrencyPanel : MonoBehaviour
{
    private const string ResourcePath = "GlobalCurrencyPanel";
    private const string CharacterCustomizationSceneName = "CharacterCustomizationScene";

    private static GlobalCurrencyPanel instance;
    private static bool desiredVisible = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI currencyBalanceText;

    private Canvas panelCanvas;
    private GraphicRaycaster panelRaycaster;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GlobalCurrencyPanel prefab = Resources.Load<GlobalCurrencyPanel>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"Global currency panel prefab missing at Resources/{ResourcePath}.");
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BindReferences();
        Refresh();
        ApplyDesiredVisibility();
    }

    private void OnEnable()
    {
        SaveSystem.OnCurrencyChanged += UpdateCurrencyBalance;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Refresh();
        ApplyDesiredVisibility();
    }

    private void OnDisable()
    {
        SaveSystem.OnCurrencyChanged -= UpdateCurrencyBalance;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void BindReferences()
    {
        if (panelCanvas == null)
        {
            panelCanvas = GetComponent<Canvas>();
        }

        if (panelRaycaster == null)
        {
            panelRaycaster = GetComponent<GraphicRaycaster>();
        }

        if (currencyBalanceText != null)
        {
            return;
        }

        currencyBalanceText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (currencyBalanceText == null)
        {
            Debug.LogWarning("GlobalCurrencyPanel needs a TextMeshProUGUI assigned or placed in its children.", this);
        }
    }

    private void Refresh()
    {
        UpdateCurrencyBalance(SaveSystem.GetCurrencyBalance());
    }

    private void UpdateCurrencyBalance(int currencyBalance)
    {
        if (currencyBalanceText != null)
        {
            currencyBalanceText.text = currencyBalance.ToString();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyDesiredVisibility();
    }

    public static void SetVisible(bool visible)
    {
        desiredVisible = visible;

        if (instance != null)
        {
            instance.ApplyDesiredVisibility();
        }
    }

    private void ApplyDesiredVisibility()
    {
        bool visible = desiredVisible && !IsSuppressedInActiveScene();

        if (panelCanvas != null)
        {
            panelCanvas.enabled = visible;
        }

        if (panelRaycaster != null)
        {
            panelRaycaster.enabled = visible;
        }
    }

    private static bool IsSuppressedInActiveScene()
    {
        return SceneManager.GetActiveScene().name == CharacterCustomizationSceneName;
    }
}
