using UnityEngine;

public class HumanPart : MonoBehaviour
{
    public GameObject fruitSlicedPrefab;
    public float startForce = 15f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Blade")
        {
            Vector3 direction = (col.transform.position - transform.position).normalized;

            GameObject slicedFruit = Instantiate(fruitSlicedPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}