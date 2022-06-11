using MelonLoader;
using OuterWildsServer.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop
{
    public static class CoopModPrefs
    {
        public const string MELONPREF_CTG = "OWC";

        private static MelonPreferences_Entry<string> _username;
        private static MelonPreferences_Entry<string> _motd;

        public static void InitialisePrefs()
        {
            MelonPreferences_Category modCtg;

            if (!MelonPreferences.Categories.Exists(x => x.Identifier == MELONPREF_CTG))
            {
                modCtg = MelonPreferences.CreateCategory(MELONPREF_CTG, display_name: "WildsCoop Mod");
                modCtg.SetFilePath(Path.Combine("UserData\\","WildsCoop.cfg"), true, true);
            }
            else
                modCtg = MelonPreferences.GetCategory(MELONPREF_CTG);


            if (!MelonPreferences.HasEntry(MELONPREF_CTG, "username"))
            {
                _username = modCtg.CreateEntry("username", $"Esker_{new System.Random().Next(10000)}",
                    description: "Username in the textbox of the connection / host menu.");
            }
            else
            {
                _username = modCtg.GetEntry<string>("username");
            }

            if (!MelonPreferences.HasEntry(MELONPREF_CTG, "motd"))
            {
                _motd = modCtg.CreateEntry("motd", ServerConfiguration.DEFAULT_MOTD,
                    description: "The self host message of the day !");
            }
            else
            {
                _motd = modCtg.GetEntry<string>("motd");
            }

            modCtg.SaveToFile();
        }

        public static string GetUsernamePref() => _username.Value;
        public static void SetUsernamePref(string newUsername) => _username.Value = newUsername;

        public static string GetMOTD() => _motd.Value;
    }
}
