using UnityEngine;

public class HumanBody : MonoBehaviour
{
    [SerializeField]
    private HumanPart _head;

    [SerializeField]
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private float _upForce = 20.0f;

    private void Start()
    {
        //_rigidbody.AddForce(transform.up * _upForce, ForceMode2D.Impulse);
        _head.GetComponent<Rigidbody2D>().AddForce(new Vector3(-_upForce,0,0));
        _head.OnPartDestroyed.AddListener(DestroyItself);
    }

    private void DestroyItself()
    {
        Destroy(this.gameObject);
    }
}
