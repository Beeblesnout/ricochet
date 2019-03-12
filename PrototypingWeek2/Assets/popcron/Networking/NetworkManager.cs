using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Popcron.Networking.Shared;
using System;
using LiteNetLib;
using LiteNetLib.Utils;
using System.IO;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

namespace Popcron.Networking
{
    public class NetworkManager : MonoBehaviour
    {
        private static bool hotReloaded = true;
        private static NetworkManager instance;
        private static List<NetworkUser> users = null;

        [SerializeField]
        private NetworkUser prefab;

        [SerializeField]
        private List<NetworkConnection> connections = new List<NetworkConnection>();

        [SerializeField]
        private Intercom intercom = new Intercom(PollSource.Editor);

        public static List<NetworkUser> Users
        {
            get
            {
                if (users == null)
                {
                    users = new List<NetworkUser>(FindObjectsOfType<NetworkUser>());
                }

                return users;
            }
        }

        public static List<NetworkConnection> Connections
        {
            get
            {
                if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

                return instance.connections;
            }
        }

        public static void ClearConnections()
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            instance.connections.Clear();

            List<NetworkUser> users = Users;
            for (int i = 0; i < users.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(users[i].gameObject);
                }
                else
                {
                    DestroyImmediate(users[i].gameObject);
                }
            }
            Users.Clear();
        }

        public static void Poll()
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();
            if (!instance) return;

            if (instance.intercom == null) return;

            instance.intercom.Callback = instance.OnProcessData;
            instance.intercom.Poll();
        }

        public static string Path
        {
            get
            {
                string root = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
                string path = System.IO.Path.Combine(root, "Networking", "Executable", "netapp.exe");
                return path;
            }
        }

        public static int ProcessID
        {
            get
            {
                return PlayerPrefs.GetInt(Settings.CurrentlyUniqueID + "_ProcessID");
            }
            set
            {
                PlayerPrefs.SetInt(Settings.CurrentlyUniqueID + "_ProcessID", value);
            }
        }

        private static Process GetProcessWithID(int id)
        {
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].Id == id)
                {
                    return processes[i];
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public static async void Start()
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();
            if (!instance) return;

            Process existingProcess = GetProcessWithID(ProcessID);
            if (existingProcess != null)
            {
                return;
            }

            instance.intercom = new Intercom(PollSource.Editor);
            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.Arguments = Settings.CurrentlyUniqueID.ToString();
            process.Start();

            Debug.Log("[PIPE] PID " + process.Id);
            Debug.Log("[PIPE] Network application started with ID " + Settings.CurrentlyUniqueID);
            ProcessID = process.Id;

            await Task.Delay(50);

            Message message = new Message(PipeMessageType.ApplicationStartedRequest);
            SendToOtherApp(message);
        }

        public static async void Close(bool forcefully = false)
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            int pid = ProcessID;
            if (!forcefully)
            {
                await Task.Delay(1000);
            }

            Message message = new Message(PipeMessageType.ShutdownApplicationRequest);
            SendToOtherApp(message);
            instance.intercom = null;

            Process existingProcess = GetProcessWithID(pid);
            if (existingProcess != null)
            {
                existingProcess.Kill();
            }
            Settings.CurrentlyUniqueID = 0;
        }
#endif

        public static void SendToOtherApp(Message message)
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            //if in editor, send this message to the network app instead
            //otherwise, directly process the request

#if UNITY_EDITOR
            instance.intercom.Send(message);
#else
            Message m = new Message(message.Type, message.Data);            
            instance.OnProcessData(m);
