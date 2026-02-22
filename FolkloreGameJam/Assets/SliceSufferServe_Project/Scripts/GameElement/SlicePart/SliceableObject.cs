using UnityEngine;
using UnityEngine.Events;

public class SliceableObject : MonoBehaviour
{
    public UnityEvent OnPartDestroyed;

    [SerializeField]
    private float startForce = 15f;

    [SerializeField]
    private bool atMainMenu;

    private Rigidbody2D rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(startForce, 0);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Blade"))
        {
            OnHitWithBlade(col);
        }
    }

    protected virtual void OnHitWithBlade(Collider2D col)
    {
        Destroy(gameObject);
        if (OnPartDestroyed != null)
        {
            OnPartDestroyed.Invoke();
        }
    }
}
