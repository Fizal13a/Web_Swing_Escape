using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpiderSwing : MonoBehaviour
{
    #region Fields

    public NetworkPlayer networkPlayer;
    private ColyseusClient colyseusClient;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Swing Shape")]
    [SerializeField] private float swingDuration = 1f;
    [SerializeField] private float swingDistance = 8f;
    [SerializeField] private float swingDipHeight = 2.5f;

    [Header("Web Visual")]
    [SerializeField] private LineRenderer webLine;
    private Vector3 _webAnchorPosition;
    [SerializeField] private Transform webOrigin;
    [SerializeField] private float webHeight = 15f;

    private PlayerController_New _player;
    private CharacterController _characterController;
    private Transform _transform;

    private bool isSwinging;
    private float swingTimer;
    private Vector3 swingStartPos;
    private Vector3 swingDirection;

    #endregion

    #region Initialize

    private void Awake()
    {
        _player = GetComponent<PlayerController_New>();
        _characterController = GetComponent<CharacterController>();
        colyseusClient = FindFirstObjectByType<ColyseusClient>();

        _transform = transform;

        if (webOrigin == null)
            webOrigin = _transform;

        if (webLine == null)
            webLine = GetComponent<LineRenderer>();

        if (webLine != null)
        {
            webLine.positionCount = 2;
            webLine.enabled = false;
        }
    }

    private void OnEnable()
    {
        _player.JumpPressedWhileAirborne += OnJumpPressedWhileAirborne;
    }

    private void OnDisable()
    {
        _player.JumpPressedWhileAirborne -= OnJumpPressedWhileAirborne;

        if (isSwinging)
            EndSwing();
    }

    #endregion

    #region Input

    private void OnJumpPressedWhileAirborne()
    {
        if (isSwinging)
            BreakSwing();
        else
            StartSwing();
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (!networkPlayer.IsOwner)
            return;

        if (!isSwinging)
            return;

        UpdateSwingDirection();

        swingTimer += Time.deltaTime;

        float t = swingTimer / swingDuration;

        if (t >= 1f)
        {
            ApplySwingAtT(1f);
            EndSwing();
            return;
        }

        ApplySwingAtT(t);
        UpdateWebLine();

        colyseusClient?.SendMovement(
            transform.position,
            transform.eulerAngles.y
        );
    }

    private void UpdateSwingDirection()
    {
        Transform cameraTransform = Camera.main.transform;

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.0001f)
            return;

        swingDirection = cameraForward.normalized;

        // Make player face swing direction
        _transform.rotation = Quaternion.LookRotation(swingDirection);
    }
    
    private void LateUpdate()
    {
        if (networkPlayer.IsOwner)
            return;

        if (!isSwinging)
            return;

        UpdateNetworkWebLine();
    }
    
    private void UpdateNetworkWebLine()
    {
        if (webLine == null || !webLine.enabled)
            return;

        webLine.SetPosition(0, webOrigin.position);

        webLine.SetPosition(
            1,
            _transform.position + Vector3.up * webHeight
        );
    }
    
    public void SetNetworkSwing(bool value)
    {
        if (networkPlayer.IsOwner)
            return;

        isSwinging = value;

        if (webLine != null)
            webLine.enabled = value;

        if (value)
        {
            _webAnchorPosition =
                _transform.position + Vector3.up * webHeight;

            UpdateWebLine();
        }
    }

    #endregion

    #region Swing Lifecycle

    private void StartSwing()
    {
        animationController.SetSwing(true);

        isSwinging = true;
        swingTimer = 0f;
        swingStartPos = _transform.position;

        UpdateSwingDirection();

        // Lock the web attachment point when swing starts.
        _webAnchorPosition =
            _transform.position + Vector3.up * webHeight;

        _player.IsExternallyControlled = true;

        if (webLine != null)
            webLine.enabled = true;

        UpdateWebLine();
    }

    private void BreakSwing()
    {
        animationController.SetSwing(false);
        float t = Mathf.Clamp01(swingTimer / swingDuration);
        float verticalVelocity = swingDipHeight * (Mathf.PI * 0.5f) / swingDuration * Mathf.Sin(t * Mathf.PI * 0.5f);

        EndSwing(verticalVelocity);
    }

    private void EndSwing(float exitVerticalVelocity = 0f)
    {
        animationController.SetSwing(false);

        isSwinging = false;
        _player.IsExternallyControlled = false;
        _player.SetVerticalVelocity(exitVerticalVelocity);

        if (webLine != null)
            webLine.enabled = false;
    }

    #endregion

    #region Movement & Visuals

    private void ApplySwingAtT(float t)
    {
        Vector3 horizontal = swingStartPos + swingDirection * (swingDistance * t);
        float verticalOffset = swingDipHeight * (1f - Mathf.Cos(t * Mathf.PI * 0.5f));
        Vector3 target = horizontal + Vector3.up * verticalOffset;
        MoveTo(target);
    }

    private void MoveTo(Vector3 worldPosition)
    {
        _characterController.Move(worldPosition - _transform.position);
    }

    private void UpdateWebLine()
    {
        if (webLine == null)
            return;

        webLine.SetPosition(0, webOrigin.position);
        webLine.SetPosition(1, _webAnchorPosition);
    }

    #endregion
}