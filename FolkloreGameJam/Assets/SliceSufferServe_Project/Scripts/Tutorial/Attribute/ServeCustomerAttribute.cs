using UnityEngine;
[CreateAssetMenu(menuName = "TutorialAttribute/ServeCustomer")]
public class ServeCustomerAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.ServeCustomer);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
        SSSAdvancedTutorialManager.Instance.ActivateCustomerGenerator();
    }
}
