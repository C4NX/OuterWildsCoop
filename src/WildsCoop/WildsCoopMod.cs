using HarmonyLib;
using MelonLoader;
using OuterWildsServer.Network;
using OuterWildsServer.Network.Packets.Server;
using OuterWildsServerLib.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WildsCoop.Network;
using WildsCoop.UI;
using WildsCoop.Utils;

namespace WildsCoop
{
    /// <summary>
    /// The main class used by melon loader.
    /// </summary>
    public class WildsCoopMod : MelonMod
    {
        /// <summary>
        /// The current instance of <see cref="WildsCoopMod"/> initialized by melon.
        /// </summary>
        public static WildsCoopMod Instance { get; private set; }

        public TitleMenu TitleMenu { get; set; }

        public WildsCoopMod()
        {
            Instance = this;
        }

        public override void OnApplicationStart()
        {
            ServerLogger.Logger?.AddWriter(new MelonLogWriter());

            CoopModPrefs.InitialisePrefs();

            LoggerInstance.Msg("The unofficial multiplayer mod for OuterWilds is now active !");

            base.OnApplicationStart();
        }

        public override void OnGUI()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (currentClient != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("[CLIENT]");
                stringBuilder.AppendLine($"IsSleeping: {currentClient.IsSleeping}");
                stringBuilder.AppendLine($"IsConnected: {currentClient.IsConnected}");
                if (currentClient.IsConnected)
                {
                    stringBuilder.AppendLine($"Username: {currentClient.GetUsername() ?? "Requesting..."}");
                }
            }

            if (currentServer != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("[SERVER]");
                stringBuilder.AppendLine($"IsSleeping: {currentServer.IsSleeping}");
                stringBuilder.AppendLine($"MOTD: {currentServer.GetMOTD()}");
                stringBuilder.AppendLine($"Players: {currentServer.PlayerCount}");
            }
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"{OWServer.SERVER_VERSION}\n{stringBuilder}");

            CoopMainMenu.OnGUI();
            base.OnGUI();
        }

        public void StartNewServer(int port, string password)
        {
            if (currentServer != null && currentServer.IsRunning)
                return;

            var owServer = OWServer.CreateServer(new ServerConfiguration() { Port = port, Password = password, MOTD = CoopModPrefs.GetMOTD() });
            owServer.Start();
            currentServer = owServer;
        }

        private OWClient currentClient;
        private OWServer currentServer;

        public void JoinServer(string hostname, int port, string password, string username, ClientEventHandler onLogged, ClientFromPacketEventHandler<LoginResultPacket> onLoginFail)
        {
            if (currentClient != null && currentClient.IsLogged)
                return;

            currentClient = new OWClient();
            MelonDebug.Msg($"Attempt to connect to {hostname}:{port}");
            if (!currentClient.ConnectTo(hostname, port, 10000))
                currentClient = null;
            else
            {
                currentClient.OnLogged += onLogged;
                currentClient.OnConnectFail = onLoginFail;


                currentClient.RequestLogin(username, password);
            }
        }

        /// <summary>
        /// Before all that fade button animation start
        /// </summary>
        /// <param name="screenManager">The current <see cref="TitleScreenManager"/> instance</param>
        /// <param name="animationController">The current <see cref="TitleAnimationController"/> used</param>
        internal void OnMenuPreFade(TitleScreenManager screenManager, TitleAnimationController animationController)
        {
            TitleMenu = new TitleMenu(screenManager);


            var joinHostButton = OWUtilities.CloneMainButton("JOIN / HOST COOP GAME", 3);

            OWUtilities.AddButtonToFadeGroup(joinHostButton, animationController);
            OWUtilities.ButtonResetClickEvent(joinHostButton, () => {
                CoopMainMenu.FillWithPrefs();
                CoopMainMenu.HostJoinVisible = !CoopMainMenu.HostJoinVisible; 
            });
        }

        /// <summary>
        /// After all that fade button animation start
        /// </summary>
        internal void OnMenuPostFade()
        {
            GameObject.Find("VersionText").GetComponent<Text>().text += $" | {Instance.Info.Name} {Instance.Info.Version} ; {OWServer.SERVER_VERSION}";
        }
    }
}
