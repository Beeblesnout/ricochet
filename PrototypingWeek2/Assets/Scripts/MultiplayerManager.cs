using Popcron.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MultiplayerManager : SingletonBase<MultiplayerManager>
{

    public static Action<Message> onMessage;

    private void OnEnable()
    {
        Net.NetworkReceiveEvent += OnReceive;
    }

    private void OnDisable()
    {
        Net.NetworkReceiveEvent -= OnReceive;
    }

    private void OnReceive(NetConnection connection, Message message)
    {
        if (connection.connectId == Net.LocalConnectionID)
        {
            onMessage?.Invoke(message);
        }
        else
        {
            if (Net.IsServer)
            {
                message.Send();
            }
        }
    }
}

public enum NetMessageType
{
    PlayerAliveState
}
