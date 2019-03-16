using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteNetLib;

namespace Popcron.Networking.Shared
{
    public class Client : Behaviour
    {
        public NetPeer Peer { get; set; }

        protected sealed override object Start(params object[] arguments)
        {
            string ip = (string)arguments[0];
            ushort port = (ushort)arguments[1];
            string password = (string)arguments[2];

            Manager = new NetManager(Listener, password);
            Manager.Start();
            Peer = Manager.Connect(ip, port);
            return Peer;
        }
    }
}
