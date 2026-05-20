using UnityEngine;

public class DeactivateOnAnimFinished : MonoBehaviour
{
    public void Deactivate()
    {
        Debug.Log("Animation finished, deactivating object.");
        gameObject.SetActive(false);
    }
}
