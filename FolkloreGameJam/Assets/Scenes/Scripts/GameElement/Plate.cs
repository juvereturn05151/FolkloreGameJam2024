using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [SerializeField] private GameObject currentCustomer;
    [SerializeField] private GameObject foodOnPlate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            foodOnPlate = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            foodOnPlate = null;
        }
    }
}
