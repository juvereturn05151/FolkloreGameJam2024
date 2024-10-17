using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public void AutoDestroyItSelf() 
    {
        Destroy(gameObject);
    }
}
