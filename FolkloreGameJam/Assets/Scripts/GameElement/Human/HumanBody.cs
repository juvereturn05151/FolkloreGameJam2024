using UnityEngine;

public class HumanBody : MonoBehaviour
{
    [SerializeField]
    private HumanPart _head;

    [SerializeField]
    private HumanPart _neck;

    [SerializeField]
    private HumanPart _body;

    [SerializeField]
    private HumanPart _leg;

    private void Start()
    {
        //_head.OnPartDestroyed.AddListener(DestroyItself);
    }

    private void Update()
    {
        if (_head == null && _neck == null && _body == null && _leg == null) 
        {
            Destroy(this.gameObject);
        }
    }

    private void DestroyItself()
    {
        Destroy(this.gameObject);
    }
}
