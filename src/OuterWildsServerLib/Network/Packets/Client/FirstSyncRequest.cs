using Lidgren.Network;
using OuterWildsServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Client
{

    public class FirstSyncRequest : INetPacket
    {
        public void Serialize(NetOutgoingMessage netOutgoingMessage)
        {
        }

        public void Deserialize(NetIncomingMessage incomingMessage)
        {
        }
    }
}
