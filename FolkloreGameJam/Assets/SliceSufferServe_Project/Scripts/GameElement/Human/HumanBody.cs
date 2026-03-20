using System.Collections.Generic;
using UnityEngine;

public class HumanBody : MonoBehaviour
{
    private List<HumanPart> _parts = new();
    private bool _isBeingDestroyed;

    private void Awake()
    {
        _parts.Clear();
        _parts.AddRange(GetComponentsInChildren<HumanPart>());

        foreach (HumanPart part in _parts)
        {
            if (part != null)
            {
                part.SetOwner(this);
            }
        }
    }

    public void NotifyPartSliced(HumanPart slicedPart)
    {
        if (_isBeingDestroyed) return;
        _isBeingDestroyed = true;

        foreach (HumanPart part in _parts)
        {
            if (part == null) continue;

            if (part == slicedPart)
            {
                part.DestroyImmediately();
            }
            else
            {
                part.StartFadeAndDestroy();
            }
        }

        Destroy(gameObject, 1f);
    }
}