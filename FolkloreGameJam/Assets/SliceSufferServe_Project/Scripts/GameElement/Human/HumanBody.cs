using System.Collections;
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
    public bool IsBeingDestroyed => _isBeingDestroyed;

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

        StartCoroutine(DestroyAndRequestReplacementAfterDelay(1f));
    }

    public void NotifyMissedDestroyer()
    {
        if (_isBeingDestroyed) return;
        _isBeingDestroyed = true;
        ComboSystem.ResetCombo();

        foreach (HumanPart part in _parts)
        {
            if (part == null) continue;
            part.DestroyWithoutFood();
        }

        RequestReplacementHuman();

        Destroy(gameObject);
    }

    public void NotifyPartMissedDestroyer(HumanPart missedPart)
    {
        if (_isBeingDestroyed || missedPart == null || !_parts.Contains(missedPart))
        {
            return;
        }

        ComboSystem.ResetCombo();
        _parts.Remove(missedPart);
        missedPart.DestroyWithoutFood();

        RequestReplacementHumanNextFrame();

        if (!HasRemainingParts())
        {
            Destroy(gameObject);
        }
    }

    private void RequestReplacementHuman()
    {
        if (CustomerGenerator.Instance != null)
        {
            CustomerGenerator.Instance.RequestReplacementHuman();
        }
    }

    private void RequestReplacementHumanNextFrame()
    {
        if (CustomerGenerator.Instance != null)
        {
            CustomerGenerator.Instance.RequestReplacementHumanNextFrame();
        }
    }

    private IEnumerator DestroyAndRequestReplacementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        RequestReplacementHuman();
        Destroy(gameObject);
    }

    public void AddAvailablePartMenus(Dictionary<Menu, int> menuCounts)
    {
        if (_isBeingDestroyed || menuCounts == null)
        {
            return;
        }

        HumanPart[] parts = GetComponentsInChildren<HumanPart>(false);
        for (int i = 0; i < parts.Length; i++)
        {
            HumanPart part = parts[i];
            if (part == null || !part.CanProduceFood)
            {
                continue;
            }

            Menu menu = part.GetProducedMenu();
            if (menu == null)
            {
                continue;
            }

            AddMenuCount(menuCounts, menu, 1);
        }
    }

    private static void AddMenuCount(Dictionary<Menu, int> menuCounts, Menu menu, int amount)
    {
        if (!menuCounts.ContainsKey(menu))
        {
            menuCounts[menu] = 0;
        }

        menuCounts[menu] += amount;
    }

    private bool HasRemainingParts()
    {
        for (int i = 0; i < _parts.Count; i++)
        {
            if (_parts[i] != null && _parts[i].gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
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
