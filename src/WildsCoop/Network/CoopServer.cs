using Lidgren.Network;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network
{
    public static class CoopServer
    {
        private static NetServer _netServer;
        private static NetPacketProcessor _packetProcessor;

        public static async Task<bool> Start(ServerConnectionInformation connectionInformation)
        {
            _netServer = new NetServer(new NetPeerConfiguration("Outer Wilds")
            {
                NetworkThreadName = "Server Network Thread",
                Port = connectionInformation.Port
            });

            MelonDebug.Msg($"Server starting for {_netServer.Configuration.LocalAddress}:{_netServer.Configuration.Port}");

            _netServer.Start();

            while (_netServer.Status != NetPeerStatus.Running)
            {
                MelonDebug.Msg($"Waiting server to start... ({_netServer.Status})");
                await Task.Delay(100);
            }

            _packetProcessor = new NetPacketProcessor(_netServer);

            _packetProcessor.Start();

            MelonDebug.Msg($"Server started for {_netServer.Configuration.LocalAddress}:{_netServer.Configuration.Port}");

            return true;
        }
    }
}
