using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneSequence", menuName = "Slice Suffer Serve/Cutscene Sequence")]
public class CutsceneSequence : ScriptableObject
{
    [SerializeField] private List<CutsceneEntry> cutscenes = new List<CutsceneEntry>();

    public IReadOnlyList<CutsceneEntry> Cutscenes => cutscenes;
    public int Count => cutscenes != null ? cutscenes.Count : 0;

    public CutsceneEntry GetCutscene(int index)
    {
        if (cutscenes == null || index < 0 || index >= cutscenes.Count)
        {
            return null;
        }

        return cutscenes[index];
    }
}

[Serializable]
public class CutsceneEntry
{
    [SerializeField] private string title;
    [SerializeField] private Sprite background;
    [TextArea(2, 5)]
    [SerializeField] private List<string> dialogueLines = new List<string>();

    public string Title => title;
    public Sprite Background => background;
    public IReadOnlyList<string> DialogueLines => dialogueLines;
    public int DialogueLineCount => dialogueLines != null ? dialogueLines.Count : 0;

    public string GetDialogueLine(int index)
    {
        if (dialogueLines == null || index < 0 || index >= dialogueLines.Count)
        {
            return string.Empty;
        }

        return dialogueLines[index];
    }
}
