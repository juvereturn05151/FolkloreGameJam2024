using UnityEngine;
[CreateAssetMenu(menuName = "TutorialAttribute/PutTrashAttribute")]
public class PutTrashAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = AdvancedTutorialManager.Instance.trashInBinCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        AdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
