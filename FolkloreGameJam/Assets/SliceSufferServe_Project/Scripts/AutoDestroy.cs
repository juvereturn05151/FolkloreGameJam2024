using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField]
    private float destroyCooldown = 2f;

    public void AutoDestroyItSelf()
    {
        Destroy(gameObject, destroyCooldown);
    }
}