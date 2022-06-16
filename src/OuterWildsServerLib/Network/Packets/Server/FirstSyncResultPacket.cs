using Lidgren.Network;
using OuterWildsServer.Network;
using OuterWildsServerLib.Network.Players;
using OuterWildsServerLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Server
{
    /// <summary>
    /// A packet to the client that gives all the configuration information of the server at time T.
    /// To use when the client connects to the game.
    /// 
    /// By default time sync is not active.
    /// </summary>
    public class FirstSyncResultPacket : INetPacket
    {
        public IList<FirstPlayerData> players;
        public float time;
        public bool timeSyncActive = false;


        public FirstSyncResultPacket() { }

        private OWServer m_server;
        private OWPlayer m_player;
        public FirstSyncResultPacket(OWServer server, OWPlayer sender)
        {
            m_server = server;
            m_player = sender;
        }

        public void Serialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(timeSyncActive);
            if (timeSyncActive)
                netOutgoingMessage.Write(time);

            netOutgoingMessage.Write((uint)m_server.PlayerCount);
            foreach (var item in m_server.GetPlayers())
            {
                if (item != m_player)
                {
                    //TODO: May static that !
                    new FirstPlayerData(item)
                        .WriteTo(netOutgoingMessage);
                }
            }
        }

        public void Deserialize(NetIncomingMessage incomingMessage)
        {
            players = new List<FirstPlayerData>();

            if(incomingMessage.ReadBoolean()) // Time sync is active !
                time = incomingMessage.ReadSingle();

            var playerCount = incomingMessage.ReadUInt32();
            for (int i = 0; i < playerCount; i++)
            {
                // read in FirstPlayerData.
                players.Add(new FirstPlayerData().ReadFrom(incomingMessage));    
            }
        }

        public class FirstPlayerData
        {
            public IPlayerData PlayerData { get; set; }
            public Guid Guid { get; set; }


            public FirstPlayerData() { PlayerData = new PlayerDataFields(); }
            public FirstPlayerData(OWPlayer player) { PlayerData = player; Guid = player.GetGuid(); }

            public FirstPlayerData ReadFrom(NetIncomingMessage incomingMessage)
            {
                Guid = incomingMessage.ReadGuid();
                PlayerData.Username = incomingMessage.ReadString();
                PlayerData.Position = incomingMessage.ReadVector3();

                return this;
            }

            public FirstPlayerData WriteTo(NetOutgoingMessage outgoingMessage)
            {
                outgoingMessage.Write(Guid);
                outgoingMessage.Write(PlayerData.Username);
                outgoingMessage.Write(PlayerData.Position);

                return this;
            }
        }
    }
}
