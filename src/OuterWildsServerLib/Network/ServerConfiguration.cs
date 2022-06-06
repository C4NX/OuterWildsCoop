using Lidgren.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServer.Network
{
    public class ServerConfiguration
    {
        public const string DEFAULT_MOTD = "A wilds coop server.";

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("motd")]
        public string MOTD { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public ServerConfiguration()
        {
            Port = OWServer.PORT_DEFAULT;
            MOTD = DEFAULT_MOTD;
            Password = null;
        }

        internal NetPeerConfiguration CreatePeerConfiguration()
        {
            return new NetPeerConfiguration(OWServer.LIDGREN_APP_IDENTIFIER)
            {
                AutoFlushSendQueue = true,
                NetworkThreadName = "Server-Network",
                ConnectionTimeout = 10,
                PingInterval = 5,
                Port = Port,
            };
        }

        /// <summary>
        /// Read configuration from a json file.
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>The readed server configuration</returns>
        /// <exception cref="JsonException">If the json failed</exception>
        public static ServerConfiguration ReadFromFile(string filename)
        {
            using (FileStream fileStream = File.OpenRead(filename))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    return JsonConvert.DeserializeObject<ServerConfiguration>(reader.ReadToEnd());
                }
            }
        }
    }
}
