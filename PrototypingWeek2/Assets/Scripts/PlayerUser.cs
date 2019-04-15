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
    [SerializeField]
    private bool isAlive = false;
    private bool lastAlive;
    [SerializeField]
    private Vector3 avatarPosition;
    private Vector3 lastPosition;
    [SerializeField]
    private Vector2 avatarEulerAngles;
    private Vector2 lastEulerAngles;
    [SerializeField]
    private bool shooting;
    private bool lastShootState;
    [SerializeField]
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
        if (IsMine) Local = this;
        isAlive = true;
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
                AvatarGun.SetGun(message.Read<int>());
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
                Avatar = Instantiate(avatarPrefab);
                Avatar.GetComponent<Player>().user = this;
                AvatarMotion = Avatar.GetComponent<CharacterMotion>();
                AvatarGun = Avatar.GetComponentInChildren<GunController>();
                AvatarHealth = Avatar.GetComponent<Health>();
                AvatarMotion.teamID = teamID;
                Avatar.transform.position = LevelManager.Instance.GetRandomSpawnLoc(teamID);
                isAlive = true;
                UIManager.Instance.LinkUIElements();
            }
            else
            {
                Avatar = Instantiate(dummyAvatarPrefab);
            }
            Avatar.GetComponent<Health>().onDeath.AddListener(KillAvatar);
        }
        else if (Avatar != null && !isAlive)
        {
            Destroy(Avatar);
        }
    }

    public void SpawnAvatar()
    {

    }

    private async void KillAvatar()
    {
        await Task.Delay(1000);
        Destroy(Avatar);
    }
    
    public void JoinTeam(int newTeam)
    {
        teamID = newTeam;
        if (Avatar) Avatar.transform.position = LevelManager.Instance.GetRandomSpawnLoc(newTeam);
    }
}
