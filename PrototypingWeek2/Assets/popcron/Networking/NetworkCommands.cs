using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Popcron.Console;

namespace Popcron.Networking
{
    using Console = Console.Console;
    [Category("Networking commands")]
    public class NetworkCommands
    {
        [Command("net status")]
        public static string Status()
        {
            string idText = "<b>ID</b>".PadRight(20);
            string statusText = "<b>Status</b>".PadRight(20);
            Console.WriteLine("\t" + idText + Network.LocalNetworkID);
            if (Network.IsServer && !Network.IsClient)
            {
                return "\t" + statusText + "Server";
            }
            else if (!Network.IsServer && Network.IsClient)
            {
                return "\t" + statusText + "Client";
            }
            else if (Network.IsServer && Network.IsClient)
            {
                return "\t" + statusText + "Server and Client";
            }
            else
            {
                return "\t" + statusText + "Disconnected";
            }
        }

        [Command("net list")]
        public static void List()
        {
            List<NetworkConnection> connections = Network.Connections;
            if (connections.Count == 0)
            {
                Console.WriteLine("No connections");
            }
            else
            {
                int padding = 20;
                long localNetworkId = Network.LocalNetworkID;
                for (int i = 0; i < connections.Count; i++)
                {
                    long connectId = connections[i].connectId;
                    bool isHost = connectId == 0;

                    Console.WriteLine("<b>" + (isHost ? "Server" : "Client") + "</b>");

                    string ownedString = "<b>Owned</b>".PadRight(padding);
                    Console.WriteLine("\t" + ownedString + (connectId == localNetworkId));

                    if (connectId != 0)
                    {
                        string connectIdString = "<b>ID</b>".PadRight(padding);
                        Console.WriteLine("\t" + connectIdString + connectId);

                        IPEndPoint endPoint = connections[i].EndPoint;
                        string endPointString = "<b>End point</b>".PadRight(padding) + endPoint.ToString();
                        Console.WriteLine("\t" + endPointString);

                        string latencyString = "<b>Latency</b>".PadRight(padding) + connections[i].latency + "ms";
                        Console.WriteLine("\t" + latencyString);
                    }
                    else
                    {
                        string connectIdString = "<b>ID</b>".PadRight(padding);
                        Console.WriteLine("\t" + connectIdString + connectId);
                    }
                }
            }
        }

        [Command("net kick")]
        public static void Kick(long connectId)
        {
            Network.CloseConnection(connectId);
        }

        [Command("net host")]
        public static void Host()
        {
            Console.WriteLine("[NET] Attempting to host server with port 25565 and " + 16 + " max connections.");
            Network.InitializeServer(25565, 16);
        }

        [Command("net host")]
        public static void Host(ushort port, byte connections)
        {
            Console.WriteLine("[NET] Attempting to host server with port " + port + " and " + connections + " max connections.");
            Network.InitializeServer(port, connections);
        }

        [Command("net connect")]
        public static void Connect()
        {
            Console.WriteLine("[NET] Attempting to connect to 127.0.0.1:" + 25565);
            Network.Connect("127.0.0.1", 25565);
        }

        [Command("net connect")]
        public static void Connect(string address)
        {
            ushort port = 25565;
            if (address.Contains(":"))
            {
                string portString = address.Substring(address.IndexOf(":") + 1);
                if (ushort.TryParse(portString, out ushort newPort))
                {
                    port = newPort;
                }
                address = address.Substring(0, address.IndexOf(":"));
            }

            Console.WriteLine("[NET] Attempting to connect to " + address + ":" + port);
            Network.Connect(address, port);
        }

        [Command("net disconnect")]
        public static void Disconnect()
        {
            Network.Disconnect();
        }
    }
}
