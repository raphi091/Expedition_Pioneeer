using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CustomInspector;


public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance = null;

    [Foldout] public List<WeaponInfo> allWeapons;
    [Foldout] public List<ItemInfo> allItems;

    private Dictionary<string, WeaponInfo> weaponDict = new Dictionary<string, WeaponInfo>();
    private Dictionary<string, ItemInfo> itemDict = new Dictionary<string, ItemInfo>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var weapon in allWeapons)
        {
            if (!weaponDict.ContainsKey(weapon.weaponID))
            {
                weaponDict.Add(weapon.weaponID, weapon);
            }
        }

        foreach (var item in allItems)
        {
            if (!itemDict.ContainsKey(item.itemID))
            {
                itemDict.Add(item.itemID, item);
            }
        }
    }

    public WeaponInfo GetWeaponByID(string weaponID)
    {
        weaponDict.TryGetValue(weaponID, out WeaponInfo weapon);
        return weapon;
    }

    public ItemInfo GetItemByID(string itemID)
    {
        itemDict.TryGetValue(itemID, out ItemInfo item);
        return item;
    }

    public List<ItemInfo> GetAllConsumableInfos()
    {
        return allItems.Where(item => item.itemType == ItemType.Consumable).ToList();
    }
}
