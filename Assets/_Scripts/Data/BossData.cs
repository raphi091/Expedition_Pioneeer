using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInspector;

[CreateAssetMenu(menuName = "Data/BossProfile")]
public class BossData : ScriptableObject
{
    [Header("Profile")]
    public int bossID;
    public string bossName = "새로운 보스";
    [Preview] public Sprite bossIcon;
    [Preview] public GameObject bossPrefab;
    [Tooltip("최대 체력")] public float maxHealth = 10000f;

    [Header("Movement")]
    [Tooltip("걷기 속도")] public float walkSpeed = 2f;
    [Tooltip("달리기 속도")] public float runSpeed = 6f;

    [Header("AI")]
    [Tooltip("순찰 반경")] public float wanderRadius = 50f;
    [Tooltip("최대 순찰 반경")] public float maxWanderDistance = 500f;
    [Tooltip("전투 반경")] public float engagementRange = 20f;
    [Tooltip("시야 반경")] public float sightRange = 30f;
    [Tooltip("최대 추격 반경")] public float maxChaseDistance = 40f;

    [Header("Stance")]
    [Tooltip("경직 스텍")] public float maxStance = 150f;
    [Tooltip("경직 상태 지속 시간")] public float stanceDuration = 5f;

    [Header("AttackData")]
    [Tooltip("공격 패턴")] public List<AttackData> attacks;
    [Tooltip("공격 후 딜레이")] public float cooldown;
    [Tooltip("공격 준비 대기시간")] public float attackPrepareTime;
}

[System.Serializable]
public class AttackData
{
    [Tooltip("공격 이름")] public string animationName;
    [Tooltip("공격 모션")] public AnimationClip animationClip;

    [Tooltip("공격 데미지")] public float damage;

    [Tooltip("공격 모션 거리")] public float moveDistance;
    [Tooltip("공격 최소 사거리")] public float minRange;
    [Tooltip("공격 최대 사거리")] public float maxRange;

    [Tooltip("사용 조건")] public int requiredPhase = 1;
}
