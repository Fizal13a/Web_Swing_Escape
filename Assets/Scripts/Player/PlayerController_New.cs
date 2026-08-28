using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_New : MonoBehaviour
{
    #region Fields

    private CharacterController _characterController;
    private PlayerInputActions _playerInputActions;
    public PlayerUIController _playerUIController;
    public StepPopupController _stepPopupController;
    private Transform _transform;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private Transform cameraTransform;

    [Header("Network")]
    public NetworkPlayer networkPlayer;
    private ColyseusClient colyseusClient;

    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float currentSpeed = 5f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float rotateSpeed = 720f;
    [SerializeField] private float animationReferenceSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float terminalVelocity = 25f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Swing")]
    private int maxSwings = 4;
    private int currentSwings = 0;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Stats")]
    private int trophyCount;

    private bool isGrounded;
    private bool jumpHeld;
    private bool isMoving = false;
    private bool onTreadmill;

    private float smoothedSpeed;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private Vector2 movementInput;
    private Vector3 currentMoveDir;
    private float verticalVelocity;

    #endregion

    #region Properties & Events

    public float CurrentSpeed
    {
        get => currentSpeed;
        set => currentSpeed = Mathf.Clamp(value, 0f, maxMoveSpeed);
    }
    public bool IsGrounded => isGrounded;

    // While true, this script's Update does nothing — SpiderSwing owns the CharacterController.
    public bool IsExternallyControlled { get; set; }
    public event Action JumpPressedWhileAirborne;

    // Lets an external controller (SpiderSwing) hand back a vertical speed when it releases control,
    // so gravity picks up smoothly instead of snapping to 0.
    public void SetVerticalVelocity(float v) => verticalVelocity = v;

    #endregion

    #region Initialize

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        _playerInputActions = new PlayerInputActions();
        colyseusClient = FindFirstObjectByType<ColyseusClient>();

        currentSpeed = Mathf.Min(currentSpeed, maxMoveSpeed);
        currentSwings = maxSwings;
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();

        _playerInputActions.PlayerInp.Move.performed += OnMove;
        _playerInputActions.PlayerInp.Move.canceled += OnMove;

        _playerInputActions.PlayerInp.Jump.performed += OnJumpPressed;
        _playerInputActions.PlayerInp.Jump.canceled += OnJumpReleased;
    }

    private void OnDisable()
    {
        _playerInputActions.PlayerInp.Move.performed -= OnMove;
        _playerInputActions.PlayerInp.Move.canceled -= OnMove;

        _playerInputActions.PlayerInp.Jump.performed -= OnJumpPressed;
        _playerInputActions.PlayerInp.Jump.canceled -= OnJumpReleased;

        _playerInputActions.Disable();
    }

    #endregion

    #region Input Actions

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!networkPlayer.IsOwner)
            return;

        movementInput = context.ReadValue<Vector2>();
        isMoving = movementInput.sqrMagnitude > 0;

        if (!onTreadmill)
            animationController.SetRun(isMoving);
        else
            animationController.SetRun(true);
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!networkPlayer.IsOwner)
            return;

        jumpHeld = true;

        if (!isGrounded && currentSwings > 0)
        {
            currentSwings -= 1;
            JumpPressedWhileAirborne?.Invoke();
            _playerUIController.ToggleSwingText(true);
            _playerUIController.OnSwing(currentSwings, maxSwings);
            return;
        }

        jumpBufferCounter = jumpBufferTime; // remember the press even if we're not grounded yet
    }

    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        if (!networkPlayer.IsOwner)
            return;

        jumpHeld = false;
    }

    #endregion

    #region Public API

    public void SetTreadmillState(bool value)
    {
        onTreadmill = value;
        animationController.SetRun(value);
    }

    public void OnLevelUp(int speedIncrement)
    {
        maxMoveSpeed += speedIncrement;
        currentSpeed = maxMoveSpeed;

        _playerUIController.OnMaxSpeedChanged((int)maxMoveSpeed);
    }

    public void TeleportTo(Vector3 position)
    {
        CharacterController controller = GetComponent<CharacterController>();

        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public float GetMaxSpeed()
    {
        return maxMoveSpeed;
    }

    public void SetCurrentSpeed(int speed)
    {
        currentSpeed = speed;
    }

    public int GetTrophyCount()
    {
        return trophyCount;
    }

    public void SetTrophyCount(int count)
    {
        trophyCount += count;
        _playerUIController.OnTrophyCountChanged(trophyCount);
    }

    public void SetCameraTransform(Camera cameraTransform)
    {
        this.cameraTransform = cameraTransform.transform;
        _stepPopupController.SetCamera(cameraTransform);
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (!networkPlayer.IsOwner)
            return;

        if (IsExternallyControlled)
            return; // SpiderSwing is driving the CharacterController this frame

        float dt = Time.deltaTime;

        CheckGround();
        HandleTimers(dt);
        HandleJump();
        HandleMovement(dt);
        HandleGravity(dt);
    }

    private void HandleTimers(float dt)
    {
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - dt;
        jumpBufferCounter -= dt;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f || coyoteCounter <= 0f)
            return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // consume both so we don't double-jump next frame
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    private void HandleMovement(float dt)
    {
        if (cameraTransform == null) return;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 targetDir =
            cameraForward * movementInput.y +
            cameraRight * movementInput.x;

        if (targetDir.sqrMagnitude > 1f)
            targetDir.Normalize();

        float targetSpeed = targetDir.magnitude * currentSpeed;

        float speedRate = targetSpeed > 0.01f
            ? acceleration
            : deceleration;

        smoothedSpeed = Mathf.MoveTowards(
            smoothedSpeed,
            targetSpeed,
            speedRate * dt
        );

        if (targetDir.sqrMagnitude > 0.0001f)
        {
            currentMoveDir = targetDir;
            RotateTowards(currentMoveDir, dt);
        }

        Vector3 velocity = currentMoveDir * smoothedSpeed;
        velocity.y = verticalVelocity;

        _characterController.Move(velocity * dt);

        // Animation speed based on actual movement speed
        float animationSpeed = smoothedSpeed / animationReferenceSpeed;

        if (onTreadmill)
            animationSpeed = 1;

        animationController.SetRunSpeed(animationSpeed);

        colyseusClient?.SendMovement(
            transform.position,
            transform.eulerAngles,
            animationController.CurrentAnimationState,
            animationController.CurrentAnimationSpeed
        );
    }

    private void HandleGravity(float dt)
    {
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            return;
        }

        // asymmetric gravity: normal rise, heavier fall = responsive arc
        float multiplier = 1f;
        if (verticalVelocity < 0f)
            multiplier = fallGravityMultiplier;
        else if (verticalVelocity > 0f && !jumpHeld)
            multiplier = lowJumpMultiplier; // tap jump = short hop

        verticalVelocity += gravity * multiplier * dt;
        verticalVelocity = Mathf.Max(verticalVelocity, -terminalVelocity);
    }

    private void RotateTowards(Vector3 direction, float dt)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation,
            targetRotation,
            rotateSpeed * dt
        );
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded)
        {
            animationController.SetAir(false);

            currentSwings = maxSwings;
            _playerUIController.ToggleSwingText(false);
        }
        else
        {
            animationController.SetAir(true);
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    #endregion
}