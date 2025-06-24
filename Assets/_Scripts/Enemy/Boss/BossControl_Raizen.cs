using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossControl_Raizen : MonoBehaviour
{
    public enum State { Setup, Idle, Chasing, Preparing, Attacking, Phase2, Dead }
    [Header("AI 상태")]
    [SerializeField] private State currentState;

    private Vector3 startPosition;
    private bool isWandering = false;

    public float rotationSpeed = 10f;

    [Header("공격 부위 참조")]
    public WeaponDamage WeaponL;
    public WeaponDamage WeaponR;

    [Header("VFX 프리팹")]
    public GameObject chainLightningPrefab;
    public GameObject flameProjectilePrefab;
    public GameObject groundAoEPrefab;

    private NavMeshAgent agent;
    private CharacterController controller;
    private Animator animator;
    private BossStats stats;
    private WeaponDamage weaponDamage;
    private Transform player;

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
            Debug.LogWarning("BossControl_Raizen ] NavMeshAgent 없음");

        if (!TryGetComponent(out controller))
            Debug.LogWarning("BossControl_Raizen ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("BossControl_Raizen ] Animator 없음");

        if (!TryGetComponent(out stats))
            Debug.LogWarning("BossControl_Raizen ] BossStats 없음");

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

        EnterState(State.Setup);
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

        if (newState == State.Attacking)
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
            case State.Preparing:
                currentStateCoroutine = StartCoroutine(Preparing_co());
                break;
            case State.Attacking:
                currentStateCoroutine = StartCoroutine(Attack_co());
                break; ;
            case State.Phase2:
                currentStateCoroutine = StartCoroutine(Phase2_co());
                break;
            case State.Dead:
                currentStateCoroutine = StartCoroutine(Death_co());
                break;
        }
    }

    private IEnumerator Setup_co()
    {
        yield return new WaitForSeconds(1f);

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
                // SoundManager.Instance.PlayBGM(BGMTrackName.Boss3);
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
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > stats.data.loseSightRange ||
                Vector3.Distance(transform.position, startPosition) > stats.data.maxChaseDistance)
            {
                // SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);
                EnterState(State.Idle);
                yield break;
            }

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

                EnterState(State.Preparing);
                yield break;
            }
            else
            {
                agent.SetDestination(player.position);
            }

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                agent.SetDestination(player.position);
            }

            yield return null;
        }
    }

    private IEnumerator Preparing_co()
    {
        if (scheduledAttackIndex == -1)
        {
            EnterState(State.Chasing);
            yield break;
        }

        agent.isStopped = true;
        transform.LookAt(player.position);

        yield return null;

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

    private IEnumerator Phase2_co()
    {
        yield return null;
    }

    private IEnumerator Death_co()
    {
        agent.enabled = false;
        controller.enabled = false;
        animator.SetTrigger("Death");
        // SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);

        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }

    private void HandlePlayerDeath()
    {
        if (currentState != State.Dead)
        {
            // SoundManager.Instance.PlayBGM(BGMTrackName.Exploration);
            hasDiscoveredPlayer = false;
            EnterState(State.Idle);
        }
    }

    private bool IsPlayerInSight()
    {
        return Vector3.Distance(transform.position, player.position) < stats.data.sightRange;
    }

    public void AnimationEvent_EnableDamageZone(string part)
    {
        if (scheduledAttackIndex == -1) return;

        WeaponDamage currentWeapon = null;

        switch (part)
        {
            case "WeaponL":
                currentWeapon = WeaponL;
                break;
            case "WeaponR":
                currentWeapon = WeaponR;
                break;
        }

        if (currentWeapon != null)
        {
            currentWeapon.GetComponent<Collider>().enabled = true;
        }
    }

    public void AnimationEvent_DisableAllDamageZones()
    {
        WeaponL.GetComponent<Collider>().enabled = false;
        WeaponR.GetComponent<Collider>().enabled = false;
    }

    public void AnimationEvent_AttackFinished()
    {
        lastAttackTime = Time.time;
        if (currentState != State.Dead)
        {
            EnterState(State.Chasing);
        }
    }

    public void AnimationEvent_SpawnGroundAoE()
    {
        if (scheduledAttackIndex != -1)
        {
            StartCoroutine(SpawnAoE_co(stats.data.attacks[scheduledAttackIndex]));
        }
    }

    public void AnimationEvent_StartChainLightning()
    {
        StartCoroutine(ChainLightning_co());
    }

    public void AnimationEvent_StartSpinAttack()
    {
        StartCoroutine(SpinAndShoot_co());
    }

    private IEnumerator SpawnAoE_co(AttackData attackData)
    {
        int count = attackData.objectCount;
        float radius = attackData.spawnRadius;
        float delay = attackData.spawnDelay;

        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            spawnPosition.y = 0;

            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Instantiate(groundAoEPrefab, hit.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator ChainLightning_co()
    {
        Vector3 startOffset = transform.forward * 3f;
        float distanceBetweenStrikes = 4f;
        float delayBetweenStrikes = 0.25f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = transform.position + startOffset + (transform.forward * (i * distanceBetweenStrikes));

            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Instantiate(chainLightningPrefab, hit.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(delayBetweenStrikes);
        }
    }

    private IEnumerator SpinAndShoot_co()
    {
        float spinDuration = 20f / 60f;
        float fireInterval = 0.1f;
        float timer = 0f;

        while (timer < spinDuration)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 1.5f;
            GameObject projectile = Instantiate(flameProjectilePrefab, spawnPos, transform.rotation);
            projectile.GetComponent<Rigidbody>().velocity = transform.forward * 20f;

            timer += fireInterval;
            yield return new WaitForSeconds(fireInterval);
        }
    }
}
