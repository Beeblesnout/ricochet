using Popcron.Networking;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUser : NetUser
{
    public int teamID;

    public static PlayerUser Local { get; private set; }
    public GameObject playerPrefab;
    public GameObject characterPrefab;
    public GameObject player { get; private set; }
    GunController playerGun;
    public bool isAlive = false;
    private bool lastAlive;
    private Vector3 lastPosition;
    private Vector2 lastEulerAngles;
    private bool lastShootState;
    private Vector3 playerPosition;
    private Vector2 playerEulerAngles;
    private bool shooting;
    private int lastGunID;
    private int lastTeamID;

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

    void Start()
    {
        teamID = UnityEngine.Random.Range(1, 3);
    }

    private async void OnPlayerConnected(NetConnection connection)
    {
        await Task.Delay(50);

        //send alive state
        Message message = new Message(NetMessageType.PlayerAliveState);
        message.Write(ConnectID);
        message.Write(isAlive);
        message.Send(connection);

        if (player)
        {
            //send position
            message = new Message(NetMessageType.PlayerPosition);
            message.Write(ConnectID);
            message.Write(player.transform.position.x);
            message.Write(player.transform.position.y);
            message.Write(player.transform.position.z);
            message.Send(connection);
            
            //send rotation
            message = new Message(NetMessageType.PlayerEulerAngles);
            message.Write(ConnectID);
            message.Write(player.transform.eulerAngles.x);
            message.Write(player.transform.eulerAngles.y);
            message.Send(connection);

            //send gun
            message = new Message(NetMessageType.PlayerGunID);
            message.Write(ConnectID);
            message.Write(playerGun.GetGunID());
            message.Send(connection);

            //send team
            message = new Message(NetMessageType.PlayerTeam);
            message.Write(ConnectID);
            message.Write(teamID);
            message.Send();
        }
    }

    private void OnMessage(Message message)
    {
        NetMessageType type = (NetMessageType)message.Type;
        if (type == NetMessageType.PlayerAliveState)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                isAlive = message.Read<bool>();
            }
        }
        else if (type == NetMessageType.PlayerPosition)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                float x = message.Read<float>();
                float y = message.Read<float>();
                float z = message.Read<float>();
                playerPosition = new Vector3(x, y, z);
            }
        }
        else if (type == NetMessageType.PlayerEulerAngles)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                float x = message.Read<float>();
                float y = message.Read<float>();
                playerEulerAngles = new Vector2(x, y);
            }
        }
        else if (type == NetMessageType.PlayerShootState)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                shooting = message.Read<bool>();
            }
        }
        else if (type == NetMessageType.PlayerGunID)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                playerGun?.SetGun(message.Read<int>());
            }
        }
        else if (type == NetMessageType.PlayerTeam)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                teamID = message.Read<int>();
            }
        }
    }

    private void Update()
    {
        if (IsMine)
        {
            Local = this;
            isAlive = true;
            if (isAlive != lastAlive)
            {
                lastAlive = !lastAlive;

                Message message = new Message(NetMessageType.PlayerAliveState);
                message.Write(ConnectID);
                message.Write(isAlive);
                message.Send();
            }

            //player is alive so send player data
            if (player)
            {
                //sync position
                if  (player.transform.position != lastPosition)
                {
                    lastPosition = player.transform.position;
                    Message message = new Message(NetMessageType.PlayerPosition);
                    message.Write(ConnectID);
                    message.Write(lastPosition.x);
                    message.Write(lastPosition.y);
                    message.Write(lastPosition.z);
                    message.Send();
                }

                //sync rotation
                if  (Camera.main.transform.eulerAngles != (Vector3)lastEulerAngles)
                {
                    lastEulerAngles = Camera.main.transform.eulerAngles;
                    Message message = new Message(NetMessageType.PlayerEulerAngles);
                    message.Write(ConnectID);
                    message.Write(lastEulerAngles.x);
                    message.Write(lastEulerAngles.y);
                    message.Send();
                }

                //sync shoot
                if (playerGun.shooting != lastShootState)
                {
                    lastShootState = playerGun.shooting;
                    Message message = new Message(NetMessageType.PlayerShootState);
                    message.Write(ConnectID);
                    message.Write(lastShootState);
                    message.Send();
                }

                //sync gun
                if (playerGun.GetGunID() != lastGunID)
                {
                    lastGunID = playerGun.GetGunID();
                    Message message = new Message(NetMessageType.PlayerGunID);
                    message.Write(ConnectID);
                    message.Write(lastGunID);
                    message.Send();
                }

                //sync team
                if (teamID != lastTeamID)
                {
                    lastTeamID = teamID;
                    Message message = new Message(NetMessageType.PlayerTeam);
                    message.Write(ConnectID);
                    message.Write(teamID);
                    message.Send();
                }
            }
        }
        else
        {
            if (player)
            {
                playerGun.shooting = shooting;
                player.transform.position = playerPosition;
                player.transform.eulerAngles = new Vector3(0, playerEulerAngles.y, 0);
                player.transform.GetChild(1).localEulerAngles = new Vector3(playerEulerAngles.x, 0, 0);
            }
        }

        if (player == null && isAlive)
        {
            if (IsMine)
            {
                player = Instantiate(playerPrefab);
                player.GetComponent<Player>().user = this;
                player.transform.position = LevelManager.Instance.GetRandomSpawnLoc(teamID);
            }
            else
            {
                player = Instantiate(characterPrefab);
            }
            player.GetComponent<Health>().onDeath.AddListener(KillPlayer);
            playerGun = player.GetComponentInChildren<GunController>();
            UIManager.Instance.LinkUIElements(player.GetComponent<CharacterMotion>());
        }
        else if (player != null && !isAlive)
        {
            Destroy(player);
        }
    }

    private async void KillPlayer()
    {
        await Task.Delay(1000);
        Destroy(player);
    }
    
    public void JoinTeam(int newTeam)
    {
        teamID = newTeam;
        if (player) player.transform.position = LevelManager.Instance.GetRandomSpawnLoc(newTeam);
    }
}