#endif
        }

        public static void AddConnection(long connectId, string host, ushort port, short latency)
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            for (int i = 0; i < instance.connections.Count; i++)
            {
                if (instance.connections[i].connectId == connectId)
                {
                    //already exists, short circuity
                    return;
                }
            }

            NetworkConnection networkConnection = new NetworkConnection(connectId, host, port)
            {
                latency = latency
            };

            instance.connections.Add(networkConnection);

            //create new user
            List<NetworkUser> users = Users;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ConnectID == networkConnection.connectId)
                {
                    return;
                }
            }

            NetworkUser user = Instantiate(instance.prefab);
            user.name = "User " + networkConnection.connectId;
            user.Initialize(networkConnection);
            Users.Add(user);
        }

        public static void RemoveConnection(long connectId)
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            for (int i = 0; i < instance.connections.Count; i++)
            {
                if (instance.connections[i].connectId == connectId)
                {
                    instance.connections.RemoveAt(i);

                    //remove the user
                    List<NetworkUser> users = Users;
                    for (int u = 0; u < users.Count; u++)
                    {
                        if (users[u].ConnectID == connectId)
                        {
                            Destroy(users[u].gameObject);
                            Users.Remove(users[u]);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public static void UpdateLatency(long connectId, short latency)
        {
            if (hotReloaded) instance = FindObjectOfType<NetworkManager>();

            for (int i = 0; i < instance.connections.Count; i++)
            {
                if (instance.connections[i].connectId == connectId)
                {
                    instance.connections[i].latency = latency;
                    return;
                }
            }
        }

        private void OnApplicationQuit()
        {
            Network.Disconnect();
        }

        public void OnEnable()
        {
            hotReloaded = false;
            instance = this;
        }

        public void Awake()
        {
            if (prefab == null)
            {
                throw new Exception("User prefab is null.");
            }

            Network.IsServer = false;
            Network.IsClient = false;
#if !UNITY_EDITOR
            Settings.CurrentlyUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
#endif
        }

        public void Update()
        {
#if UNITY_EDITOR
            Poll();
#endif
        }

        private void OnProcessData(Message message)
        {
            PipeMessageType type = (PipeMessageType)message.Type;
            if (type == PipeMessageType.ServerInitializedEvent)
            {
                if (!Network.IsServer)
                {
                    Network.IsServer = true;
                    Network.LocalNetworkID = 0;

                    //add this connection to the list
                    ClearConnections();
                    AddConnection(0, "", 0, 0);

                    Network.ServerInitializedEvent?.Invoke();
                }
            }
            else if (type == PipeMessageType.ServerInitializedFailedEvent)
            {
                Network.IsServer = false;
                Network.LocalNetworkID = 0;

                Network.ServerInitializedFailedEvent?.Invoke();
                ClearConnections();
            }
            else if (type == PipeMessageType.ClientClosedVoluntarilyEvents)
            {
                Network.DisconnectedFromServerEvent?.Invoke(DisconnectReason.DisconnectPeerCalled);
                ClearConnections();
            }
            else if (type == PipeMessageType.ServerClosedVoluntarilyEvent)
            {
                Network.ServerShutdownEvent?.Invoke();
                ClearConnections();
            }
            else if (type == PipeMessageType.NetworkLatencyUpdateEvent)
            {
                long connectId = message.Read<long>();
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                int latency = message.Read<int>();

                NetworkConnection connection = new NetworkConnection(connectId, host, port);
                Network.NetworkLatencyUpdateEvent?.Invoke(connection, latency);
            }
            else if (type == PipeMessageType.NetworkReceiveUnconnectedEvent)
            {
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                UnconnectedMessageType messageType = (UnconnectedMessageType)message.Read<byte>();
                ushort dataLength = message.Read<ushort>();
                byte[] data = message.ReadBytes(dataLength);

                NetEndPoint endPoint = new NetEndPoint(host, port);
                NetDataReader reader = new NetDataReader(data);
                Network.NetworkReceiveUnconnectedEvent?.Invoke(endPoint, reader, messageType);
            }
            else if (type == PipeMessageType.PeerDisconnectedEvent)
            {
                long connectId = message.Read<long>();
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                bool fromClient = message.Read<bool>();
                DisconnectReason reason = (DisconnectReason)message.Read<byte>();

                if (fromClient)
                {
                    if (reason == DisconnectReason.ConnectionFailed)
                    {
                        NetworkConnection connection = new NetworkConnection(connectId, host, port);
                        Network.FailedToConnectEvent?.Invoke(connection);
                    }
                    else
                    {
                        Network.DisconnectedFromServerEvent?.Invoke(reason);
                    }
                    ClearConnections();
                    Network.IsClient = false;
                }
                else
                {
                    NetworkConnection connection = new NetworkConnection(connectId, host, port);
                    Network.PlayerDisconnectedEvent?.Invoke(connection, reason);

                    //remove this connection from the list
                    RemoveConnection(connectId);

                    //send message to all clients that this connection should be destroyed
                    SendConnectionRemoveRequest(connectId);
                }
            }
            else if (type == PipeMessageType.PeerConnectedEvent)
            {
                long connectId = message.Read<long>();
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                bool fromClient = message.Read<bool>();

                Debug.Log("[TEST] PeerConnectedEvent " + connectId);

                if (fromClient)
                {
                    Network.LocalNetworkID = connectId;
                    Network.IsClient = true;

                    NetworkConnection connection = new NetworkConnection(connectId, "", 0);
                    Network.ConnectedToServerEvent?.Invoke(connection);

                    SendAcknowledgementRequest(connectId, host, port);
                }
            }
            else if (type == PipeMessageType.NetworkReceiveEvent)
            {
                long connectId = message.Read<long>();
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                ushort messageType = message.Read<ushort>();
                ushort dataLength = message.Read<ushort>();
                byte[] data = message.ReadBytes(dataLength);
                bool isInternal = IsInternalMessage(messageType);

                NetworkConnection connection = new NetworkConnection(connectId, host, port);
                Message m = new Message(messageType, data);
                if (isInternal)
                {
                    ProcessInternalMessage(connection, m);
                }
                else
                {
                    Network.NetworkReceiveEvent?.Invoke(connection, m);
                }
            }
            else if (type == PipeMessageType.NetworkErrorEvent)
            {
                string host = message.Read<string>();
                ushort port = message.Read<ushort>();
                int socketErrorCode = message.Read<int>();

                NetEndPoint endPoint = new NetEndPoint(host, port);
                Network.NetworkErrorEvent?.Invoke(endPoint, socketErrorCode);
            }
        }

        private static bool IsInternalMessage(ushort type)
        {
            return Enum.IsDefined(typeof(InternalMessageType), type);
        }

        private static void ProcessInternalMessage(NetworkConnection connection, Message message)
        {
            InternalMessageType type = (InternalMessageType)message.Type;
            if (Network.IsServer)
            {
                if (type == InternalMessageType.AcknowledgeRequest)
                {
                    //add the new client to the list
                    long newConnectId = message.Read<long>();
                    string newHost = message.Read<string>();
                    ushort newPort = message.Read<ushort>();
                    AddConnection(newConnectId, newHost, newPort, 0);

                    //trigger event
                    NetworkConnection newConnection = new NetworkConnection(newConnectId, newHost, newPort);
                    Network.PlayerConnectedEvent?.Invoke(newConnection);

                    //send message to other clients that this new client joined
                    SendConnectionAddRequest(newConnectId, newHost, newPort, 0);

                    //process the request itself
                    Debug.Log("[NET] Received acknowledgement request");

                    //received an ack request, send them all users connected
                    Message response = new Message(InternalMessageType.AcknowledgeResponse);
                    List<NetworkConnection> connections = Connections;
                    response.Write((byte)connections.Count);
                    for (int i = 0; i < connections.Count; i++)
                    {
                        NetworkConnection c = connections[i];
                        response.Write(c.connectId);
                        response.Write(c.host);
                        response.Write(c.port);
                        response.Write(c.latency);
                    }

                    response.Send(connection);
                }
            }
            else if (Network.IsClient)
            {
                if (type == InternalMessageType.ConnectionRemoveRequest)
                {
                    long otherConnectId = message.Read<long>();
                    Debug.Log("[NET] Received request from server to destroy user with id " + otherConnectId);
                    RemoveConnection(otherConnectId);
                }
                else if (type == InternalMessageType.ConnectionAddRequest)
                {
                    long newConnectId = message.Read<long>();

                    //ignore this request if the user being added is our own
                    //because it was already added during the acknowledge response message
                    if (newConnectId == Network.LocalNetworkID) return;

                    string newHost = message.Read<string>();
                    ushort newPort = message.Read<ushort>();
                    short newLatency = message.Read<short>();

                    Debug.Log("[NET] Received request from server to add a new user with id " + newConnectId);
                    AddConnection(newConnectId, newHost, newPort, newLatency);
                }
                else if (type == InternalMessageType.AcknowledgeResponse)
                {
                    Debug.Log("[NET] Received acknowledgement response from server");

                    //received an ack from the server
                    //this data includes all connected users currently
                    int connections = message.Read<byte>();
                    for (int i = 0; i < connections; i++)
                    {
                        long newConnectId = message.Read<long>();
                        string newHost = message.Read<string>();
                        ushort newPort = message.Read<ushort>();
                        short newLatency = message.Read<short>();

                        Debug.Log("[NET] Add connection with id " + newConnectId + ", " + newHost + ":" + newPort + ", " + newLatency + "ms");
                        AddConnection(newConnectId, newHost, newPort, newLatency);
                    }

                    Debug.Log("[NET] Created " + connections + " connection objects");
                }
            }
        }

        private static void SendAcknowledgementRequest(long connectId, string host, ushort port)
        {
            Debug.Log("[NET] Sent server acknowledgement request");

            //this happens on the client that connects to a server
            //send a request to create our own user
            Message message = new Message(InternalMessageType.AcknowledgeRequest);
            message.Write(connectId);
            message.Write(host);
            message.Write(port);
            message.Send();
        }

        private static void SendConnectionAddRequest(long connectId, string host, ushort port, short ms)
        {
            Debug.Log("[NET] Sent connection add request to client");

            Message message = new Message(InternalMessageType.ConnectionAddRequest);
            message.Write(connectId);
            message.Write(host);
            message.Write(port);
            message.Write(ms);
            message.Send();
        }

        private static void SendConnectionRemoveRequest(long connectId)
        {
            Debug.Log("[NET] Sent connection remove request to client");

            Message message = new Message(InternalMessageType.ConnectionRemoveRequest);
            message.Write(connectId);
            message.Send();
        }
    }
}