using UnityEngine;

public class SliceLoadScene : LoadSceneEventBase
{
    [SerializeField]
    private HumanPart humanPart;

    private void Start()
    {
        // Add listener for part destruction
        if (humanPart != null)
            humanPart.OnPartDestroyed.AddListener(() => OnLoadSceneEvent());
    }
}
