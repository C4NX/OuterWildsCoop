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
        /// The current instance of <see cref="WildsCoopMod"/> created by melon.
        /// </summary>
        public static WildsCoopMod Instance { get; private set; }

        /// <summary>
        /// A <see cref="GameObject"/> that contains all of the 'static' <see cref="Component"/> and <see cref="MonoBehaviour"/>
        /// </summary>
        private GameObject wildCoopGlobal;
        /// <summary>
        /// A <see cref="GameObject"/> that contains all main menu scene <see cref="Component"/> and <see cref="MonoBehaviour"/>
        /// </summary>
        private GameObject wildCoopMainMenu;
        /// <summary>
        /// The <see cref="CoopMainMenu"/> <see cref="MonoBehaviour"/> script to manage server connection and hosting gui, destroy when an other scene is starting.
        /// </summary>
        private CoopMainMenu mainMenuGUI;

        public WildsCoopMod()
        {
            Instance = this;
        }

        public override void OnApplicationStart()
        {
            ServerLogger.Logger?.AddWriter(new MelonLogWriter());

            LoggerInstance.Msg("The unofficial multiplayer mod for OuterWilds is now active !");

            base.OnApplicationStart();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (wildCoopGlobal == null)
            {
                wildCoopGlobal = new GameObject("WildsCoopMod");
                GameObject.DontDestroyOnLoad(wildCoopGlobal);
            }

            LoggerInstance.Msg($"Scene was loaded {buildIndex} '{sceneName}'");
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public void StartNewServer(string hostname, int port, string password)
        {
            var owServer = OWServer.CreateServer(new ServerConfiguration() { Port = port, Password = password });
            owServer.Start();
        }

        private OWClient currentClient;

        public void JoinServer(string hostname, int port, string password, ClientEventHandler onLogged, ClientFromPacketEventHandler<LoginResultPacket> onLoginFail)
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


                currentClient.RequestLogin("C4NX", password);
            }
        }

        /// <summary>
        /// Before all that fade button animation start
        /// </summary>
        /// <param name="screenManager">The current <see cref="TitleScreenManager"/> instance</param>
        /// <param name="animationController">The current <see cref="TitleAnimationController"/> used</param>
        internal void OnMenuPreFade(TitleScreenManager screenManager, TitleAnimationController animationController)
        {
            var joinHostButton = OWUtilities.CloneMainButton("JOIN / HOST COOP GAME", 3);

            OWUtilities.AddButtonToFadeGroup(joinHostButton, animationController);
            OWUtilities.ButtonResetClickEvent(joinHostButton, () => { mainMenuGUI.HostJoinVisible = !mainMenuGUI.HostJoinVisible; });

            wildCoopMainMenu = new GameObject("WildsCoopModMainMenu");
            mainMenuGUI = wildCoopMainMenu.AddComponent<CoopMainMenu>();
        }

        /// <summary>
        /// After all that fade button animation start
        /// </summary>
        internal void OnMenuPostFade()
        {
            GameObject.Find("VersionText").GetComponent<Text>().text += $" | {Instance.Info.Name} {Instance.Info.Version}";
        }
    }
}
