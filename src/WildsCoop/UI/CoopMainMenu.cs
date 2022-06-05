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

namespace WildsCoop.UI
{
    public class CoopMainMenu : MonoBehaviour
    {
        /// <summary>
        /// Set or Get if the host join windows are visible
        /// </summary>
        public bool HostJoinVisible { get; set; }
        /// <summary>
        /// Set or Get if the error message window is visible, please only use <see cref="SetShowErrorMessage(string)"/> to show an error message
        /// </summary>
        public bool ErrorMessageVisible { get; set; }

        private string login_ip = "localhost";
        private string login_password = string.Empty;
        private string login_port = OWServer.PORT_DEFAULT.ToString();

        private string host_ip = "localhost";
        private string host_password = string.Empty;
        private string host_port = OWServer.PORT_DEFAULT.ToString();

        private string _error_message;

        public void OnGUI()
        {
            if (HostJoinVisible)
            {
                // main menu GUI
                GUI.Window(0, new Rect((int)(Screen.width / 1.5 - (500 / 2)), Screen.height / 2 - (500 / 2), 500, 300), (e) =>
                   {
                       GUI.Label(new Rect(5, 15, 490, 20), "IP :");
                       login_ip = GUI.TextField(new Rect(5,30, 490, 20), login_ip);
                       GUI.Label(new Rect(5, 60, 490, 20), "Port :");
                       login_port = GUI.TextField(new Rect(5, 80, 490, 20), login_port);
                       GUI.Label(new Rect(5, 100, 490, 20), "Password :");
                       login_password = GUI.TextField(new Rect(5, 120, 490, 20), login_password);

                       if(GUI.Button(new Rect(5, 245, 495, 50), "Join") && !ErrorMessageVisible)
                       {
                           int port = 0;
                           if (int.TryParse(login_port, out port))
                               WildsCoopMod.Instance.JoinServer(
                                   login_ip, 
                                   port, 
                                   string.IsNullOrWhiteSpace(login_password) ? null : login_password,
                                   (client) => //On Success
                                   {
                                       HostJoinVisible = false;
                                   }, 
                                   (client, loginResult) => //On Fail
                                   {
                                       SetShowErrorMessage($"Login failed : {loginResult.Message}");
                                   });
                           else
                               SetShowErrorMessage("Invalid port.");
                       }
                   }, "Join Game");

                GUI.Window(1, new Rect((int)(Screen.width / 1.5 - (500 / 2)), (Screen.height / 2 - (500 / 2) + 350), 500, 300), (e) =>
                {
                    GUI.Label(new Rect(5, 15, 490, 20), "IP :");
                    host_ip = GUI.TextField(new Rect(5, 30, 490, 20), host_ip);
                    GUI.Label(new Rect(5, 60, 490, 20), "Port :");
                    host_port = GUI.TextField(new Rect(5, 80, 490, 20), host_port);
                    GUI.Label(new Rect(5, 100, 490, 20), "Password :");
                    host_password = GUI.TextField(new Rect(5, 120, 490, 20), host_password);

                    if(GUI.Button(new Rect(5, 245, 495, 50), "Host") && !ErrorMessageVisible)
                    {
                        int port = 0;
                        if (int.TryParse(host_port, out port))
                            WildsCoopMod.Instance.StartNewServer(host_ip, port, string.IsNullOrWhiteSpace(host_password) ? null : host_password);
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
        public void SetShowErrorMessage(string message)
        {
            ErrorMessageVisible = true;
            _error_message = message;
        }
    }
}
