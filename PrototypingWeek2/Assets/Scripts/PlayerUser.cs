using Popcron.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUser : NetUser
{
    public GameObject playerPrefab;
    GameObject player;
    public bool isAlive = false;
    private bool lastAlive;

    private void OnEnable()
    {
        MultiplayerManager.onMessage += OnMessage;
        Net.PlayerConnectedEvent += OnPlayerConnected;
    }

    private void OnDisable()
    {
        MultiplayerManager.onMessage -= OnMessage;
        Net.PlayerConnectedEvent -= OnPlayerConnected;
    }

    private void OnPlayerConnected(NetConnection connection)
    {
        Message message = new Message(NetMessageType.PlayerAliveState);
        message.Write(ConnectID);
        message.Write(isAlive);
        message.Send(connection);
    }

    private void OnMessage(Message message)
    {
        NetMessageType type = (NetMessageType)message.Type;
        if (type == NetMessageType.PlayerAliveState)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                isAlive = message.Read<bool>();
            }
        }
    }

    private void Update()
    {
        if (IsMine)
        {
            if (isAlive != lastAlive)
            {
                lastAlive = !lastAlive;

                Message message = new Message(NetMessageType.PlayerAliveState);
                message.Write(ConnectID);
                message.Write(isAlive);
                message.Send();
            }
        }

        if (player == null && isAlive)
        {
            player = Instantiate(playerPrefab);
        }
        else if (player != null && !isAlive)
        {
            Destroy(player);
        }
    }
}
