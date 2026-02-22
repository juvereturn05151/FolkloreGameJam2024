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

        // Spawn at part position, but do NOT parent it
        GameObject bomb = Instantiate(_bombPrefab,
            targetPart.transform.position,
            Quaternion.identity);

        // Optional: give slight random force so it looks natural
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), 2f), ForceMode2D.Impulse);
        }
    }
}