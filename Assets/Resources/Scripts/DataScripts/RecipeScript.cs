using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
public class RecipeScript : ScriptableObject
{
    public float Speed;
    public ItemsPackScript Items;
}
