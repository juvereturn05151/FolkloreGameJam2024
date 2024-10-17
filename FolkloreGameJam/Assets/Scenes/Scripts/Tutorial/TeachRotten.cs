using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/TeachRotten")]
public class TeachRotten : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _clear = AdvancedTutorialManager.Instance.rottenCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        AdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
