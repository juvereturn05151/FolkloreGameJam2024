using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject trashFX;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            if (GameUtility.AdvancedTutorialManagerExists())
            {
                if (GameManager.Instance.IsTutorial && SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.PutTrashToBin)
                {
                    SSSAdvancedTutorialManager.Instance.trashInBinCount++;
                }
            }

            if (_animator != null) 
            {
                _animator.SetTrigger("Hover");
            }

            SoundManager.instance.PlaySFX("Trash");
            Instantiate(trashFX, transform.position, Quaternion.identity);
            Destroy(food.gameObject);
        }
    }
}