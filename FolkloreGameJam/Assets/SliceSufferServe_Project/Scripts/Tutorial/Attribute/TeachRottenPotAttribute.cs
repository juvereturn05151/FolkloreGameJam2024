using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/TeachRottenPot")]
public class TeachRottenPotAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.PutFoodInRottenPot);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
    }
}
