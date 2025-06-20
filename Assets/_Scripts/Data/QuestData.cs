using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    [Header("목표")]
    public GameObject targetBossPrefab;

    [Header("시간 제한")]
    public float timeLimitInMinutes = 30f;
}
