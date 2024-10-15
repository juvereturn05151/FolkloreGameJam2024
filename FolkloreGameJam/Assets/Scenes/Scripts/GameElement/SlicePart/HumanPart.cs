using System;
using UnityEngine;
using UnityEngine.Events;

public class HumanPart : MonoBehaviour
{
    public UnityEvent OnPartDestroyed;

    public GameObject fruitSlicedPrefab;
    public float startForce = 15f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(startForce, 0);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Blade")
        {
            Vector3 direction = (col.transform.position - transform.position).normalized;

            GameObject slicedFruit = Instantiate(fruitSlicedPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            if (OnPartDestroyed != null) 
            {
                OnPartDestroyed.Invoke();
            }
        }
    }
}
