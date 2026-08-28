using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera")]
    [SerializeField] private float distance = 7f;
    [SerializeField] private float lookHeight = 1.5f;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("Vertical Limits")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private PlayerInputActions _inputActions;

    private float _yaw;
    private float _pitch;

    private Vector2 _lookInput;
    private bool _isLooking;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    public void SetPlayerTarget(Transform target)
    {
        this.target = target;
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.PlayerInp.Look.performed += OnLook;
        _inputActions.PlayerInp.Look.canceled += OnLook;

        _inputActions.PlayerInp.LookClick.performed += OnLookPressed;
        _inputActions.PlayerInp.LookClick.canceled += OnLookReleased;
    }

    private void OnDisable()
    {
        _inputActions.PlayerInp.Look.performed -= OnLook;
        _inputActions.PlayerInp.Look.canceled -= OnLook;

        _inputActions.PlayerInp.LookClick.performed -= OnLookPressed;
        _inputActions.PlayerInp.LookClick.canceled -= OnLookReleased;

        _inputActions.Disable();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (_isLooking)
        {
            _yaw += _lookInput.x * mouseSensitivity;
            _pitch -= _lookInput.y * mouseSensitivity;

            _pitch = Mathf.Clamp(
                _pitch,
                minPitch,
                maxPitch
            );
        }

        Quaternion rotation =
            Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 targetPosition =
            target.position +
            Vector3.up * lookHeight;

        transform.SetPositionAndRotation(
            targetPosition -
            rotation * Vector3.forward * distance,
            rotation
        );
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookPressed(InputAction.CallbackContext context)
    {
        _isLooking = true;
    }

    private void OnLookReleased(InputAction.CallbackContext context)
    {
        _isLooking = false;
        _lookInput = Vector2.zero;
    }
}