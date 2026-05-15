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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ActivateHumanGenerator()
    {
        _textBox.SetActive(false);
        _tutorialDisplayBackGround.SetActive(false);
        SetGeneratorActive(_humanGenerator, true);
        SetGeneratorActive(_humanGenerator2, true);
    }

    public void ActivateCustomerGenerator()
    {
        SetGeneratorActive(_customerGenerator, true);
    }

    public void DeactivateGenerator()
    {
        SetGeneratorActive(_humanGenerator, false);
        SetGeneratorActive(_humanGenerator2, false);
        SetGeneratorActive(_customerGenerator, false);
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
    }

    public void ReportProgress(TutorialType tutorialType, int amount = 1)
    {
        if (!CanAcceptProgress(tutorialType) || amount <= 0)
        {
            return;
        }

        int newValue = GetProgress(tutorialType) + amount;
        progressCounts[tutorialType] = newValue;
    }

    public int GetProgress(TutorialType tutorialType)
    {
        return progressCounts.GetValueOrDefault(tutorialType);
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

    private void SetGeneratorActive(MonoBehaviour generator, bool isActive)
    {
        if (generator != null)
        {
            generator.gameObject.SetActive(isActive);
        }
    }
}
