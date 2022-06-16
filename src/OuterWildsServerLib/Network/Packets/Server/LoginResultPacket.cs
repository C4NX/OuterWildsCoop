using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Server
{
    public class LoginResultPacket : INetPacket
    {
        public bool IsLoggedIn { get; set; }
        public string Message { get; internal set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public void Serialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(IsLoggedIn);
            netOutgoingMessage.Write(Message);
            if (IsLoggedIn)
            {
                netOutgoingMessage.Write(UserId);
                netOutgoingMessage.Write(Username);
            }
        }

        public void Deserialize(NetIncomingMessage incomingMessage)
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
