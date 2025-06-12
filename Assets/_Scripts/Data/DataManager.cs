using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance = null;

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
    }

    public GameData gameData;

    private string GetPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"gamedata_{slotIndex}.json");
    }

    public void SaveGame(int slotIndex)
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(GetPath(slotIndex), json);

        Debug.Log($"데이터 저장 완료: 슬롯 {slotIndex}");
    }

    public bool LoadGame(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"데이터 로드 완료: 슬롯 {slotIndex}");
            return true;
        }
        else
        {
            Debug.Log($"슬롯 {slotIndex}에 세이브 파일이 없습니다.");
            return false;
        }
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(setting, true);
        File.WriteAllText(settingFilePath, json);
        Debug.Log("설정 저장 완료.");
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
