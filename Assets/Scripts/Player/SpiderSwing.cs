using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpiderSwing : MonoBehaviour
{
    [Header("Swing Shape")]
    [SerializeField] private float swingDuration = 1f;    
    [SerializeField] private float swingDistance = 8f;    
    [SerializeField] private float swingDipHeight = 2.5f; 
    [SerializeField] private float startLiftHeight = 1f;  

    [Header("Web Visual")]
    [SerializeField] private LineRenderer webLine;    
    [SerializeField] private Transform webOrigin;    
    [SerializeField] private float webHeight = 15f;  

    private PlayerController_New _player;
    private CharacterController _characterController;
    private Transform _transform;

    private bool isSwinging;
    private float swingTimer;
    private Vector3 swingStartPos;
    private Vector3 swingDirection;

    private void Awake()
    {
        _player = GetComponent<PlayerController_New>();
        _characterController = GetComponent<CharacterController>();
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

    private void OnJumpPressedWhileAirborne()
    {
        if (isSwinging)
            BreakSwing();
        else
            StartSwing();
    }

    private void Update()
    {
        if (!isSwinging)
            return;

        swingTimer += Time.deltaTime;
        float t = swingTimer / swingDuration;

        if (t >= 1f)
        {
            // snap to the exact end point, then release
            MoveTo(swingStartPos + swingDirection * swingDistance);
            EndSwing();
            return;
        }

        ApplySwingAtT(t);
        UpdateWebLine();
    }

    private void StartSwing()
    {
        isSwinging = true;
        swingTimer = 0f;

        // hop up first so the "U" dip has room to sag without clipping into the ground
        MoveTo(_transform.position + Vector3.up * startLiftHeight);
        swingStartPos = _transform.position;

        Vector3 facing = _transform.forward;
        facing.y = 0f;
        swingDirection = facing.sqrMagnitude > 0.0001f ? facing.normalized : _transform.forward;

        _player.IsExternallyControlled = true;

        if (webLine != null)
            webLine.enabled = true;

        UpdateWebLine();
    }

    private void BreakSwing()
    {
        float t = Mathf.Clamp01(swingTimer / swingDuration);
        float verticalVelocity = -swingDipHeight * Mathf.PI / swingDuration * Mathf.Cos(Mathf.PI * t);

        EndSwing(verticalVelocity);
    }

    private void EndSwing(float exitVerticalVelocity = 0f)
    {
        isSwinging = false;
        _player.IsExternallyControlled = false;
        _player.SetVerticalVelocity(exitVerticalVelocity);

        if (webLine != null)
            webLine.enabled = false;
    }

    private void UpdateWebLine()
    {
        if (webLine == null)
            return;

        Vector3 origin = webOrigin.position;
        webLine.SetPosition(0, origin);
        webLine.SetPosition(1, origin + Vector3.up * webHeight);
    }

    private void ApplySwingAtT(float t)
    {
        // horizontal: straight line in the facing direction
        Vector3 horizontal = swingStartPos + swingDirection * (swingDistance * t);

        // vertical: "U" shape — 0 at start, -dipHeight at the middle, back to 0 at the end
        float verticalOffset = -swingDipHeight * Mathf.Sin(Mathf.PI * t);

        Vector3 target = horizontal + Vector3.up * verticalOffset;
        MoveTo(target);
    }

    private void MoveTo(Vector3 worldPosition)
    {
        _characterController.Move(worldPosition - _transform.position);
    }
}