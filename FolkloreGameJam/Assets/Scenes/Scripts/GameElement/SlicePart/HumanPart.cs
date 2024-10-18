using System;
using UnityEngine;
using UnityEngine.Events;

public class HumanPart : MonoBehaviour
{
    public UnityEvent OnPartDestroyed;

    public GameObject fruitSlicedPrefab;
    public float startForce = 15f;

    [SerializeField]
    bool atMainMenu;

    Rigidbody2D rb;
    
    [SerializeField] private GameObject bloodFX;
    [SerializeField] private GameObject bloodSplashFX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(startForce, 0);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Blade")
        {
            Instantiate(bloodFX, transform.position, Quaternion.identity);
            Instantiate(bloodSplashFX, transform.position, Quaternion.identity);
            
            if (!atMainMenu) 
            {
                if (GameManager.Instance.IsTutorial && AdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.CutHuman)
                {
                    AdvancedTutorialManager.Instance._humanKillCount++;
                }
            }


            Vector3 direction = (col.transform.position - transform.position).normalized;

            GameObject slicedFruit = Instantiate(fruitSlicedPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            if (OnPartDestroyed != null) 
            {
                OnPartDestroyed.Invoke();
            }
        }
    }
}
