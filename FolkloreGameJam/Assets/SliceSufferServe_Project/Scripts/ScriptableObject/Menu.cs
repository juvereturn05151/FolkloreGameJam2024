using UnityEngine;

public enum FoodType
{
    Brain,
    Blood,
    Intestine,
    Poo
}

[CreateAssetMenu(fileName = "Food", menuName = "Scriptable Objects/Food")]
public class Menu : ScriptableObject
{
    [Header("Menu Properties")]
    public FoodType FoodType;
    public Sprite Sprite;
    public Sprite MediumRottenSprite;
    public Sprite SuperRottenSprite;
    public int Score;
}
