using Lidgren.Network;
using OuterWildsServerLib.Network.Packets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServer.Network.Packets.Server
{
    public class LoginResultPacket : INetPacket
    {
        public bool IsLoggedIn { get; set; }
        public string Message { get; internal set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public void Deserialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(IsLoggedIn);
            netOutgoingMessage.Write(Message);
            if (IsLoggedIn)
            {
                netOutgoingMessage.Write(UserId);
                netOutgoingMessage.Write(Username);
            }
        }

        public void Serialize(NetIncomingMessage incomingMessage)
        {
            IsLoggedIn = incomingMessage.ReadBoolean();
            Message = incomingMessage.ReadString();
            if (IsLoggedIn)
            {
                UserId = incomingMessage.ReadGuid();
                Username = incomingMessage.ReadString();
            }
        }
    }
}
