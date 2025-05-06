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
    [Tooltip("Appear At First Dialogue")]
    private List<GameObject> _advancedTutorialUI = new List<GameObject>();
    public List<GameObject> AdvancedTutorialUI => _advancedTutorialUI;

    [SerializeField]
    private List<GameObject> _appearOnSecondDialogue = new List<GameObject>();
    public List<GameObject> AppearOnSecondDialogue => _appearOnSecondDialogue;

    [SerializeField]
    private List<GameObject> _appearOnLastDialogue = new List<GameObject>();
    public List<GameObject> AppearOnLastDialogue => _appearOnLastDialogue;

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
        if (list != null && index >= 0 && index < list.Count && list[index] != null)
        {
            list[index].SetActive(false);
        }
    }
}
