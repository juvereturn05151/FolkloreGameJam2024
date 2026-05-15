using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/TeachSuperMeter")]
public class TeachSuperMeterAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.UseSuperMeter);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
