using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class ItemScript : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public RecipeScript Recipe;
    public bool IsInstrument;
    public bool IsClothes;
    public int HealCount;
    public int FeedCount;
}
