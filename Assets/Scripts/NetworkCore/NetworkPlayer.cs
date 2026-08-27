using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public string SessionId { get; private set; }
    public bool IsOwner { get; private set; }

    public void Initialize(string sessionId, string localSessionId)
    {
        SessionId = sessionId;
        IsOwner = sessionId == localSessionId;
    }
}