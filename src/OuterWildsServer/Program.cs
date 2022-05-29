using OuterWildsServer.Network;
using OuterWildsServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServer
{
    public class Program
    {
        public static OWServer Instance { get; private set; }
        static void Main(string[] args)
        {
            SimpleLogger.Instance.Info("Starting Server...");

            Instance = OWServer.CreateServer(new ServerConfiguration { PrintLogs=true });
            Instance.Start();

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
