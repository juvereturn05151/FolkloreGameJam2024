using System.Collections.Generic;
using UnityEngine;

public class HumanBody : MonoBehaviour
{
    [SerializeField] private HumanPart _head;
    [SerializeField] private HumanPart _neck;
    [SerializeField] private HumanPart _body;
    [SerializeField] private HumanPart _leg;

    private List<HumanPart> _parts = new List<HumanPart>();
    private bool _isBeingDestroyed;

    private void Awake()
    {
        ResolvePartReferences();
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

    public void ApplyLevelConfig(StageLevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            return;
        }

        ResolvePartReferences();
        SetPartActive(_head, levelConfig.IsBodyPartEnabled(HumanBodyPartType.Head));
        SetPartActive(_neck, levelConfig.IsBodyPartEnabled(HumanBodyPartType.Neck));
        SetPartActive(_body, levelConfig.IsBodyPartEnabled(HumanBodyPartType.Body));
        SetPartActive(_leg, levelConfig.IsBodyPartEnabled(HumanBodyPartType.Leg));
    }

    public void ApplyMovementSpeedMultiplier(float multiplier)
    {
        ResolvePartReferences();

        HumanPart[] parts = GetComponentsInChildren<HumanPart>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null)
            {
                parts[i].SetStartForceMultiplier(multiplier);
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

    private void ResolvePartReferences()
    {
        HumanPart[] parts = GetComponentsInChildren<HumanPart>(true);

        for (int i = 0; i < parts.Length; i++)
        {
            HumanPart part = parts[i];

            if (part == null)
            {
                continue;
            }

            string partName = part.name;

            if (_head == null && partName.Contains("Head"))
            {
                _head = part;
            }
            else if (_neck == null && partName.Contains("Neck"))
            {
                _neck = part;
            }
            else if (_body == null && partName.Contains("Body"))
            {
                _body = part;
            }
            else if (_leg == null && partName.Contains("Leg"))
            {
                _leg = part;
            }
        }
    }

    private void SetPartActive(HumanPart part, bool isActive)
    {
        if (part != null)
        {
            part.gameObject.SetActive(isActive);
        }
    }
}
