using Lidgren.Network;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WildsCoop.Network.Packets.Client;
using WildsCoop.Network.Packets.Server;

namespace WildsCoop.Network
{
    public class OuterWildsClient : IDisposable
    {
        private NetClient _client;
        private NetConnection _serverConnection;
        private NetPacketsProvider _packetProvider;
        private ServerInformationPacket _serverInformation;

        public bool IsConnected => _client != null && _client.ConnectionStatus == NetConnectionStatus.Connected;
        public bool IsRunningOrConnected => (_client != null && _client.Status == NetPeerStatus.Running) || IsConnected;

        public OuterWildsClient()
        {
        }

        public bool ConnectTo(string ipOrHost, int port = OuterWildsServer.PORT_DEFAULT, long timeoutMillisecond = Timeout.Infinite)
        {
            if (IsConnected)
                return true;

            _client = new NetClient(new NetPeerConfiguration(OuterWildsServer.LIDGREN_APP_IDENTIFIER));
            _packetProvider = new NetPacketsProvider(_client)
                //Add Client/Side Packets
                .AddPacket<ServerInformationRequestPacket>(1)
                //Add Server/Side Packets
                .AddPacket<ServerInformationPacket>(101);

            LogPacketProvider(_packetProvider);

            _client.Start();
            _serverConnection = _client.Connect(ipOrHost, port);

            ThreadPool.QueueUserWorkItem(OnMessageThreadItem, this);

            Stopwatch timeoutStopwatch = Stopwatch.StartNew();

            ClientLog($"Before while connection is : {_client.ConnectionStatus}");
            while (_client.ConnectionStatus != NetConnectionStatus.Connected)
            {
                ClientLog($"Waiting for Connected state got {_client.ConnectionStatus}");
                if (timeoutMillisecond != -1 && timeoutStopwatch.ElapsedMilliseconds > timeoutMillisecond)
                    break;
            }
            timeoutStopwatch.Stop();

            ClientLog($"DEBUG: IsConnected: {IsConnected}, Connection Status: {_client.ConnectionStatus}");

            return IsConnected;
        }

        public bool SendMotdRequest(bool requestDisconnectAfter,string clientVersion = OuterWildsServer.VERSION)
        {
            return _serverConnection.SendMessage(_packetProvider.Deserialize(new ServerInformationRequestPacket() { ClientVersion = clientVersion, WantToDisconnectAfter=requestDisconnectAfter }), NetDeliveryMethod.ReliableSequenced, 0) == NetSendResult.Sent;
        }

        internal void PushDataMessage(NetIncomingMessage netIncomingMessage)
        {
            ClientLog($"Data received {netIncomingMessage.LengthBytes}B from the server");

            uint packetId = 0;
            var packetReceived = _packetProvider.Serialize(netIncomingMessage, out packetId);

            ClientLog($"Packet is {packetReceived.GetType().FullName} ({packetId})");

            if (packetReceived is ServerInformationPacket)
            {
                _serverInformation = (ServerInformationPacket)packetReceived;
                ClientLog($"Server information: IP={_serverConnection.RemoteEndPoint} MOTD={_serverInformation.MOTD}");
            }
        }

        internal void PushStateMessage(NetIncomingMessage netIncomingMessage)
        {
            ClientLog($"Server State changed to {netIncomingMessage.SenderConnection?.Status}");
        }

        public void Dispose()
        {
            _client.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            _client.Socket.Close(5000);
            _client = null;
        }

        private static void OnMessageThreadItem(object sender)
        {
            OuterWildsClient client = (OuterWildsClient)sender;
            NetIncomingMessage incomingMessage = null;
            while (client.IsRunningOrConnected)
            {
                while ((incomingMessage = client._client.ReadMessage()) != null)
                {
                    if (incomingMessage.MessageType == NetIncomingMessageType.Data)
                        client.PushDataMessage(incomingMessage);
                    else if (incomingMessage.MessageType == NetIncomingMessageType.StatusChanged)
                        client.PushStateMessage(incomingMessage);
                }
            }
        }

        private static void ClientLog(string message) => MelonDebug.Msg($"[CLIENT] {message}");

        private static void LogPacketProvider(NetPacketsProvider packetsProvider)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in packetsProvider)
                sb.AppendLine($"{item.Key}: {item.Value.FullName}");
            ClientLog(sb.ToString());
        }
    }
}
