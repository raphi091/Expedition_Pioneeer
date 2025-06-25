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

public class PlayerControl : MonoBehaviour, IDamage
{
    public event Action<float, float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public static event Action OnPlayerDied;

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
    private bool isDead = false;

    [Header("Controller")]
    public PlayerMoveControl moveControl;
    public PlayerAttackControl attackControl;
    public PlayerInteractionControl interactionControl;

    [Header("Weapon")]
    public Transform handSlotR;
    public Transform handSlotL;
    public Transform armSlotL;
    public Transform backSlot;
    public Transform hipSlot;

    [Header("Animation")]
    public float attackmove = 100f;

    [Header("Buffs")]
    private float attackBuffTimer = 0f;
    private float defenseBuffTimer = 0f;
    public float DamageMultiplier { get; private set; } = 1f;
    public float DefenseMultiplier { get; private set; } = 1f;

    private List<GameObject> activeWeaponInstances = new List<GameObject>();

    private RuntimeAnimatorController baseAnimatorController;

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

    private Coroutine activeHealCoroutine = null;
    private Coroutine attackBuffCoroutine = null;
    private Coroutine defenseBuffCoroutine = null;


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

        if (!TryGetComponent(out interactionControl))
            Debug.LogWarning("PlayerControl ] PlayerInteractionController 없음");

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
            input.actionInput.Player.Inventory.performed += OnPouchOpenInput; // 'I' 키
            input.actionInput.Player.UseItem.performed += OnUseItemInput;   // 'E' 키
            input.actionInput.Player.Interaction.performed += OnInteractInput;
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
        input.actionInput.Player.Inventory.performed -= OnPouchOpenInput;
        input.actionInput.Player.UseItem.performed -= OnUseItemInput;
        input.actionInput.Player.Interaction.performed -= OnInteractInput;
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

        if (attackBuffTimer > 0)
        {
            attackBuffTimer -= Time.deltaTime;
        }
        if (defenseBuffTimer > 0)
        {
            defenseBuffTimer -= Time.deltaTime;
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
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || isDead) return;

