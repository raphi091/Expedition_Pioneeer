using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    NONE = 0,
    Consumable,
    Material
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item")]
public class ItemInfo : ScriptableObject
{
    public string itemID;
    public string itemName;   

    [TextArea]
    public string itemDescription;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStack;
}
