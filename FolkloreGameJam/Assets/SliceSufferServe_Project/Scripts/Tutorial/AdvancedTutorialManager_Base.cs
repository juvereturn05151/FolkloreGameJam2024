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

    protected int _currentTutorialIndex;
    protected bool _isOperating;

    public TutorialStep CurrentTutorial { get; protected set; }
    public bool IsOperating => _isOperating;

    protected virtual void OnEnable()
    {
        if (_dialogueManager == null)
        {
            return;
        }

        _dialogueManager.SecondLineAppeared += OnSecondDialogue;
        _dialogueManager.LastLineAppeared += OnLastDialogue;
        _dialogueManager.DialogueEnded += OnDialogueEnd;
    }

    protected virtual void OnDisable()
    {
        if (_dialogueManager == null)
        {
            return;
        }

        _dialogueManager.SecondLineAppeared -= OnSecondDialogue;
        _dialogueManager.LastLineAppeared -= OnLastDialogue;
        _dialogueManager.DialogueEnded -= OnDialogueEnd;
    }

    protected virtual void Start()
    {
        _skipButton.onClick.AddListener(LoadGameScene);

        SetGameState(GameManager.GameState.Stop);

        if (_tutorialList != null && _tutorialList.Count > 0)
        {
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
        Debug.Log("Second dialogue appeared.");
        if (CurrentTutorial != null && CurrentTutorial.ShowOnSecondDialogue)
        {
            Debug.Log("Showing second dialogue guide.");
            _advancedTutorialUIController.ShowSecondDialogueGuide(_currentTutorialIndex);
        }
    }

    public void OnLastDialogue()
    {
        if (CurrentTutorial != null && CurrentTutorial.ShowOnLastDialogue)
        {
            _advancedTutorialUIController.ShowLastDialogueGuide(_currentTutorialIndex);
        }
    }

    public virtual void OnDialogueEnd()
    {
        _isOperating = true;
        _backGround.SetActive(false);
        _nextButton.SetActive(false);
        _advancedTutorialUIController.OnDialogueEnd(_currentTutorialIndex);
        SetGameState(GameManager.GameState.StartGame);
        CurrentTutorial?.StartOperating();
    }

    protected virtual void OnTutorialEnd()
    {
        _isOperating = false;
        SetGameState(GameManager.GameState.Stop);
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
        }
    }

    protected virtual void ActivateTutorial()
    {
        CurrentTutorial = _tutorialList[_currentTutorialIndex];

        _dialogueManager.StartDialogue(CurrentTutorial.DialogueLines, CurrentTutorial.ObjectiveDialogue, CurrentTutorial.WhatToDoDialogue);
    }

    private void LoadGameScene()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayGameplayBGM();
        }

        if (FadingUI.Instance != null)
        {
            FadingUI.Instance.StartFadeIn();
            FadingUI.Instance.OnStopFading.AddListener(LoadScene);
            return;
        }

        LoadScene();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(StageSelection.GetSelectedGameplaySceneName());
    }

    private void SetGameState(GameManager.GameState state)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.State = state;
        }
    }
}
