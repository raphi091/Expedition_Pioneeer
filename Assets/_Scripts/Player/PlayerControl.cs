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
    public ActorProfile Profile { get; private set; }
    public WeaponInfo Weapon { get; private set; }
    public PlayerState State;
    public bool IsWeaponEquipped { get; private set; } = false;

    [Header("Core")]
    public InputControl input;
    public CharacterController characterController;
    public Animator animator;
    public Transform maincamera;
    public Transform modelSpawnpoint;

    [Header("Controller")]
    public PlayerMoveControl moveControl;
    public PlayerAttackControl attackControl;

    [Header("Weapon")]
    public Transform handSlot;
    public Transform backSlot;
    public Transform waistSlot;

    private RuntimeAnimatorController baseAnimatorController;
    private GameObject currentHandWeaponInstance;
    private GameObject currentSheathWeaponInstance;

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
    }

    public void SetupCharacter(GameData data, ActorProfile profile)
    {
        Profile = profile;

        Instantiate(Profile.model, modelSpawnpoint);
        animator.avatar = Profile.avatar;
        baseAnimatorController = animator.runtimeAnimatorController;

        moveControl?.Initialize(this);
        attackControl?.Initialize(this);

        RecalculateStats(data);
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
        if (moveControl.isDodging) return;

        if (IsWeaponEquipped) 
            RequestUnequipWeapon();
        else 
            moveControl.Crouch();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (moveControl.isDodging || moveControl.isCrouched) return;

        if (!IsWeaponEquipped)
            RequestEquipWeapon();
        else 
            attackControl.RequestPrimaryAttack();
    }

    public void RequestEquipWeapon()
    {
        if (IsWeaponEquipped || moveControl.isDodging || moveControl.isCrouched || Weapon == null) return;

        animator.SetBool(AnimatorHashSet.WEAPON, true);
    }

    public void RequestUnequipWeapon()
    {
        if (!IsWeaponEquipped || moveControl.isDodging) return;

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
        if (currentHandWeaponInstance != null) 
            Destroy(currentHandWeaponInstance);

        if (currentSheathWeaponInstance != null) 
            Destroy(currentSheathWeaponInstance);

        if (Weapon == null) return;

        if (IsWeaponEquipped)
        {
            if (Weapon.weaponPrefab != null && handSlot != null)
                currentHandWeaponInstance = Instantiate(Weapon.weaponPrefab, handSlot);
        }
        else
        {
            if (Weapon.weaponPrefab != null)
            {
                Transform targetSlot = (Weapon.equipPostion == EquipPostion.Waist) ? waistSlot : backSlot;
                if (targetSlot != null)
                    currentSheathWeaponInstance = Instantiate(Weapon.weaponPrefab, targetSlot);
            }
        }
    }
}

