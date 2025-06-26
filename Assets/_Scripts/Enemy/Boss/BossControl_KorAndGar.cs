using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossControl_KorAndGar : MonoBehaviour
{
    public enum State { Setup, Idle, Chasing, Pause, Attacking, Phase2, Broken, Dead }
    [Header("AI 상태")]
    [SerializeField] private State currentState;

    private Vector3 startPosition;
    private bool isWandering = false;

    public float rotationSpeed = 10f;

    public AnimationClip[] idleClip;

    [Header("공격 부위 참조")]
    public WeaponDamage Weapon;
    public WeaponDamage Hand;
    public WeaponDamage Foot;

    private NavMeshAgent agent;
    private CharacterController controller;
    private Animator animator;
    private BossStats stats;
    private WeaponDamage weaponDamage;
    private Transform player;
    private MiniMap map;

    private bool hasDiscoveredPlayer = false;
    private int currentPhase = 1;
    private float lastStateChangeTime;
    private float lastAttackTime;
    private int scheduledAttackIndex = -1;
    private bool useRootMotionLogic = false;

    private Coroutine currentStateCoroutine;


    private void Awake()
    {
        if (!TryGetComponent(out agent))
            Debug.LogWarning("BossControl_KorAndGar ] NavMeshAgent 없음");

        if (!TryGetComponent(out controller))
            Debug.LogWarning("BossControl_KorAndGar ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("BossControl_KorAndGar ] Animator 없음");

        if (!TryGetComponent(out stats))
            Debug.LogWarning("BossControl_KorAndGar ] BossStats 없음");

        weaponDamage = GetComponentInChildren<WeaponDamage>();
    }

    private void OnEnable()
    {
        PlayerControl.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerControl.OnPlayerDied -= HandlePlayerDeath;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        startPosition = transform.position;

        agent.updatePosition = false;
        agent.updateRotation = false;

        stats.OnDeath += () => EnterState(State.Dead);
        stats.OnPhaseTransition += () => EnterState(State.Phase2);
        stats.OnStanceBroken += (id) => EnterState(State.Broken);

        EnterState(State.Setup);

        map = FindObjectOfType<MiniMap>();
    }

    private void Update()
    {
        if (currentState == State.Dead) return;

        if (!useRootMotionLogic && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
    }

    private void OnAnimatorMove()
    {
        if (!controller.enabled || Time.deltaTime <= 0) return;

        Vector3 finalMoveDelta;

        if (useRootMotionLogic)
        {
            finalMoveDelta = animator.deltaPosition;
        }
        else
        {
            finalMoveDelta = agent.velocity * Time.deltaTime;
        }

        if (!controller.isGrounded)
        {
            finalMoveDelta.y -= 20f * Time.deltaTime;
        }

        controller.Move(finalMoveDelta);
    }

    private void EnterState(State newState)
    {
        if (currentStateCoroutine != null)
        {
            StopCoroutine(currentStateCoroutine);
        }
        currentState = newState;
        lastStateChangeTime = Time.time;

        if (newState == State.Attacking || newState == State.Broken)
        {
            useRootMotionLogic = true;
            agent.isStopped = true;
        }
        else
        {
            useRootMotionLogic = false;
            if (agent.enabled) 
                agent.isStopped = false;
        }

        switch (currentState)
        {
            case State.Setup:
                currentStateCoroutine = StartCoroutine(Setup_co());
                break;
            case State.Idle:
                currentStateCoroutine = StartCoroutine(Idle_co());
                break;
            case State.Chasing:
                currentStateCoroutine = StartCoroutine(Chasing_co());
                break;
            case State.Attacking:
                if (player != null)
                {
                    Vector3 directionToPlayer = player.position - transform.position;
                    directionToPlayer.y = 0;
                    if (directionToPlayer != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(directionToPlayer);
                    }
                }
                currentStateCoroutine = StartCoroutine(Attack_co());
                break; ;
            case State.Pause:
                currentStateCoroutine = StartCoroutine(Pause_co());
                break;
            case State.Phase2:
                currentStateCoroutine = StartCoroutine(Phase2_co());
                break;
            case State.Broken:
                currentStateCoroutine = StartCoroutine(Break_co());
                break;
            case State.Dead:
                currentStateCoroutine = StartCoroutine(Death_co());
                break;
        }
    }

    private IEnumerator Setup_co()
    {
        yield return new WaitForSeconds(idleClip[1].length);

        EnterState(State.Idle);
    }

    private IEnumerator Idle_co()
    {
        agent.isStopped = true;
        agent.ResetPath();
        isWandering = false;
        animator.SetFloat("MoveSpeed", 0);

        while (true)
        {
            yield return null;

            if (IsPlayerInSight())
            {
                SoundManager.Instance.PlayBGM(BGMTrackName.Boss1);
                map.SetTargetColor();
                EnterState(State.Chasing);
                yield break;
            }

            if (isWandering)
            {
                if (agent.remainingDistance < 0.5f)
                {
                    isWandering = false;
                    lastStateChangeTime = Time.time;
                }
            }
            else
            {
                if (Time.time > lastStateChangeTime + 5f)
                {
                    int randAction = Random.Range(0, 10);

                    if (randAction < 5)
                    {
                        Vector3 randomDirection = Random.insideUnitSphere * stats.data.wanderRadius;
                        randomDirection += transform.position;

                        NavMeshHit hit;
                        NavMesh.SamplePosition(randomDirection, out hit, stats.data.wanderRadius, 1);
                        Vector3 finalPosition = hit.position;

                        if (Vector3.Distance(finalPosition, startPosition) > stats.data.maxWanderDistance)
                        {
                            finalPosition = startPosition;
                        }

                        agent.speed = stats.data.walkSpeed;
                        agent.SetDestination(finalPosition);
                        agent.isStopped = false;
                        isWandering = true;
                    }
                    else
                    {
                        int idle = Random.Range(0, idleClip.Length);
                        animator.SetInteger("IdleID", idle + 1);
                        animator.SetTrigger("Idle");

                        yield return new WaitForSeconds(idleClip[idle].length);

                        lastStateChangeTime = Time.time;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator Chasing_co()
    {
        agent.isStopped = false;
        agent.speed = stats.data.runSpeed;
        hasDiscoveredPlayer = true;

        float repathTimer = 0f;
        float repathInterval = 0.2f;

        while (true)
        {
            yield return null;

            if (IsPlayerOutOfRange())
            {
                SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);
                map.SetBaseColor();
                hasDiscoveredPlayer = false;
                EnterState(State.Idle);
                yield break;
            }

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                agent.SetDestination(player.position);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            List<int> availableAttackIndices = new List<int>();

            for (int i = 0; i < stats.data.attacks.Count; i++)
            {
                AttackData pattern = stats.data.attacks[i];
                if (distanceToPlayer >= pattern.minRange &&
                    distanceToPlayer <= pattern.maxRange &&
                    currentPhase >= pattern.requiredPhase)
                {
                    availableAttackIndices.Add(i);
                }
            }

            if (availableAttackIndices.Count > 0)
            {
                scheduledAttackIndex = availableAttackIndices[Random.Range(0, availableAttackIndices.Count)];

                EnterState(State.Attacking);
                yield break;
            }
            else
            {
                agent.SetDestination(player.position);
            }

            yield return null;
        }
    }

    private IEnumerator Pause_co()
    {
        agent.isStopped = true; 

        yield return new WaitForSeconds(stats.data.cooldown);

        EnterState(State.Chasing);
    }

    private IEnumerator Attack_co()
    {
        if (scheduledAttackIndex == -1)
        {
            EnterState(State.Chasing);
            yield break;
        }

        AttackData chosenAttack = stats.data.attacks[scheduledAttackIndex];
        weaponDamage.damage = chosenAttack.damage;

        animator.SetTrigger(chosenAttack.animationName);
        lastAttackTime = Time.time;
    }

    private IEnumerator Phase2_co()
    {
        animator.SetBool("SetPhase2", true);
        animator.SetTrigger("Phase2");

        yield return new WaitForSeconds(stats.data.phase2Duration);

        animator.SetBool("SetPhase2", false);
        animator.SetTrigger("Phase2");
        EnterState(State.Chasing);
    }

    private IEnumerator Break_co()
    {
        yield return new WaitForSeconds(stats.data.stanceDuration);

        EnterState(State.Chasing);
    }

    private IEnumerator Death_co()
    {
        SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);
        map.SetBaseColor();
        agent.enabled = false;
        controller.enabled = false;
        animator.SetTrigger("Death");

        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }

    private void HandlePlayerDeath()
    {
        if (currentState != State.Dead)
        {
            SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);
            map.SetBaseColor();
            hasDiscoveredPlayer = false;
            EnterState(State.Idle);
        }
    }

    private bool IsPlayerInSight()
    {
        return Vector3.Distance(transform.position, player.position) < stats.data.sightRange;
    }

    private bool IsPlayerOutOfRange()
    {
        return Vector3.Distance(transform.position, player.position) > stats.data.loseSightRange;
    }

    public void AnimationEvent_EnableDamageZone(string part)
    {
        if (scheduledAttackIndex == -1) return;

        WeaponDamage currentWeapon = null;

        switch (part)
        {
            case "Weapon":
                currentWeapon = Weapon;
                break;
            case "Hand":
                currentWeapon = Hand;
                break;
            case "Foot":
                currentWeapon = Foot;
                break;
        }

        if (currentWeapon != null)
        {
            currentWeapon.GetComponent<Collider>().enabled = true;
        }
    }

    public void AnimationEvent_DisableAllDamageZones()
    {
        Weapon.GetComponent<Collider>().enabled = false;
        Hand.GetComponent<Collider>().enabled = false;
        Foot.GetComponent<Collider>().enabled = false;
    }

    public void AnimationEvent_AttackFinished()
    {
        lastAttackTime = Time.time;
        if (currentState != State.Dead)
        {
            agent.Warp(transform.position);
            EnterState(State.Pause);
        }
    }
}
