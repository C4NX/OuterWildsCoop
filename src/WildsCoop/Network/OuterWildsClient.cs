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
    /// <summary>
    /// A class that represents an OuterWilds client, which can connect to a OuterWilds Server.
    /// </summary>
    public class OuterWildsClient : IDisposable
    {
        private NetClient _client;
        private NetConnection _serverConnection;
        private NetPacketsProvider _packetProvider;
        private ServerInformationPacket _serverInformation;

        /// <summary>
        /// Get if the client is connected to the server and is not disposed.
        /// </summary>
        public bool IsConnected => _client != null && _client.ConnectionStatus == NetConnectionStatus.Connected;

        /// <summary>
        /// Get if the client is running or connected, running does not mean that he is connected to a server.
        /// </summary>
        public bool IsRunningOrConnected => (_client != null && _client.Status == NetPeerStatus.Running) || IsConnected;

        /// <summary>
        /// Get if the server information was received, to get the information needed, ask him nicely <see cref="RequestServerInformation(bool, string)"/>
        /// </summary>
        public bool HasServerInformation => _serverInformation != null;
        /// <summary>
        /// Get the game version given by the server or null.
        /// </summary>
        public string ServerGameVersion => _serverInformation.GameVersion;

        public OuterWildsClient()
        {
        }

        /// <summary>
        /// A connection from the client to a host, the connection is synchronous and 
        /// will await a timeout or until the server responds to the connection, the 
        /// default port is defined by <see cref="OuterWildsServer.PORT_DEFAULT"/> and the timeout is by default <see cref="Timeout.Infinite"/>.
        /// </summary>
        /// <param name="ipOrHost">IP or host to connect</param>
        /// <param name="port">Port to connect, by default <see cref="OuterWildsServer.PORT_DEFAULT"/></param>
        /// <param name="timeoutMillisecond">Server connection timeout, by default <see cref="Timeout.Infinite"/></param>
        /// <returns></returns>
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

        /// <summary>
        /// Request the server for its information, such as game versions, motd, etc...
        /// Basicly it will send a fresh new <see cref="ServerInformationRequestPacket"/>.
        /// </summary>
        /// <param name="requestDisconnectAfter">Tells the server if it should close the connection right after, used for server listing.</param>
        /// <param name="clientVersion">Client version to give to the server, default to <see cref="OuterWildsServer.VERSION"/></param>
        /// <returns></returns>
        public bool RequestServerInformation(bool requestDisconnectAfter,string clientVersion = OuterWildsServer.VERSION)
        {
            return _serverConnection.SendMessage(_packetProvider.Deserialize(new ServerInformationRequestPacket{ ClientVersion = clientVersion, WantToDisconnectAfter=requestDisconnectAfter }), NetDeliveryMethod.ReliableSequenced, 0) == NetSendResult.Sent;
        }

        /// <summary>
        /// Transform the received data into a readable message with <see cref="NetPacketsProvider"/>, and execute the action associated to this message.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushDataMessage(NetIncomingMessage netIncomingMessage)
        {
            ClientLog($"Data received {netIncomingMessage.LengthBytes}B from the server");

            uint packetId = 0;
            var packetReceived = _packetProvider.Serialize(netIncomingMessage, out packetId);

            ClientLog($"Packet is {packetReceived.GetType().FullName} ({packetId})");

            if (packetReceived is ServerInformationPacket)
            {
                _serverInformation = (ServerInformationPacket)packetReceived;
                ClientLog($"{_serverInformation} IP={_serverConnection.RemoteEndPoint}");
            }
        }

        /// <summary>
        /// Execute the action associated to this new state.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushStateMessage(NetIncomingMessage netIncomingMessage)
        {
            ClientLog($"Server State changed to {netIncomingMessage.SenderConnection?.Status}");
        }

        /// <summary>
        /// Closes all communication with the server, without telling him to free the client's use.
        /// </summary>
        public void Dispose()
        {
            _client.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            _client.Socket.Close(5000);
            _client = null;
        }

        /// <summary>
        /// Method that is executed on the <see cref="ThreadPool"/>, and which serves as a message reading loop for the client.
        /// </summary>
        /// <param name="sender">The <see cref="OuterWildsClient"/> Instance to use</param>
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
            sb.AppendLine();
            sb.AppendLine("This Client Provider has :");
            foreach (var item in packetsProvider)
                sb.AppendLine($"{item.Key}: {item.Value.FullName}");
            ClientLog(sb.ToString());
        }
    }
}
