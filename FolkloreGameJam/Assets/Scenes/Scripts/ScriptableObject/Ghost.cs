using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ghost", menuName = "Scriptable Objects/Ghost")]
public class Ghost : ScriptableObject
{
    [Header("Ghost Properties")]
    public string Name;
    public Sprite Sprite;
    public List<MenuRating> FavoriteMenu = new List<MenuRating>();
    public List<MenuRating> UnfavoriteMenu = new List<MenuRating>();

    public List<Menu> possibleRequest = new List<Menu>();
}

[Serializable]
public class MenuRating
{
    public Menu Menu;
    
    [Tooltip("If likes this menu will + (score / patience ??) by value but if dislike then the (score / patience) will be - by value")]
    [Range(0, 10)]
    public int Value;
}
