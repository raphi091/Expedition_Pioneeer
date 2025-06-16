using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInspector;


public struct PlayerState
{
    public int health;
    public int stamina;
    public int damage;
    public float attackrange;

    public void Set(ActorProfile profile)
    {
        health = profile.health;
        health = profile.stamina;
        damage = profile.attackdamage;
        attackrange = profile.attackrange;
    }
}

public class PlayerControl : MonoBehaviour
{
    public ActorProfile Profile { get => profile; set => profile = value; }
    // [ReadOnly, SerializeField] private ActorProfile profile;
    public ActorProfile profile;

    public PlayerState State;

    public InputControl input;
    public CharacterController characterController;
    public Animator animator;
    public Transform maincamera;


    private void Awake()
    {
        if (!TryGetComponent(out input))
            Debug.LogWarning("PlayerControl ] InputControl 없음");

        if (!TryGetComponent(out characterController))
            Debug.LogWarning("PlayerControl ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("PlayerControl ] Animator 없음");

        maincamera = Camera.main.transform;
    }

    private void Start()
    {
        animator.avatar = profile.avatar;
    }
}
