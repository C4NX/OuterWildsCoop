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
                Console.Write('>');
                var cmdargs = Console.ReadLine().Split('"')
                                             .Select((element, index) => index % 2 == 0
                                             ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                             : new string[] { element })
                                              .SelectMany(element => element).ToList();
                if (cmdargs.Count >= 1)
                {
                    var cmdname = cmdargs[0].ToUpper();

                    switch (cmdname)
                    {
                        case "STOP":
                            Instance.Stop();
                            break;
                        case "CLEAR":
                            Console.Clear();
                            break;
                        case "STATS":
                            SimpleLogger.Instance.Info(GetServerInfoString());
                            break;
                        case "MOTD":
                            var newMotd = cmdargs.ElementAtOrDefault(1);
                            if (newMotd == null)
                            {
                                SimpleLogger.Instance.Info($"MOTD is '{Instance.GetMOTD()}'");
                            }
                            else
                            {
                                Instance.SetMOTD(newMotd);
                                SimpleLogger.Instance.Info($"MOTD was set to '{Instance.GetMOTD()}'");
                            }
                            break;
                        default:
                            SimpleLogger.Instance.Info($"Command '{cmdname}' not found.");
                            break;
                    }
                }
            }
            Instance.Dispose();
        }

        public static string GetServerInfoString()
        {
            StringBuilder sb = new StringBuilder();

            var currentProcess = Process.GetCurrentProcess();

            sb.AppendLine();
            sb.AppendLine("-----PROCESS-----");
            sb.AppendLine($"PT: {currentProcess.TotalProcessorTime}");
            sb.AppendLine($"Threads: {currentProcess.Threads.Count}");
            sb.AppendLine($"Memory: {currentProcess.PrivateMemorySize64 / 1000000}mo / {currentProcess.PeakVirtualMemorySize64 / 1000000}mo");
            return sb.ToString();
        }
    }
}
