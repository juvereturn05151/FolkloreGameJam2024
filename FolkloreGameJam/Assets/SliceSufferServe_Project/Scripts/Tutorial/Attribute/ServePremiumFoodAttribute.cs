using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/ServePremiumFood")]
public class ServePremiumFoodAttribute : TutorialAttribute
{
    [SerializeField] private HumanBody tutorialHumanPrefab;
    [SerializeField] private float movementSpeedMultiplier = 0.8f;

    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.ServePremiumFood);
    }

    public override void SetBegin()
    {
        base.SetBegin();
        SSSAdvancedTutorialManager.Instance.ActivateHumanGenerator();
        SSSAdvancedTutorialManager.Instance.ActivateCustomerGenerator();
        SSSAdvancedTutorialManager.Instance.RestrictHumanGeneratorsToTutorialHuman(tutorialHumanPrefab);
        SSSAdvancedTutorialManager.Instance.SpawnTutorialHuman(tutorialHumanPrefab, movementSpeedMultiplier);
    }
}
