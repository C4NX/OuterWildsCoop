using Lidgren.Network;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OuterWildsServer.Network.Packets.Client;
using OuterWildsServer.Network.Packets.Server;
using OuterWildsServer.Network;
using System.Collections;
using System.IO;

namespace WildsCoop.Network
{

    /// <summary>
    /// An event coming from a <see cref="OWClient"/>
    /// </summary>
    public delegate void ClientEventHandler(OWClient client);

    /// <summary>
    /// An event coming from a <see cref="OWClient"/> but is linked to a <see cref="INetPacket"/>
    /// </summary>
    public delegate void ClientFromPacketEventHandler<T>(OWClient client, T packetValue) where T : INetPacket;

    /// <summary>
    /// A class that represents an OuterWilds client, which can connect to a OuterWilds Server.
    /// </summary>
    public class OWClient : IDisposable
    {
        #region Fields

        private NetClient _client;
        private NetConnection _serverConnection;
        private NetPacketsProvider _packetProvider;
        private DateTime _lastMessageTime;
        private ServerInformationPacket _serverInformation;
        private bool _isLoggedIn;
        private Guid _playerId;
        private string _username;

        #endregion

        #region GetSet

        /// <summary>
        /// Get if the client is connected to the server and is not disposed.
        /// </summary>
        public bool IsConnected => _client != null && _client.ConnectionStatus == NetConnectionStatus.Connected;

        /// <summary>
        /// Get if the client is running or connected, running does not mean that he is connected to a server.
        /// </summary>
        public bool IsRunningOrConnected => (_client != null && _client.Status == NetPeerStatus.Running) || IsConnected;

        /// <summary>
        /// Get if the server is sleeping, message thread will wait a lot more.
        /// </summary>
        public bool IsSleeping => (DateTime.Now - _lastMessageTime).TotalSeconds > 10;

        /// <summary>
        /// Get if the server information was received, to get the information needed, ask him nicely <see cref="RequestServerInformation(bool, string)"/>
        /// </summary>
        public bool HasServerInformation => _serverInformation != null;
        /// <summary>
        /// Get the game version given by the server or null.
        /// </summary>
        public string ServerGameVersion => _serverInformation.GameVersion;

        /// <summary>
        /// Get if the client is connected and logged in to the server.
        /// </summary>
        public bool IsLogged => IsConnected && _isLoggedIn;

        #endregion

        #region Events

        /// <summary>
        /// An event that can have several handlers, it is used to inform that the client is well logged on the server.
        /// To check if the client is well logged, please use <see cref="IsLogged"/>.
        /// </summary>
        public event ClientEventHandler OnLogged;

        /// <summary>
        /// A unique event, it notifies that the connection is not possible. 
        /// It is also linked to a <see cref="LoginResultPacket"/> because it is also 
        /// in charge of notifying that the login has been refused by the server.
        /// To identify both usages, the packet information (<see cref="LoginResultPacket"/>) is not specified if the connection 
        /// was not made successfully with the server.
        /// </summary>
        public ClientFromPacketEventHandler<LoginResultPacket> OnConnectFail;

        #endregion

        public OWClient()
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
        public bool ConnectTo(string ipOrHost, int port = OWServer.PORT_DEFAULT, long timeoutMillisecond = Timeout.Infinite)
        {
            if (IsConnected)
                return true;

            _client = new NetClient(new NetPeerConfiguration(OWServer.LIDGREN_APP_IDENTIFIER));
            _packetProvider = new NetPacketsProvider(_client)
                //Add Client/Side Packets
                .AddPacket<ServerInformationRequestPacket>(1)
                .AddPacket<LoginRequestPacket>(2)
                //Add Server/Side Packets
                .AddPacket<ServerInformationPacket>(101)
                .AddPacket<LoginResultPacket>(102);

            LogPacketProvider(_packetProvider);

            _client.Start();
            _serverConnection = _client.Connect(ipOrHost, port);

            ThreadPool.QueueUserWorkItem(OnMessageThreadItem, this);

            Stopwatch timeoutStopwatch = Stopwatch.StartNew();

            while (_client.ConnectionStatus != NetConnectionStatus.Connected)
            {
                Thread.Sleep(500);
                if (timeoutMillisecond != -1 && timeoutStopwatch.ElapsedMilliseconds > timeoutMillisecond)
                    break;
            }
            timeoutStopwatch.Stop();

            ClientLog($"DEBUG: IsConnected: {IsConnected}, Connection Status: {_client.ConnectionStatus}");

            if (!IsConnected)
                OnConnectFail?.Invoke(this, null);

            return IsConnected;
        }

