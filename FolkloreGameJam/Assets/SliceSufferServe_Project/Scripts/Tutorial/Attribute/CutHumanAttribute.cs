using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/CutHumanAttribute")]
public class CutHumanAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.CutHuman);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
