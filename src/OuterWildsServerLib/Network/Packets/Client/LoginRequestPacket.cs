using Lidgren.Network;
using OuterWildsServerLib.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets.Client
{
    /// <summary>
    /// Use to ask the server for a real connection to play, this implies receiving all the information from other players, etc... 
    /// <list type="number"> 
    /// <item>
    ///    <term>String</term>
    ///    <description>Username</description>
    /// </item>
    /// <item>
    ///    <term>String</term>
    ///    <description>Password</description>
    /// </item>
    /// <item>
    ///    <term>String</term>
    ///    <description>GameVersion, usually <see cref="OWServer.GAME_VERSION"/></description>
    /// </item>
    /// <item>
    ///    <term>String</term>
    ///    <description>ClientVersion</description>
    /// </item>
    /// </list>
    /// </summary>
    public class LoginRequestPacket : INetPacket
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GameVersion { get; set; }
        public string ClientVersion { get; set; }

        public bool HasPassword => Password != null && !string.IsNullOrWhiteSpace(Password);

        public void Serialize(NetOutgoingMessage netOutgoingMessage)
        {
            netOutgoingMessage.Write(Username);
            netOutgoingMessage.Write(Password);
            netOutgoingMessage.Write(GameVersion);
            netOutgoingMessage.Write(ClientVersion);
        }

        public void Deserialize(NetIncomingMessage incomingMessage)
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
