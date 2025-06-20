using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance = null;

    private BossEcologyManager ecologyManager;
    private QuestData currentQuest;
    private float questTimer;
    private bool isQuestActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (TryGetComponent(out ecologyManager))
            Debug.LogWarning("QuestManager ] BossEcologyManager 없음");
    }

    public void AcceptQuest(QuestData quest)
    {
        Debug.Log($"퀘스트 수락: {quest.questName}");
        currentQuest = quest;
        questTimer = quest.timeLimitInMinutes * 60f;
        isQuestActive = true;

        ecologyManager.SpawnQuestBoss(quest.targetBossPrefab, questTimer);
    }

    void Update()
    {
        if (isQuestActive)
        {
            // 시간 제한이 있는 퀘스트의 경우 타이머 작동
            if (questTimer > 0)
            {
                questTimer -= Time.deltaTime;
                if (questTimer <= 0)
                {
                    FailQuest("시간 초과");
                }
            }
        }
    }

    public void OnTargetBossDefeated(GameObject defeatedBossPrefab)
    {
        if (isQuestActive && defeatedBossPrefab.name == currentQuest.targetBossPrefab.name)
        {
            CompleteQuest();
        }
    }

    void CompleteQuest()
    {
        Debug.Log($"퀘스트 성공: {currentQuest.questName}");
        isQuestActive = false;
        // 보스 정리 요청은 EcologyManager가 스스로 하거나 여기서 호출
        ecologyManager.EndQuestBoss(true);
    }

    void FailQuest(string reason)
    {
        Debug.Log($"퀘스트 실패: {currentQuest.questName} (이유: {reason})");
        isQuestActive = false;
        // 보스 정리 요청
        ecologyManager.EndQuestBoss(false);
    }
}
