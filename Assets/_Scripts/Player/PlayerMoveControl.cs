using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMoveControl : MonoBehaviour
{
    [Header("Setting")]
    private PlayerControl pc;
    private Animator animator;
    private CharacterController cc;
    private Transform maincamera;

    [Header("Movement")]
    public float gravity = -9.81f;
    public float animationSpeed = 10f;

    [Header("Weapon Settings")]
    public Transform handWeaponSlot;
    public Transform EquipWeaponSlot;
    private GameObject currentHandWeaponInstance;
    private GameObject currentSheathWeaponInstance;
    private bool isWeaponEquipped = false;


    private Vector2 moveInputRaw;
    private Vector3 velocity;
    private bool isCrouched = false;
    private bool isDodging = false;
    private bool isRunning = false;
    private bool wantsToDodge = false;

    private float walkSpeed;
    private float runSpeed;
    private float crouchWalkSpeed;
    private float rotationSpeed;
    private float dodgeDistance;
    private float dodgeDuration;

    [Header("Head Look Settings")]
    [Range(0, 1)] public float overallLookAtWeight = 1.0f;
    [Range(0, 1)] public float bodyLookAtWeight = 0.15f;
    [Range(0, 1)] public float headLookAtWeight = 0.8f;
    [Range(0, 1)] public float eyesLookAtWeight = 1.0f;
    [Range(0, 1)] public float clampLookAtWeight = 0.5f;
    public float lookAtTargetDistance = 10f;

    [Header("Lock-On System")]
    public float lockOnRange = 15f;
    public float lockOnAngle = 120f;
    public LayerMask enemyLayerMask;
    public LayerMask obstacleLayerMask;
    private Transform currentLockedTarget;
    private bool isLockedOn = false;
    public float lockOnRotationSpeed = 15f;

    [Header("Cinemachine Lock-On Camera")]
    public CinemachineFreeLook FollowCam;
    public CinemachineVirtualCamera LockOnCam;

    [Header("Cursor")]
    public bool lockCursorOnStart = true;

    public bool IsCrouched => isCrouched;
    public bool IsDodging => isDodging;
    public bool IsRunning => isRunning;


    private void Awake()
    {
        if (FollowCam != null && LockOnCam != null)
        {
            FollowCam.Priority = 10;
            LockOnCam.Priority = 5;
            LockOnCam.LookAt = null;
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            LockCursor();
        }
    }

    public void Initialize(PlayerControl playerControl)
    {
        pc = playerControl;
        cc = playerControl.characterController;
        animator = playerControl.animator;
        maincamera = playerControl.maincamera;

        LoadStatsFromProfile(playerControl.Profile);
    }

    private void LoadStatsFromProfile(ActorProfile profile)
    {
        walkSpeed = profile.walkspeed;
        runSpeed = profile.runspeed;
        crouchWalkSpeed = profile.crouchwalkspeed;
        rotationSpeed = profile.rotatespeed;
        dodgeDistance = profile.dodgedistance;
        dodgeDuration = profile.dodgeduration;
    }

    public void Move(Vector2 input)
    {
        if (isDodging) return;
        moveInputRaw = input;
    }
    public void Run(bool isPressed)
    {
        if (isDodging) return;
        isRunning = isPressed;
    }

    public void Dodge()
    {
        if (!isDodging)
        {
            wantsToDodge = true;
        }
    }

    public void Crouch()
    {
        if (isDodging) return;

        isCrouched = !isCrouched;
        animator.SetBool(AnimatorHashSet.CROUCH, isCrouched);
        UpdateCC(isCrouched);
    }

    void Update()
    {
        UpdateAnimator();

        if (isLockedOn)
            MaintainLockOn();
    }

    void FixedUpdate()
    {
        if (wantsToDodge)
        {
            if (!isDodging)
            {
                StartCoroutine(DodgeCo());
            }
            wantsToDodge = false;
        }

        MovementAndRotate();
        UpdateGravity();
        cc.Move(velocity * Time.fixedDeltaTime);
    }

    public void CursorLockState()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float movespeed = 0f;
        float magnitude = moveInputRaw.magnitude;

        if (isDodging)
        {
            // 구르기 중에는 특정 애니메이션이 재생 (DodgeTrigger 사용)
        }
        else if (isCrouched)
        {
            if (magnitude > 0.1f) movespeed = 1.0f;
            else movespeed = 0f;
        }
        else
        {
            if (isRunning && magnitude > 0.1f)
            {
                movespeed = 2.0f;
            }
            else if (magnitude > 0.1f)
            {
                movespeed = 1.0f;
            }
            else
            {
                movespeed = 0.0f;
            }
        }

        float currentmoveanimation = animator.GetFloat(AnimatorHashSet.MOVESPEED);
        float moveanimation = Mathf.Lerp(currentmoveanimation, movespeed, Time.deltaTime * animationSpeed);
        animator.SetFloat(AnimatorHashSet.MOVESPEED, moveanimation);
    }

    private void MovementAndRotate()
    {
        if (isDodging) return;

        float movespeed = 0f;
        float magnitude = moveInputRaw.magnitude;

        if (isCrouched)
        {
            if (magnitude > 0.1f)
                movespeed = crouchWalkSpeed;
        }
        else
        {
            if (isRunning && magnitude > 0.1f)
            {
                movespeed = runSpeed;
            }
            else if (magnitude > 0.1f)
            {
                movespeed = walkSpeed;
            }
        }

        if (isRunning && moveInputRaw.magnitude > 0.1f)
        {
            if (pc.TryConsumeStamina(pc.GetRunStaminaCostPerSecond() * Time.fixedDeltaTime))
            {
                movespeed = runSpeed;
            }
            else
            {
                isRunning = false;
                movespeed = walkSpeed;
            }
        }
        else if (moveInputRaw.magnitude > 0.1f)
        {
            movespeed = walkSpeed;
        }

        Vector3 moveDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
        Vector3 worldMoveDir = Vector3.zero;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            worldMoveDir = Quaternion.Euler(0, maincamera.eulerAngles.y, 0) * moveDir;
            worldMoveDir.Normalize();
        }

        velocity.x = worldMoveDir.x * movespeed;
        velocity.z = worldMoveDir.z * movespeed;

        if (worldMoveDir.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(worldMoveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void UpdateGravity()
    {
        if (cc == null) return;

        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.fixedDeltaTime;
    }

    private void UpdateCC(bool crouch)
    {
        if (crouch)
        {
            cc.height = 1f;
            cc.center = new Vector3(0f, 0.57f, 0f);
        }
        else
        {
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.97f, 0);
        }
    }

    private IEnumerator DodgeCo()
    {
        if (!pc.TryConsumeStamina(pc.GetDodgeStaminaCost()))
        {
            yield break;
        }

        isDodging = true;
        animator.SetTrigger(AnimatorHashSet.DODGE);

        float startTime = Time.time;
        Vector3 dodgeDirection = transform.forward;

        if (moveInputRaw.sqrMagnitude > 0.01f)
        {
            Vector3 inputDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y).normalized;
            dodgeDirection = Quaternion.Euler(0, maincamera.eulerAngles.y, 0) * inputDir;
            dodgeDirection = dodgeDirection.normalized;
        }

        while (Time.time < startTime + dodgeDuration)
        {
            Vector3 horizontalDodgeVelocity = dodgeDirection * (dodgeDistance / dodgeDuration);
            velocity.x = horizontalDodgeVelocity.x;
            velocity.z = horizontalDodgeVelocity.z;

            yield return new WaitForFixedUpdate();
        }

        velocity.x = 0;
        velocity.z = 0;
        isDodging = false;
    }

    public void ToggleLockOn()
    {
        if (isLockedOn)
        {
            EndLockOn();
        }
        else
        {
            StartLockOn();
        }
    }

    private void StartLockOn()
    {
        Transform potentialTarget = FindLockOnTarget();

        if (potentialTarget != null)
        {
            currentLockedTarget = potentialTarget;
            isLockedOn = true;

            if (LockOnCam != null)
            {
                LockOnCam.LookAt = currentLockedTarget;
                LockOnCam.Priority = 15;
            }
        }
    }

    private void EndLockOn()
    {
        isLockedOn = false;
        currentLockedTarget = null;

        if (LockOnCam != null)
        {
            LockOnCam.Priority = 5;
            LockOnCam.LookAt = null;
        }
    }

    private Transform FindLockOnTarget()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayerMask);
        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        Vector3 playerForward = maincamera.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        foreach (Collider col in potentialTargets)
        {
            Vector3 directionToTarget = col.transform.position - transform.position;
            directionToTarget.y = 0;
            float angle = Vector3.Angle(playerForward, directionToTarget.normalized);

            if (angle < lockOnAngle / 2)
            {
                if (!Physics.Linecast(transform.position + Vector3.up * 0.5f, col.transform.position + Vector3.up * 0.5f, obstacleLayerMask))
                {
                    float distanceToTarget = directionToTarget.magnitude;
                    if (distanceToTarget < minDistance)
                    {
                        minDistance = distanceToTarget;
                        closestTarget = col.transform;
                    }
                }
            }
        }
        return closestTarget;
    }

    private void MaintainLockOn()
    {
        if (!isLockedOn || currentLockedTarget == null || !currentLockedTarget.gameObject.activeInHierarchy)
        {
            if (isLockedOn) EndLockOn();
            return;
        }

        if (Vector3.Distance(transform.position, currentLockedTarget.position) > lockOnRange + 2f)
        {
            EndLockOn();
            return;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || maincamera == null)
        {
            return;
        }

        if (layerIndex == 0)
        {
            animator.SetLookAtWeight(overallLookAtWeight, bodyLookAtWeight, headLookAtWeight, eyesLookAtWeight, clampLookAtWeight);
            Vector3 lookAtTargetPosition = maincamera.position + maincamera.forward * lookAtTargetDistance;
            animator.SetLookAtPosition(lookAtTargetPosition);
        }
    }
}
