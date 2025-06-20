using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CustomInspector;


public struct PlayerState
{
    public int level;
    public int health;
    public int stamina;
    public int damage;
    public float attackrange;

    public void Set(ActorProfile profile)
    {
        health = profile.health;
        stamina = profile.stamina;
        damage = profile.damage;
    }
}

public class PlayerControl : MonoBehaviour
{
    public event Action<float, float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;

    public ActorProfile Profile { get; private set; }
    public WeaponInfo Weapon { get; private set; }
    public PlayerState State;
    public bool IsWeaponEquipped { get; private set; } = false;

    [Header("Core")]
    public InputControl input;
    public CharacterController characterController;
    public Animator animator;
    public Transform maincamera;
    public Transform model;

    [Header("Controller")]
    public PlayerMoveControl moveControl;
    public PlayerAttackControl attackControl;

    [Header("Weapon")]
    public Transform handSlotR;
    public Transform handSlotL;
    public Transform armSlotL;
    public Transform backSlot;
    public Transform hipSlot;

    [Header("Animation")]
    public float attackmove = 100f;

    private List<GameObject> activeWeaponInstances = new List<GameObject>();

    private RuntimeAnimatorController baseAnimatorController;
    private GameObject currentHandWeaponInstance;
    private GameObject currentSheathWeaponInstance;

    private float currentHealth;
    private float recoverableHealth;
    private float maxHealth;
    private float healthRegenRate;
    private float healthRegenDelay;
    private float timeSinceLastDamage;

    private float currentStamina;
    private float maxStamina;
    private float staminaRegenRate;
    private float runStaminaCost;
    private float dodgeStaminaCost;

    public float CurrentHealth => currentHealth;
    public float RecoverableHealth => recoverableHealth;
    public float MaxHealth => maxHealth;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;


    private void Awake()
    {
        if (!TryGetComponent(out input))
            Debug.LogWarning("PlayerControl ] InputControl 없음");

        if (!TryGetComponent(out characterController))
            Debug.LogWarning("PlayerControl ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("PlayerControl ] Animator 없음");

        if (!TryGetComponent(out moveControl))
            Debug.LogWarning("PlayerControl ] Animator 없음");

        if (!TryGetComponent(out attackControl))
            Debug.LogWarning("PlayerControl ] Animator 없음");

        maincamera = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (input != null && input.actionInput != null)
        {
            input.actionInput.Player.Move.performed += OnMove;
            input.actionInput.Player.Move.canceled += OnMove;
            input.actionInput.Player.Run.performed += OnRun;
            input.actionInput.Player.Run.canceled += OnRun;
            input.actionInput.Player.Crouch.performed += OnCrouch;
            input.actionInput.Player.Dodge.performed += OnDodge;
            input.actionInput.Player.LockOn.performed += OnLockOn;
            input.actionInput.Player.Attack.performed += OnAttack;
            input.actionInput.Player.SecondaryAttack.performed += OnSecondaryAttack;
            input.actionInput.Player.ChargeOrGuard.started += OnChargeOrGuard;
            input.actionInput.Player.ChargeOrGuard.canceled += OnChargeOrGuard;
        }
    }

    private void OnDisable()
    {
        input.actionInput.Player.Move.performed -= OnMove;
        input.actionInput.Player.Move.canceled -= OnMove;
        input.actionInput.Player.Run.performed -= OnRun;
        input.actionInput.Player.Run.canceled -= OnRun;
        input.actionInput.Player.Crouch.performed -= OnCrouch;
        input.actionInput.Player.Dodge.performed -= OnDodge;
        input.actionInput.Player.LockOn.performed -= OnLockOn;
        input.actionInput.Player.Attack.performed -= OnAttack;
        input.actionInput.Player.SecondaryAttack.performed -= OnSecondaryAttack;
        input.actionInput.Player.ChargeOrGuard.started -= OnChargeOrGuard;
        input.actionInput.Player.ChargeOrGuard.canceled -= OnChargeOrGuard;
    }

    private void Update()
    {
        timeSinceLastDamage += Time.deltaTime;

        if (currentHealth < recoverableHealth && timeSinceLastDamage >= healthRegenDelay)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, recoverableHealth);

