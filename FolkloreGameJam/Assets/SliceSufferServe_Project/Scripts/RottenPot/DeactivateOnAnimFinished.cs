using UnityEngine;

public class DeactivateOnAnimFinished : MonoBehaviour
{
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
