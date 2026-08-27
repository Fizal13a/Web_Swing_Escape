using System;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;
using System.Collections.Generic;

public class ColyseusClient : MonoBehaviour
{
    private Client client;
    private Room<MyRoomState> room;

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
                bool isOwner = networkPlayer.IsOwner;
                players[sessionId] = playerObject;

                playerObject.transform.position = new Vector3(
                    player.x,
                    player.y,
                    player.z
                );

                if (!isOwner)
                {
                    callbacks.Listen(player, p => p.x, (value, previous) =>
                    {
                        playerObject.transform.position = new Vector3(
                            value,
                            playerObject.transform.position.y,
                            playerObject.transform.position.z
                        );
                    });

                    callbacks.Listen(player, p => p.y, (value, previous) =>
                    {
                        playerObject.transform.position = new Vector3(
                            playerObject.transform.position.x,
                            value,
                            playerObject.transform.position.z
                        );
                    });

                    callbacks.Listen(player, p => p.z, (value, previous) =>
                    {
                        playerObject.transform.position = new Vector3(
                            playerObject.transform.position.x,
                            playerObject.transform.position.y,
                            value
                        );
                    });
                }
            }
        );
    }
    
    public async void SendMovement(Vector3 position, Vector3 rotation)
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
            rotZ = rotation.z
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