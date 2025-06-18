using System.Collections.Generic;
using UnityEngine;
using CustomInspector;


public enum ActorType 
{ 
    NONE = 0, 
    PLAYER, 
    ENEMY, 
    NPC 
}

[CreateAssetMenu(menuName = "Data/ActorProfile")]
public class ActorProfile : ScriptableObject
{
    [Title("PROFILE", alignment = TextAlignment.Center, fontSize = 15), HideField] public bool _T0;
    [Space(10), HorizontalLine(color:FixedColor.CloudWhite, message = "PREFAB"), HideField] public bool _h0;

    public ActorType actorType = ActorType.NONE;
    public string alias;
    [Preview(Size.medium)] public GameObject model;
    public Avatar avatar;

    [Space(15), HorizontalLine(color:FixedColor.CloudWhite, message = "ATTRIBUTES"), HideField] public bool _h2;

    [Tooltip("체력")] public int health = 100;
    [Tooltip("초당 체력 회복량")] public float healthRegenRate = 1f;
    [Tooltip("피격 후 회복 대기 시간")] public float healthRegenDelay = 4f;


    [Tooltip("행동력")] public int stamina = 100;
    [Tooltip("초당 행동력 회복량")] public float staminaRegenRate = 10f;
    [Tooltip("초당 달리기 행동력")] public float runStaminaCost = 4f;
    [Tooltip("구르기 1회 행동력")] public float dodgeStaminaCost = 20f;

    [Tooltip("초당 걷는 속도 ( sec )")] public float walkspeed = 2.0f;
    [Tooltip("초당 뛰는 속도 ( sec )")] public float runspeed = 5.0f;
    [Tooltip("초당 숙여서 걷는 속도 ( sec )")] public float crouchwalkspeed = 1.0f;
    [Tooltip("초당 회전 속도 ( sec )")] public float rotatespeed = 10.0f;

    [Tooltip("회피 거리")] public float dodgedistance = 3f;
    [Tooltip("회피 시간")] public float dodgeduration = 0.4f;

    [Tooltip("시야 범위")] public float sightrange;
}
