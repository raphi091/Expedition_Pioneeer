using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public int level;
    public long experience;
}

[System.Serializable]
public class GameData
{
    public float playTime;
    public CharacterData characterInfo;
    // 재화, 아이템, 장비 등 추가

    public GameData()
    {
        playTime = 0;
        characterInfo = new CharacterData();
        characterInfo.level = 1;
        characterInfo.experience = 0;
        characterInfo.characterName = "새 캐릭터";
    }
}
