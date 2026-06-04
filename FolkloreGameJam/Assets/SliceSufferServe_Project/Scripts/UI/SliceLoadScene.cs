using UnityEngine;

public class SliceLoadScene : LoadSceneEventBase
{
    [SerializeField]
    private HumanPart humanPart;

    private void Start()
    {
        RefreshLockedVisualState();

        if (!IsLoadUnlocked())
        {
            if (humanPart != null)
            {
                humanPart.enabled = false;
            }

            return;
        }

        // Add listener for part destruction
        if (humanPart != null)
            humanPart.OnPartDestroyed.AddListener(() => OnLoadSceneEvent());
    }
}
