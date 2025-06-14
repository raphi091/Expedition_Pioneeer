using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class CharacterPrefabMapping
{
    public string actorID;
    public GameObject characterPrefab;
}

public class CharacterPreview : MonoBehaviour
{
    [Header("SpawnPoint")]
    public Transform characterStage;

    [Header("CharacterID")]
    public List<CharacterPrefabMapping> characterMappings;
    private Dictionary<string, GameObject> characterPrefabDict;

    private GameObject currentCharacterInstance;


    private void Awake()
    {
        characterPrefabDict = new Dictionary<string, GameObject>();

        foreach (var mapping in characterMappings)
        {
            if (!string.IsNullOrEmpty(mapping.actorID) && mapping.characterPrefab != null)
            {
                characterPrefabDict[mapping.actorID] = mapping.characterPrefab;
            }
        }
    }

    public void ShowCharacter(string actorID)
    {
        ClearCharacter();

        if (characterPrefabDict.TryGetValue(actorID, out GameObject prefabToShow))
        {
            currentCharacterInstance = Instantiate(prefabToShow, characterStage);
        }
        else
        {
            Debug.LogWarning("매핑 목록에 없는 actorID 입니다: " + actorID);
        }
    }

    public void ClearCharacter()
    {
        if (currentCharacterInstance != null) 
        {
            Destroy(currentCharacterInstance); 
        }
    }
}
