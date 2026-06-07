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
        SetAllHumanGeneratorsActive(true);
    }

    public void ActivateCustomerGenerator()
    {
        SetGeneratorActive(_customerGenerator, true);
    }

    public void SpawnTutorialHuman(HumanBody humanPrefab, float movementSpeedMultiplier = 1f)
    {
        if (humanPrefab == null)
        {
            return;
        }

        HumanGenerator generator = GetPrimaryHumanGenerator();
        if (generator != null)
        {
            generator.gameObject.SetActive(true);
            generator.SpawnSpecificHuman(humanPrefab, movementSpeedMultiplier);
        }
    }

    public void RestrictHumanGeneratorsToTutorialHuman(HumanBody humanPrefab, bool clearExistingHumans = true)
    {
        if (clearExistingHumans)
        {
            ClearActiveHumans();
        }

        if (_customerGenerator != null)
        {
            _customerGenerator.ClearActiveCustomers();
        }

        HumanGenerator[] generators = GetSceneHumanGenerators();
        for (int i = 0; i < generators.Length; i++)
        {
            SetTutorialOnlyHuman(generators[i], humanPrefab);
        }

        if (_customerGenerator != null)
        {
            _customerGenerator.RefreshHumanGenerators();
        }
    }

    public void ClearHumanGeneratorRestriction()
    {
        HumanGenerator[] generators = GetSceneHumanGenerators();
        for (int i = 0; i < generators.Length; i++)
        {
            SetTutorialOnlyHuman(generators[i], null);
        }
    }

    public void DeactivateGenerator()
    {
        if (_customerGenerator != null)
        {
            _customerGenerator.ClearActiveCustomers();
        }

        ClearHumanGeneratorRestriction();
        SetAllHumanGeneratorsActive(false);
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

    private void SetTutorialOnlyHuman(HumanGenerator generator, HumanBody humanPrefab)
    {
        if (generator == null)
        {
            return;
        }

        if (humanPrefab == null)
        {
            generator.ClearTutorialOnlyHuman();
            return;
        }

        generator.SetTutorialOnlyHuman(humanPrefab);
    }

    private void ClearActiveHumans()
    {
        HumanBody[] humans = FindObjectsByType<HumanBody>(FindObjectsSortMode.None);
        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i] != null)
            {
                Destroy(humans[i].gameObject);
            }
        }
    }

    private HumanGenerator GetPrimaryHumanGenerator()
    {
        if (_humanGenerator != null)
        {
            return _humanGenerator;
        }

        if (_humanGenerator2 != null)
        {
            return _humanGenerator2;
        }

        HumanGenerator[] generators = GetSceneHumanGenerators();
        return generators.Length > 0 ? generators[0] : null;
    }

    private HumanGenerator[] GetSceneHumanGenerators()
    {
        HumanGenerator[] generators = FindObjectsByType<HumanGenerator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<HumanGenerator> uniqueGenerators = new List<HumanGenerator>();
        AddUniqueGenerator(uniqueGenerators, _humanGenerator);
        AddUniqueGenerator(uniqueGenerators, _humanGenerator2);

        for (int i = 0; i < generators.Length; i++)
        {
            AddUniqueGenerator(uniqueGenerators, generators[i]);
        }

        return uniqueGenerators.ToArray();
    }

    private void SetAllHumanGeneratorsActive(bool isActive)
    {
        HumanGenerator[] generators = GetSceneHumanGenerators();
        for (int i = 0; i < generators.Length; i++)
        {
            SetGeneratorActive(generators[i], isActive);
        }

        if (_customerGenerator != null)
        {
            _customerGenerator.RefreshHumanGenerators();
        }
    }

    private void AddUniqueGenerator(List<HumanGenerator> generators, HumanGenerator generator)
    {
        if (generator != null && !generators.Contains(generator))
        {
            generators.Add(generator);
        }
    }
}
