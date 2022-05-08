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
        public Uri WebSocketUri { get; private set; }

        public bool HasPassword => Password != null;

        public ServerConnectionInformation(Uri webSocketUri, string password)
        {
            if(webSocketUri == null)
                throw new ArgumentNullException("webSocketUri");

            WebSocketUri = webSocketUri;
            Password = password;
        }
    }
}
