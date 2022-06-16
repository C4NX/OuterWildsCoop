using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OuterWildsServerLib.Network.Players;
using OuterWildsServerLib.Utils;
using OuterWildsServerLib.Utils.Logger;
using System.IO;
using OuterWildsServerLib.Network.Packets;
using OuterWildsServerLib.Network.Packets.Client;
using OuterWildsServerLib.Network.Packets.Server;
using OuterWildsServerLib.Network;

namespace OuterWildsServer.Network
{
    /// <summary>
    /// A class that represents an OuterWilds server.
    /// </summary>
    public class OWServer : IDisposable
    {
        public const string LIDGREN_APP_IDENTIFIER = "OUTERWILDS";
        public const int PORT_DEFAULT = 14177;
        public const string SERVER_VERSION = "OWC 1.0";
        public const string GAME_VERSION = "1.1.10.47";

        private NetServer _server;
        private List<OWPlayer> _players;
        private NetPacketProvider _packetProvider;
        private ServerConfiguration _configuration;
        private DateTime _lastMessageTime;

        private IOwTime _timeManager;

        /// <summary>
        /// Get if this server is loaded in Outer Wilds directly.
        /// </summary>
        public static bool IsServerInGame { get; protected set; } = AppDomain.CurrentDomain.GetAssemblies().Any((e) => e.GetName().Name.Equals("MelonLoader", StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        /// Get if the server is running, more exactly its socket.
        /// </summary>
        public bool IsRunning => _server != null && _server.Status == NetPeerStatus.Running;

        /// <summary>
        /// Get if the server is sleeping, message thread will wait a lot more.
        /// </summary>
        public bool IsSleeping => (DateTime.Now - _lastMessageTime).TotalSeconds > 10;

        public int PlayerCount => _players.Count;

        private OWServer(ServerConfiguration configuration)
        {
            _configuration = configuration;
            _server = new NetServer(_configuration.CreatePeerConfiguration());
            _packetProvider = new NetPacketProvider(_server);
            _packetProvider.AddPackets(typeof(OWServer).Assembly);

            _players = new List<OWPlayer>();
            _players.Add(OWPlayer.CreateFake(Guid.NewGuid(), "Luz"));
        }

        /// <summary>
        /// Start the server if it is not already running, and start a <see cref="ThreadPool"/> to read messages.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                return;
            _server.Start();
            ThreadPool.QueueUserWorkItem(OnMessageThreadItem, this);
        }

        public void Stop() => _server.Shutdown(string.Empty);

        /// <summary>
        /// Create an outer wilds server with a specific configuration.
        /// </summary>
        /// <param name="serverConfiguration">The <see cref="ServerConfiguration"/> to be used</param>
        /// <returns></returns>
        public static OWServer CreateServer(ServerConfiguration serverConfiguration)
        {
            return new OWServer(serverConfiguration);
        }

        /// <summary>
        /// Closes all communication, free the socket.
        /// </summary>
        public void Dispose()
        {
            if (_server.Socket.Connected)
            {
                _server.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                _server.Socket.Close(5000);
            }
            _server = null;
        }

        /// <summary>
        /// Transform the received data into a readable message with <see cref="NetPacketProvider"/>, and execute the action associated to this message.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushDataMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"Received {netIncomingMessage.LengthBytes}B from {netIncomingMessage.SenderEndPoint}", true);

            uint packetId = 0;
            var packetReceived = _packetProvider.Deserialize(netIncomingMessage, out packetId);

            if (packetReceived != null)
            {
                ServerLog($"Packet is {packetReceived.GetType().FullName} ({packetId})", true);

                if (packetReceived is ServerInformationRequestPacket)
                {
                    //Send information
                    ServerSend(netIncomingMessage.SenderConnection, new ServerInformationPacket() { 
                        IsDisconnectRequest = ((ServerInformationRequestPacket)packetReceived).WantToDisconnectAfter, 
                        MOTD = _configuration.MOTD, GameVersion=GAME_VERSION
                    });
                    return;
                }

                if (packetReceived is LoginRequestPacket)
                {
                    var loginPacket = (LoginRequestPacket)packetReceived;

                    string loginMessage = string.Empty;
                    bool isLoggedIn = true;

                    //Password is wrong sooo...
                    if (!string.IsNullOrWhiteSpace(_configuration.Password) && loginPacket.Password != _configuration.Password)
                    {
                        ServerLog($"Attempt to connect from {netIncomingMessage.SenderEndPoint}, but wrong password was given.", true);
                        loginMessage = "Incorect password.";
                        isLoggedIn = false;
                    }

                    //Username is wrong soo....
                    if (string.IsNullOrWhiteSpace(loginPacket.Username))
                    {
                        ServerLog($"Attempt to connect from {netIncomingMessage.SenderEndPoint}, but bad username was given.", true);
                        loginMessage = "Invalid username.";
                        isLoggedIn = false;
                    }

                    if (isLoggedIn)
                    {
                        //Create the new player and add it.
                        var newPlayer = new OWPlayer(Guid.NewGuid(), netIncomingMessage.SenderConnection, loginPacket.Username.Trim());
                        _players.Add(newPlayer);


                        ServerLog($"New Player as joined {newPlayer.GetUsername()}({newPlayer.GetGuid()})", false);

                        if (_timeManager == null)
                        {
                            _timeManager = new ServerTimeManager();
                            ServerLog($"Created OwTime: {_timeManager.GetTime()}s --> {_timeManager.GetTime()}s");
                        }

                        //Send OK, with id,username,message.
                        ServerSend(netIncomingMessage.SenderConnection, new LoginResultPacket
                        {
                            IsLoggedIn = true,
                            UserId = newPlayer.GetGuid(),
                            Username = newPlayer.GetUsername(),
                            Message = loginMessage
                        });
                    }
                    else
                    {
                        //Send failed, with message.
                        ServerSend(netIncomingMessage.SenderConnection, new LoginResultPacket
                        {
                            IsLoggedIn = false,
                            Message = loginMessage
                        });
                    }

                    return;
                }

                if (packetReceived is FirstSyncRequest)
                {
                    var player = GetPlayerFromMessage(netIncomingMessage);

                    if (player != null)
                    {
                        var fsyncPacket = new FirstSyncResultPacket(this, player);
                        fsyncPacket.time = _timeManager.GetTime();
                        ServerLog($"Sent FirstSyncResultPacket, {ServerSend(player, fsyncPacket)}");
                    }
                }
            }
            else
            {
                ServerLog($"Ignore {packetId} from {netIncomingMessage.SenderEndPoint}", true);
            }
        }

