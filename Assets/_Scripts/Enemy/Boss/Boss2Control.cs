using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent를 사용하려면 필수!

// 보스 캐릭터에 필요한 컴포넌트들을 강제함
[RequireComponent(typeof(NavMeshAgent), typeof(CharacterController), typeof(Animator))]
public class Boss2Control : MonoBehaviour
{
    [Header("BossData")]
    public BossData bossData;

    [Header("상태 디버깅")]
    [SerializeField] private BossState currentState;
    [SerializeField] private bool isActionInProgress = false;

    private NavMeshAgent agent;
    private CharacterController controller;
    private Animator animator;

    private Transform player;

    private Vector3 initialPosition;
    private float timeSinceLastAttack = 0f;

    private float verticalVelocity;
    private float gravity = 14.0f;

    private bool inAttackWindup = false;
    private float damageTakenDuringWindup = 0f;

    private float currentHealth;

    public enum BossState
    {
        Spawning,
        Patrolling,
        Chasing,
        Combat,
        Stunned,
        Dead
    }


    #region Unity Lifecycle
    private void Awake()
    {
        if (!TryGetComponent(out agent))
            Debug.LogWarning("Boss2Control ] NavMeshAgent 없음");

        if (!TryGetComponent(out controller))
            Debug.LogWarning("Boss2Control ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("Boss2Control ] Animator 없음");

        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (bossData == null)
        {
            Debug.LogError("BossData가 할당되지 않았습니다!", this);
            this.enabled = false;
            return;
        }

        initialPosition = transform.position;
        currentHealth = bossData.maxHealth;

        ChangeState(BossState.Spawning);
    }

    private void Update()
    {
        if (player == null || currentState == BossState.Dead) return;

        if (controller.isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 finalMove = agent.velocity;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(agent.velocity.x, 0, agent.velocity.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed, 0.1f, Time.deltaTime);

        if (isActionInProgress) return;

        switch (currentState)
        {
            case BossState.Patrolling: 
                HandlePatrolling(); 
                break;
            case BossState.Chasing: 
                HandleChasing(); 
                break;
            case BossState.Combat: 
                HandleCombat(); 
                break;
        }
    }

    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            Vector3 rootMotion = animator.deltaPosition;
            rootMotion.y = verticalVelocity;
            controller.Move(rootMotion);
        }
    }
    #endregion

    #region State Machine & Handlers
    private void ChangeState(BossState newState)
    {
        if (currentState == newState) return;

        inAttackWindup = false;

        currentState = newState;
        StopAllCoroutines();
        isActionInProgress = false;
        animator.applyRootMotion = false;

        switch (currentState)
        {
            case BossState.Spawning: 
                StartCoroutine(SpawningRoutine()); 
                break;
            case BossState.Patrolling: 
                StartCoroutine(PatrollingRoutine());
                break;
            case BossState.Chasing: 
                agent.speed = bossData.runSpeed; agent.stoppingDistance = 0.1f;
                break;
            case BossState.Combat:
                agent.speed = bossData.walkSpeed; agent.stoppingDistance = bossData.engagementDistance;
                timeSinceLastAttack = 0f;
                break;
            case BossState.Stunned: 
                StartCoroutine(StunnedRoutine());
                break;
            case BossState.Dead: StartCoroutine(DeadRoutine());
                break;
        }
    }

    private IEnumerator SpawningRoutine()
    {
        isActionInProgress = true;

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isActionInProgress = false;
        ChangeState(BossState.Patrolling);
    }

    private IEnumerator PatrollingRoutine()
    {
        while (currentState == BossState.Patrolling)
        {
            Vector3 randomDirection = Random.insideUnitSphere * bossData.patrolRadius;
            randomDirection += initialPosition;
            NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, bossData.patrolRadius, 1);
            agent.SetDestination(hit.position);

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isActionInProgress);

            if (Random.value > 0.7f)
            {
                isActionInProgress = true;

                animator.SetTrigger("DoPatrolPrayer");

                yield return new WaitForSeconds(GetAnimationClipLength("Prayer_Patrol"));

                isActionInProgress = false;
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(2f, 4f));
            }
        }
    }


    private void HandlePatrolling()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < bossData.detectionRadius)
        {
            // TODO: 시야각 체크 로직 추가 가능
            ChangeState(BossState.Chasing);
        }
    }

    private void HandleChasing()
    {
        agent.SetDestination(player.position);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= bossData.engagementDistance)
        {
            ChangeState(BossState.Combat);
        }
        else if (distanceToPlayer > bossData.maxChaseDistance)
        {
            ChangeState(BossState.Patrolling);
        }
    }

    private void HandleCombat()
    {
        agent.SetDestination(player.position);
        transform.LookAt(player);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > bossData.maxChaseDistance) 
        {
            ChangeState(BossState.Patrolling);
            return;
        }

        timeSinceLastAttack += Time.deltaTime;

        List<AttackData> availableAttacks = new List<AttackData>();
        foreach (var attack in bossData.attacks)
        {
            if (distanceToPlayer >= attack.minRange && distanceToPlayer <= attack.maxRange && timeSinceLastAttack >= attack.cooldown)
            {
                availableAttacks.Add(attack);
            }
        }

        if (availableAttacks.Count > 0)
        {
            AttackData chosenAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
            StartCoroutine(AttackRoutine(chosenAttack));
        }
        else
        {
            agent.stoppingDistance = bossData.engagementDistance;
        }
    }

    private IEnumerator AttackRoutine(AttackData attack)
    {
        isActionInProgress = true;
        agent.isStopped = true;

        inAttackWindup = true;
        damageTakenDuringWindup = 0f;

        animator.SetTrigger("Tell");

        yield return new WaitForSeconds(GetAnimationClipLength("Tell"));

        inAttackWindup = false;

        animator.SetTrigger(attack.animationName);
        animator.applyRootMotion = true;
        timeSinceLastAttack = 0f;

        yield return new WaitForSeconds(attack.animationClip.length + 0.1f);

        animator.applyRootMotion = false;
        agent.isStopped = false;
        isActionInProgress = false;
    }

    private IEnumerator StunnedRoutine()
    {
        isActionInProgress = true;
        agent.isStopped = true;

        animator.SetTrigger("StaggerTrigger");
        yield return new WaitForSeconds(1f);

        animator.SetBool("IsStunned", true);

        yield return new WaitForSeconds(bossData.stunDuration);

        animator.SetBool("IsStunned", false);
        isActionInProgress = false;
        agent.isStopped = false;
        ChangeState(BossState.Combat);
    }

    private IEnumerator DeadRoutine()
    {
        isActionInProgress = true;

        animator.SetTrigger("DeadTrigger");
        agent.enabled = false;
        controller.enabled = false;

        yield return new WaitForSeconds(60f);

        Destroy(gameObject);
    }
    #endregion

    #region Public Methods
    public void TakeDamage(float damage)
    {
        if (currentState == BossState.Dead) return;

        if (inAttackWindup)
        {
            damageTakenDuringWindup += damage;
            if (damageTakenDuringWindup >= bossData.cancelAttackDamageThreshold)
            {
                ChangeState(BossState.Stunned);
                return;
            }
        }

        currentHealth -= damage;
        /* ... 체력 감소, 사망 처리 등 */
    }

    public void OnPlayerDied()
    {
        if (currentState == BossState.Dead) return;
        ChangeState(BossState.Patrolling);
    }
    #endregion

    #region Helpers
    private float GetAnimationClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }
    #endregion
}