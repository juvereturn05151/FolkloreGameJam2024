using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/ServeWithSuper")]
public class ServeWithSuperAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.ServeWithSuper);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
        SSSAdvancedTutorialManager.Instance.ActivateCustomerGenerator();
    }
}
