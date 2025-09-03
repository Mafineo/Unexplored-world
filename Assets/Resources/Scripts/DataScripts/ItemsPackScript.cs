using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Items pack", menuName = "Items pack")]
public class ItemsPackScript : ScriptableObject
{
    public List<ItemClass> Items = new List<ItemClass>();

    public ItemClass FindItem(ItemScript Item)
    {
        foreach (ItemClass NeedItem in Items) if (NeedItem.Item == Item) return NeedItem;
        return null;
    }

    [Serializable]
    public class ItemClass
    {
        public ItemScript Item;
        public int Count;
    }
}
