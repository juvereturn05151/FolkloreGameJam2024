using UnityEngine;
[CreateAssetMenu(menuName = "TutorialAttribute/PutTrashAttribute")]
public class PutTrashAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = SSSAdvancedTutorialManager.Instance.trashInBinCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
