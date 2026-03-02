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

    public int _humanKillCount;
    public int rottenCount;
    public int serveCount;
    public int trashInBinCount;

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
        if (GameManager.Instance.IsTutorial &&
            CurrentTutorial.Type == TutorialType.CutHuman)
        {
            _humanKillCount++;
        }
    }
}