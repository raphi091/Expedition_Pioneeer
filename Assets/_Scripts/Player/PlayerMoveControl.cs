using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMoveController : MonoBehaviour
{
    [Header("Setting")]
    private PlayerControl pc;
    private InputControl input;
    private Animator animator;
    private CharacterController cc;
    private Transform maincamera;

    [Header("Movement")]
    public float gravity = -9.81f;
    public float animationSpeed = 10f;

    private Vector2 moveInputRaw;
    private bool isRunning = false;
    private bool wantsToDodge = false;

    private bool isCrouched = false;
    private Vector3 velocity;
    private bool isDodging = false;

    private float walkSpeed;
    private float runSpeed;
    private float crouchWalkSpeed;
    private float rotationSpeed;
    private float dodgeDistance;
    private float dodgeDuration;

    [Header("Cursor")]
    public bool lockCursorOnStart = true;


    private void Awake()
    {
        if (!TryGetComponent(out pc))
            Debug.LogWarning("PlayerMovementController ] PlayerControl 없음");

        if (!TryGetComponent(out input))
            Debug.LogWarning("PlayerControl ] InputControl 없음");

        if (!TryGetComponent(out cc))
            Debug.LogWarning("PlayerControl ] CharacterController 없음");

        if (!TryGetComponent(out animator))
            Debug.LogWarning("PlayerControl ] Animator 없음");

        maincamera = Camera.main.transform;

        if (pc.Profile != null)
        {
            PlayerState tempState = pc.State;
            tempState.Set(pc.Profile);
            pc.State = tempState;

            LoadStatsFromProfile(pc.Profile);
        }
        else
        {
            Debug.LogWarning("PlayerMovementController ] PlayerControl ] PlayerState 없음");
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            LockCursor();
        }
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

    void OnEnable()
    {
        if (input != null && input.actionInput != null)
        {
            input.actionInput.Player.Move.performed += OnMove;
            input.actionInput.Player.Move.canceled += OnMove;
            input.actionInput.Player.Run.performed += OnRun;
            input.actionInput.Player.Run.canceled += OnRunStop;
            input.actionInput.Player.Crouch.performed += OnCrouch;
            input.actionInput.Player.Dodge.performed += OnDodge;
        }
    }

    void OnDisable()
    {
        if (input != null && input.actionInput != null)
        {
            input.actionInput.Player.Move.performed -= OnMove;
            input.actionInput.Player.Move.canceled -= OnMove;
            input.actionInput.Player.Run.performed -= OnRun;
            input.actionInput.Player.Run.canceled -= OnRunStop;
            input.actionInput.Player.Crouch.performed -= OnCrouch;
            input.actionInput.Player.Dodge.performed -= OnDodge;
        }
    }

    //-----입력 이벤트 핸들러
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInputRaw = context.ReadValue<Vector2>();
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = true;
    }

    private void OnRunStop(InputAction.CallbackContext context)
    {
        isRunning = false;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (isDodging) return;

        isCrouched = !isCrouched;
        animator.SetBool(AnimatorHashSet.CROUCH, isCrouched);
        UpdateCC(isCrouched);
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (!isDodging)
        {
            wantsToDodge = true;
        }
    }

    void Update()
    {
        UpdateAnimator();
        CursorLockState();
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

    private void CursorLockState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

        Vector3 moveDirection = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
        Vector3 worldMoveDirection = Vector3.zero;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            worldMoveDirection = Quaternion.Euler(0, maincamera.eulerAngles.y, 0) * moveDirection;
            worldMoveDirection.Normalize();
        }

        velocity.x = worldMoveDirection.x * movespeed;
        velocity.z = worldMoveDirection.z * movespeed;

        if (worldMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(worldMoveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void UpdateGravity()
    {
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
            cc.center = new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.9f, 0);
        }
    }

    private IEnumerator DodgeCo()
    {
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
}
