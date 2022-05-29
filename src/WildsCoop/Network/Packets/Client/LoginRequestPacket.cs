using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network.Packets.Client
{
    public class LoginRequestPacket : INetPacket
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GameVersion { get; set; }
        public string ClientVersion { get; set; }

        public bool HasPassword => Password != null && !string.IsNullOrWhiteSpace(Password);

        public void Deserialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(Username);
            netOutgoingMessage.Write(Password);
            netOutgoingMessage.Write(GameVersion);
            netOutgoingMessage.Write(ClientVersion);
        }

        public void Serialize(NetIncomingMessage incomingMessage)
        {
            Username = incomingMessage.ReadString();
            Password = incomingMessage.ReadString();
            GameVersion = incomingMessage.ReadString();
            ClientVersion = incomingMessage.ReadString();
        }

        public override string ToString()
        {
            return $"LoginRequest Username={Username} HasPassword={HasPassword} Password={Password} GameVersion={GameVersion} ClientVersion={ClientVersion}";
        }
    }
}
