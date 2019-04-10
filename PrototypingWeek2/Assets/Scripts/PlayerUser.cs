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
    public GameObject avatarPrefab;
    public GameObject dummyAvatarPrefab;
    public GameObject avatar { get; private set; }
    GunController avatarGun;
    public bool isAlive = false;
    private bool lastAlive;
    private Vector3 lastPosition;
    private Vector2 lastEulerAngles;
    private bool lastShootState;
    private Vector3 avatarPosition;
    private Vector2 avatarEulerAngles;
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
        Message message = new Message(NMType.PlayerAliveState);
        message.Write(ConnectID);
        message.Write(isAlive);
        message.Send(connection);

        if (avatar)
        {
            //send position
            message = new Message(NMType.PlayerPosition);
            message.Write(ConnectID);
            message.Write(avatar.transform.position.x);
            message.Write(avatar.transform.position.y);
            message.Write(avatar.transform.position.z);
            message.Send(connection);
            
            //send rotation
            message = new Message(NMType.PlayerEulerAngles);
            message.Write(ConnectID);
            message.Write(avatar.transform.eulerAngles.x);
            message.Write(avatar.transform.eulerAngles.y);
            message.Send(connection);

            //send gun
            message = new Message(NMType.PlayerGunID);
            message.Write(ConnectID);
            message.Write(avatarGun.GetGunID());
            message.Send(connection);

            //send team
            message = new Message(NMType.PlayerTeam);
            message.Write(ConnectID);
            message.Write(teamID);
            message.Send();
        }
    }

    private void OnMessage(Message message)
    {
        NMType type = (NMType)message.Type;
        if (type == NMType.PlayerAliveState)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                isAlive = message.Read<bool>();
            }
        }
        else if (type == NMType.PlayerPosition)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                float x = message.Read<float>();
                float y = message.Read<float>();
                float z = message.Read<float>();
                avatarPosition = new Vector3(x, y, z);
            }
        }
        else if (type == NMType.PlayerEulerAngles)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                float x = message.Read<float>();
                float y = message.Read<float>();
                avatarEulerAngles = new Vector2(x, y);
            }
        }
        else if (type == NMType.PlayerShootState)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                shooting = message.Read<bool>();
            }
        }
        else if (type == NMType.PlayerGunID)
        {
            if (IsMine) return;

            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                avatarGun?.SetGun(message.Read<int>());
            }
        }
        else if (type == NMType.PlayerTeam)
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

                Message message = new Message(NMType.PlayerAliveState);
                message.Write(ConnectID);
                message.Write(isAlive);
                message.Send();
            }

            //player is alive so send player data
            if (avatar)
            {
                //sync position
                if  (avatar.transform.position != lastPosition)
                {
                    lastPosition = avatar.transform.position;
                    Message message = new Message(NMType.PlayerPosition);
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
                    Message message = new Message(NMType.PlayerEulerAngles);
                    message.Write(ConnectID);
                    message.Write(lastEulerAngles.x);
                    message.Write(lastEulerAngles.y);
                    message.Send();
                }

                //sync shoot
                if (avatarGun.shooting != lastShootState)
                {
                    lastShootState = avatarGun.shooting;
                    Message message = new Message(NMType.PlayerShootState);
                    message.Write(ConnectID);
                    message.Write(lastShootState);
                    message.Send();
                }

                //sync gun
                if (avatarGun.GetGunID() != lastGunID)
                {
                    lastGunID = avatarGun.GetGunID();
                    Message message = new Message(NMType.PlayerGunID);
                    message.Write(ConnectID);
                    message.Write(lastGunID);
                    message.Send();
                }

                //sync team
                if (teamID != lastTeamID)
                {
                    lastTeamID = teamID;
                    Message message = new Message(NMType.PlayerTeam);
                    message.Write(ConnectID);
                    message.Write(teamID);
                    message.Send();
                }
            }
        }
        else
        {
            if (avatar)
            {
                // set position
                avatar.transform.position = avatarPosition;
                // set rotation
                avatar.transform.eulerAngles = new Vector3(0, avatarEulerAngles.y, 0);
                avatar.transform.GetChild(1).localEulerAngles = new Vector3(avatarEulerAngles.x, 0, 0);
                // set shooting
                avatarGun.shooting = shooting;
            }
        }

        if (avatar == null && isAlive)
        {
            if (IsMine)
            {
                avatar = Instantiate(avatarPrefab);
                avatar.GetComponent<Player>().user = this;
                avatar.transform.position = LevelManager.Instance.GetRandomSpawnLoc(teamID);
            }
            else
            {
                avatar = Instantiate(dummyAvatarPrefab);
            }
            avatar.GetComponent<Health>().onDeath.AddListener(KillPlayer);
            avatarGun = avatar.GetComponentInChildren<GunController>();
            UIManager.Instance.LinkUIElements(avatar.GetComponent<CharacterMotion>());
        }
        else if (avatar != null && !isAlive)
        {
            Destroy(avatar);
        }
    }

    private async void KillPlayer()
    {
        await Task.Delay(1000);
        Destroy(avatar);
    }
    
    public void JoinTeam(int newTeam)
    {
        teamID = newTeam;
        if (avatar) avatar.transform.position = LevelManager.Instance.GetRandomSpawnLoc(newTeam);
    }
}
