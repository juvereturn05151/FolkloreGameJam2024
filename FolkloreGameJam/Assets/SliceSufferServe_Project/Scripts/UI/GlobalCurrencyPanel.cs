using TMPro;
using UnityEngine;

public class GlobalCurrencyPanel : MonoBehaviour
{
    private const string ResourcePath = "GlobalCurrencyPanel";

    private static GlobalCurrencyPanel instance;
    private static bool desiredVisible = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI currencyBalanceText;

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
        Refresh();
    }

    private void OnDisable()
    {
        SaveSystem.OnCurrencyChanged -= UpdateCurrencyBalance;
    }

    private void BindReferences()
    {
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
        gameObject.SetActive(desiredVisible);
    }
}
