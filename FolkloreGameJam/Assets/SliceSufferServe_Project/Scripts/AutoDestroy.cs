using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private bool destroyOnEnable = true;

    private float timer;

    private void OnEnable()
    {
        if (destroyOnEnable)
            timer = lifetime;
    }

    private void Update()
    {
        if (!destroyOnEnable) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // Optional: manual trigger
    public void AutoDestroyItSelf(float delay)
    {
        Destroy(gameObject, delay);
    }

    // Optional: start countdown manually
    public void StartCountdown(float duration)
    {
        lifetime = duration;
        timer = lifetime;
        destroyOnEnable = true;
    }

    // Optional: cancel destruction
    public void CancelCountdown()
    {
        destroyOnEnable = false;
    }
}