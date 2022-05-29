using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network
{
    public class ServerConfiguration
    {
        public const string DEFAULT_MOTD = "A wilds coop server.";

        private int _port;

        public int Port => _port;
        public string MOTD { get; set; }
        public string Password { get; set; }

        public ServerConfiguration(int port = OuterWildsServer.PORT_DEFAULT)
        {
            _port = port;
            MOTD = DEFAULT_MOTD;
            Password = null;
        }

        internal NetPeerConfiguration CreatePeerConfiguration()
        {
            return new NetPeerConfiguration(OuterWildsServer.LIDGREN_APP_IDENTIFIER)
            {
                AutoFlushSendQueue = true,
                NetworkThreadName = "Server-Network",
                ConnectionTimeout = 10,
                PingInterval = 5,
                Port = _port,
            };
        }
    }
}
