using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string actorProfileID;

    public string characterName;
    public int level;
    public long experience;
}

[System.Serializable]
public class PlayerItemData
{
    public string itemID;
    public int count;
}

[System.Serializable]
public class PlayerEquipmentData
{
    public string weaponID;
    public int enhancementLevel;
}

[System.Serializable]
public class GameData
{
    public float playTime;
    public long gold;

    public CharacterData characterInfo;

    public List<PlayerItemData> pouchItems;
    public List<PlayerItemData> stashItems;

    public PlayerEquipmentData currentEquippedWeapon;
    public List<PlayerEquipmentData> equipmentStash;

    public Vector3 playerPosition;
    public Quaternion playerRotation;

    public bool hasReceivedItemsFromTestNPC;

    public GameData(string newCharacterName)
    {
        playTime = 0;
        gold = 1000;
        characterInfo = new CharacterData 
        { 
            characterName = newCharacterName, 
            level = 1 
        };

        pouchItems = new List<PlayerItemData>
        {
            new PlayerItemData { itemID = "1", count = 10 },
            new PlayerItemData { itemID = "3", count = 10 }
        };
        stashItems = new List<PlayerItemData>();

        currentEquippedWeapon = null;
        equipmentStash = new List<PlayerEquipmentData>
        {
            new PlayerEquipmentData { weaponID = "1" },
            new PlayerEquipmentData { weaponID = "5" },
            new PlayerEquipmentData { weaponID = "9" },
            new PlayerEquipmentData { weaponID = "13" }
        };

        playerPosition = Vector3.zero;
        playerRotation = Quaternion.identity;

        hasReceivedItemsFromTestNPC = false;
    }
}
