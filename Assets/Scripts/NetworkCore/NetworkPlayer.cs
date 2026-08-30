using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public string SessionId { get; private set; }
    public bool IsOwner { get; private set; }

    [Header("Interpolation")]
    [SerializeField] private float positionSharpness = 15f;
    [SerializeField] private float rotationSharpness = 15f;

    private Transform playerTransform;

    private Vector3 targetPosition;
    private float targetRotationY;

    public void Initialize(string sessionId, string localSessionId)
    {
        SessionId = sessionId;
        IsOwner = sessionId == localSessionId;

        PlayerController_New playerController =
            GetComponentInChildren<PlayerController_New>();

        if (playerController != null)
        {
            playerTransform = playerController.transform;

            targetPosition = playerTransform.position;
            targetRotationY = playerTransform.eulerAngles.y;
        }
    }

    public void SetTargetTransform(
        Vector3 position,
        Quaternion rotation)
    {
        targetPosition = position;
        targetRotationY = rotation.eulerAngles.y;
    }

    public void SetTargetX(float value)
    {
        targetPosition.x = value;
    }

    public void SetTargetY(float value)
    {
        targetPosition.y = value;
    }

    public void SetTargetZ(float value)
    {
        targetPosition.z = value;
    }

    public void SetTargetRotationY(float value)
    {
        targetRotationY = value;
    }

    private void LateUpdate()
    {
        if (IsOwner || playerTransform == null)
            return;

        float positionT =
            1f - Mathf.Exp(-positionSharpness * Time.deltaTime);

        float rotationT =
            1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);

        playerTransform.position = Vector3.Lerp(
            playerTransform.position,
            targetPosition,
            positionT
        );

        Quaternion targetRotation =
            Quaternion.Euler(0f, targetRotationY, 0f);

        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            targetRotation,
            rotationT
        );
    }
}