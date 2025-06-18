using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] string ProfileID1, ProfileID2;
    [SerializeField] private GameObject player;
    
    private PlayerControl playerControl;


    private void Awake()
    {
        if (DataManager.Instance == null || DataManager.Instance.gameData == null)
        {
            Debug.LogError("DataManager가 준비되지 않았습니다! 로비 씬에서 시작해주세요.");
            enabled = false;
            return;
        }

        playerControl = player.GetComponent<PlayerControl>();
        SetupPlayer();
    }

    private void SetupPlayer()
    {
        GameData data = DataManager.Instance.gameData;

        if (data.characterInfo.actorProfileID.Equals(ProfileID1))
        {
            ActorProfile profile =  ProfileManager.Instance.GetProfile(ProfileID1);
            playerControl.SetupCharacter(data, profile);
        }
        else if (data.characterInfo.actorProfileID.Equals(ProfileID2))
        {
            ActorProfile profile = ProfileManager.Instance.GetProfile(ProfileID2);
            playerControl.SetupCharacter(data, profile);
        }
        else
        {
            Debug.LogError("actorProfileID에 맞는 캐릭터 모델 프리팹이 없음: " + data.characterInfo.actorProfileID);
        }
    }
}
