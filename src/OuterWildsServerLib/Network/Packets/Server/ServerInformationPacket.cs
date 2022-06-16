using Lidgren.Network;
using OuterWildsServerLib.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Server
{
    /// <summary>
    /// Used to give to the client information from the server, must be issued from a <see cref="Client.ServerInformationRequestPacket"/>
    /// <list type="number"> 
    /// <item>
    ///    <term>Bool</term>
    ///    <description>IsDisconnectRequest</description>
    /// </item>
    /// <item>
    ///    <term>String</term>
    ///    <description>GameVersion</description>
    /// </item>
    /// <item>
    ///    <term>String</term>
    ///    <description>MOTD</description>
    /// </item>
    /// </list>
    /// </summary>
    public class ServerInformationPacket : INetPacket
    {
        public string GameVersion { get; set; }
        public string MOTD { get; set; }
        public bool IsDisconnectRequest { get; set; }

        public void Serialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(IsDisconnectRequest);
            netOutgoingMessage.Write(GameVersion);
            netOutgoingMessage.Write(MOTD);
        }

        public void Deserialize(NetIncomingMessage incomingMessage)
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
