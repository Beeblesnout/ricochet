using Popcron.Console;
using Popcron.Networking;
using System.Collections.Generic;
using System.Net;

[Category("Networking commands")]
public class CommandsNetworking
{
    [Command("net status")]
    public static string Status()
    {
        string idText = "<b>ID</b>".PadRight(20);
        string statusText = "<b>Status</b>".PadRight(20);
        Console.WriteLine("\t" + idText + Net.LocalConnectionID);
        if (Net.IsServer && !Net.IsClient)
        {
            return "\t" + statusText + "Server";
        }
        else if (!Net.IsServer && Net.IsClient)
        {
            return "\t" + statusText + "Client";
        }
        else if (Net.IsServer && Net.IsClient)
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
        List<NetConnection> connections = Net.Connections;
        if (connections.Count == 0)
        {
            Console.WriteLine("No connections");
        }
        else
        {
            int padding = 20;
            long localNetworkId = Net.LocalConnectionID;
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
        Net.CloseConnection(connectId);
    }

    [Command("net host")]
    public static void Host()
    {
        Net.InitializeServer(25565, 16);
    }

    [Command("net host")]
    public static void Host(ushort port, byte connections)
    {
        Net.InitializeServer(port, connections);
    }

    [Command("net connect")]
    public static void Connect()
    {
        Net.Connect("127.0.0.1", 25565);
    }

    [Command("net connect")]
    public static void Connect(string address, ushort port)
    {
        Net.Connect(address, port);
    }

    [Command("net disconnect")]
    public static void Disconnect()
    {
        Net.Disconnect();
    }
}
