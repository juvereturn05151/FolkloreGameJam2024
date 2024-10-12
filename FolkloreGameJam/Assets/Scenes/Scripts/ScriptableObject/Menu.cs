using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Scriptable Objects/Menu")]
public class Menu : ScriptableObject
{
    [Header("Menu Properties")]
    public string MenuName;
    public Sprite Sprite;
    public float Score;
}
