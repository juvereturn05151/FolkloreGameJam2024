using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdvancedTutorialManager : MonoBehaviour
{
    public static AdvancedTutorialManager Instance => instance;
    private static AdvancedTutorialManager instance;

    [SerializeField]
    private DialogueManager dialogueManager;

    [SerializeField]
    private GameObject _backGround;

    [SerializeField]
    private GameObject _tutorialDisplayBackGround;

    [SerializeField]
    private GameObject _textBox;

    [SerializeField]
    private GameObject _nextButton;

    [SerializeField]
    private Button _skipButton;

    [SerializeField]
    private List<TutorialStep> _tutorialList = new List<TutorialStep>();

    [SerializeField]
    private AdvancedTutorialUIController _advancedTutorialUIController;

    [SerializeField]
    private HumanGenerator _humanGenerator;

    [SerializeField]
    private HumanGenerator _humanGenerator2;


    [SerializeField]
    private CustomerGenerator _customerGenerator;

    public TutorialStep CurrentTutorial { get; private set; }

    private const string _firstGameplayScene = "GameplayScene";
    private bool _isOperating;
    private int _currentTutorialIndex;

    public int _humanKillCount;

    public int rottenCount;

    public int serveCount;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        _skipButton.onClick.AddListener(LoadGameScene);

        GameManager.Instance.State = GameManager.GameState.Stop;

        if (_tutorialList != null && _tutorialList.Count > 0)
        {
            InitializeDialogue();
            ActivateTutorial();
        }
    }

    private void Update()
    {
        if (CurrentTutorial != null)
        {
            if (_isOperating)
            {
                CurrentTutorial.TutorialAttribute.CheckingObjective();

                if (CurrentTutorial.TutorialAttribute.Clear)
                {

                    OnTutorialEnd();
                }
            }
        }
    }

    public void OnSecondDialogue()
    {
        if (CurrentTutorial.ShowOnSecondDialogue)
        {
            _advancedTutorialUIController.AppearOnSecondDialogue[_currentTutorialIndex].SetActive(true);
        }
    }

    public void OnLastDialogue()
    {
        if (CurrentTutorial.ShowOnLastDialogue)
        {
            if (_advancedTutorialUIController.AppearOnLastDialogue[_currentTutorialIndex]) 
            {
                _advancedTutorialUIController.AppearOnLastDialogue[_currentTutorialIndex].SetActive(true);
            }

            if (_advancedTutorialUIController.AppearOnSecondDialogue[_currentTutorialIndex]) 
            {
                _advancedTutorialUIController.AppearOnSecondDialogue[_currentTutorialIndex].SetActive(false);
            }
        }
    }

    public void OnDialogueEnd()
    {
        Debug.Log("_isOperating");
        _isOperating = true;
        _backGround.SetActive(false);
        _nextButton.SetActive(false);
        //_tutorialDisplayBackGround.SetActive(true);
        _advancedTutorialUIController.OnDialogueEnd(_currentTutorialIndex);
        GameManager.Instance.State = GameManager.GameState.StartGame;
    }

    private void OnTutorialEnd()
    {
        _isOperating = false;
        GameManager.Instance.State = GameManager.GameState.Stop;
        _backGround.SetActive(true);
        _nextButton.SetActive(true);
        //_tutorialDisplayBackGround.SetActive(false);
        _advancedTutorialUIController.OnTutorialEnd(_currentTutorialIndex);
        DeactivateGenerator();
        _currentTutorialIndex++;

        if (_currentTutorialIndex >= _tutorialList.Count)
        {
            LoadGameScene();
            return;
        }

        if (_tutorialList != null && _tutorialList.Count > 0)
        {
            ActivateTutorial();
            ResetEndDialogue();
        }
    }

    private void ActivateTutorial()
    {
        CurrentTutorial = _tutorialList[_currentTutorialIndex];

        if (CurrentTutorial.ShowTutorialGuideOnStart)
        {
            _advancedTutorialUIController.AdvancedTutorialUI[_currentTutorialIndex].SetActive(true);
            //_tutorialDisplayBackGround.SetActive(CurrentTutorial.ShowTutorialGuideBackground);
        }

        dialogueManager._onDialogueEnd.AddListener(CurrentTutorial.StartOperating);
        dialogueManager._onDialogueEnd.AddListener(OnDialogueEnd);
        dialogueManager.StartDialogue(CurrentTutorial.DialogueLines, CurrentTutorial.ObjectiveDialogue, CurrentTutorial.WhatToDoDialogue);
    }

    private void InitializeDialogue()
    {
        ResetDialogueListener();
        dialogueManager._onSecondLineAppear.AddListener(OnSecondDialogue);
        dialogueManager._onLastLineAppear.AddListener(OnLastDialogue);

    }

    private void ResetEndDialogue()
    {
        dialogueManager._onDialogueEnd.RemoveAllListeners();
        dialogueManager._onDialogueEnd.AddListener(CurrentTutorial.StartOperating);
        dialogueManager._onDialogueEnd.AddListener(OnDialogueEnd);
    }

    private void ResetDialogueListener()
    {
        dialogueManager._onDialogueEnd.RemoveAllListeners();
        dialogueManager._onSecondLineAppear.RemoveAllListeners();
        dialogueManager._onLastLineAppear.RemoveAllListeners();
    }

    private void LoadGameScene()
    {
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadScene);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(_firstGameplayScene);
    }

    public void ActivateHumanGenerator()
    {
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
}