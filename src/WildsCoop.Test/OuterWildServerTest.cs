using Lidgren.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using WildsCoop.Network;

namespace WildsCoop.Test
{
    [TestClass]
    public class OuterWildServerTest
    {
        public string[] resolvesDirectories = new string[]
        {
            Path.Combine(Environment.GetCommandLineArgs()[0], "/MelonLoader/"),
            Path.Combine(Environment.GetCommandLineArgs()[0], "/Mods/"),
            Path.Combine(Environment.GetCommandLineArgs()[0], "/UserLibs/"),
            Path.Combine(Environment.GetCommandLineArgs()[0], "/OuterWilds_Data/Managed/"),
        };

        public OuterWildServerTest()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                foreach (var item in resolvesDirectories)
                {
                    foreach (var item2 in Directory.GetFiles(item, "*.dll", SearchOption.AllDirectories))
                    {
                        if (Path.GetFileNameWithoutExtension(item2) == new AssemblyName(e.Name).Name)
                        {
                            return Assembly.LoadFile(item2);
                        }
                    }
                }
                return null;
            };
        }

        /// <summary>
        /// Basic server socket running
        /// </summary>
        [TestMethod]
        public void CreateServer()
        {
            var server = OuterWildsServer.CreateServer(new ServerConfiguration());
            server.Start();
            Assert.IsTrue(server.IsRunning);
            server.Dispose();
        }

        [TestMethod]
        public void ListenningMessageToServer()
        {
            var server = OuterWildsServer.CreateServer(new ServerConfiguration() { MOTD = "Hello World"});
            server.Start();

            OuterWildsClient outerWildsClient = new OuterWildsClient();
            Assert.IsTrue(outerWildsClient.ConnectTo("127.0.0.1", timeoutMillisecond: 5000), "Failed to connect to the server");

            outerWildsClient.RequestServerInformation(true);
            WaitForSync(() => !outerWildsClient.HasServerInformation, TimeSpan.FromSeconds(5));

            server.Dispose();
            outerWildsClient.Dispose();
        }

        public void WaitForSync(Func<bool> cond, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (cond())
            {
                Thread.Sleep(500);
                if (stopwatch.Elapsed > timeout)
                {
                    throw new TimeoutException("WaitForSync: Timed out !");
                }
            }
        }
    }
}
