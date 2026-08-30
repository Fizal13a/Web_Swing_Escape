using System;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;

public class ColyseusClient : MonoBehaviour
{
    private Client client;
    private Room<MyRoomState> room;

    [Header("Camera")]
    public Camera playerCamera;
    public ThirdPersonCamera cameraController;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    private readonly Dictionary<string, GameObject> players =
        new Dictionary<string, GameObject>();

    public bool IsConnected => room != null;


    // =========================================================
    // CONNECTION
    // =========================================================

    private async void Start()
    {
        try
        {
            client = new Client("ws://localhost:2567");

            room = await client.JoinOrCreate<MyRoomState>("my_room");

            Debug.Log("Joined Colyseus room!");
            Debug.Log("Room ID: " + room.Id);
            Debug.Log("Session ID: " + room.SessionId);

            AssignCallbacks();
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Colyseus connection failed: " + e
            );
        }
    }


    // =========================================================
    // CALLBACKS
    // =========================================================

    private void AssignCallbacks()
    {
        var callbacks = Callbacks.Get(room);

        callbacks.OnAdd(
            state => state.players,
            (sessionId, player) =>
            {
                SpawnPlayer(
                    sessionId,
                    player,
                    callbacks
                );
            }
        );

        callbacks.OnRemove(
            state => state.players,
            (sessionId, player) =>
            {
                RemovePlayer(sessionId);
            }
        );
    }


    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnPlayer(
        string sessionId,
        Player player,
        StateCallbackStrategy<MyRoomState> callbacks)
    {
        Debug.Log(
            "Player spawned: " + sessionId
        );

        GameObject playerObject =
            Instantiate(playerPrefab);

        players[sessionId] =
            playerObject;


        NetworkPlayer networkPlayer =
            playerObject.GetComponent<NetworkPlayer>();

        networkPlayer.Initialize(
            sessionId,
            room.SessionId
        );


        PlayerController_New playerController =
            playerObject.GetComponentInChildren<PlayerController_New>();

        PlayerAnimationController animationController =
            playerObject.GetComponentInChildren<PlayerAnimationController>();

        Transform playerTransform =
            playerController.transform;


        // -----------------------------------------------------
        // INITIAL POSITION
        // -----------------------------------------------------

        playerTransform.position =
            new Vector3(
                player.x,
                player.y,
                player.z
            );

        playerTransform.rotation =
            Quaternion.Euler(
                0f,
                player.rotY,
                0f
            );


        // -----------------------------------------------------
        // OWNER
        // -----------------------------------------------------

        if (networkPlayer.IsOwner)
        {
            cameraController.SetPlayerTarget(
                playerTransform
            );

            playerController.SetCameraTransform(
                playerCamera
            );

            return;
        }


        // -----------------------------------------------------
// REMOTE PLAYER
// -----------------------------------------------------

        networkPlayer.SetTargetTransform(
            playerTransform.position,
            playerTransform.rotation
        );

        animationController.SetNetworkAnimationState(
            (int)player.animationState
        );

        animationController.SetAnimationSpeed(
            player.animationSpeed
        );


        // =====================================================
        // POSITION
        // =====================================================

        callbacks.Listen(
            player,
            p => p.x,
            (value, previous) =>
            {
                networkPlayer.SetTargetX(value);
            }
        );

        callbacks.Listen(
            player,
            p => p.y,
            (value, previous) =>
            {
                networkPlayer.SetTargetY(value);
            }
        );

        callbacks.Listen(
            player,
            p => p.z,
            (value, previous) =>
            {
                networkPlayer.SetTargetZ(value);
            }
        );


        // =====================================================
        // ROTATION - Y ONLY
        // =====================================================

        callbacks.Listen(
            player,
            p => p.rotY,
            (value, previous) =>
            {
                networkPlayer.SetTargetRotationY(value);
            }
        );


        // =====================================================
        // ANIMATION
        // =====================================================

        callbacks.Listen(
            player,
            p => p.animationState,
            (value, previous) =>
            {
                animationController.SetNetworkAnimationState(
                    value
                );
            }
        );

        callbacks.Listen(
            player,
            p => p.animationSpeed,
            (value, previous) =>
            {
                animationController.SetAnimationSpeed(
                    value
                );
            }
        );
    }


    // =========================================================
    // REMOVE PLAYER
    // =========================================================

    private void RemovePlayer(string sessionId)
    {
        if (!players.TryGetValue(
                sessionId,
                out GameObject playerObject))
        {
            return;
        }

        Destroy(playerObject);

        players.Remove(sessionId);
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    public async void SendMovement(
        Vector3 position,
        float rotationY)
    {
        if (room == null)
            return;

        await room.Send(
            "move",
            new
            {
                x = position.x,
                y = position.y,
                z = position.z,

                rotY = rotationY
            }
        );
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    public async void SendAnimation(
        int animationState,
        float animationSpeed)
    {
        if (room == null)
            return;

        await room.Send(
            "animation",
            new
            {
                state = animationState,
                speed = animationSpeed
            }
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private async void OnDestroy()
    {
        if (room != null)
        {
            await room.Leave();
        }
    }
}