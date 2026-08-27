using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInputActions inputActions;
    private NetworkPlayer networkPlayer;
    private ColyseusClient colyseusClient;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        networkPlayer = GetComponent<NetworkPlayer>();
        colyseusClient = FindFirstObjectByType<ColyseusClient>();
    }

    private void OnEnable()
    {
        inputActions.PlayerInp.Enable();
    }

    private void OnDisable()
    {
        inputActions.PlayerInp.Disable();
    }

    private void Update()
    {
        if (!networkPlayer.IsOwner)
            return;

        Vector2 input = inputActions.PlayerInp.Move.ReadValue<Vector2>();

        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
            transform.forward = direction;

        colyseusClient.SendMovement(
            transform.position,
            transform.eulerAngles
        );
    }
}