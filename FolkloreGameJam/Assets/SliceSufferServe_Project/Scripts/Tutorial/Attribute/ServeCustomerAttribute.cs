using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TutorialAttribute/ServeCustomer")]
public class ServeCustomerAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = SSSAdvancedTutorialManager.Instance.serveCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
        SSSAdvancedTutorialManager.Instance.ActivateCustomerGenerator();
    }
}
