using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network.Packets.Server
{
    public class ServerInformationPacket : INetPacket
    {
        public string GameVersion { get; set; }
        public string MOTD { get; set; }
        public bool IsDisconnectRequest { get; set; }

        public void Deserialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(IsDisconnectRequest);
            netOutgoingMessage.Write(GameVersion);
            netOutgoingMessage.Write(MOTD);
        }

        public void Serialize(NetIncomingMessage incomingMessage)
        {
            IsDisconnectRequest = incomingMessage.ReadBoolean();
            GameVersion = incomingMessage.ReadString();
            MOTD = incomingMessage.ReadString();
        }

        public override string ToString()
        {
            return $"Server information: MOTD={MOTD} GAME_VERSION={GameVersion}";
        }
    }
}
