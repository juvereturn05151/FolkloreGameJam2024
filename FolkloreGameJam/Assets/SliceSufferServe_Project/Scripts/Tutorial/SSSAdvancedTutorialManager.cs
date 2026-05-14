using System.Collections.Generic;
using UnityEngine;

public class SSSAdvancedTutorialManager : AdvancedTutorialManager_Base
{
    public static SSSAdvancedTutorialManager Instance { get; private set; }

    [SerializeField]
    private HumanGenerator _humanGenerator;

    [SerializeField]
    private HumanGenerator _humanGenerator2;

    [SerializeField]
    private CustomerGenerator _customerGenerator;

    [SerializeField, HideInInspector] private int _humanKillCount;
    [SerializeField, HideInInspector] private int rottenCount;
    [SerializeField, HideInInspector] private int serveCount;
    [SerializeField, HideInInspector] private int trashInBinCount;

    private readonly Dictionary<TutorialType, int> progressCounts = new Dictionary<TutorialType, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    public void ActivateHumanGenerator()
    {
        _textBox.SetActive(false);
        _tutorialDisplayBackGround.SetActive(false);
        _humanGenerator.gameObject.SetActive(true);
        _humanGenerator2.gameObject.SetActive(true);
    }

    public void ActivateCustomerGenerator()
    {
        _customerGenerator.gameObject.SetActive(true);
    }

    public void DeactivateGenerator()
    {
        _humanGenerator.gameObject.SetActive(false);
        _humanGenerator2.gameObject.SetActive(false);
        _customerGenerator.gameObject.SetActive(false);
    }

    protected override void OnTutorialEnd()
    {
        base.OnTutorialEnd();
        DeactivateGenerator();
    }

    public void Hook(HumanPart part)
    {
        part.Sliced -= OnHumanPartSliced;
        part.Sliced += OnHumanPartSliced;
    }

    public void Unhook(HumanPart part)
    {
        part.Sliced -= OnHumanPartSliced;
    }

    public void ResetProgress(TutorialType tutorialType)
    {
        progressCounts[tutorialType] = 0;

        switch (tutorialType)
        {
            case TutorialType.CutHuman:
                _humanKillCount = 0;
                break;
            case TutorialType.WaitForRotten:
                rottenCount = 0;
                break;
            case TutorialType.ServeCustomer:
                serveCount = 0;
                break;
            case TutorialType.PutTrashToBin:
                trashInBinCount = 0;
                break;
        }
    }

    public void ReportProgress(TutorialType tutorialType, int amount = 1)
    {
        if (!CanAcceptProgress(tutorialType) || amount <= 0)
        {
            return;
        }

        int newValue = GetProgress(tutorialType) + amount;
        progressCounts[tutorialType] = newValue;
        SetLegacyProgressCounter(tutorialType, newValue);
    }

    public int GetProgress(TutorialType tutorialType)
    {
        if (progressCounts.TryGetValue(tutorialType, out int progress))
        {
            return progress;
        }

        return GetLegacyProgressCounter(tutorialType);
    }

    private void OnHumanPartSliced(Vector3 pos, IReadOnlyList<FeedbackRequest> requests)
    {
        if (requests == null) return;

        foreach (var req in requests)
        {
            if (req.feedbackID == "") continue;

            switch (req.feedbackID)
            {
                case "CutHumanTutorial":
                    FeedbackCutHuman();
                    break;
            }
        }
    }

    public void FeedbackCutHuman()
    {
        ReportProgress(TutorialType.CutHuman);
    }

    private bool CanAcceptProgress(TutorialType tutorialType)
    {
        return GameManager.Instance != null
            && GameManager.Instance.IsTutorial
            && IsOperating
            && CurrentTutorial != null
            && CurrentTutorial.Type == tutorialType;
    }

    private int GetLegacyProgressCounter(TutorialType tutorialType)
    {
        return tutorialType switch
        {
            TutorialType.CutHuman => _humanKillCount,
            TutorialType.WaitForRotten => rottenCount,
            TutorialType.ServeCustomer => serveCount,
            TutorialType.PutTrashToBin => trashInBinCount,
            _ => 0
        };
    }

    private void SetLegacyProgressCounter(TutorialType tutorialType, int value)
    {
        switch (tutorialType)
        {
            case TutorialType.CutHuman:
                _humanKillCount = value;
                break;
            case TutorialType.WaitForRotten:
                rottenCount = value;
                break;
            case TutorialType.ServeCustomer:
                serveCount = value;
                break;
            case TutorialType.PutTrashToBin:
                trashInBinCount = value;
                break;
        }
    }
}
