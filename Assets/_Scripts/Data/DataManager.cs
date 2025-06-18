using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameStartType
{
    NewGame,
    LoadGame
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance = null;

    [Header("PlayerData")]
    public GameData gameData = null;
    public int currentSlotIndex = -1;
    public GameStartType startType;

    [Header("Settiong")]
    public GameSetting setting;
    private string settingFilePath;

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

        settingFilePath = Path.Combine(Application.persistentDataPath, "setting.json");
        LoadSettings();
    }

    private string GetPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"gamedata_{slotIndex}.json");
    }

    //-----게임 관련
    public void SaveGame()
    {
        if (currentSlotIndex < 0)
        {
            return;
        }
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(GetPath(currentSlotIndex), json);
    }

    public bool LoadGame(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(json);
            currentSlotIndex = slotIndex;
            return true;
        }
        else
        {
            return false;
        }
    }

    public GameData GetDataForSlot(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            return null;
        }
    }

    public void DeleteData(int slotIndex)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);

            if (slotIndex == currentSlotIndex)
            {
                gameData = null;
                currentSlotIndex = -1;
            }
        }
        else
        {
            Debug.LogWarning($"슬롯 {slotIndex}에 삭제할 데이터 파일이 없습니다.");
        }
    }

    public bool RenameCharacter(int slotIndex, string newName)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData dataToModify = JsonUtility.FromJson<GameData>(json);

            dataToModify.characterInfo.characterName = newName;

            string newJson = JsonUtility.ToJson(dataToModify, true);
            File.WriteAllText(path, newJson);

            if (slotIndex == currentSlotIndex)
            {
                gameData.characterInfo.characterName = newName;
            }

            return true;
        }
        else
        {
            Debug.LogError($"슬롯 {slotIndex}에 이름 변경을 할 데이터 파일이 없습니다.");
            return false;
        }
    }

    //-----설정 관련
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(setting, true);
        File.WriteAllText(settingFilePath, json);
    }

    public void LoadSettings()
    {
        if (File.Exists(settingFilePath))
        {
            string json = File.ReadAllText(settingFilePath);
            setting = JsonUtility.FromJson<GameSetting>(json);
        }
        else
        {
            setting = new GameSetting();
        }
    }
}