        /// <summary>
        /// Request the server for its information, such as game versions, motd, etc...
        /// Basicly it will send a fresh new <see cref="ServerInformationRequestPacket"/>.
        /// </summary>
        /// <param name="requestDisconnectAfter">Tells the server if it should close the connection right after, used for server listing.</param>
        /// <param name="clientVersion">Client version to give to the server, default to <see cref="OuterWildsServer.VERSION"/></param>
        /// <returns></returns>
        public bool RequestServerInformation(bool requestDisconnectAfter,string clientVersion = OWServer.SERVER_VERSION)
        {
            return _serverConnection.SendMessage(_packetProvider.Deserialize(new ServerInformationRequestPacket{ ClientVersion = clientVersion, WantToDisconnectAfter=requestDisconnectAfter }), NetDeliveryMethod.ReliableSequenced, 0) == NetSendResult.Sent;
        }

        public bool RequestLogin(string username, string password = "", string clientVersion = OWServer.SERVER_VERSION)
        {
            return _serverConnection.SendMessage(_packetProvider.Deserialize(new LoginRequestPacket { ClientVersion= clientVersion, GameVersion=UnityEngine.Application.version, Password=password, Username=username}), NetDeliveryMethod.ReliableSequenced, 0) == NetSendResult.Sent;
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

            if (packetReceived is LoginResultPacket)
            {
                var loginResult = (LoginResultPacket)packetReceived;
                if (loginResult.IsLoggedIn)
                {
                    _playerId = loginResult.UserId;
                    _username = loginResult.Username;
                    _isLoggedIn = true;
                    ClientLog($"Logged to the server ID={_playerId} USERNAME={_username}");

                    OnLogged?.Invoke(this);
                }
                else
                {
                    ClientLog($"Login to the server failed: {loginResult.Message}");
                    OnConnectFail?.Invoke(this, loginResult);
                }
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
        /// <param name="sender">The <see cref="OWClient"/> Instance to use</param>
        private static void OnMessageThreadItem(object sender)
        {
            OWClient client = (OWClient)sender;
            NetIncomingMessage incomingMessage = null;
            while (client.IsRunningOrConnected)
            {
                if (client.IsSleeping)
                    Thread.Sleep(500);

                while ((incomingMessage = client._client.ReadMessage()) != null)
                {
                    client._lastMessageTime = DateTime.Now;

                    if (incomingMessage.MessageType == NetIncomingMessageType.Data)
                        client.PushDataMessage(incomingMessage);
                    else if (incomingMessage.MessageType == NetIncomingMessageType.StatusChanged)
                        client.PushStateMessage(incomingMessage);
                }
            }
        }

        /// <summary>
        /// Print a client log message, but uses an obsolete version 
        /// for logging due to the following <see cref="MelonDebug"/> not working in a different thread.
        /// </summary>
        /// <param name="message"></param>
        private static void ClientLog(string message)
        {
            MelonLogger.Log($"[CLIENT/{Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString()}] {message}");
        }

        private static void LogPacketProvider(NetPacketsProvider packetsProvider)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("This Client Provider has :");
            foreach (var item in packetsProvider)
                sb.AppendLine($"{item.Key}: {item.Value.FullName}");
            ClientLog(sb.ToString());
        }

        public void RequestSync()
        {
            
        }

        public string GetUsername() => _username;
    }
}
