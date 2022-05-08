using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network
{
    public class ServerConnectionInformation
    {
        public string Password { get; private set; }
        public int Port { get; private set; }
        public IPAddress IP { get; private set; }

        public bool HasPassword => Password != null;

        public ServerConnectionInformation(IPAddress address, int port, string password)
        {
            if(address == null)
                throw new ArgumentNullException("address");

            IP = address;
            Password = password;
            Port = port;
        }
    }
}
