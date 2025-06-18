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
    public Transform backSlot;
    public Transform hipSlot;

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
            input.actionInput.Player.SecondaryAttack.performed += OnAttack;
            input.actionInput.Player.Charge.performed += OnAttack;
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
        input.actionInput.Player.SecondaryAttack.performed -= OnAttack;
        input.actionInput.Player.Charge.performed -= OnAttack;
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

        if (!moveControl.IsRunning && !moveControl.IsDodging /* && !attackControl.IsAttacking */)
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
        moveControl?.Move(context.ReadValue<Vector2>());
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        moveControl?.Run(context.ReadValueAsButton());
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        moveControl?.Dodge();
    }

    private void OnLockOn(InputAction.CallbackContext context)
    {
        moveControl?.ToggleLockOn();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (moveControl.IsDodging) return;

        if (IsWeaponEquipped)
            RequestUnequipWeapon();
        else
            moveControl.Crouch();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (moveControl.IsDodging || moveControl.IsCrouched) return;

        if (!IsWeaponEquipped)
            RequestEquipWeapon();
        else
            attackControl.RequestPrimaryAttack();
    }

    //-----무기 관련
    public void RequestEquipWeapon()
    {
        if (IsWeaponEquipped || moveControl.IsDodging || moveControl.IsCrouched || Weapon == null) return;

        animator.SetBool(AnimatorHashSet.WEAPON, true);
    }

    public void RequestUnequipWeapon()
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
        if (currentHandWeaponInstance != null) Destroy(currentHandWeaponInstance);

        if (currentSheathWeaponInstance != null) Destroy(currentSheathWeaponInstance);

        WeaponInfo weaponData = this.Weapon;
        if (weaponData == null) return;

        if (IsWeaponEquipped)
        {
            if (weaponData.weaponPrefab != null && handSlotR != null)
                currentHandWeaponInstance = Instantiate(weaponData.weaponPrefab, handSlotR);
        }
        else
        {
            if (weaponData.weaponPrefab != null)
            {
                Transform targetSlot = (weaponData.equipPostion == EquipPostion.Hip) ? hipSlot : backSlot;

                if (targetSlot != null)
                    currentSheathWeaponInstance = Instantiate(weaponData.weaponPrefab, targetSlot);
                else
                    Debug.LogWarning($"{weaponData.name}의 Sheath Slot을 찾지 못해 무기 외형을 표시할 수 없습니다.");
            }
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
            // 죽음 처리
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

