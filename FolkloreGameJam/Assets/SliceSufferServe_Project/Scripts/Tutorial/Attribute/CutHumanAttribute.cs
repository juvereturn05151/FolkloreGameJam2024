using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/CutHumanAttribute")]
public class CutHumanAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = SSSAdvancedTutorialManager.Instance._humanKillCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
