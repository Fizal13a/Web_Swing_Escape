using System;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;
using System.Collections.Generic;

public class ColyseusClient : MonoBehaviour
{
    private Client client;
    private Room<MyRoomState> room;

    public Camera playerCamera;
    public ThirdPersonCamera cameraController;
    [SerializeField]
    private GameObject playerPrefab;

    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    async void Start()
    {
        try
        {
            client = new Client("ws://localhost:2567");

            room = await client.JoinOrCreate<MyRoomState>("my_room");

            Debug.Log("Joined Colyseus room!");
            Debug.Log("Room ID: " + room.Id);

            AssignCallbacks();
        }
        catch (Exception e)
        {
            Debug.LogError("Colyseus connection failed: " + e);
        }
    }

    private void AssignCallbacks()
    {
        var callbacks = Callbacks.Get(room);

        callbacks.OnAdd(
            state => state.players,
            (sessionId, player) =>
            {
                Debug.Log("Player spawned: " + sessionId);

                GameObject playerObject = Instantiate(playerPrefab);

                NetworkPlayer networkPlayer = playerObject.GetComponent<NetworkPlayer>();
                networkPlayer.Initialize(sessionId, room.SessionId);
                
                PlayerController_New playerControllerNew =
                    playerObject.GetComponentInChildren<PlayerController_New>();

                bool isOwner = networkPlayer.IsOwner;
                players[sessionId] = playerObject;

                if (isOwner)
                {
                    cameraController.SetPlayerTarget(playerControllerNew.transform);
                    playerControllerNew.SetCameraTransform(playerCamera);
                }

                Transform playerTransform =
                    playerObject.GetComponentInChildren<PlayerController_New>().transform;
                
                PlayerAnimationController animationController =
                    playerObject.GetComponentInChildren<PlayerAnimationController>();

                playerTransform.position = new Vector3(
                    player.x,
                    player.y,
                    player.z
                );

                if (!isOwner)
                {
                    callbacks.Listen(player, p => p.x, (value, previous) =>
                    {
                        Vector3 position = playerTransform.position;
                        position.x = value;
                        playerTransform.position = position;
                    });

                    callbacks.Listen(player, p => p.y, (value, previous) =>
                    {
                        Vector3 position = playerTransform.position;
                        position.y = value;
                        playerTransform.position = position;
                    });

                    callbacks.Listen(player, p => p.z, (value, previous) =>
                    {
                        Vector3 position = playerTransform.position;
                        position.z = value;
                        playerTransform.position = position;
                    });

                    callbacks.Listen(player, p => p.rotX, (value, previous) =>
                    {
                        Vector3 rotation = playerTransform.eulerAngles;
                        rotation.x = value;
                        playerTransform.eulerAngles = rotation;
                    });

                    callbacks.Listen(player, p => p.rotY, (value, previous) =>
                    {
                        Vector3 rotation = playerTransform.eulerAngles;
                        rotation.y = value;
                        playerTransform.eulerAngles = rotation;
                    });

                    callbacks.Listen(player, p => p.rotZ, (value, previous) =>
                    {
                        Vector3 rotation = playerTransform.eulerAngles;
                        rotation.z = value;
                        playerTransform.eulerAngles = rotation;
                    });
                    
                    callbacks.Listen(player, p => p.animationState, (value, previous) =>
                    {
                        animationController.SetNetworkAnimationState((int)value);
                    });
                    
                    callbacks.Listen(player, p => p.animationSpeed, (value, previous) =>
                    {
                        animationController.SetAnimationSpeed(value);
                    });
                }
            }
        );
    }
    
    public async void SendMovement(
        Vector3 position,
        Vector3 rotation,
        int animationState,
        float animationSpeed)
    {
        if (room == null)
            return;

        await room.Send("move", new
        {
            x = position.x,
            y = position.y,
            z = position.z,

            rotX = rotation.x,
            rotY = rotation.y,
            rotZ = rotation.z,

            animationState = animationState,
            animationSpeed = animationSpeed
        });
    }

    async void OnDestroy()
    {
        if (room != null)
        {
            await room.Leave();
        }
    }
}