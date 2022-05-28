using Lidgren.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using WildsCoop.Network;

namespace WildsCoop.Test
{
    [TestClass]
    public class OuterWildServerTest
    {
        /// <summary>
        /// Basic server socket running
        /// </summary>
        [TestMethod]
        [TestCategory("Base")]
        public void CreateServer()
        {
            var server = OuterWildsServer.CreateServer(new ServerConfiguration());
            server.Start();
            Assert.IsTrue(server.IsRunning);
            server.Dispose();
        }

        [TestMethod]
        [TestCategory("Base")]
        public void ListenningMessageToServer()
        {
            var server = OuterWildsServer.CreateServer(new ServerConfiguration());
            server.Start();

            Assert.IsTrue(server.IsRunning);

            ThreadPool.QueueUserWorkItem(FakeThreadThread, server.GetLidgrenServer());

            NetClient netClient = new NetClient(new NetPeerConfiguration(OuterWildsServer.LIDGREN_APP_IDENTIFIER)
            {
                Port = OuterWildsServer.PORT_DEFAULT-1
            });
            netClient.Start();
            WaitForPeer(netClient, TimeSpan.FromSeconds(5));

            var serverConnection = netClient.Connect("127.0.0.1", OuterWildsServer.PORT_DEFAULT);
            ThreadPool.QueueUserWorkItem(FakeThreadThread, netClient);

            WaitForConnect(serverConnection, TimeSpan.FromSeconds(5));

            var sendResult = serverConnection.SendMessage(netClient.CreateMessage("Hello World"), NetDeliveryMethod.ReliableOrdered, 0);
            Assert.IsTrue(sendResult == NetSendResult.Sent || sendResult == NetSendResult.Queued, $"Impossible d'envoyer un message: {sendResult}");

            netClient.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            netClient.Socket.Close(5000);
        }

        public static void WaitForPeer(NetPeer netPeer, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (netPeer.Status != NetPeerStatus.Running)
            {
                Thread.Sleep(500);
                if (stopwatch.Elapsed > timeout)
                    throw new TimeoutException("WaitForPeer: Peer timed-out");
            }
        }

        public static void WaitForConnect(NetConnection netConnection, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (netConnection.Status != NetConnectionStatus.Connected)
            {
                Thread.Sleep(500);
                if (stopwatch.Elapsed > timeout)
                    throw new TimeoutException("WaitForConnect: Connection timed-out");
            }
        }

        private void FakeThreadThread(object clientOrServerSender)
        {
            bool isServer = clientOrServerSender is NetServer;
            NetIncomingMessage netIncomingMessage = null;

            if (isServer)
            {
                while (((NetServer)clientOrServerSender).Status == NetPeerStatus.Running)
                {
                    while ((netIncomingMessage = ((NetServer)clientOrServerSender).ReadMessage()) != null)
                    {
                        if(netIncomingMessage.MessageType == NetIncomingMessageType.StatusChanged)
                        {
                            if (netIncomingMessage.SenderConnection.Status == NetConnectionStatus.Connected)
                            {
                                Console.WriteLine($"[SERVER] Client {netIncomingMessage.SenderEndPoint} connected");
                            }
                        }
                        Console.WriteLine($"[SERVER] Event: {netIncomingMessage.MessageType} ({netIncomingMessage.SenderConnection?.Status})");
                        Console.WriteLine($"[SERVER] Received from client: {((clientOrServerSender is NetServer) ? "Server" : "Client")}: {netIncomingMessage.Data.Length} bytes");
                    }
                }
            }
            else
            {
                while (((NetClient)clientOrServerSender).Status == NetPeerStatus.Running)
                {
                    while ((netIncomingMessage = ((NetClient)clientOrServerSender).ReadMessage()) != null)
                    {
                        Console.WriteLine($"[CLIENT] Event: {netIncomingMessage.MessageType} ({netIncomingMessage.SenderConnection.Status})");
                        Console.WriteLine($"[CLIENT] Received from Server: {netIncomingMessage.Data.Length} bytes");
                    }
                }
            }
        }
    }
}
