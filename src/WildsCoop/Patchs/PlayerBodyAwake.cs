using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Patchs
{
    [HarmonyPatch(typeof(PlayerBody), "Awake")]
    internal static class PlayerBodyAwake
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GameSync.SyncGameWithServer();
        }
    }
}
