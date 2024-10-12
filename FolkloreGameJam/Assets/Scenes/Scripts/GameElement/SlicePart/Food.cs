using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Food : MonoBehaviour
{
    [SerializeField]
    private Menu menu;
    public Menu Menu => menu;

    private void OnMouseDrag()
    {
        var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(_mousePos.x, _mousePos.y, transform.position.z);
    }
}