            if (State.health != Mathf.FloorToInt(currentHealth))
            {
                State.health = Mathf.FloorToInt(currentHealth);
                OnHealthChanged?.Invoke(currentHealth, recoverableHealth, Profile.health);
            }
        }

        if (!moveControl.IsRunning && !moveControl.IsDodging && !attackControl.IsAttacking && !attackControl.IsCharging)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }
    }

    public void SetupCharacter(GameData data, ActorProfile profile)
    {
        Profile = profile;

        Instantiate(Profile.model, model);
        animator.avatar = Profile.avatar;
        baseAnimatorController = animator.runtimeAnimatorController;

        moveControl?.Initialize(this);
        attackControl?.Initialize(this);

        FindAndCacheSlots(model);
        InitializeState();
        RecalculateStats(data);
    }

    private void FindAndCacheSlots(Transform modelRoot)
    {
        if (modelRoot == null) return;

        handSlotR = modelRoot.FindSlot("WEAPON_HAND_R", "HANDSLOTR");
        handSlotL = modelRoot.FindSlot("WEAPON_HAND_L", "HANDSLOTL");
        armSlotL = modelRoot.FindSlot("WEAPON_ARM_L", "ARMSLOTL");
        backSlot = modelRoot.FindSlot("BACK_SLOT", "BACKSLOT");
        hipSlot = modelRoot.FindSlot("HIP_SLOT", "HIPSLOT");
    }

    public void InitializeState()
    {
        if (Profile != null)
        {
            maxHealth = Profile.health;
            currentHealth = maxHealth;
            recoverableHealth = maxHealth;
            healthRegenRate = Profile.healthRegenRate;
            healthRegenDelay = Profile.healthRegenDelay;

            State.Set(Profile);
            maxStamina = Profile.stamina;
            currentStamina = maxStamina;
            staminaRegenRate = Profile.staminaRegenRate;
            runStaminaCost = Profile.runStaminaCost;
            dodgeStaminaCost = Profile.dodgeStaminaCost;

            OnHealthChanged?.Invoke(currentHealth, recoverableHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    public void RecalculateStats(GameData data)
    {
        State.Set(this.Profile);

        // GameData에 저장된 장비 정보를 바탕으로 추가 능력치를 계산
        // WeaponData weapon = WeaponDatabase.GetWeapon(data.equipment.weaponID);
        // if(weapon != null)
        // {
        //     State.damage += weapon.damage;
        // }
    }

    //-----이동 및 공격
    private void OnMove(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding) return;

        moveControl?.Move(context.ReadValue<Vector2>());
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding) return;

        moveControl?.Run(context.ReadValueAsButton());
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging) return;

        moveControl?.Dodge();
    }

    private void OnLockOn(InputAction.CallbackContext context)
    {
        moveControl?.ToggleLockOn();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging) return;

        if (IsWeaponEquipped)
            UnequipWeapon();
        else
            moveControl.Crouch();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging || moveControl.IsCrouched) return;

        if (IsWeaponEquipped)
            attackControl.RequestPrimaryAttack();
        else
            EquipWeapon();
    }

    private void OnSecondaryAttack(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging || moveControl.IsCrouched) return;

        if (IsWeaponEquipped)
            attackControl.RequestSecondaryAttack();
        else
            EquipWeapon();
    }

    private void OnChargeOrGuard(InputAction.CallbackContext context)
    {
        if (attackControl.IsAttacking || moveControl.IsDodging || moveControl.IsCrouched || !IsWeaponEquipped) return;

        if (context.started)
        {
            attackControl.RequestChargeOrGuard_Start();
        }
        else if (context.canceled)
        {
            attackControl.RequestChargeOrGuard_End();
        }
    }

    //-----무기 관련
    public void SetWeapon(WeaponInfo newWeapon)
    {
        if (IsWeaponEquipped) return;

        this.Weapon = newWeapon;

        IsWeaponEquipped = false;
        UpdateWeaponVisuals();
    }

    public void EquipWeapon()
    {
        if (IsWeaponEquipped || moveControl.IsDodging || moveControl.IsCrouched || Weapon == null) return;

        animator.SetBool(AnimatorHashSet.WEAPON, true);
    }

    public void UnequipWeapon()
    {
        if (!IsWeaponEquipped || moveControl.IsDodging) return;

        animator.SetBool(AnimatorHashSet.WEAPON, false);
    }

    public void ApplyWeaponDataToState()
    {
        if (Weapon != null)
        {
            State.damage = Weapon.Damage;
            State.attackrange = Weapon.AttackRange;

            if (animator != null)
                animator.runtimeAnimatorController = Weapon.animatorOverride ?? baseAnimatorController;
        }
    }

    public void OnAnimation_EquipComplete()
    {
        IsWeaponEquipped = true;

        ApplyWeaponDataToState();
        UpdateWeaponVisuals();
    }

    public void OnAnimation_UnequipComplete()
    {
        IsWeaponEquipped = false;

        ApplyWeaponDataToState();
        UpdateWeaponVisuals();
    }

    private void UpdateWeaponVisuals()
    {
        foreach (GameObject instance in activeWeaponInstances)
        {
            Destroy(instance);
        }
        activeWeaponInstances.Clear();

        if (Weapon == null) return;

        List<WeaponPart> partsToShow = IsWeaponEquipped ? Weapon.equippedParts : Weapon.sheathedParts;
        if (partsToShow == null) return;

        foreach (WeaponPart part in partsToShow)
        {
            if (part.prefab == null) continue;

            Transform targetSlot = GetSlotTransform(part.equipPoint);
            if (targetSlot != null)
            {
                GameObject newInstance = Instantiate(part.prefab, targetSlot);
                activeWeaponInstances.Add(newInstance);
            }
            else
            {
                Debug.LogWarning($"{part.equipPoint} 슬롯을 찾을 수 없어 {part.prefab.name}을 생성하지 못했습니다.");
            }
        }

        animator.runtimeAnimatorController = Weapon.animatorOverride;
    }

    private Transform GetSlotTransform(EquipPostion equipPoint)
    {
        switch (equipPoint)
        {
            case EquipPostion.HandR: return handSlotR;
            case EquipPostion.HandL: return handSlotL;
            case EquipPostion.ArmL: return armSlotL;
            case EquipPostion.Back: return backSlot;
            case EquipPostion.Hip: return hipSlot;
            default: return null;
        }
    }

    //-----체력 및 스테미너
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        float permanentDamage = damage * (1.0f - Profile.recoverableDamageRatio);

        currentHealth -= damage;
        recoverableHealth -= permanentDamage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        recoverableHealth = Mathf.Clamp(recoverableHealth, currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, recoverableHealth, maxHealth);
        timeSinceLastDamage = 0f;

        if (currentHealth <= 0) 
        {
            animator.SetTrigger(AnimatorHashSet.DEATH);
        }
    }

    public bool TryConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }

    public float GetDodgeStaminaCost()
    {
        return dodgeStaminaCost;
    }

    public float GetRunStaminaCostPerSecond()
    {
        return runStaminaCost;
    }
}

