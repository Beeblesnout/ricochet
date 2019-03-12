using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using LiteNetLib;
using LiteNetLib.Utils;

namespace Popcron.Networking.Shared
{
    public abstract class Behaviour
    {
        public NetManager Manager { get; protected set; }
        public EventBasedNetListener Listener { get; private set; }

        private string Name
        {
            get
            {
                return "[" + GetType().Name.ToUpper() + "]";
            }
        }

        public async void Initialize(params object[] arguments)
        {
            if (Manager != null)
            {
                Program.WriteLine(Name + " Tried to create a new instance.");
                return;
            }

            Program.WriteLine(Name + " Created.");
            Listener = new EventBasedNetListener();
            Listener.NetworkReceiveEvent += NetworkReceiveEvent;
            Listener.NetworkErrorEvent += NetworkErrorEvent;
            Listener.PeerConnectedEvent += PeerConnectedEvent;
            Listener.PeerDisconnectedEvent += PeerDisconnectedEvent;
            Listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnectedEvent;
            Listener.NetworkLatencyUpdateEvent += NetworkLatencyUpdateEvent;

            object result = Start(arguments);
            if (result is bool success)
            {
                if (!success)
                {
                    Message message = new Message(PipeMessageType.ServerInitializedFailedEvent);
                    Program.Send(message);
                    Close();
                }
                else
                {
                    Message message = new Message(PipeMessageType.ServerInitializedEvent);
                    Program.Send(message);
                }
            }
            else if (result is NetPeer peer)
            {

            }

            while (true)
            {
                if (Manager == null) break;

                Manager.PollEvents();
                await Task.Delay(1);
            }
        }

        protected abstract object Start(params object[] arguments);

        public void Close()
        {
            if (Manager != null)
            {
                Program.WriteLine(Name + " Closed.");
                if (this is Client)
                {
                    Message message = new Message(PipeMessageType.ClientClosedVoluntarilyEvents);
                    message.Write(true);

                    Program.Send(message);
                }
                else if (this is Server)
                {
                    Message message = new Message(PipeMessageType.ServerClosedVoluntarilyEvent);
                    message.Write(false);

                    Program.Send(message);
                }

                Manager.Stop();
                Manager = null;
            }
        }

        public void NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            Message message = new Message(PipeMessageType.NetworkLatencyUpdateEvent);
            message.Write(peer.ConnectId);
            message.Write(peer.EndPoint.Host);
            message.Write((ushort)peer.EndPoint.Port);
            message.Write(latency);

            Program.Send(message);
        }

        public void NetworkReceiveUnconnectedEvent(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            Program.WriteLine(Name + " Unconnected event for " + remoteEndPoint.Host + ":" + remoteEndPoint.Port + ", type: " + messageType);

            Message message = new Message(PipeMessageType.NetworkReceiveUnconnectedEvent);
            message.Write(remoteEndPoint.Host);
            message.Write((ushort)remoteEndPoint.Port);
            message.Write((byte)messageType);
            message.Write((ushort)reader.Data.Length);
            message.Write(reader.Data);

            Program.Send(message);
        }

        public void PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Program.WriteLine(Name + " Disconnected event for " + peer.ConnectId + ", reason: " + disconnectInfo.Reason);

            bool fromClient = this is Client;
            if (fromClient)
            {
                Close();
            }

            Message message = new Message(PipeMessageType.PeerDisconnectedEvent);
            message.Write(peer.ConnectId);
            message.Write(peer.EndPoint.Host);
            message.Write((ushort)peer.EndPoint.Port);
            message.Write(fromClient);
            message.Write((byte)disconnectInfo.Reason);

            Program.Send(message);
        }

        public void PeerConnectedEvent(NetPeer peer)
        {
            Program.WriteLine(Name + " Connection event for " + peer.ConnectId);

            bool fromClient = this is Client;

            Message message = new Message(PipeMessageType.PeerConnectedEvent);
            message.Write(peer.ConnectId);
            message.Write(peer.EndPoint.Host);
            message.Write((ushort)peer.EndPoint.Port);
            message.Write(fromClient);

            Program.Send(message);
        }

        public void NetworkErrorEvent(NetEndPoint endPoint, int socketErrorCode)
        {
            Program.WriteLine(Name + " Error for " + endPoint.Host + ":" + endPoint.Port + ", code: " + socketErrorCode);

            Message message = new Message(PipeMessageType.NetworkErrorEvent);
            message.Write(endPoint.Host);
            message.Write((ushort)endPoint.Port);
            message.Write(socketErrorCode);

            Program.Send(message);
        }

        public void NetworkReceiveEvent(NetPeer peer, NetDataReader reader)
        {
            //Program.WriteLine(Name + " Data event from " + peer.ConnectId + " (" + reader.Data.Length + " bytes)");

            Message message = new Message(PipeMessageType.NetworkReceiveEvent);
            message.Write(peer.ConnectId);
            message.Write(peer.EndPoint.Host);
            message.Write((ushort)peer.EndPoint.Port);

            message.Write(reader.GetUShort());
            message.Write((ushort)reader.Data.Length);
            message.Write(reader.GetRemainingBytes());

            Program.Send(message);
        }

        public void NetworkReceiveFromServerEvent(ushort messageType, byte[] data)
        {
            //Program.WriteLine(Name + " Data event from server (" + data.Length + " bytes)");

            Message message = new Message(PipeMessageType.NetworkReceiveEvent);
            message.Write((long)0);
            message.Write("");
            message.Write((ushort)0);

            message.Write(messageType);
            message.Write((ushort)data.Length);
            message.Write(data);

            Program.Send(message);
        }
    }
}