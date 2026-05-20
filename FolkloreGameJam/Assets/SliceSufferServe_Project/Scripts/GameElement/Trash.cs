using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject trashFX;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            if (GameUtility.SSSAdvancedTutorialManagerExists())
            {
                SSSAdvancedTutorialManager.Instance.ReportProgress(TutorialType.PutTrashToBin);
            }

            if (_animator != null) 
            {
                _animator.SetTrigger("Hover");
            }

            SoundManager.instance.PlaySFX("Trash");
            Instantiate(trashFX, transform.position, Quaternion.identity);
            Destroy(food.gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddSuperMeterFromTrash();
            }
        }
    }
}
