using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossControl_Raizen : MonoBehaviour
{
    public enum State { Setup, Idle, Chasing, Preparing, Attacking, ChangForm, Dead }
    [Header("AI 상태")]
    [SerializeField] private State currentState;

    private Vector3 startPosition;
    private bool isWandering = false;

    public float rotationSpeed = 10f;

    [Header("공격 부위 참조")]
    public WeaponDamage WeaponL;
    public WeaponDamage WeaponR;

    [Header("VFX")]
    public GameObject chainLightningPrefab;
    public GameObject flameProjectilePrefab;
    public GameObject groundAoEPrefab;
    public Transform FlamePoint;

    [Header("속성 표시용 Renderer")]
    [SerializeField] private List<SkinnedMeshRenderer> renderers;
    [SerializeField] private string targetMaterialName = "M_Raijin_Drums";
    [SerializeField] private Color lightningColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color fireColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private float emissionIntensity = 2.5f;
    private Coroutine emissionCoroutine;

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
    private int consecutiveAttacksInSameForm = 0;
    private bool isFlying = false;

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
        renderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        player = GameObject.FindGameObjectWithTag("Player").transform;

        startPosition = transform.position;

        agent.updatePosition = false;
        agent.updateRotation = false;

        stats.OnDeath += () => EnterState(State.Dead);
        stats.OnPhaseTransition += () => EnterState(State.ChangForm);

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
            case State.ChangForm:
                currentStateCoroutine = StartCoroutine(ChangeForm_co());
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
                SoundManager.Instance.PlayBGM(BGMTrackName.Boss3);
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

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            List<int> availableAttackIndices = new List<int>();

            for (int i = 0; i < stats.data.attacks.Count; i++)
            {
                AttackData pattern = stats.data.attacks[i];
                if (distanceToPlayer >= pattern.minRange &&
                    distanceToPlayer <= pattern.maxRange &&
                    currentPhase >= pattern.requiredPhase)
                {
                    if (!isFlying && !pattern.isAnotherForm)
                        availableAttackIndices.Add(i);
                    if (isFlying && pattern.isAnotherForm)
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

        AttackData attack = stats.data.attacks[scheduledAttackIndex];

        if (attack != null)
        {
            if (attack.animationName == "Attack1" || attack.animationName == "Attack2" || attack.animationName == "Attack5")
            {
                StartEmissionFade(lightningColor, 1f);
            }
            else if (attack.animationName == "Attack3" || attack.animationName == "Attack4" || attack.animationName == "Attack6")
            {
                StartEmissionFade(fireColor, 1f);
            }
        }

        yield return new WaitForSeconds(stats.data.attackPrepareTime);

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

    private IEnumerator ChangeForm_co()
    {
        isFlying = !isFlying;
        animator.SetBool("SetPhase2", isFlying);
        animator.SetTrigger("Phase2");

        yield return new WaitForSeconds(1f);

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

    private void StartEmissionFade(Color color, float duration = 1f)
    {
        if (emissionCoroutine != null)
            StopCoroutine(emissionCoroutine);

        emissionCoroutine = StartCoroutine(EmissionFadeIn(color, duration));
    }

    private void StartEmissionFadeOut(float duration = 0.7f)
    {
        if (emissionCoroutine != null)
            StopCoroutine(emissionCoroutine);

        emissionCoroutine = StartCoroutine(EmissionFadeOut(duration));
    }

    private IEnumerator EmissionFadeIn(Color targetColor, float duration)
    {
        float time = 0f;

        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(targetMaterialName))
                {
                    materials[i].EnableKeyword("_EMISSION");
                }
            }
        }

        while (time < duration)
        {
            float t = time / duration;
            Color currentColor = targetColor * (t * emissionIntensity);

            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains(targetMaterialName))
                    {
                        materials[i].SetColor("_EmissionColor", currentColor);
                    }
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(targetMaterialName))
                {
                    materials[i].SetColor("_EmissionColor", targetColor * emissionIntensity);
                }
            }
        }
    }

    private IEnumerator EmissionFadeOut(float duration)
    {
        float time = 0f;

        Color currentEmission = Color.black;

        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(targetMaterialName))
                {
                    currentEmission = materials[i].GetColor("_EmissionColor");
                    break;
                }
            }
        }

        while (time < duration)
        {
            float t = time / duration;
            Color newColor = Color.Lerp(currentEmission, Color.black, t);

            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains(targetMaterialName))
                    {
                        materials[i].SetColor("_EmissionColor", newColor);
                    }
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(targetMaterialName))
                {
                    materials[i].SetColor("_EmissionColor", Color.black);
                }
            }
        }
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
        StartEmissionFadeOut(0.5f);

        if (currentState != State.Dead)
        {
            if (currentPhase >= 2)
            {
                consecutiveAttacksInSameForm++;

                if (consecutiveAttacksInSameForm >= 2)
                {
                    if (Random.Range(0, 10) > 5)
                    {
                        consecutiveAttacksInSameForm = 0;
                        EnterState(State.ChangForm);
                        return;
                    }
                }
            }
            EnterState(State.Chasing);
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

    public void AnimationEvent_SpawnGroundAoE()
    {
        if (scheduledAttackIndex != -1)
        {
            StartCoroutine(SpawnAoE_co(stats.data.attacks[scheduledAttackIndex]));
        }
    }

    public void AnimationEvent_StartRadialLightning()
    {
        if (scheduledAttackIndex != -1)
        {
            StartCoroutine(RadialLightning_co(stats.data.attacks[scheduledAttackIndex]));
        }
    }

    private IEnumerator ChainLightning_co()
    {
        Vector3 startOffset = transform.forward * 3.5f;
        float distanceBetweenStrikes = 5f;
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
            GameObject projectile = Instantiate(flameProjectilePrefab, FlamePoint);
            projectile.GetComponent<Rigidbody>().velocity = transform.forward * 20f;

            timer += fireInterval;
            yield return new WaitForSeconds(fireInterval);
        }
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

    private IEnumerator RadialLightning_co(AttackData attackData)
    {
        Vector3 centerPoint = transform.position;

        int waves = attackData.objectCount;
        float radiusStep = attackData.spawnRadius;
        float delayBetweenWaves = attackData.spawnDelay;
        int baseStrikesPerWave = 6;

        for (int i = 0; i < waves; i++)
        {
            float currentRadius = radiusStep * (i + 1);
            int strikeCount = baseStrikesPerWave + (i * 3);

            for (int j = 0; j < strikeCount; j++)
            {
                float angle = (360f / strikeCount) * j;
                float radian = angle * Mathf.Deg2Rad;

                Vector3 spawnOffset = new Vector3(Mathf.Cos(radian) * currentRadius, 0, Mathf.Sin(radian) * currentRadius);
                Vector3 spawnPosition = centerPoint + spawnOffset;


                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    Instantiate(chainLightningPrefab, hit.position, Quaternion.identity);
                }
            }

            yield return new WaitForSeconds(delayBetweenWaves);
        }
    }
}
