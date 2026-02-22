using System.Collections.Generic;
using UnityEngine;

public class HumanBody : SpawnableObject
{
    [SerializeField] private HumanPart _head;
    [SerializeField] private HumanPart _neck;
    [SerializeField] private HumanPart _body;
    [SerializeField] private HumanPart _leg;

    [SerializeField] private GameObject _bombPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float _bombSpawnChance = 0.4f;

    private void Start()
    {
        TrySpawnBomb();
    }

    private void Update()
    {
        if (_head == null && _neck == null && _body == null && _leg == null)
        {
            Destroy(gameObject);
        }
    }

    private void TrySpawnBomb()
    {
        if (_bombPrefab == null) return;

        if (Random.value > _bombSpawnChance)
            return;

        List<HumanPart> availableParts = new List<HumanPart>();

        if (_head != null) availableParts.Add(_head);
        if (_neck != null) availableParts.Add(_neck);
        if (_body != null) availableParts.Add(_body);
        if (_leg != null) availableParts.Add(_leg);

        if (availableParts.Count == 0)
            return;

        HumanPart targetPart = availableParts[Random.Range(0, availableParts.Count)];

        GameObject bomb = Instantiate(_bombPrefab, targetPart.transform);
        bomb.transform.localPosition = Vector3.zero;
    }
}