using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteNetLib;

namespace Popcron.Networking.Shared
{
    public class Server : Behaviour
    {
        protected sealed override object Start(params object[] arguments)
        {
            ushort port = (ushort)arguments[0];
            byte maxConnections = (byte)arguments[1];
            string password = (string)arguments[2];

            Manager = new NetManager(Listener, maxConnections, password);
            bool success = Manager.Start(port);
            return success;
        }
    }
}
