using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HumanPart : MonoBehaviour
{
    public UnityEvent OnPartDestroyed;

    public event Action<Vector3, IReadOnlyList<FeedbackRequest>> Sliced;

    [Header("Slice Feedback Settings")]
    [SerializeField] private List<FeedbackRequest> feedbackRequests = new();

    private bool _sliced;

    [SerializeField]
    private GameObject foodPrefab;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float startForce = 15f;
    [SerializeField]
    private bool atMainMenu;

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        rb.linearVelocity = new Vector2(startForce, 0);
    }

    private void OnEnable()
    {
        if (GameUtility.FeedbackManagerExists())
            FeedbackManager.Instance.Hook(this);

        if (!atMainMenu && GameUtility.SSSAdvancedTutorialManagerExists())
            SSSAdvancedTutorialManager.Instance.Hook(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_sliced) return;
        if (!col.CompareTag(GameTagContainer.BladeTag)) return;

        _sliced = true;

        if (foodPrefab)
            Instantiate(foodPrefab, transform.position, Quaternion.identity);

        Sliced?.Invoke(transform.position, feedbackRequests);

        Destroy(gameObject);
    }
}
