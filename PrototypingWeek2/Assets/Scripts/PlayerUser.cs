using Popcron.Networking;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUser : NetUser
{
    public static PlayerUser Local { get; private set; }

    [Header("Avatar Prefabs")]
    [SerializeField]
    private GameObject avatarPrefab;
    [SerializeField]
    private GameObject dummyAvatarPrefab;

    // Avatar References
    public GameObject Avatar { get; private set; }
    private CharacterMotion p_avatarMotion;
    public CharacterMotion AvatarMotion {
        get
        {
            if (p_avatarMotion == null) p_avatarMotion = Avatar.GetComponent<CharacterMotion>();
            return p_avatarMotion;
        }
        private set {
            p_avatarMotion = value;
        }
    }
    private GunController p_avatarGun;
    public GunController AvatarGun {
        get
        {
            if (p_avatarGun == null) p_avatarGun = Avatar.GetComponentInChildren<GunController>();
            return p_avatarGun;
        }
        private set {
            p_avatarGun = value;
        }
    }
    private Health p_avatarHealth;
    public Health AvatarHealth {
        get
        {
            if (p_avatarHealth == null) p_avatarHealth = Avatar.GetComponentInChildren<Health>();
            return p_avatarHealth;
        }
        private set {
            p_avatarHealth = value;
        }
    }
    
    [Header("User Params")]
    public bool isAlive = false;
    private bool lastAlive;
    public Vector3 avatarPosition;
    private Vector3 lastPosition;
    public Vector2 avatarEulerAngles;
    private Vector2 lastEulerAngles;
    public bool shooting;
    private bool lastShootState;
    private int lastGunID;
    public int teamID;
    private int lastTeamID;
    public bool hasFlag;
    private bool lastHasFlag;

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

    void Awake()
    {
        teamID = UnityEngine.Random.Range(1, 3);
        if (IsMine) Local = this;
    }

    private async void OnPlayerConnected(NetConnection connection)
    {
        Message message = new Message(NMType.LevelSend);
        message.Write(LevelManager.Instance.loadedLevel);
        message.Send();

        await Task.Delay(50);

        //send alive state
        message = new Message(NMType.PlayerAliveState);
        message.Write(ConnectID);
        message.Write(isAlive);
        message.Send(connection);

        if (Avatar)
        {
            //send position
            message = new Message(NMType.PlayerPosition);
            message.Write(ConnectID);
            message.Write(Avatar.transform.position.x);
            message.Write(Avatar.transform.position.y);
            message.Write(Avatar.transform.position.z);
            message.Send(connection);
            
            //send rotation
            message = new Message(NMType.PlayerEulerAngles);
            message.Write(ConnectID);
            message.Write(Avatar.transform.eulerAngles.x);
            message.Write(Avatar.transform.eulerAngles.y);
            message.Send(connection);

            //send gun
            message = new Message(NMType.PlayerGunID);
            message.Write(ConnectID);
            message.Write(AvatarGun.GetGunID());
            message.Send(connection);

            //send team
            message = new Message(NMType.PlayerTeam);
            message.Write(ConnectID);
            message.Write(teamID);
            message.Send();

            //send has flag
            message = new Message(NMType.HasFlag);
            message.Write(ConnectID);
            message.Write(hasFlag);
            message.Send();
        }
    }

    private void OnMessage(Message message)
    {
        if (IsMine) return;

        NMType type = (NMType)message.Type;
        if (type == NMType.PlayerAliveState)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                isAlive = message.Read<bool>();
            }
        }
        else if (type == NMType.PlayerPosition)
        {
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
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                shooting = message.Read<bool>();
            }
        }
        else if (type == NMType.PlayerGunID)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                AvatarGun.SetGun(message.Read<int>());
            }
        }
        else if (type == NMType.PlayerTeam)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                teamID = message.Read<int>();
            }
        }
        else if (type == NMType.DeathEvent)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                AvatarHealth.Kill();
            }
        }
        else if (type == NMType.HasFlag)
        {
            message.Rewind();
            if (message.Read<long>() == ConnectID)
            {
                hasFlag = message.Read<bool>();
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

                Message message = new Message(NMType.PlayerAliveState);
                message.Write(ConnectID);
                message.Write(isAlive);
                message.Send();
            }

            //player is alive so send player data
            if (Avatar)
            {
                //sync position
                if  (Avatar.transform.position != lastPosition)
                {
                    lastPosition = Avatar.transform.position;
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
                if (AvatarGun.shooting != lastShootState)
                {
                    lastShootState = AvatarGun.shooting;
                    Message message = new Message(NMType.PlayerShootState);
                    message.Write(ConnectID);
                    message.Write(lastShootState);
                    message.Send();
                }

                //sync gun
                if (AvatarGun.GetGunID() != lastGunID)
                {
                    lastGunID = AvatarGun.GetGunID();
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

                //sync has flag
                if (hasFlag != lastHasFlag)
                {
                    lastHasFlag = hasFlag;
                    Message message = new Message(NMType.HasFlag);
                    message.Write(ConnectID);
                    message.Write(hasFlag);
                    message.Send();
                }
            }
        }
        else
        {
            if (Avatar)
            {
                // set position
                Avatar.transform.position = avatarPosition;
                // set rotation
                Avatar.transform.eulerAngles = new Vector3(0, avatarEulerAngles.y, 0);
                Avatar.transform.GetChild(0).localEulerAngles = new Vector3(avatarEulerAngles.x, 0, 0);
                // set shooting
                AvatarGun.shooting = shooting;
            }
        }
        
        if (Avatar == null && isAlive)
        {
            if (IsMine)
            {
                SpawnAvatar();
            }
            else
            {
                Avatar = Instantiate(dummyAvatarPrefab);
            }
            Avatar.GetComponent<Health>().onDeath.AddListener(Kill);
        }
        else if (Avatar != null && !isAlive)
        {
            Destroy(Avatar);
            LevelManager.Instance.players.Remove(Avatar.transform);
        }
    }

    public void SpawnAvatar()
    {
        Avatar = Instantiate(avatarPrefab);
        Avatar.GetComponent<Player>().user = this;
        AvatarMotion = Avatar.GetComponent<CharacterMotion>();
        AvatarMotion.teamID = teamID;
        AvatarGun = Avatar.GetComponentInChildren<GunController>();
        AvatarHealth = Avatar.GetComponent<Health>();
        AvatarHealth.onDeath.AddListener(Kill);
        UIManager.Instance.LinkUIElements();
        LevelManager.Instance.players.Add(Avatar.transform);
        Respawn();
    }

    private async void Kill()
    {
        if (!IsMine)
        {
            Message message = new Message(NMType.DeathEvent);
            message.Write(ConnectID);
            message.Send();
        }

        isAlive = false;
        await Task.Delay(3000);
        isAlive = true;
    }
    
    public void JoinTeam(int newTeam)
    {
        teamID = newTeam;
        AvatarMotion.teamID = teamID;
        Respawn();
    }

    public void Respawn()
    {
        Avatar.transform.position = LevelManager.Instance.GetRandomSpawnLoc(teamID);
        isAlive = true;
    }
}
