using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dialogueText;
    [SerializeField]
    private TextMeshProUGUI _objectiveText;
    [SerializeField]
    private Button nextButton;
    [SerializeField]
    private GameObject _objectiveBG;

    public float typingSpeed = 0.05f;
    public event Action SecondLineAppeared;
    public event Action LastLineAppeared;
    public event Action DialogueEnded;

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private string objectiveDialogue;
    private string _howToPlayDialogue;

    private void Start()
    {
        nextButton.onClick.AddListener(DisplayNextLine);
    }

    public void StartDialogue(string[] lines, string objective, string howToPlay)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning($"{nameof(DialogueManager)} needs at least one dialogue line.");
            return;
        }

        _objectiveText.text = "";
        _objectiveBG.SetActive(false);
        dialogueLines = lines;
        currentLineIndex = 0;
        nextButton.gameObject.SetActive(true);
        StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
        objectiveDialogue = objective;
        _howToPlayDialogue = howToPlay;
    }

    public void DisplayNextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
            {
                if (currentLineIndex == 1)
                {
                    SecondLineAppeared?.Invoke();
                }
                else if (currentLineIndex == dialogueLines.Length - 1)
                {
                    LastLineAppeared?.Invoke();
                }

                StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueText.text = "";
        dialogueText.text = _howToPlayDialogue;
        _objectiveText.text = objectiveDialogue;
        nextButton.gameObject.SetActive(false);
        _objectiveBG.SetActive(true);
        DialogueEnded?.Invoke();
    }
}
