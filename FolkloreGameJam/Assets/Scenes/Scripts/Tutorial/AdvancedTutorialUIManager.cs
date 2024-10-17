using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedTutorialUIController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _advancedTutorialUI = new List<GameObject>();
    public List<GameObject> AdvancedTutorialUI => _advancedTutorialUI;

    [SerializeField]
    private List<GameObject> _appearOnSecondDialogue = new List<GameObject>();
    public List<GameObject> AppearOnSecondDialogue => _appearOnSecondDialogue;

    [SerializeField]
    private List<GameObject> _appearOnLastDialogue = new List<GameObject>();
    public List<GameObject> AppearOnLastDialogue => _appearOnLastDialogue;

    public void OnTutorialEnd(int _currentTutorialIndex)
    {
        /*if (AdvancedTutorialUI != null && AdvancedTutorialUI[_currentTutorialIndex])
            AdvancedTutorialUI[_currentTutorialIndex].SetActive(false);*/

        if (AppearOnSecondDialogue != null && AppearOnSecondDialogue[_currentTutorialIndex])
            AppearOnSecondDialogue[_currentTutorialIndex].SetActive(false);

        if (AppearOnLastDialogue != null && AppearOnLastDialogue[_currentTutorialIndex])
            AppearOnLastDialogue[_currentTutorialIndex].SetActive(false);
    }

    public void OnDialogueEnd(int _currentTutorialIndex)
    {
        if(AppearOnLastDialogue != null && AppearOnLastDialogue[_currentTutorialIndex])
        AppearOnLastDialogue[_currentTutorialIndex].SetActive(false);

        if (AppearOnSecondDialogue != null && AppearOnSecondDialogue[_currentTutorialIndex])
            AppearOnSecondDialogue[_currentTutorialIndex].SetActive(false);

        /*if (AdvancedTutorialUI != null &&  AdvancedTutorialUI[_currentTutorialIndex])
            AdvancedTutorialUI[_currentTutorialIndex].SetActive(true);*/
    }
}
