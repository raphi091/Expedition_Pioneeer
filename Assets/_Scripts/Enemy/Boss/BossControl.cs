using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(BossMoveControl), typeof(BossAttackControl))]
public class BossControl : MonoBehaviour
{
    public ActorProfile Profile;

    public NavMeshAgent Agent;
    public Animator Animator;
    public BossStats Stats;

    [Header("Control")]
    public BossMoveControl MoveControl;
    public BossAttackControl AttackControl;


    public enum State
    {
        Patrol,
        Positioning,
        Attacking,
        Cooldown,
        Dead
    }
    [Header("AI State")]
    public State currentState = State.Patrol;
    private Coroutine currentStateCoroutine;

    [Header("AI Settings")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 7f;
    public float territoryRadius = 30f;
    public float patrolWaitTime = 5f;
    public float chaseRange = 25f;
    public float attackRange = 4f;
    public float cooldownTime = 2f;

    private Transform Player;


    private void Awake()
    {
        if (!TryGetComponent(out Agent))
            Debug.LogWarning("BossStats ] NavMeshAgent 없음");

        if (!TryGetComponent(out Animator))
            Debug.LogWarning("BossStats ] Animator 없음");

        if (!TryGetComponent(out Stats))
            Debug.LogWarning("BossStats ] BossStats 없음");

        if (!TryGetComponent(out MoveControl))
            Debug.LogWarning("BossStats ] BossMoveControl 없음");

        if (!TryGetComponent(out AttackControl))
            Debug.LogWarning("BossStats ] BossAttackControl 없음");

        Stats.profile = this.Profile;
        Stats.Initialize();

        this.Animator.avatar = Profile.avatar;

        MoveControl.Initialize(this);
        AttackControl.Initialize(this);
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        SwitchState(State.Patrol);
    }

    private void Update()
    {
        if (currentState == State.Dead) return;
        if (MoveControl != null)
        {
            MoveControl.UpdateAnimator();
        }
    }

    public void SwitchState(State newState)
    {
        if (currentState == newState) return;

        if (currentStateCoroutine != null) StopCoroutine(currentStateCoroutine);
        currentState = newState;

        switch (currentState)
        {
            case State.Patrol:
                Animator.SetBool("IsInCombat", false);
                currentStateCoroutine = StartCoroutine(PatrolState());
                break;
            case State.Positioning:
                Animator.SetBool("IsInCombat", true);
                currentStateCoroutine = StartCoroutine(PositioningState());
                break;
            case State.Attacking:
                Animator.SetBool("IsInCombat", true);
                currentStateCoroutine = StartCoroutine(AttackingState());
                break;
            case State.Cooldown:
                Animator.SetBool("IsInCombat", true);
                currentStateCoroutine = StartCoroutine(CooldownState());
                break;
            case State.Dead:
                Animator.SetBool("IsInCombat", false);
                currentStateCoroutine = StartCoroutine(DeadState());
                break;
        }
    }

    private bool IsPlayerInChaseRange()
    {
        if (Player == null) return false;

        return Vector3.Distance(transform.position, Player.position) < chaseRange;
    }

    private bool IsPlayerInAttackRange()
    {
        if (Player == null) return false;

        return Vector3.Distance(transform.position, Player.position) < attackRange;
    }

    private IEnumerator PatrolState() 
    {
        MoveControl.Patrol();

        while (true)
        {
            MoveControl.Patrol();

            if (IsPlayerInChaseRange())
            {
                SwitchState(State.Positioning);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator PositioningState()
    {
        while (true)
        {
            MoveControl.Chase(Player);

            if (IsPlayerInAttackRange())
            {
                SwitchState(State.Attacking);
                yield break;
            }

            if (!IsPlayerInChaseRange())
            {
                SwitchState(State.Patrol);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator AttackingState()
    {
        MoveControl.Stop();
        // AttackControl.PerformAttack(Player);
        // OnAttackFinished가 상태를 전환해줄 때까지 대기
        yield return null;
    }

    private IEnumerator CooldownState()
    {
        Animator.SetBool("IsCoolingDown", true);
        MoveControl.Stop();

        yield return new WaitForSeconds(cooldownTime);

        Animator.SetBool("IsCoolingDown", false);
        SwitchState(State.Positioning);
    }

    private IEnumerator DeadState()
    {
        Debug.Log("죽음");
        MoveControl.Stop();
        Animator.SetTrigger("Die");
        Agent.enabled = false;
        yield return null;
    }

    public void OnDeath()
    {
        SwitchState(State.Dead);
    }
}
