using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/TeachRotten")]
public class TeachRotten : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.WaitForRotten);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
