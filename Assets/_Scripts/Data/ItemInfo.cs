using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInspector;


public enum ItemType
{
    NONE = 0,
    Consumable,
    Material
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item")]
public class ItemInfo : ScriptableObject
{
    public ItemType itemType;

    public string itemID;
    public string itemName;   

    [Preview] public Sprite itemIcon;
    [TextArea] public string itemDescription;
    public int pouchCapacity;
    public int maxStack = 999;
}
