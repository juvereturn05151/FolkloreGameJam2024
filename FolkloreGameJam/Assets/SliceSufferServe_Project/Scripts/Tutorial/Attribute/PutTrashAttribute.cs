using UnityEngine;
[CreateAssetMenu(menuName = "TutorialAttribute/PutTrashAttribute")]
public class PutTrashAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.PutTrashToBin);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
