using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAttackControl : MonoBehaviour
{
    private PlayerControl playerControl;
    private Animator animator;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRadius = 0.7f;
    public LayerMask enemyLayerMask;

    [Header("Basic Combo")]
    public int basicComboMaxCount = 4;
    public float basicComboResetTime = 1.2f;
    private int basicComboCounter = 0;
    private float lastBasicAttackTime = 0f;

    [Header("Strong Combo")]
    public int strongComboMaxCount = 2;
    public float strongComboResetTime = 1.5f;
    private int strongComboCounter = 0;
    private float lastStrongAttackTime = 0f;

    [Header("Charge / Guard")]
    public float chargeLevel2Time = 1.5f;
    public float chargeDamageMultiplierLvl2 = 2f;
    public float chargeStaminaCostLvl1 = 10f;
    public float chargeStaminaCostLvl2 = 20f;
    public float chargeStaminaDrainPerSecond = 2f;
    private float lastAttackDamageMultiplier = 1f;

    private bool isGuarding = false;
    private bool isCharging = false;
    private bool isAttacking = false;

    private float currentChargeTime = 0f;

    public bool IsGuarding => isGuarding;
    public bool IsCharging => isCharging;
    public bool IsAttacking => isAttacking;
    

    public void Initialize(PlayerControl playerControl)
    {
        this.playerControl = playerControl;
        this.animator = playerControl.animator;
    }

    private void Update()
    {
        if (Time.time - lastBasicAttackTime > basicComboResetTime) 
            basicComboCounter = 0;

        if (Time.time - lastStrongAttackTime > strongComboResetTime) 
            strongComboCounter = 0;

        if (IsCharging)
        {
            currentChargeTime += Time.deltaTime;
            // 효과음 및 이펙트 작업

            float drainAmount = chargeStaminaDrainPerSecond * Time.deltaTime;
            if (!playerControl.TryConsumeStamina(drainAmount))
            {
                CancelCharge();
            }
        }
    }

    public void AnimationEvent_AttackStarted()
    {
        isAttacking = true;
    }

    public void AnimationEvent_AttackEnded()
    {
        isAttacking = false;
    }

    public void RequestPrimaryAttack()
    {
        if (isGuarding || isCharging || isAttacking) return;

        if (Time.time - lastBasicAttackTime > basicComboResetTime) 
            basicComboCounter = 1;
        else 
            basicComboCounter++;

        if (basicComboCounter > basicComboMaxCount) 
            basicComboCounter = 1;

        lastBasicAttackTime = Time.time;
        lastAttackDamageMultiplier = 1f;

        animator.SetInteger(AnimatorHashSet.ATTACK_COMBO, basicComboCounter);
        animator.SetTrigger(AnimatorHashSet.ATTACK);
    }

    public void RequestSecondaryAttack()
    {
        if (isGuarding || isCharging || isAttacking) return;

        if (Time.time - lastStrongAttackTime > strongComboResetTime) 
            strongComboCounter = 1;
        else 
            strongComboCounter++;

        if (strongComboCounter > strongComboMaxCount) strongComboCounter = 1;

        lastStrongAttackTime = Time.time;
        lastAttackDamageMultiplier = 1.5f; 

        animator.SetInteger(AnimatorHashSet.SECONDARYATTACK_COMBO, strongComboCounter);
        animator.SetTrigger(AnimatorHashSet.SECONDARYATTACK);
    }

    public void RequestChargeOrGuard_Start()
    {
        if (IsAttacking) return;

        if (playerControl.Weapon.weaponType == WeaponType.OnehandSword)
        {
            isGuarding = true;
            animator.SetBool(AnimatorHashSet.GUARD, true);
        }
        else
        {
            isCharging = true;
            currentChargeTime = 0f;
            animator.SetBool(AnimatorHashSet.CHARGE, true);
        }
    }

    public void RequestChargeOrGuard_End()
    {
        if (IsGuarding)
        {
            isGuarding = false;
            animator.SetBool(AnimatorHashSet.GUARD, false);
        }
        else if (IsCharging)
        {
            isCharging = false;
            animator.SetBool(AnimatorHashSet.CHARGE, false);

            int chargeLevel;
            float staminaCost;

            if (currentChargeTime >= chargeLevel2Time)
            {
                chargeLevel = 2;
                staminaCost = chargeStaminaCostLvl2;
                lastAttackDamageMultiplier = chargeDamageMultiplierLvl2;
            }
            else
            {
                chargeLevel = 1;
                staminaCost = chargeStaminaCostLvl1;
                lastAttackDamageMultiplier = 1f;
            }

            if (playerControl.TryConsumeStamina(staminaCost))
            {
                animator.SetInteger(AnimatorHashSet.CHARGE_LEVEL, chargeLevel);
                animator.SetTrigger(AnimatorHashSet.CHARGED_ATTACK);
            }
            else
            {
                lastAttackDamageMultiplier = 1f;
            }
        }
    }

    private void CancelCharge()
    {
        if (!isCharging) return;

        isCharging = false;
        currentChargeTime = 0f;
        animator.SetBool(AnimatorHashSet.CHARGE, false);
    }

    public void AnimationEvent_PerformHitCheck()
    {
        if (playerControl == null) return;

        int finalDamage = Mathf.RoundToInt((playerControl.State.damage + playerControl.Weapon.Damage) * lastAttackDamageMultiplier);

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayerMask);
        foreach (Collider enemy in hitEnemies)
        {
            IDamage damageable = enemy.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.TakeDamage(finalDamage);
            }
        }

        lastAttackDamageMultiplier = 1f;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
