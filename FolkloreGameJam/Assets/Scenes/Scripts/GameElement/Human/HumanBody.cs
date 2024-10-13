using UnityEngine;

public class HumanBody : MonoBehaviour
{
    [SerializeField]
    private HumanPart _head;

    private void Start()
    {
        _head.OnPartDestroyed.AddListener(DestroyItself);
    }

    private void DestroyItself()
    {
        Destroy(this.gameObject);
    }
}
