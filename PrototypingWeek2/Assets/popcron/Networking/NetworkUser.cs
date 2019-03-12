using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Popcron.Networking
{
    public class NetworkUser : MonoBehaviour
    {
        [SerializeField]
        private NetworkConnection connection;

        public long ConnectID
        {
            get
            {
                return connection.connectId;
            }
        }

        public int Latency
        {
            get
            {
                return connection.latency;
            }
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return connection.EndPoint;
            }
        }

        public bool IsMine
        {
            get
            {
                return connection.connectId == Network.LocalNetworkID;
            }
        }

        public bool IsServer
        {
            get
            {
                return Network.IsServer;
            }
        }

        public bool IsClient
        {
            get
            {
                return Network.IsClient;
            }
        }

        public bool IsConnected
        {
            get
            {
                return Network.IsConnected;
            }
        }

        public void Initialize(NetworkConnection connection)
        {
            this.connection = connection;
        }
    }
}