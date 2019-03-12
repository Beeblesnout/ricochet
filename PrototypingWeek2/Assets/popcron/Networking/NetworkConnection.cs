using System;
using System.Net;
using UnityEngine;

using LiteNetLib;
using LiteNetLib.Utils;
using System.Globalization;

namespace Popcron.Networking
{
    [Serializable]
    public class NetworkConnection
    {
        public long connectId;
        public short latency;
        public string host;
        public ushort port;

        private IPEndPoint endPoint = null;

        public IPEndPoint EndPoint
        {
            get
            {
                if (host != "" && port != 0)
                {
                    if (endPoint == null)
                    {
                        endPoint = CreateIPEndPoint(host + ":" + port);
                    }
                }

                return endPoint;
            }
        }

        public NetworkConnection(long connectId, string host, ushort port)
        {
            this.connectId = connectId;
            this.host = host;
            this.port = port;
            if (host != "" && port != 0)
            {
                endPoint = CreateIPEndPoint(host + ":" + port);
            }
        }

        private static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public static implicit operator long(NetworkConnection connection)
        {
            return connection.connectId;
        }
    }
}