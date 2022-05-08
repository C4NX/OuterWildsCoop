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
            if (_webSocket != null && _webSocket.IsRunning)
                return true;

            _webSocket = new WebsocketClient(connectionInformation.WebSocketUri);

            try
            {
                Log($"Connecting to {connectionInformation.WebSocketUri}");
                await _webSocket.StartOrFail();


            }
            catch (Exception ex)
            {
                Log($"ConnectAsync: Websocket error for {connectionInformation.WebSocketUri} ({ex.Message})");
                return false;
            }

            _webSocket.MessageReceived.Subscribe((e) =>
            {
                Log($"WebPacket received : {e.MessageType} {(e.MessageType == System.Net.WebSockets.WebSocketMessageType.Text ? e.Text : e.Binary.Length.ToString())}");
            });

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    _webSocket.Send("Hello World");
                    await Task.Delay(5 * 1000);
                }
            }).ConfigureAwait(false);

            if (_webSocket.IsRunning)
            {
                _webSocket.Send("Hello World");
            }

            return _webSocket.IsRunning;
        }

        internal static void Log(string str) => MelonDebug.Msg($"[WEBSOCKET] {str}");
    }
}
