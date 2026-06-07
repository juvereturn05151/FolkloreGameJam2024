using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickSoundPlayer : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        PlayClickSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (selectable != null && !selectable.IsInteractable())
        {
            return;
        }

        SoundManager.instance?.PlayClickSFX();
    }
}
