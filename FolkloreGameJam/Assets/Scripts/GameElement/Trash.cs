using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private GameObject trashFX;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            if (GameUtility.AdvancedTutorialManagerExists())
            {
                if (GameManager.Instance.IsTutorial && AdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.PutTrashToBin)
                {
                    AdvancedTutorialManager.Instance.trashInBinCount++;
                }
            }

            SoundManager.instance.PlaySFX("Trash");
            Instantiate(trashFX, transform.position, Quaternion.identity);
            Destroy(food.gameObject);
        }
    }
}