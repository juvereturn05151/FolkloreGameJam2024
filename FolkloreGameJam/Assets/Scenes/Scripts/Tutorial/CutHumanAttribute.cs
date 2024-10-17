using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/CutHumanAttribute")]
public class CutHumanAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _clear = AdvancedTutorialManager.Instance._humanKillCount >= 3;
    }

    public override void SetBegin()
    {
        base.SetBegin();
        AdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
