using Lidgren.Network;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WildsCoop.Network.Packets.Client;
using WildsCoop.Network.Packets.Server;

namespace WildsCoop.Network
{
    public class OuterWildsServer : IDisposable
    {
        public const string LIDGREN_APP_IDENTIFIER = "OUTERWILDS";
        public const int PORT_DEFAULT = 14177;
        public const string VERSION = "OWC 1.0";

        private NetServer _server;
        private NetPacketsProvider _packetProvider;
        private ServerConfiguration _configuration;

        public bool IsRunning => _server != null && _server.Status == NetPeerStatus.Running;

        private OuterWildsServer(ServerConfiguration configuration)
        {
            _configuration = configuration;
            _server = new NetServer(_configuration.CreatePeerConfiguration());
            _packetProvider = new NetPacketsProvider(_server)
                //Add Client/Side Packets
                .AddPacket<ServerInformationRequestPacket>(1)
                //Add Server/Side Packets
                .AddPacket<ServerInformationPacket>(101);
        }

        public void Start()
        {
            if (IsRunning)
                return;
            _server.Start();
            ThreadPool.QueueUserWorkItem(OnMessageThreadItem, this);
        }

        public static OuterWildsServer CreateServer(ServerConfiguration serverConfiguration)
        {
            return new OuterWildsServer(serverConfiguration);
        }

        public void Dispose()
        {
            _server.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            _server.Socket.Close(5000);
            _server = null;
        }

        internal void PushDataMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"Data received {netIncomingMessage.LengthBytes}B from {netIncomingMessage.SenderEndPoint}");

            uint packetId = 0;
            var packetReceived = _packetProvider.Serialize(netIncomingMessage, out packetId);

            if (packetReceived != null)
            {
                if (packetReceived is ServerInformationRequestPacket)
                {
                    ReplyWith(netIncomingMessage.SenderConnection, new ServerInformationPacket() { IsDisconnectRequest = ((ServerInformationRequestPacket)packetReceived).WantToDisconnectAfter, MOTD = _configuration.MOTD });
                }

                ServerLog($"Packet is {packetReceived.GetType().FullName} ({packetId})");
            }
            else
            {
                ServerLog($"Ignore {packetId} from {netIncomingMessage.SenderEndPoint}");
            }
        }

        internal void PushStateMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"State of {netIncomingMessage.SenderEndPoint} changed to {netIncomingMessage.SenderConnection?.Status}");
        }

        public bool ReplyWith(NetConnection netConnection, INetPacket netPacket)
        {
            var sendResult = netConnection.SendMessage(_packetProvider.Deserialize(netPacket), NetDeliveryMethod.ReliableOrdered, 0);
            return sendResult == NetSendResult.Sent || sendResult == NetSendResult.Queued;
        }

        public NetServer GetLidgrenServer() => _server;

        private static void OnMessageThreadItem(object sender)
        {
            OuterWildsServer server = (OuterWildsServer)sender;
            NetIncomingMessage incomingMessage = null;
            while (server.IsRunning)
            {
                while ((incomingMessage = server._server.ReadMessage()) != null)
                {
                    if (incomingMessage.MessageType == NetIncomingMessageType.Data)
                        server.PushDataMessage(incomingMessage);
                    else if (incomingMessage.MessageType == NetIncomingMessageType.StatusChanged)
                        server.PushStateMessage(incomingMessage);
                }
            }
        }

        private static void ServerLog(string message) => MelonDebug.Msg($"[SERVER] {message}");
    }
}
