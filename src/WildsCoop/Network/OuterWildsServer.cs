﻿using Lidgren.Network;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WildsCoop.Network.Packets.Client;
using WildsCoop.Network.Packets.Server;
using WildsCoop.Network.Players;

namespace WildsCoop.Network
{
    /// <summary>
    /// A class that represents an OuterWilds server.
    /// </summary>
    public class OuterWildsServer : IDisposable
    {
        public const string LIDGREN_APP_IDENTIFIER = "OUTERWILDS";
        public const int PORT_DEFAULT = 14177;
        public const string VERSION = "OWC 1.0";

        private NetServer _server;
        private List<OwPlayer> _players;
        private NetPacketsProvider _packetProvider;
        private ServerConfiguration _configuration;

        /// <summary>
        /// Get if the server is running, more exactly its socket.
        /// </summary>
        public bool IsRunning => _server != null && _server.Status == NetPeerStatus.Running;

        private OuterWildsServer(ServerConfiguration configuration)
        {
            _configuration = configuration;
            _server = new NetServer(_configuration.CreatePeerConfiguration());
            _packetProvider = new NetPacketsProvider(_server)
                //Add Client/Side Packets
                .AddPacket<ServerInformationRequestPacket>(1)
                .AddPacket<LoginRequestPacket>(2)
                //Add Server/Side Packets
                .AddPacket<ServerInformationPacket>(101)
                .AddPacket<LoginResultPacket>(102);

            _players = new List<OwPlayer>();
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

        /// <summary>
        /// Create an outer wilds server with a specific configuration.
        /// </summary>
        /// <param name="serverConfiguration">The <see cref="ServerConfiguration"/> to be used</param>
        /// <returns></returns>
        public static OuterWildsServer CreateServer(ServerConfiguration serverConfiguration)
        {
            return new OuterWildsServer(serverConfiguration);
        }

        /// <summary>
        /// Closes all communication, free the socket.
        /// </summary>
        public void Dispose()
        {
            _server.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            _server.Socket.Close(5000);
            _server = null;
        }

        /// <summary>
        /// Transform the received data into a readable message with <see cref="NetPacketsProvider"/>, and execute the action associated to this message.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushDataMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"Received {netIncomingMessage.LengthBytes}B from {netIncomingMessage.SenderEndPoint}");

            uint packetId = 0;
            var packetReceived = _packetProvider.Serialize(netIncomingMessage, out packetId);

            if (packetReceived != null)
            {
                if (packetReceived is ServerInformationRequestPacket)
                {
                    //Send information
                    ServerRespond(netIncomingMessage.SenderConnection, new ServerInformationPacket() { 
                        IsDisconnectRequest = ((ServerInformationRequestPacket)packetReceived).WantToDisconnectAfter, 
                        MOTD = _configuration.MOTD, GameVersion=UnityEngine.Application.version 
                    });
                }

                if (packetReceived is LoginRequestPacket)
                {
                    //TODO: Add password checks & more checks & more checks....
                    var loginPacket = (LoginRequestPacket)packetReceived;

                    string loginMessage = "Welcome Home !";
                    bool isLoggedIn = true;

                    //Password is wrong sooo...
                    if (!string.IsNullOrWhiteSpace(_configuration.Password) && loginPacket.Password != _configuration.Password)
                    {
                        loginMessage = "Incorect password.";
                        isLoggedIn = false;
                    }

                    if (isLoggedIn)
                    {
                        //Create the new player and add it.
                        var newPlayer = new OwPlayer(Guid.NewGuid(), netIncomingMessage.SenderConnection, loginPacket.Username);
                        _players.Add(newPlayer);


                        ServerLog($"New Player as joined {newPlayer.GetUsername()}({newPlayer.GetGuid()})");

                        //Send OK, with id,username,message.
                        ServerRespond(netIncomingMessage.SenderConnection, new LoginResultPacket
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
                        ServerRespond(netIncomingMessage.SenderConnection, new LoginResultPacket
                        {
                            IsLoggedIn = false,
                            Message = loginMessage
                        });
                    }
                }

                ServerLog($"Packet is {packetReceived.GetType().FullName} ({packetId})");
            }
            else
            {
                ServerLog($"Ignore {packetId} from {netIncomingMessage.SenderEndPoint}");
            }
        }

        /// <summary>
        /// Execute the action associated to this new state.
        /// </summary>
        /// <param name="netIncomingMessage">Data</param>
        internal void PushStateMessage(NetIncomingMessage netIncomingMessage)
        {
            ServerLog($"State of {netIncomingMessage.SenderEndPoint} changed to {netIncomingMessage.SenderConnection?.Status}");
        }

        /// <summary>
        /// Responds to a <see cref="NetConnection"/> with a <see cref="INetPacket"/>.
        /// </summary>
        /// <param name="netConnection">The <see cref="NetConnection"/></param>
        /// <param name="netPacket">The <see cref="INetPacket"/></param>
        /// <exception cref="ArgumentException">If this packet is not registered in the <see cref="NetPacketsProvider"/></exception>
        /// <returns>If the message was <see cref="NetSendResult.Sent"/> or <see cref="NetSendResult.Queued"/></returns>
        public bool ServerRespond(NetConnection netConnection, INetPacket netPacket)
        {
            var sendResult = netConnection.SendMessage(_packetProvider.Deserialize(netPacket), NetDeliveryMethod.ReliableOrdered, 0);
            return sendResult == NetSendResult.Sent || sendResult == NetSendResult.Queued;
        }

        /// <summary>
        /// Get the current <see cref="NetServer"/> used.
        /// </summary>
        /// <returns></returns>
        public NetServer GetLidgrenServer() => _server;

        /// <summary>
        /// Method that is executed on the <see cref="ThreadPool"/>, and which serves as a message reading loop for the server.
        /// </summary>
        /// <param name="sender">The <see cref="OuterWildsServer"/> instance to use</param>
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
