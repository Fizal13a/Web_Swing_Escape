using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ThirdPersonCamera : MonoBehaviour
{
    public NetworkPlayer networkPlayer;
    
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera")]
    [SerializeField] private float distance = 7f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float lookHeight = 1.5f;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("Vertical Limits")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private PlayerInputActions _inputActions;

    private float _yaw;
    private float _pitch;

    private Vector2 _lookInput;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        if (!networkPlayer.IsOwner)
        {
            gameObject.SetActive(false);
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.PlayerInp.Look.performed += OnLook;
        _inputActions.PlayerInp.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        _inputActions.PlayerInp.Look.performed -= OnLook;
        _inputActions.PlayerInp.Look.canceled -= OnLook;

        _inputActions.Disable();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        _yaw += _lookInput.x * mouseSensitivity;
        _pitch -= _lookInput.y * mouseSensitivity;

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 targetPosition = target.position + Vector3.up * lookHeight;

        transform.SetPositionAndRotation(
            targetPosition - rotation * Vector3.forward * distance,
            rotation
        );
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }
}