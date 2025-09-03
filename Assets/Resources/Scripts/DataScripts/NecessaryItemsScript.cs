using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NecessaryItemsPack", menuName = "NecessaryItemsPack")]
public class NecessaryItemsScript : ScriptableObject
{
    public ItemsGroupClass[] Items;

    [Serializable]
    public class ItemsGroupClass
    {
        public GroupClass[] Items;

        public ItemScript[][] NeededItems
        {
            get
            {
                ItemScript[][] Temp = new ItemScript[Items.Length][];
                for (int TempID = 0; TempID < Items.Length; TempID++)
                {
                    if (Items[TempID].Items[0] == null) Temp[TempID] = new ItemScript[0];
                    else Temp[TempID] = Items[TempID].Items;
                }
                return Temp;
            }
        }

        public ItemScript[][] AllItems
        {
            get
            {
                ItemScript[][] Temp = new ItemScript[Items.Length][];
                for (int TempID = 0; TempID < Items.Length; TempID++) Temp[TempID] = Items[TempID].Items;
                return Temp;
            }
        }

        [Serializable] public class GroupClass { public ItemScript[] Items; }
    }
}
