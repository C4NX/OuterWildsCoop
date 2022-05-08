using MelonLoader;
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

        private string _ip = "localhost";
        private string _password = string.Empty;
        private string _port = "5050";

        private string _error_message;

        public void OnGUI()
        {
            if (HostJoinVisible)
            {
                // main menu GUI
                GUI.Window(0, new Rect((int)(Screen.width / 1.5 - (500 / 2)), Screen.height / 2 - (500 / 2), 500, 300), (e) =>
                   {
                       GUI.Label(new Rect(5, 15, 490, 20), "IP :");
                       _ip = GUI.TextField(new Rect(5,30, 490, 20), _ip);
                       GUI.Label(new Rect(5, 60, 490, 20), "Port :");
                       _port = GUI.TextField(new Rect(5, 80, 490, 20), _port);
                       GUI.Label(new Rect(5, 100, 490, 20), "Password :");
                       _password = GUI.TextField(new Rect(5, 120, 490, 20), _password);

                       if(GUI.Button(new Rect(5, 245, 495, 50), "Join"))
                       {
                           var connectionInformation = GetServerConnectionInformation(true);
                           if(connectionInformation != null)
                               SetShowErrorMessage($"WIP: from {connectionInformation.IP}:{connectionInformation.Port} with password '{connectionInformation.Password ?? "<null>"}'");
                       }
                   }, "Join Game");

                GUI.Window(1, new Rect((int)(Screen.width / 1.5 - (500 / 2)), (Screen.height / 2 - (500 / 2) + 350), 500, 300), (e) =>
                {
                    GUI.Label(new Rect(5, 15, 490, 20), "IP :");
                    _ip = GUI.TextField(new Rect(5, 30, 490, 20), _ip);
                    GUI.Label(new Rect(5, 60, 490, 20), "Port :");
                    _port = GUI.TextField(new Rect(5, 80, 490, 20), _port);
                    GUI.Label(new Rect(5, 100, 490, 20), "Password :");
                    _password = GUI.TextField(new Rect(5, 120, 490, 20), _password);

                    if(GUI.Button(new Rect(5, 245, 495, 50), "Host") && !ErrorMessageVisible)
                    {
                        var connectionInformation = GetServerConnectionInformation(true);
                        if (connectionInformation != null)
                            SetShowErrorMessage($"WIP: host from {connectionInformation.IP}:{connectionInformation.Port} with password '{connectionInformation.Password ?? "<null>"}'");
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

        public ServerConnectionInformation GetServerConnectionInformation(bool showError = true)
        {
            IPAddress ip = null;
            int port = 0;
            string password = string.IsNullOrWhiteSpace(_password) ? null : _password;

            if (!IPAddress.TryParse(_ip, out ip))
            {
                try
                {
                    var firstDnsAddress = Dns.GetHostAddresses(_ip).FirstOrDefault();
                    if (firstDnsAddress == null)
                    {
                        if(showError) 
                            SetShowErrorMessage($"Host not found '{_ip}'");
                        return null;
                    }
                    else
                    {
                        ip = firstDnsAddress;
                    }
                }
                catch (SocketException)
                {
                    if (showError)
                        SetShowErrorMessage($"Host not found '{_ip}'");
                    return null;
                }
            }
            if (!int.TryParse(_port, out port))
            {
                if (showError)
                    SetShowErrorMessage("You must enter a valid port number !");
                return null;
            }

            return new ServerConnectionInformation(ip, port, password);
        }
    }
}
