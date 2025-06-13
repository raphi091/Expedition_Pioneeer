using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance = null;

    [Header("PlayerData")]
    public int currentSlotIndex = -1;
    public GameData gameData;

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
