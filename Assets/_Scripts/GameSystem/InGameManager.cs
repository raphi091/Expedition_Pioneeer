using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance = null;

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

        player = FindObjectOfType<PlayerControl>();
        respawnPoint = player.transform;

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

        yield return new WaitForSeconds(2f);

        player.Respawn(respawnPoint.position, respawnPoint.rotation);

        yield return new WaitForSeconds(5f);

        inGameUI.PlayerRespawn();
    }
}
