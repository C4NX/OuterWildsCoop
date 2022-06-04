using Newtonsoft.Json;
using OuterWildsServer.Network;
using OuterWildsServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            ServerConfiguration serverConfiguration = new ServerConfiguration();

            try
            {
                if (File.Exists("owserver.json"))
                    serverConfiguration = ServerConfiguration.ReadFromFile("owserver.json");
            }
            catch (JsonException ex)
            {
                SimpleLogger.Instance.Error($"Failed to load the server configuration : {ex.Message}");
            }
            finally
            {
                serverConfiguration.PrintLogs = true;
            }

            Instance = OWServer.CreateServer(serverConfiguration);

            try
            {
                Instance.Start();
            }
            catch (SocketException ex)
            {
                switch (ex.SocketErrorCode)
                {
                    case SocketError.AddressAlreadyInUse:
                        SimpleLogger.Instance.Warning($"You could not use the port '{Instance.GetLidgrenServer().Configuration.Port}', because it is already in use");
                        break;
                }
                SimpleLogger.Instance.Fatal($"Error while starting the server: {ex.Message}");
                return;
            }

            if (Instance.IsRunning)
                SimpleLogger.Instance.Info($"Server is running at {Instance.GetLidgrenServer().Configuration.LocalAddress}:{Instance.GetLidgrenServer().Port}");

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
                        case "PASSWORD":
                            var newPassword = cmdargs.ElementAtOrDefault(1);
                            if (newPassword == null)
                            {
                                SimpleLogger.Instance.Info($"Password is '{Instance.GetPassword() ?? "<null>"}'");
                            }
                            else
                            {
                                var passwordUpperTrimed = newPassword.Trim().ToUpper();
                                newPassword = (passwordUpperTrimed == "NULL" || passwordUpperTrimed == "OFF" || passwordUpperTrimed == "NO") ? null : newPassword;

                                Instance.SetPassword(newPassword);
                                SimpleLogger.Instance.Info($"Password was set to '{Instance.GetPassword() ?? "<null>"}'");
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

            sb.AppendLine()
                .AppendLine();
            sb.AppendLine("-----PROCESS-----");
            sb.AppendLine($"PT: {currentProcess.TotalProcessorTime}");
            sb.AppendLine($"Threads: {currentProcess.Threads.Count}");
            sb.AppendLine($"Memory: {currentProcess.PrivateMemorySize64 / 1000000}mo / {currentProcess.PeakVirtualMemorySize64 / 1000000}mo");
            return sb.ToString();
        }
    }
}
