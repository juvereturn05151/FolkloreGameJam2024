using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/ServeUniversalFood")]
public class ServeUniversalFoodAttribute : TutorialAttribute
{
    [SerializeField] private HumanBody tutorialHumanPrefab;
    [SerializeField] private float movementSpeedMultiplier = 0.8f;

    public override void CheckingObjective()
    {
        CompleteWhenProgressReaches(TutorialType.ServeUniversalFood);
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
