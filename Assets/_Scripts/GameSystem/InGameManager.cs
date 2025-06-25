using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Awake()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RegisterPlayer(player);
            InventoryManager.Instance.LoadInventoryFromDataManager();
        }
        else
        {
            Debug.LogError("IngameManager: 씬에 InventoryManager가 없습니다! InventoryManager가 DontDestroyOnLoad로 설정되어 있는지 확인하세요.");
        }

        // TODO: 이 곳에 플레이어 생성, 몬스터 스폰 등
        // 인게임 씬이 시작될 때 해야 할 다른 일들을 추가하면 됩니다.
    }
}
