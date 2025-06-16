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


    // [Space(15), HorizontalLine(color:FixedColor.CloudWhite, message = "ANIMATION"), HideField] public bool _h1;

    // public AnimatorOverrideController animatorOverride;
    // public List<AnimationClip> ATTACK;
    // public List<AnimationClip> DAMAGE;
    // public List<AnimationClip> DEATH;


    [Space(15), HorizontalLine(color:FixedColor.CloudWhite, message = "ATTRIBUTES"), HideField] public bool _h2;

    [Tooltip("체력")] public int health = 100;
    [Tooltip("행동력")] public int stamina = 100;

    [Tooltip("공격력")] public int attackdamage;
    [Tooltip("초당 공격 속도 ( sec )")] public float attackspeed;
    [Tooltip("공격 범위")] public float attackrange;

    [Tooltip("초당 걷는 속도 ( sec )")] public float walkspeed = 2.0f;
    [Tooltip("초당 뛰는 속도 ( sec )")] public float runspeed = 5.0f;
    [Tooltip("초당 숙여서 걷는 속도 ( sec )")] public float crouchwalkspeed = 1.0f;
    [Tooltip("초당 회전 속도 ( sec )")] public float rotatespeed = 10.0f;

    [Tooltip("회피 거리")] public float dodgedistance = 3f;
    [Tooltip("회피 시간")] public float dodgeduration = 0.4f;

    [Tooltip("시야 범위")] public float sightrange;


    // [Space(15), HorizontalLine(color:FixedColor.CloudWhite, message = "ABILITIES"), HideField] public bool _h3;

    // public List<AbilityData> abilities;
}
