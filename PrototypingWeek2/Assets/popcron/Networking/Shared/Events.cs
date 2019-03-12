using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteNetLib;
using UnityEngine;

namespace Popcron.Networking.Shared
{
    public class Events
    {
        public static void InitializeServerRequest(Server server, ushort port, byte connections, string password)
        {
            server.Initialize(port, connections, password);
        }

        public static void ConnectRequest(Client client, string address, ushort port, string password)
        {
            client.Initialize(address, port, password);
        }

        public static void DisconnectRequest(Server server, Client client)
        {
            server.Close();
            client.Close();
        }

        public static void CloseConnectionRequest(Server server, long connectId)
        {
            NetPeer[] peers = server.Manager.GetPeers();
            for (int i = 0; i < peers.Length; i++)
            {
                if (peers[i].ConnectId == connectId)
                {
                    server.Manager.DisconnectPeer(peers[i]);
                }
            }
        }

        public static void SendNetworkDataToConnectionRequest(Server server, SendOptions options, long connectId, ushort type, byte[] data)
        {
            //this only gets called from a server
            List<byte> newData = new List<byte>(data);
            newData.InsertRange(0, BitConverter.GetBytes(type));
            data = newData.ToArray();

            NetPeer[] peers = server.Manager.GetPeers();
            for (int i = 0; i < peers.Length; i++)
            {
                if (peers[i].ConnectId == connectId)
                {
                    peers[i].Send(data, options);
                }
            }
        }

        public static void SendNetworkDataRequest(Server server, Client client, SendOptions options, ushort type, byte[] data)
        {
            List<byte> newData = new List<byte>(data);
            newData.InsertRange(0, BitConverter.GetBytes(type));
            data = newData.ToArray();

            if (server?.Manager != null)
            {
                //send to all clients if from server
                server.Manager.SendToAll(data, options);
            }
            else if (client?.Manager != null && client?.Peer != null)
            {
                //send to server if from client
                client.Peer.Send(data, options);
            }
        }

        public static void ConsoleMessageRequest(string text)
        {
            Program.WriteLine("[PIPE] " + text);
        }
    }
}