        moveControl?.Move(context.ReadValue<Vector2>());
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || isDead) return;

        moveControl?.Run(context.ReadValueAsButton());
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || isDead) return;

        moveControl?.Dodge();
    }

    private void OnLockOn(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause || isDead) return;
        moveControl?.ToggleLockOn();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging || isDead) return;

        if (IsWeaponEquipped)
            UnequipWeapon();
        else
            moveControl.Crouch();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging || moveControl.IsCrouched || isDead) return;

        if (IsWeaponEquipped)
            attackControl.RequestPrimaryAttack();
        else
            EquipWeapon();
    }

    private void OnSecondaryAttack(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || attackControl.IsCharging || attackControl.IsGuarding || moveControl.IsDodging || moveControl.IsCrouched || isDead) return;

        if (IsWeaponEquipped)
            attackControl.RequestSecondaryAttack();
        else
            EquipWeapon();
    }

    private void OnChargeOrGuard(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;
        if (attackControl.IsAttacking || moveControl.IsDodging || moveControl.IsCrouched || !IsWeaponEquipped || isDead) return;

        if (context.started)
        {
            attackControl.RequestChargeOrGuard_Start();
        }
        else if (context.canceled)
        {
            attackControl.RequestChargeOrGuard_End();
        }
    }

    private void OnPouchOpenInput(InputAction.CallbackContext context)
    {
        if (isDead) return;

        interactionControl?.TogglePouchUI();
    }

    private void OnUseItemInput(InputAction.CallbackContext context)
    {
        if (isDead) return;

        interactionControl?.RequestUseQuickSlotItem();
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (isDead) return;

        interactionControl?.RequestInteraction();
    }

    //-----무기 관련
    public void SetWeapon(WeaponInfo newWeapon)
    {
        if (IsWeaponEquipped) return;

        this.Weapon = newWeapon;

        attackControl.attackRadius = Weapon.AttackRange;
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
    public void TakeDamage(float damage)
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
            characterController.enabled = false;
            isDead = true;
            OnPlayerDied?.Invoke();
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

    public void Respawn(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        characterController.enabled = true;

        InitializeState();
        Heal(maxHealth);
        RecoverStamina(maxStamina);
        

        isDead = false;
    }

    //-----아이템 사용 관련
    public void Heal(float amount)
    {
        if (activeHealCoroutine != null)
        {
            StopCoroutine(activeHealCoroutine);
        }

        float targetHealth = Mathf.Min(currentHealth + amount, maxHealth);

        recoverableHealth = targetHealth;

        OnHealthChanged?.Invoke(currentHealth, recoverableHealth, maxHealth);

        activeHealCoroutine = StartCoroutine(HealOverTimeCoroutine(targetHealth, 1.0f));
    }

    private IEnumerator HealOverTimeCoroutine(float targetHealth, float duration)
    {
        float elapsedTime = 0f;
        float startingHealth = currentHealth;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            currentHealth = Mathf.Lerp(startingHealth, targetHealth, elapsedTime / duration);

            OnHealthChanged?.Invoke(currentHealth, recoverableHealth, maxHealth);

            yield return null;
        }

        currentHealth = targetHealth;
        OnHealthChanged?.Invoke(currentHealth, recoverableHealth, maxHealth);

        activeHealCoroutine = null;
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void RequestUseItem(ItemInfo itemInfo)
    {
        if (moveControl.IsRunning || moveControl.IsDodging || attackControl.IsAttacking || attackControl.IsGuarding || attackControl.IsCharging)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemInfo.itemID, 1);
            }
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(AnimatorHashSet.ITEM);
        }

        StartCoroutine(ApplyItemEffectAfterAnimation(itemInfo));
    }

    private IEnumerator ApplyItemEffectAfterAnimation(ItemInfo itemInfo)
    {
        yield return new WaitForSeconds(1.0f);

        OnAnimation_ApplyItemEffect(itemInfo);
    }

    public void OnAnimation_ApplyItemEffect(ItemInfo itemInfo)
    {
        switch (itemInfo.itemID)
        {
            case "1":
                Heal(30);
                break;

            case "2":
                Heal(70);
                break;

            case "3":
                RecoverStamina(60);
                break;

            case "4":
                ApplyAttackBuff(30f, 1.7f);
                break;

            case "5":
                ApplyDefenseBuff(60f, 0.7f);
                break;

            case "6":
                Heal(maxHealth);
                RecoverStamina(maxStamina);
                break;
        }
    }

    public void ApplyAttackBuff(float duration, float multiplier)
    {
        if (attackBuffCoroutine != null)
        {
            StopCoroutine(attackBuffCoroutine);
        }
        attackBuffCoroutine = StartCoroutine(AttackBuffCoroutine(duration, multiplier));
    }

    public void ApplyDefenseBuff(float duration, float multiplier)
    {
        if (defenseBuffCoroutine != null)
        {
            StopCoroutine(defenseBuffCoroutine);
        }
        defenseBuffCoroutine = StartCoroutine(DefenseBuffCoroutine(duration, multiplier));
    }

    private IEnumerator AttackBuffCoroutine(float duration, float multiplier)
    {
        attackBuffTimer = duration;
        DamageMultiplier = multiplier;
        // TODO: 공격력 버프 UI 표시

        yield return new WaitForSeconds(duration);

        DamageMultiplier = 1f;
        attackBuffTimer = 0f;
        attackBuffCoroutine = null;
        // TODO: 공격력 버프 UI 숨기기
    }

    private IEnumerator DefenseBuffCoroutine(float duration, float multiplier)
    {
        defenseBuffTimer = duration;
        DefenseMultiplier = multiplier;
        // TODO: 방어력 버프 UI 표시

        yield return new WaitForSeconds(duration);

        DefenseMultiplier = 1f;
        defenseBuffTimer = 0f;
        defenseBuffCoroutine = null;
        // TODO: 방어력 버프 UI 숨기기
    }
}

