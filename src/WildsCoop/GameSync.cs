using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildsCoop.Network;

namespace WildsCoop
{
    public static class GameSync
    {
        public static OWClient Client { get; internal set; }

        public static void SyncGameWithServer()
        {
            if (Client != null && Client.IsConnected)
            {
                //TODO: Say to the server to send the info of the current game !
                MelonLoader.MelonDebug.Msg("Sync request sent !");
                Client.RequestSync();
            }
        }
    }
}
