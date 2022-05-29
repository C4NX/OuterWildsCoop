using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace OuterWildsServer.Network
{
    public static class LidgrenUtils
    {
        public static void Write(this NetOutgoingMessage netOutgoingMessage, Guid guid) => netOutgoingMessage.Write(guid.ToByteArray());
        public static Guid ReadGuid(this NetIncomingMessage netIncomingMessage) => new Guid(netIncomingMessage.ReadBytes(16));
    }
}
