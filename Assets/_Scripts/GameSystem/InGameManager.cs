using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance = null;

    [SerializeField] string ProfileID1, ProfileID2;

    private Transform respawnPoint;
    private PlayerControl player;
    private InGameUIManager inGameUI;


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
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerControl>();

        if (player != null)
        {
            respawnPoint = player.transform;
            SetupPlayer();
        }
        else
        {
            Debug.LogError("IngameManager ] PlayerControl 없음");
        }

        inGameUI = FindObjectOfType<InGameUIManager>();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RegisterPlayer(player);
            InventoryManager.Instance.LoadInventoryFromDataManager();
        }
        else
        {
            Debug.LogError("IngameManager ] InventoryManager 없음");
        }
    }

    private void OnEnable()
    {
        PlayerControl.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerControl.OnPlayerDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(PlayerDeathandResapwn_co());
    }

    private IEnumerator PlayerDeathandResapwn_co()
    {
        inGameUI.PlayerDeath();

        yield return new WaitForSeconds(5f);

        player.StartRespawn(respawnPoint.position, respawnPoint.rotation);

        yield return new WaitForSeconds(7f);

        inGameUI.PlayerRespawn();
    }

    private void SetupPlayer()
    {
        GameData data = DataManager.Instance.gameData;

        if (data.characterInfo.actorProfileID.Equals(ProfileID1))
        {
            ActorProfile profile = ProfileManager.Instance.GetProfile(ProfileID1);
            player.SetupCharacter(data, profile);
        }
        else if (data.characterInfo.actorProfileID.Equals(ProfileID2))
        {
            ActorProfile profile = ProfileManager.Instance.GetProfile(ProfileID2);
            player.SetupCharacter(data, profile);
        }
        else
        {
            Debug.LogError("actorProfileID에 맞는 캐릭터 모델 프리팹이 없음: " + data.characterInfo.actorProfileID);
        }
    }
}
