using UnityEngine;

public class HumanPart : SliceableObject
{
    [SerializeField]
    private GameObject fruitSlicedPrefab;

    [SerializeField] private GameObject bloodFX;
    [SerializeField] private GameObject bloodSplashFX;

    protected override void OnHitWithBlade(Collider2D col)
    {
        Instantiate(bloodFX, transform.position, Quaternion.identity);
        Instantiate(bloodSplashFX, transform.position, Quaternion.identity);

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 0.25f);
        }

        if (!atMainMenu)
        {
            if (GameManager.Instance.IsTutorial && SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.CutHuman)
            {
                SSSAdvancedTutorialManager.Instance._humanKillCount++;
            }
        }


        Vector3 direction = (col.transform.position - transform.position).normalized;

        GameObject slicedFruit = Instantiate(fruitSlicedPrefab, transform.position, Quaternion.identity);
        base.OnHitWithBlade(col);
    }
}
