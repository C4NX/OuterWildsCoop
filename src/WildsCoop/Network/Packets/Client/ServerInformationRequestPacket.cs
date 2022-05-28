using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network.Packets.Client
{
    /// <summary>
    /// Used to ask the server for this information, must send back a <see cref="Server.ServerInformationPacket"/>
    /// <list type="number"> 
    /// <item>
    ///    <term>String</term>
    ///    <description>ClientVersion</description>
    /// </item>
    /// <item>
    ///    <term>Bool</term>
    ///    <description>WantToDisconnectAfter</description>
    /// </item>
    /// </list>
    /// </summary>
    public class ServerInformationRequestPacket : INetPacket
    {
        public string ClientVersion { get; set; }
        public bool WantToDisconnectAfter { get; set; }

        public void Deserialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(ClientVersion);
            netOutgoingMessage.Write(WantToDisconnectAfter);
        }

        public void Serialize(NetIncomingMessage incomingMessage)
        {
            ClientVersion = incomingMessage.ReadString();
            WantToDisconnectAfter = incomingMessage.ReadBoolean();
        }
    }
}
