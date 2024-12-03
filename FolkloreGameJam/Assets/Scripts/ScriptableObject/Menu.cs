using UnityEngine;

public enum FoodType
{
    Brain,
    Blood,
    Intestine,
    Poo
}

[CreateAssetMenu(fileName = "Menu", menuName = "Scriptable Objects/Menu")]
public class Menu : ScriptableObject
{
    [Header("Menu Properties")]
    public FoodType FoodType;
    public Sprite Sprite;
    public Sprite MediumRottenSprite;
    public Sprite SuperRottenSprite;
    public int Score;
}
