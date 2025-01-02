/*
 Brief: Tutorial Step is a tutorial task that players need to completed
*/
using UnityEngine;

public enum TutorialType
{
    CutHuman,           // Cut a human-like object in the game
    WaitForRotten,      // Wait for an object to rot
    ServeCustomer,      // Serve a customer in-game
    EndTutorial,        // Marks the end of the tutorial
    PutTrashToBin       // Dispose of trash properly
}

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step", fileName = "NewTutorialStep")]
public class TutorialStep : ScriptableObject
{
    [Header("General Settings")]
    [Tooltip("Type of tutorial step.")]
    public TutorialType Type;

    [Tooltip("Lines of dialogue shown during this step.")]
    [TextArea(3, 5)]
    public string[] DialogueLines = 
    {
        "Hello there!",
        "Welcome to our game.",
        "Enjoy your adventure!"
    };

    [Header("Dialogue Display Options")]
    [Tooltip("Show the tutorial guide at the start of this step.")]
    public bool ShowTutorialGuideOnStart;

    [Tooltip("Show a guide during the second dialogue.")]
    public bool ShowOnSecondDialogue;

    [Tooltip("Show a guide during the last dialogue.")]
    public bool ShowOnLastDialogue;

    [Header("Objective Settings")]
    [Tooltip("Text explaining the player's objective.")]
    public string ObjectiveDialogue = "Complete the objective.";

    [Tooltip("Text describing what the player needs to do.")]
    public string WhatToDoDialogue = "Perform the action.";

    public TutorialAttribute TutorialAttribute;

    /// <summary>
    /// Begins the tutorial step by invoking its attribute logic.
    /// </summary>
    public void StartOperating()
    {
        if (TutorialAttribute != null)
        {
            TutorialAttribute.SetBegin();
        }
        else
        {
            Debug.LogWarning("TutorialAttribute is not assigned for this step.");
        }
    }

    /// <summary>
    /// Called when the object is modified in the editor.
    /// Ensures that required fields are assigned.
    /// </summary>
    private void OnValidate()
    {
        if (TutorialAttribute == null)
        {
            Debug.LogWarning($"TutorialAttribute is missing in {name}. Assign one to avoid runtime issues.");
        }

        if (DialogueLines == null || DialogueLines.Length == 0)
        {
            Debug.LogWarning($"DialogueLines is empty in {name}. Add at least one dialogue line.");
        }
    }
}
