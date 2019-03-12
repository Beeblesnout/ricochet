using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteNetLib;
using LiteNetLib.Utils;
using Popcron.Networking.Shared;
using UnityEngine;

namespace Popcron.Networking
{
    public class Network
    {
        //connections
        public static OnPlayerConnected PlayerConnectedEvent;
        public static OnConnectedToServer ConnectedToServerEvent;

        //disconnections
        public static OnFailedToConnect FailedToConnectEvent;
        public static OnDisconnectedFromServer DisconnectedFromServerEvent;
        public static OnPlayerDisconnected PlayerDisconnectedEvent;
        public static OnServerShutdown ServerShutdownEvent;

        //rest
        public static OnNetworkError NetworkErrorEvent;
        public static OnNetworkReceive NetworkReceiveEvent;
        public static OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;
        public static OnNetworkLatencyUpdate NetworkLatencyUpdateEvent;
        public static OnServerInitialized ServerInitializedEvent;
        public static OnServerInitializedFailed ServerInitializedFailedEvent;

        //connections
        public delegate void OnPlayerConnected(NetworkConnection connection);
        public delegate void OnConnectedToServer(NetworkConnection connection);

        //disconnections
        public delegate void OnFailedToConnect(NetworkConnection connection);
        public delegate void OnDisconnectedFromServer(DisconnectReason reason);
        public delegate void OnPlayerDisconnected(NetworkConnection connection, DisconnectReason reason);
        public delegate void OnServerShutdown();

        //rest
        public delegate void OnNetworkError(NetEndPoint endPoint, int socketErrorCode);
        public delegate void OnNetworkReceive(NetworkConnection connection, Message message);
        public delegate void OnNetworkReceiveUnconnected(NetEndPoint endPoint, NetDataReader reader, UnconnectedMessageType messageType);
        public delegate void OnNetworkLatencyUpdate(NetworkConnection connection, int latency);
        public delegate void OnServerInitialized();
        public delegate void OnServerInitializedFailed();

#if !UNITY_EDITOR
        private static Server server = new Server();
        private static Client client = new Client();
#endif

        /// <summary>
        /// Is a server currently running?
        /// </summary>
        public static bool IsServer
        {
            get
            {
                string key = Settings.CurrentlyUniqueID + "_IsServer";
                return PlayerPrefs.GetInt(key, 0) == 1;
            }
            set
            {
                string key = Settings.CurrentlyUniqueID + "_IsServer";
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Is a client currently running?
        /// </summary>
        public static bool IsClient
        {
            get
            {
                string key = Settings.CurrentlyUniqueID + "_IsClient";
                return PlayerPrefs.GetInt(key, 0) == 1;
            }
            set
            {
                string key = Settings.CurrentlyUniqueID + "_IsClient";
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }

        public static bool IsConnected
        {
            get
            {
                return IsServer || IsClient;
            }
        }

        public static long LocalNetworkID
        {
            get
            {
                string key = Settings.CurrentlyUniqueID + "_ConnectID";
                return long.Parse(PlayerPrefs.GetString(key, "0"));
            }
            set
            {
                string key = Settings.CurrentlyUniqueID + "_ConnectID";
                PlayerPrefs.SetString(key, value.ToString());
            }
        }

        public static List<NetworkConnection> Connections
        {
            get
            {
                return NetworkManager.Connections;
            }
        }

        /// <summary>
        /// Initialize the server.
        /// </summary>
        /// <param name="connections">The number of allowed incoming connections (note that this is generally not the same as the number of players).</param>
        /// <param name="listenPort">The port number we want to listen to.</param>
        /// <param name="password"></param>
        public static void InitializeServer(ushort port, byte connections, string password = "")
        {
            if (IsServer) return;

#if UNITY_EDITOR
            Message message = new Message(PipeMessageType.InitializeServerRequest);
            message.Write(port);
            message.Write(connections);
            message.Write(password);

            NetworkManager.SendToOtherApp(message);
#else
            Events.InitializeServerRequest(server, port, connections, password);
#endif
        }

        /// <summary>
        /// Connect to the specified host (ip or domain name) and server port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="remotePort"></param>
        /// <param name="password"></param>
        public static void Connect(string address, ushort port, string password = "")
        {
            if (IsClient) return;

#if UNITY_EDITOR
            Message message = new Message(PipeMessageType.ConnectRequest);
            message.Write(address);
            message.Write(port);
            message.Write(password);

            NetworkManager.SendToOtherApp(message);
#else
            Events.ConnectRequest(client, address, port, password);
#endif
        }

        /// <summary>
        /// Close all open connections and shuts down the network interface.
        /// </summary>
        public static void Disconnect()
        {
            IsServer = false;
            IsClient = false;

#if UNITY_EDITOR
            Message message = new Message(PipeMessageType.DisconnectRequest);
            NetworkManager.SendToOtherApp(message);
#else
            Events.DisconnectRequest(server, client);
#endif
        }

        /// <summary>
        /// Close the connection to another system from the server.
        /// </summary>
        /// <param name="connectId">The target connection to kick.</param>
        public static void CloseConnection(long connectId)
        {
            if (IsServer)
            {
#if UNITY_EDITOR
                Message message = new Message(PipeMessageType.CloseConnectionRequest);
                message.Write(connectId);
                NetworkManager.SendToOtherApp(message);
#else
                Events.CloseConnectionRequest(server, connectId);
#endif
            }
        }

        public static void Send(long connectId, Message message, SendOptions options = SendOptions.ReliableOrdered)
        {
            ushort type = message.Type;
            byte[] data = message.Data;
            Message m = new Message(type, data);

            if (IsServer)
            {
#if UNITY_EDITOR
                Message request = new Message(PipeMessageType.SendNetworkDataToConnectionRequest);
                request.Write((byte)options);                       //send option enum
                request.Write(connectId);                           //connect id
                request.Write(type);                                //payload type
                request.Write((ushort)data.Length);                 //payload length
                request.Write(data);                                //actual data to send

                NetworkManager.SendToOtherApp(request);
#else
                Events.SendNetworkDataToConnectionRequest(server, options, connectId, type, data);
#endif
            }

            //send to self
            //NetworkConnection connection = new NetworkConnection(0, "", 0);
            //NetworkReceiveEvent?.Invoke(connection, m);
        }

        public static void Send(Message message, SendOptions options = SendOptions.ReliableOrdered)
        {
            ushort type = message.Type;
            byte[] data = message.Data;
            Message m = new Message(type, data);

#if UNITY_EDITOR
            Message request = new Message(PipeMessageType.SendNetworkDataRequest);
            request.Write((byte)options);                   //send option enum
            request.Write(type);                            //payload type
            request.Write((ushort)data.Length);             //payload length
            request.Write(data);                            //actual data to send

            NetworkManager.SendToOtherApp(request);
#else
            Events.SendNetworkDataRequest(server, client, options, type, data);
#endif

            if (IsServer)
            {
                //send to self
                NetworkConnection connection = new NetworkConnection(0, "", 0);
                NetworkReceiveEvent?.Invoke(connection, m);
            }
        }
    }
}