        /// <summary>
        /// Execute the action associated to this new state.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushStateMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"State of {netIncomingMessage.SenderEndPoint} changed to {netIncomingMessage.SenderConnection?.Status}", true);


            if(netIncomingMessage.SenderConnection?.Status == NetConnectionStatus.Disconnected)
            {
                OWPlayer player = null;
                foreach (var item in GetPlayers())
                {
                    if(item.GetConnection() == netIncomingMessage.SenderConnection)
                    {
                        player = item;
                        break;
                    }
                }

                if (_players.Remove(player))
                {
                    ServerLog($"Disconnected {player}.");
                }
            }
        }

        #region Sends

        public OWPlayer GetPlayerFromMessage(NetIncomingMessage message)
        {
            foreach (var item in GetPlayers())
            {
                if (item.GetConnection() == message.SenderConnection)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Send a <see cref="INetPacket"/> to a <see cref="NetConnection"/>.
        /// </summary>
        /// <param name="netConnection">The <see cref="NetConnection"/></param>
        /// <param name="netPacket">The <see cref="INetPacket"/></param>
        /// <exception cref="ArgumentException">If this packet is not registered in the <see cref="NetPacketProvider"/></exception>
        /// <returns>If the message was <see cref="NetSendResult.Sent"/> or <see cref="NetSendResult.Queued"/></returns>
        public bool ServerSend(NetConnection netConnection, INetPacket netPacket)
        {
            if (netConnection == null)
                return false;

            var sendResult = netConnection.SendMessage(_packetProvider.Serialize(netPacket), NetDeliveryMethod.ReliableOrdered, 0);
            return sendResult == NetSendResult.Sent || sendResult == NetSendResult.Queued;
        }

        /// <summary>
        /// Send a <see cref="INetPacket"/> to a <see cref="OWPlayer"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="netPacket"></param>
        /// <returns></returns>
        public bool ServerSend(OWPlayer player, INetPacket netPacket)
            => ServerSend(player.GetConnection(), netPacket);

        /// <summary>
        /// Broadcast a <see cref="INetPacket"/> to all players.
        /// </summary>
        /// <param name="netPacket">The packet to send</param>
        public void ServerBroadcast(INetPacket netPacket)
        {
            var outMessage = _packetProvider.Serialize(netPacket);
            foreach (var item in GetPlayers())
                item.GetConnection()?.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Broadcast a <see cref="INetPacket"/> to all players which are not in the ingores parameter.
        /// </summary>
        /// <param name="netPacket">The packet to send</param>
        /// <param name="ignores">Players who are ignored</param>
        public void ServerBroadcast(INetPacket netPacket, OWPlayer[] ignores)
        {
            var outMessage = _packetProvider.Serialize(netPacket);
            foreach (var item in GetPlayers())
                if (!ignores.Contains(item))
                {
                    item.GetConnection()?.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered, 0);
                }
        }

        #endregion

        /// <summary>
        /// Get the current <see cref="NetServer"/> used.
        /// </summary>
        /// <returns></returns>
        public NetServer GetLidgrenServer() => _server;

        /// <summary>
        /// Set the server message of the day
        /// </summary>
        /// <param name="motd">The new message of the day</param>
        /// <exception cref="ArgumentException">If the motd is null</exception>
        public void SetMOTD(string motd)
        {
            if (motd == null)
                throw new ArgumentException("motd is null");
            _configuration.MOTD = motd;
        }
        /// <summary>
        /// Get the current message of the day
        /// </summary>
        /// <returns></returns>
        public string GetMOTD() => _configuration.MOTD;

        /// <summary>
        /// Set the server password
        /// </summary>
        /// <param name="motd">The new password or null</param>
        public void SetPassword(string password)
        {
            _configuration.Password = password;
        }
        /// <summary>
        /// Get the current password
        /// </summary>
        /// <returns>The current password</returns>
        public string GetPassword() => _configuration.Password;

        public IReadOnlyCollection<OWPlayer> GetPlayers() => _players.AsReadOnly();

        /// <summary>
        /// Method that is executed on the <see cref="ThreadPool"/>, and which serves as a message reading loop for the server.
        /// </summary>
        /// <param name="sender">The <see cref="OWServer"/> instance to use</param>
        private static void OnMessageThreadItem(object sender)
        {
            OWServer server = (OWServer)sender;
            NetIncomingMessage incomingMessage = null;
            while (server.IsRunning)
            {
                if (server.IsSleeping)
                    Thread.Sleep(500);

                while ((incomingMessage = server._server.ReadMessage()) != null)
                {
                    server._lastMessageTime = DateTime.Now;

                    if (incomingMessage.MessageType == NetIncomingMessageType.Data)
                        server.PushDataMessage(incomingMessage);
                    else if (incomingMessage.MessageType == NetIncomingMessageType.StatusChanged)
                        server.PushStateMessage(incomingMessage);
                }
            }
        }

        private void ServerLog(string message, bool trace = false) 
            => ServerLogger.Logger?.Log(trace ? LogLevel.TRACE : LogLevel.INFO, message);
    }
}
