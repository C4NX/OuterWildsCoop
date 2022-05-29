using OuterWildsServer.Network;
using OuterWildsServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib
{
    public class ProgramMain
    {
        public static OWServer Instance { get; private set; }

        public static void Main(string[] args)
        {
            SimpleLogger.Instance.Info("Starting Server...");

            Instance = OWServer.CreateServer(new ServerConfiguration { PrintLogs=true });

            try
            {
                Instance.Start();
            }
            catch (SocketException ex)
            {
                SimpleLogger.Instance.Error($"Error while starting the server: {ex.Message}");
                return;
            }

            if (Instance.IsRunning)
                SimpleLogger.Instance.Info($"Server is running at 127.0.0.1:{Instance.GetLidgrenServer().Port}");

            while (Instance.IsRunning)
            {
                var cmd = Console.ReadLine();
                if(cmd.Trim().ToUpper() == "STOP")
                {
                    Instance.Stop();
                }
                else if(cmd.Trim().ToUpper() == "STATS")
                {
                    SimpleLogger.Instance.Info($"PT: {Process.GetCurrentProcess().TotalProcessorTime}");
                }
            }
            Instance.Dispose();
        }
    }
}
