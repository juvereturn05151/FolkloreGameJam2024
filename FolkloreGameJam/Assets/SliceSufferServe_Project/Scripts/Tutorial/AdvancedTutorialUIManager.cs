/*
 Brief: I designed the advanced tutorial system to make 
 the tutorial UI either appear at start(_advancedTutorialUI), second dialogue,
 or last dialogue.
*/

using System.Collections.Generic;
using UnityEngine;

public class AdvancedTutorialUIController : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> _appearOnSecondDialogue = new List<GameObject>();
    public List<GameObject> AppearOnSecondDialogue => _appearOnSecondDialogue;

    [SerializeField]
    private List<GameObject> _appearOnLastDialogue = new List<GameObject>();
    public List<GameObject> AppearOnLastDialogue => _appearOnLastDialogue;

    public void ShowSecondDialogueGuide(int currentTutorialIndex)
    {
        SetElementActive(_appearOnSecondDialogue, currentTutorialIndex, true);
    }

    public void ShowLastDialogueGuide(int currentTutorialIndex)
    {
        SetElementActive(_appearOnLastDialogue, currentTutorialIndex, true);
        SetElementActive(_appearOnSecondDialogue, currentTutorialIndex, false);
    }

    /// <summary>
    /// Deactivates the relevant UI elements when a tutorial ends.
    /// </summary>
    /// <param name="currentTutorialIndex">The index of the current tutorial.</param>
    public void OnTutorialEnd(int currentTutorialIndex)
    {
        DeactivateElement(_appearOnSecondDialogue, currentTutorialIndex);
        DeactivateElement(_appearOnLastDialogue, currentTutorialIndex);
    }

    /// <summary>
    /// Deactivates the relevant UI elements when a dialogue ends.
    /// </summary>
    /// <param name="currentTutorialIndex">The index of the current tutorial.</param>
    public void OnDialogueEnd(int currentTutorialIndex)
    {
        DeactivateElement(_appearOnSecondDialogue, currentTutorialIndex);
        DeactivateElement(_appearOnLastDialogue, currentTutorialIndex);
    }

    /// <summary>
    /// Deactivates a GameObject from a list based on the specified index.
    /// </summary>
    /// <param name="list">The list of GameObjects.</param>
    /// <param name="index">The index of the GameObject to deactivate.</param>
    private void DeactivateElement(List<GameObject> list, int index)
    {
        SetElementActive(list, index, false);
    }

    private void SetElementActive(List<GameObject> list, int index, bool isActive)
    {
        GameObject element = GetElement(list, index);

        if (element != null)
        {
            element.SetActive(isActive);
        }
    }

    private GameObject GetElement(List<GameObject> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            return null;
        }

        return list[index];
    }
}
