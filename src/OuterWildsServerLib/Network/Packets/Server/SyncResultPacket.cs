using Lidgren.Network;
using OuterWildsServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Server
{
    public class SyncResultPacket : INetPacket
    {
        public void Deserialize(NetOutgoingMessage netOutgoingMessage)
        {
            throw new NotImplementedException();
        }

        public void Serialize(NetIncomingMessage incomingMessage)
        {
            throw new NotImplementedException();
        }
    }
}
