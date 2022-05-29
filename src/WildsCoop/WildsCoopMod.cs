using HarmonyLib;
using MelonLoader;
using OuterWildsServer.Network;
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
            LoggerInstance.Msg("Wilds Mods Loaded !");

            /*var owServer = OWServer.CreateServer(new ServerConfiguration { MOTD = "Simple Outer wilds server.", Password="Test" });
            owServer.Start();*/

            MelonDebug.Msg($"DEBUG IS GAME: {OWServer.IsServerInGame}");

            var owClient = new OWClient();
            var result = owClient.ConnectTo("127.0.0.1", timeoutMillisecond: 5000);
            if (!result)
                MelonDebug.Msg("Failed to connect to 127.0.0.1");
            owClient.RequestServerInformation(true);
            owClient.RequestLogin("C4NX");

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
