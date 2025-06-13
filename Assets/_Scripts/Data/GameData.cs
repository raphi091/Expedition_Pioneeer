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
    public List<PlayerItemData> inventory;
    public List<PlayerEquipmentData> equipmentStash;

    public GameData()
    {
        playTime = 0;
        gold = 0;
        characterInfo = new CharacterData { characterName = "새 캐릭터", level = 1 };
        inventory = new List<PlayerItemData>();
        equipmentStash = new List<PlayerEquipmentData>();
    }
}
