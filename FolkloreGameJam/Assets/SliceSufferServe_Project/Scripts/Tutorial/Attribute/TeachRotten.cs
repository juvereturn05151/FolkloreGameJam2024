using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/TeachRotten")]
public class TeachRotten : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = SSSAdvancedTutorialManager.Instance.rottenCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
