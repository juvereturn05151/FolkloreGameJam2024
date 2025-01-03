using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdvancedTutorialManager_Base : MonoBehaviour
{
    [SerializeField]
    protected DialogueManager _dialogueManager;

    [SerializeField]
    protected GameObject _backGround;

    [SerializeField]
    protected GameObject _tutorialDisplayBackGround;

    [SerializeField]
    protected GameObject _textBox;

    [SerializeField]
    protected GameObject _nextButton;

    [SerializeField]
    protected Button _skipButton;

    [SerializeField]
    protected List<TutorialStep> _tutorialList = new List<TutorialStep>();

    [SerializeField]
    protected AdvancedTutorialUIController _advancedTutorialUIController;

    private const string _firstGameplayScene = "GameplayScene";

    protected int _currentTutorialIndex;
    protected bool _isOperating;

    public TutorialStep CurrentTutorial { get; protected set; }
    public bool IsOperating => _isOperating;

    protected virtual void Start()
    {
        _skipButton.onClick.AddListener(LoadGameScene);

        GameManager.Instance.State = GameManager.GameState.Stop;

        if (_tutorialList != null && _tutorialList.Count > 0)
        {
            InitializeDialogue();
            ActivateTutorial();
        }
    }

    protected virtual void Update()
    {
        if (CurrentTutorial != null && _isOperating)
        {
            CurrentTutorial.TutorialAttribute.CheckingObjective();

            if (CurrentTutorial.TutorialAttribute.IsComplete)
            {
                OnTutorialEnd();
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

    public virtual void OnDialogueEnd()
    {
        _isOperating = true;
        _backGround.SetActive(false);
        _nextButton.SetActive(false);
        _advancedTutorialUIController.OnDialogueEnd(_currentTutorialIndex);
        GameManager.Instance.State = GameManager.GameState.StartGame;
    }

    protected virtual void OnTutorialEnd()
    {
        _isOperating = false;
        GameManager.Instance.State = GameManager.GameState.Stop;
        _backGround.SetActive(true);
        _nextButton.SetActive(true);
        _advancedTutorialUIController.OnTutorialEnd(_currentTutorialIndex);
        _currentTutorialIndex++;

        if (_currentTutorialIndex >= _tutorialList.Count)
        {
            LoadGameScene();
            return;
        }

        if (_tutorialList != null && _tutorialList.Count > 0)
        {
            ActivateTutorial();
            _textBox.SetActive(true);
            _tutorialDisplayBackGround.SetActive(true);
            ResetEndDialogue();
        }
    }

    protected virtual void ActivateTutorial()
    {
        CurrentTutorial = _tutorialList[_currentTutorialIndex];

        if (CurrentTutorial.ShowTutorialGuideOnStart)
        {
            _advancedTutorialUIController.AdvancedTutorialUI[_currentTutorialIndex].SetActive(true);
        }

        _dialogueManager._onDialogueEnd.AddListener(CurrentTutorial.StartOperating);
        _dialogueManager._onDialogueEnd.AddListener(OnDialogueEnd);
        _dialogueManager.StartDialogue(CurrentTutorial.DialogueLines, CurrentTutorial.ObjectiveDialogue, CurrentTutorial.WhatToDoDialogue);
    }

    protected virtual void InitializeDialogue()
    {
        ResetDialogueListener();
        _dialogueManager._onSecondLineAppear.AddListener(OnSecondDialogue);
        _dialogueManager._onLastLineAppear.AddListener(OnLastDialogue);
    }

    private void ResetEndDialogue()
    {
        _dialogueManager._onDialogueEnd.RemoveAllListeners();
        _dialogueManager._onDialogueEnd.AddListener(CurrentTutorial.StartOperating);
        _dialogueManager._onDialogueEnd.AddListener(OnDialogueEnd);
    }

    private void ResetDialogueListener()
    {
        _dialogueManager._onDialogueEnd.RemoveAllListeners();
        _dialogueManager._onSecondLineAppear.RemoveAllListeners();
        _dialogueManager._onLastLineAppear.RemoveAllListeners();
    }

    private void LoadGameScene()
    {
        SoundManager.instance.PlayGameplayBGM();
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadScene);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(_firstGameplayScene);
    }
}
