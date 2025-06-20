using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEcologyManager : MonoBehaviour
{
    [Header("Set")]
    [Tooltip("스폰 가능한 모든 보스 프리팹 목록")]
    public GameObject[] allBossPrefabs;

    [Tooltip("필드에 동시에 존재할 수 있는 최대 보스 수")]
    public int maxActiveBosses = 2;

    [Header("Spawn")]
    [Tooltip("보스가 스폰될 수 있는 위치 목록")]
    public Transform[] spawnPoints;

    [Header("BossCycle")]
    [Tooltip("보스가 교체되기까지 걸리는 시간 (분 단위)")]
    public float rotationTimeInMinutes = 15f;

    private List<GameObject> activeBossInstances = new List<GameObject>();
    private GameObject questBossInstance;
    private Coroutine bossRotationCoroutine;
    private bool isQuestMode = false;


    private void Start()
    {
        if (!isQuestMode)
        {
            SpawnInitialBosses();
            bossRotationCoroutine = StartCoroutine(BossRotationCoroutine());
        }
    }

    //-----평상시
    private void SpawnInitialBosses()
    {
        List<GameObject> spawnPool = allBossPrefabs.ToList().OrderBy(x => Random.value).ToList();

        List<Transform> availableSpawnPoints = spawnPoints.ToList().OrderBy(x => Random.value).ToList();

        int spawnCount = Mathf.Min(maxActiveBosses, spawnPool.Count, availableSpawnPoints.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject bossPrefab = spawnPool[i];
            Transform spawnPoint = availableSpawnPoints[i];

            GameObject spawnedBoss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            activeBossInstances.Add(spawnedBoss);
        }
    }

    private IEnumerator BossRotationCoroutine()
    {
        while (true)
        {
            float waitTimeInSeconds = rotationTimeInMinutes * 60;
            yield return new WaitForSeconds(waitTimeInSeconds);

            if (activeBossInstances.Count == 0 || allBossPrefabs.Length <= activeBossInstances.Count)
            {
                continue;
            }

            int bossToRemoveIndex = Random.Range(0, activeBossInstances.Count);
            GameObject bossToRemove = activeBossInstances[bossToRemoveIndex];
            Vector3 oldBossPosition = bossToRemove.transform.position;

            // 보스가 필드를 떠나는 애니메이션 작업
            activeBossInstances.RemoveAt(bossToRemoveIndex);
            Destroy(bossToRemove, 5f);

            List<GameObject> availableNewBosses = allBossPrefabs.Where(p => !activeBossInstances.Any(b => b.name.StartsWith(p.name))).ToList();
            if (availableNewBosses.Count > 0)
            {
                GameObject newBossPrefab = availableNewBosses[Random.Range(0, availableNewBosses.Count)];

                GameObject spawnedBoss = Instantiate(newBossPrefab, oldBossPosition, Quaternion.identity);
                activeBossInstances.Add(spawnedBoss);
            }
        }
    }

    //-----퀘스트시
    public void SpawnQuestBoss(GameObject bossPrefab, float duration)
    {
        isQuestMode = true;

        if (bossRotationCoroutine != null)
        {
            StopCoroutine(bossRotationCoroutine);
        }

        foreach (var boss in activeBossInstances)
        {
            Destroy(boss);
        }

        activeBossInstances.Clear();

        Transform spawnPoint = spawnPoints[0]; // 퀘스트용 스폰 위치를 따로 지정하는 것이 좋음
        questBossInstance = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);

        if (duration > 0)
        {
            StartCoroutine(QuestBossTimerCoroutine(duration));
        }
    }

    private IEnumerator QuestBossTimerCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (isQuestMode && questBossInstance != null)
        {
            Debug.Log("퀘스트 시간 초과! 보스가 사라집니다.");
            // QuestManager에 퀘스트 실패를 알릴 수 있음
            // QuestManager.Instance.FailQuest("시간 초과");
            // EndQuestBoss()는 QuestManager가 호출할 것이므로 여기서는 직접 호출하지 않을 수 있음
        }
    }

    public void EndQuestBoss(bool isSuccess)
    {
        if (!isQuestMode) return;

        if (questBossInstance != null)
        {
            Destroy(questBossInstance);
        }

        isQuestMode = false;

        Start();
    }
}