using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
    public enum MovementMode
    {
        None = 0,
        WalkRun = 1,
        Hover = 2,
        Flight = 3,
        Swim = 4,
        EdgeHange = 5,
        EdgeSide = 6
    }
    public enum CameraRotateMode
    {
        Free,
        Locked,
        FollowY,
        FollowAll,
    }
    public bool HasNegativeVerticalVelocity => rigidbody.linearVelocity.y < 0;
    public bool IsPhysicallyGrounded => Physics.CheckSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius, groundLayer);
    public bool IsAlmostGrounded => Physics.CheckSphere(transform.position + Vector3.down * hangGroundCheckDistance, groundCheckRadius, groundLayer);
    public float HorizontalInput => Input.GetAxis("HorizontalLessSmooth");
    public float VerticalInput => Input.GetAxis("VerticalLessSmooth");
    public bool SprintInput => Input.GetKey(KeyCode.LeftShift);
    private bool JumpInput => Input.GetKey(KeyCode.Space);

    [SerializeField, BoxGroup("Basic"), ReadOnly]
    private MovementMode movementMode;
    [SerializeField, BoxGroup("Basic"), ReadOnly]
    private CameraRotateMode rotateMode;
    [SerializeField, BoxGroup("Basic"), ReadOnly]
    private CameraRotateMode cachedRotateMode;

    [SerializeField, FoldoutGroup("Walk Run")]
    private Vector2 baseGroundSpeed;
    [SerializeField, FoldoutGroup("Walk Run")]
    private Vector2 sprintModifier;
    [SerializeField, FoldoutGroup("Walk Run")]
    private float customRunGravity;
    [SerializeField, FoldoutGroup("Walk Run")]
    private float groundSpeedMultiplier = 1;
    [SerializeField, FoldoutGroup("Walk Run")]
    private float inAirSpeedMultiplier = 1;
    [SerializeField, FoldoutGroup("Walk Run"), Range(0, 1)]
    private float speedDamp = .5f;
    [SerializeField, FoldoutGroup("Walk Run")]
    private float defaultDrag;

    [SerializeField, FoldoutGroup("Jumping")]
    private float initialJumpForce = 1;
    [SerializeField, FoldoutGroup("Jumping")]
    private float progressiveJumpForce = .1f;
    [SerializeField, FoldoutGroup("Jumping")]
    private float progressiveJumpMaxDuration = .1f;
    [SerializeField, FoldoutGroup("Jumping")]
    private float customGravity = .1f;
    [SerializeField, FoldoutGroup("Jumping")]
    public float groundCheckOriginOffset = 0.2f;
    [SerializeField, FoldoutGroup("Jumping")]
    public float groundCheckDistance = 0.2f;
    [SerializeField, FoldoutGroup("Jumping")]
    public float groundCheckRadius = 0.2f;
    [SerializeField, FoldoutGroup("Jumping")]
    public LayerMask groundLayer;
    [SerializeField, FoldoutGroup("Jumping"), ReadOnly]
    private bool isGrounded;
    [SerializeField, FoldoutGroup("Jumping"), ReadOnly]
    private bool progressiveJumpActive;
    [SerializeField, FoldoutGroup("Jumping"), ReadOnly]
    private float progressJumpTime;
    private Coroutine checkForGroundRoutine;

    [SerializeField, ReadOnly, FoldoutGroup("Jumping")]
    private bool freshJumpInputDown;
    [SerializeField, ReadOnly, FoldoutGroup("Jumping")]
    private bool jumpInputDown;

    [SerializeField, FoldoutGroup("Hanging")]
    private Transform hangPoint;
    [SerializeField, FoldoutGroup("Hanging")]
    private float hangKipForce;
    [SerializeField, FoldoutGroup("Hanging")]
    public float hangGroundCheckDistance = 0.2f;
    [SerializeField, FoldoutGroup("Hanging"), ReadOnly]
    private bool isHanging;

    [SerializeField, FoldoutGroup("Flight")]
    private Vector2 baseFlightSpeed;
    [SerializeField, FoldoutGroup("Flight")]
    private float flightSpeedMultiplier = 1;
    [SerializeField, FoldoutGroup("Flight")]
    private float flightSprintMultiplier = 1;
    [SerializeField, FoldoutGroup("Flight")]
    private float flightDrag;
    [SerializeField, FoldoutGroup("Flight")]
    private float hoverSpeedMultiplier;
    [SerializeField, FoldoutGroup("Flight")]
    private float hoverGravityMultiplier;
    [SerializeField, FoldoutGroup("Flight")]
    private float hoverDrag;

    private RaycastHit groundInfo;
    private Camera playerCamera;
    private Rigidbody rigidbody;
    private CharacterAnimator characterAnimator;
    private MovementEdgeChecker edgeChecker;

    #region Jump Control
    private void JumpInputStart()
    {
        Vector3 initJumpForce = Vector3.up * initialJumpForce;
        rigidbody.AddForce(initJumpForce, ForceMode.Impulse);
        progressJumpTime = 0;
        progressiveJumpActive = true;
        characterAnimator.SetBool("InAir", true);
        characterAnimator.SetTrigger("Jump");
        BecomeUngrounded();
    }

    private IEnumerator CheckForGroundRoutine()
    {
        yield return new WaitForSeconds(.5f);
        while (!IsPhysicallyGrounded) {
            yield return null;
        }
        OnHitGround();
    }
    private void BecomeUngrounded()
    {
        isGrounded = false;
        if (checkForGroundRoutine != null) {
            StopCoroutine(checkForGroundRoutine);
            checkForGroundRoutine = null;
        }
        checkForGroundRoutine = StartCoroutine(CheckForGroundRoutine());
    }
    private void CheckJumpInput()
    {
        //bool grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        //if (grounded && !isGrounded) {
        //    OnHitGround();
        //}

        if (isGrounded && freshJumpInputDown) {
            JumpInputStart();
        }
        if (progressiveJumpActive) {
            if (!jumpInputDown) {
                JumpInputStop();
            }
        }
    }

    private void JumpInputUpdate()
    {
        if (progressiveJumpActive) {

            if (progressJumpTime >= progressiveJumpMaxDuration || !jumpInputDown) {
                progressiveJumpActive = false;
                freshJumpInputDown = false;
            } else {
                rigidbody.AddForce(Vector3.up * progressiveJumpForce, ForceMode.Acceleration);
            }
            progressJumpTime += Time.deltaTime;
        }
    }

    private void JumpInputStop()
    {
        progressJumpTime = 0;
        inAirSpeedMultiplier = 1;
        //DOTween.To(() => inAirSpeedMultiplier, x => inAirSpeedMultiplier = x, .5f, .5f);
    }

    private void OnHitGround()
    {
        characterAnimator.SetBool("InAir", false);
        isGrounded = true;
    }
    #endregion

    #region Hanging
    [ButtonGroup("Hanging/Controls"), Button("Hang")]
    private void StartHang()
    {
        isHanging = true;
        characterAnimator.SetTrigger("Hang");
        rotateMode = CameraRotateMode.Locked;
        rigidbody.isKinematic = true;
    }

    [ButtonGroup("Hanging/Controls"), Button("Jump")]
    private void JumpFromHang()
    {
        StartCoroutine(JumpFromHangRoutine());
    }

    private IEnumerator JumpFromHangRoutine()
    {
        isHanging = false;
        characterAnimator.SetTrigger("Climb");
        rigidbody.isKinematic = false;
        rigidbody.AddForce(Vector3.up * hangKipForce, ForceMode.Impulse);
        edgeChecker.OnLeaveEdge();
        yield return new WaitForSeconds(.2f);
        Debug.Log("Fwd");
        rigidbody.AddForce(transform.forward * (hangKipForce * .25f), ForceMode.Impulse);
        rotateMode = CameraRotateMode.FollowY;
    }

    [ButtonGroup("Hanging/Controls"), Button("Drop")]
    private void DropFromHang()
    {
        isHanging = false;
        characterAnimator.SetTrigger("Drop");
        rigidbody.isKinematic = false;
        rigidbody.AddForce(-transform.forward * (hangKipForce / 2), ForceMode.Impulse);
        edgeChecker.OnLeaveEdge();
        rotateMode = CameraRotateMode.FollowY;
    }

    private void AssignHangingCallbacks()
    {
        edgeChecker.OnGrabbedEdge += StartHang;
    }
    #endregion

    #region Flight
    private void CheckFlightModeInput()
    {
        if (Input.GetKeyDown(KeyCode.F)) {
            if (movementMode == MovementMode.WalkRun || movementMode == MovementMode.Hover) {
                BeginFlightMode();
            } else if (movementMode == MovementMode.Flight) {
                EndFlightMode();
            }
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            if (movementMode == MovementMode.WalkRun) {
                BeginHoverMode();
            } else if (movementMode == MovementMode.Hover) {
                EndHoverMode();
            }
        }
    }

    private void FlightModeUpdate()
    {
        Vector3 input = new Vector3(HorizontalInput, 0, VerticalInput).normalized;
        characterAnimator.SetXYInput(HorizontalInput, VerticalInput);
        Vector3 flyForce = new Vector3(input.x * baseFlightSpeed.x, 0, input.z * baseFlightSpeed.y);
        flyForce = transform.TransformDirection(flyForce) * flightSpeedMultiplier;
        if (SprintInput) {
            flyForce *= flightSprintMultiplier;
        }
        rigidbody.AddForce(flyForce, ForceMode.Acceleration);
    }

    [ButtonGroup("Flight/Controls1"), Button("Start Hover")]
    private void BeginHoverMode()
    {
        rotateMode = CameraRotateMode.FollowY;
        movementMode = MovementMode.Hover;
        characterAnimator.SetBool("Flying", true);
        rigidbody.linearDamping = hoverDrag;
    }

    [ButtonGroup("Flight/Controls1"), Button("End Hover")]
    private void EndHoverMode()
    {
        rotateMode = CameraRotateMode.FollowY;
        movementMode = MovementMode.WalkRun;
        characterAnimator.SetBool("Flying", false);
        rigidbody.linearDamping = defaultDrag;
    }

    [ButtonGroup("Flight/Controls2"), Button("Start Flight")]
    private void BeginFlightMode()
    {
        rotateMode = CameraRotateMode.FollowAll;
        movementMode = MovementMode.Flight;
        characterAnimator.SetBool("Flying", true);
        rigidbody.useGravity = false;
        rigidbody.linearDamping = flightDrag;
    }

    [ButtonGroup("Flight/Controls2"), Button("End Flight")]
    private void EndFlightMode()
    {
        rotateMode = CameraRotateMode.FollowY;
        movementMode = MovementMode.WalkRun;
        characterAnimator.SetBool("Flying", false);
        rigidbody.useGravity = true;
        rigidbody.linearDamping = defaultDrag;
    }
    #endregion

    private void ManageRotation()
    {
        if (rotateMode == CameraRotateMode.FollowY || rotateMode == CameraRotateMode.FollowAll) {
            Vector3 forward = playerCamera.transform.TransformDirection(Vector3.forward);
            if (rotateMode == CameraRotateMode.FollowY) {
                forward.y = 0.0f;
            }
            forward = forward.normalized;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    private void HandleMovementInput()
    {
        Vector2 input = new Vector2(HorizontalInput, VerticalInput);
        characterAnimator.SetXYInput(HorizontalInput, VerticalInput);
        bool isWalking = input.magnitude > 0;
        characterAnimator.SetBool("Sprinting", SprintInput);
        Vector3 runForce = new Vector3(HorizontalInput, 0, VerticalInput).normalized;
        runForce.x *= baseGroundSpeed.x;
        runForce.z *= baseGroundSpeed.y;
        if (movementMode == MovementMode.Hover) {
            runForce = transform.TransformDirection(runForce) * hoverSpeedMultiplier;
        } else {
            if (!isGrounded) {
                runForce *= inAirSpeedMultiplier;
            } else {
                runForce.y = -customRunGravity;
                if (SprintInput) {
                    runForce.x *= sprintModifier.x;
                    runForce.z *= sprintModifier.y;
                }
            }
            runForce = transform.TransformDirection(runForce) * groundSpeedMultiplier;
        }

        if (isWalking || movementMode == MovementMode.Hover) {
            rigidbody.AddForce(runForce, ForceMode.Acceleration);
        } else {
            if (!rigidbody.isKinematic) {
                Vector3 smoothVelocity = new Vector3(rigidbody.linearVelocity.x * (1 - speedDamp), rigidbody.linearVelocity.y, rigidbody.linearVelocity.z * (1 - speedDamp));
                rigidbody.linearVelocity = smoothVelocity;
            }
        }
    }

    private void HandleJumpInput()
    {
        if (!freshJumpInputDown) {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
                Debug.Log("Jump Input Down Detected");
                freshJumpInputDown = true;
            }
        }

        if (!jumpInputDown) {
            if (JumpInput && isGrounded) {
                jumpInputDown = true;
            }
        } else {
            if (!JumpInput) {
                Debug.Log("Jump Input Up Detected");
                jumpInputDown = false;
                freshJumpInputDown = false;
            }
        }
    }

    private void HandleWalkRunMovement()
    {
        if (!isHanging) {
            HandleMovementInput();
            CheckJumpInput();
            JumpInputUpdate();
        }
        if (isGrounded) {
            if (!progressiveJumpActive && !IsPhysicallyGrounded) {
                BecomeUngrounded();
            }
        } else {
            rigidbody.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
            if (IsPhysicallyGrounded && !progressiveJumpActive)
                OnHitGround();
        }
    }
    void OnDrawGizmos()
    {
        // Visualize the ray in Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * hangGroundCheckDistance, groundCheckRadius);
    }

    private void Update()
    {
        HandleJumpInput();
        CheckFlightModeInput();
        if (isHanging) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                JumpFromHang();
            }
            if (VerticalInput < 0) {
                DropFromHang();
            }
        }
        edgeChecker.ToggleCheckForEdge(!IsAlmostGrounded && HasNegativeVerticalVelocity);
        if (Input.GetMouseButtonDown(2)) {
            if (rotateMode != CameraRotateMode.Free)
                cachedRotateMode = rotateMode;
            rotateMode = CameraRotateMode.Free;
        }
        if (Input.GetMouseButtonUp(2)) {
            rotateMode = cachedRotateMode;
        }
    }

    private void FixedUpdate()
    {
        ManageRotation();
        if (movementMode == MovementMode.WalkRun || movementMode == MovementMode.Hover) {
            HandleWalkRunMovement();
        } else if (movementMode == MovementMode.Flight) {
            FlightModeUpdate();
        }
    }

    private void Awake()
    {
        if (characterAnimator == null) {
            characterAnimator = GetComponent<CharacterAnimator>();
        }
        movementMode = MovementMode.WalkRun;
        rotateMode = CameraRotateMode.FollowY;
        playerCamera = GetComponentInChildren<Camera>();
        edgeChecker = GetComponentInChildren<MovementEdgeChecker>();
        rigidbody = GetComponentInChildren<Rigidbody>();
        AssignHangingCallbacks();
    }
}
