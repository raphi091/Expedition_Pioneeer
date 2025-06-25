using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossControl_Grod : MonoBehaviour
{
    public enum State { Setup, Idle, Praying, BattleCry, Chasing, Preparing, Attacking, Broken, Dead }
    [Header("AI 상태")]
    [SerializeField] private State currentState;

    private Vector3 startPosition;
    private bool isWandering = false;

    public float rotationSpeed = 10f;

    public AnimationClip prayClip;
    public AnimationClip roarClip;
    public AnimationClip lookAroundClip;
    public AnimationClip ReadytoAttackClip;
    public AnimationClip breakClip;

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
            Debug.LogWarning("BossControl_Grod ] NavMeshAgent 없음");

        if (!TryGetComponent(out controller))
            Debug.LogWarning("BossControl_Grod ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("BossControl_Grod ] Animator 없음");

        if (!TryGetComponent(out stats))
            Debug.LogWarning("BossControl_Grod ] BossStats 없음");

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
        stats.OnDamaged += HandleDamage;
        stats.OnPhaseTransition += () => EnterState(State.BattleCry);
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
            case State.Praying:
                currentStateCoroutine = StartCoroutine(Pray_co());
                break;
            case State.BattleCry:
                currentStateCoroutine = StartCoroutine(StartBattle_co());
                break;
            case State.Chasing:
                currentStateCoroutine = StartCoroutine(Chasing_co());
                break;
            case State.Preparing:
                currentStateCoroutine = StartCoroutine(Preparing_co());
                break;
            case State.Attacking:
                currentStateCoroutine = StartCoroutine(Attack_co());
                break; ;
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
        yield return new WaitForSeconds(prayClip.length);

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
                SoundManager.Instance.PlayBGM(BGMTrackName.Boss2);
                map.SetTargetColor();
                EnterState(State.BattleCry);
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
                    else if (randAction < 9)
                    {
                        animator.SetTrigger("LookAround");

                        yield return new WaitForSeconds(lookAroundClip.length);

                        lastStateChangeTime = Time.time;
                    }
                    else
                    {
                        EnterState(State.Praying);
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator Pray_co()
    {
        animator.SetTrigger("Pray");

        yield return new WaitForSeconds(prayClip.length);

        EnterState(State.Idle);
    }

    private IEnumerator StartBattle_co()
    {
        hasDiscoveredPlayer = true;

        transform.LookAt(player.position);
        animator.SetTrigger("ReadytoBattle");

        yield return new WaitForSeconds(roarClip.length);

        EnterState(State.Chasing);
    }

    private IEnumerator Chasing_co()
    {
        agent.isStopped = false;
        agent.speed = stats.data.runSpeed;

        float repathTimer = 0f;
        float repathInterval = 0.2f;

        while (true)
        {
            yield return null;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > stats.data.loseSightRange ||
                Vector3.Distance(transform.position, startPosition) > stats.data.maxChaseDistance)
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

            if (Time.time < lastAttackTime + stats.data.cooldown)
            {
                agent.speed = stats.data.walkSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                List<int> availableAttackIndices = new List<int>();
                for (int i = 0; i < stats.data.attacks.Count; i++)
                {
                    if (currentPhase >= stats.data.attacks[i].requiredPhase)
                    {
                        availableAttackIndices.Add(i);
                    }
                }
                if (availableAttackIndices.Count == 0)
                {
                    agent.SetDestination(player.position);
                    yield return null;
                    continue;
                }

                int chosenAttackIndex = availableAttackIndices[Random.Range(0, availableAttackIndices.Count)];
                AttackData desiredAttack = stats.data.attacks[chosenAttackIndex];

                if (distanceToPlayer >= desiredAttack.minRange && distanceToPlayer <= desiredAttack.maxRange)
                {
                    scheduledAttackIndex = chosenAttackIndex;
                    EnterState(State.Preparing);
                    yield break;
                }
                else if (distanceToPlayer < desiredAttack.minRange)
                {
                    agent.speed = stats.data.runSpeed;
                    agent.isStopped = false;
                    Vector3 retreatPosition = transform.position + (transform.position - player.position).normalized * (desiredAttack.minRange - distanceToPlayer);
                    agent.SetDestination(retreatPosition);
                }
                else 
                {
                    agent.speed = stats.data.runSpeed;
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
            }

            yield return null;
        }
    }

    private IEnumerator Preparing_co()
    {
        agent.isStopped = true;
        transform.LookAt(player.position);
        animator.SetTrigger("ReadytoAttack");

        float prepareTime = (currentPhase == 1) ? stats.data.attackPrepareTime : stats.data.attackPrepareTime / 1.5f;
        float timer = 0f;

        while (timer < prepareTime)
        {
            if (stats.CheckAndBreakStance())
            {
                EnterState(State.Broken);
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        EnterState(State.Attacking);
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

    private IEnumerator Break_co()
    {
        animator.SetTrigger("Break");

        yield return new WaitForSeconds(breakClip.length + stats.data.stanceDuration);

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

    private void HandleDamage()
    {
        if (currentState == State.Praying)
        {
            EnterState(State.BattleCry);
        }
    }

    private bool IsPlayerInSight()
    {
        return Vector3.Distance(transform.position, player.position) < stats.data.sightRange;
    }

    public void AnimationEvent_EnableWeapon()
    {
        weaponDamage.GetComponent<Collider>().enabled = true;
    }

    public void AnimationEvent_DisableWeapon()
    {
        weaponDamage.GetComponent<Collider>().enabled = false;
    }

    public void AnimationEvent_AttackFinished()
    {
        lastAttackTime = Time.time;
        AnimationEvent_ReturnToChase();
    }

    public void AnimationEvent_ReturnToChase()
    {
        if (currentState != State.Dead)
        {
            EnterState(State.Chasing);
        }
    }
}
