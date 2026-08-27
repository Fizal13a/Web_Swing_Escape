using System;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public string SessionId { get; private set; }
    public bool IsOwner { get; private set; }

    public bool isTesting;

    private void Start()
    {
        if (isTesting)
            IsOwner = true;
    }

    public void Initialize(string sessionId, string localSessionId)
    {
        SessionId = sessionId;
        IsOwner = sessionId == localSessionId;
    }
}