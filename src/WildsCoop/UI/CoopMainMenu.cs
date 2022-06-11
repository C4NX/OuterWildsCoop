using MelonLoader;
using OuterWildsServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildsCoop.Network;
using WildsCoop.Patchs;

namespace WildsCoop.UI
{
    public static class CoopMainMenu
    {
        /// <summary>
        /// Set or Get if the host join windows are visible
        /// </summary>
        public static bool HostJoinVisible { get; set; }
        /// <summary>
        /// Set or Get if the error message window is visible, please only use <see cref="SetShowErrorMessage(string)"/> to show an error message
        /// </summary>
        public static bool ErrorMessageVisible { get; set; }

        internal static string login_ip = "localhost";
        internal static string login_password = string.Empty;
        internal static string login_port = OWServer.PORT_DEFAULT.ToString();
        internal static string login_username = string.Empty;

        internal static string host_username = string.Empty;
        internal static string host_password = string.Empty;
        internal static string host_port = OWServer.PORT_DEFAULT.ToString();

        private static string _error_message;

        public static void FillWithPrefs()
        {
            host_username = CoopModPrefs.GetUsernamePref();
            login_username = CoopModPrefs.GetUsernamePref();
        }

        public static void OnGUI()
        {
            if (HostJoinVisible)
            {
                // main menu GUI
                GUI.Window(0, new Rect((int)(Screen.width / 1.5 - (500 / 2)), Screen.height / 2 - (500 / 2), 500, 300), (e) =>
                   {
                       GUI.Label(new Rect(5, 15, 490, 20), "IP :");
                       login_ip = GUI.TextField(new Rect(5,35, 490, 20), login_ip);
                       GUI.Label(new Rect(5, 55, 490, 20), "Port :");
                       login_port = GUI.TextField(new Rect(5, 75, 490, 20), login_port);
                       GUI.Label(new Rect(5, 95, 490, 20), "Password :");
                       login_password = GUI.TextField(new Rect(5, 115, 490, 20), login_password);
                       GUI.Label(new Rect(5, 135, 490, 20), "Username :");
                       login_username = GUI.TextField(new Rect(5, 155, 490, 20), login_username);

                       if (GUI.Button(new Rect(5, 245, 495, 50), "Join") && !ErrorMessageVisible)
                       {
                           JoinServerAction();
                       }
                   }, "Join Game");

                GUI.Window(1, new Rect((int)(Screen.width / 1.5 - (500 / 2)), (Screen.height / 2 - (500 / 2) + 350), 500, 300), (e) =>
                {
                    GUI.Label(new Rect(5, 15, 490, 20), "Username :");
                    host_username = GUI.TextField(new Rect(5, 35, 490, 20), host_username);
                    GUI.Label(new Rect(5, 55, 490, 20), "Port :");
                    host_port = GUI.TextField(new Rect(5, 75, 490, 20), host_port);
                    GUI.Label(new Rect(5, 95, 490, 20), "Password :");
                    host_password = GUI.TextField(new Rect(5, 115, 490, 20), host_password);

                    if(GUI.Button(new Rect(5, 245, 495, 50), "Host") && !ErrorMessageVisible)
                    {
                        int port = 0;
                        if (int.TryParse(host_port, out port))
                        {
                            // start the server and run join !
                            WildsCoopMod.Instance.StartNewServer(port, string.IsNullOrWhiteSpace(host_password) ? null : host_password);
                            login_username = host_username;
                            login_password = host_password;
                            login_port = host_port;
                            login_ip = "localhost";

                            JoinServerAction();
                        }
                        else
                            SetShowErrorMessage("Invalid port.");

                    }
                }, "Host Game");
            }

            if (ErrorMessageVisible)
            {
                GUI.Window(10, new Rect((int)(Screen.width / 2 - (300 / 2)), (int)(Screen.height / 2 - (200 / 2)), 300, 200), (e) =>
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(10, 20, 290, 190), _error_message);
                    GUI.color = Color.white;
                    if (GUI.Button(new Rect(5, 145, 290, 50), "OK"))
                        ErrorMessageVisible = false;
                }, "Error");
            }
        }

        /// <summary>
        /// Set visible an error message on the center of the screen with <see cref="GUI"/>
        /// </summary>
        /// <param name="message"></param>
        public static void SetShowErrorMessage(string message)
        {
            ErrorMessageVisible = true;
            _error_message = message;
        }

        public static void JoinServerAction()
        {
            int port = 0;
            if (int.TryParse(login_port, out port))
                WildsCoopMod.Instance.JoinServer(
                    login_ip,
                    port,
                    string.IsNullOrWhiteSpace(login_password) ? null : login_password,
                    login_username,
                    (client) => //On Success
                                   {
                                       //TODO: Add Join World, sync, etc....

                                       CoopModPrefs.SetUsernamePref(login_username);

                        GameSync.Client = client;
                        if (!WildsCoopMod.Instance.TitleMenu.ResumeGame())
                            WildsCoopMod.Instance.TitleMenu.NewGame();


                        HostJoinVisible = false;
                    },
                    (client, loginResult) => //On Fail
                                   {
                        SetShowErrorMessage($"Login failed : {loginResult.Message}");
                    });
            else
                SetShowErrorMessage("Invalid port.");
        }
    }
}
