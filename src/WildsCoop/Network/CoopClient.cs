using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace WildsCoop.Network
{
    public static class CoopClient
    {
        private static WebsocketClient _webSocket;

        private static CancellationTokenSource _connectConcellationTokenSource;

        public static void CancelConnect() => _connectConcellationTokenSource.Cancel();

        public static async Task<bool> ConnectAsync(ServerConnectionInformation connectionInformation)
        {
            _webSocket = new WebsocketClient(connectionInformation.WebSocketUri);

            try
            {
                Log($"Connecting to {connectionInformation.WebSocketUri}");
                await _webSocket.StartOrFail();


            }
            catch (Exception ex)
            {
                Log($"ConnectAsync: Websocket error for {connectionInformation.WebSocketUri} ({ex.GetType().FullName})");
                return false;
            }

            return _webSocket.IsRunning;
        }

        internal static void Log(string str) => MelonDebug.Msg($"[WEBSOCKET] {str}");
    }
}
