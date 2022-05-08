using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace WildsCoop.Patchs
{
    [HarmonyPatch(typeof(TitleScreenManager), "FadeInMenuOptions")]
    internal static class FadeInMenuOptions
    {
        [HarmonyPostfix]
        public static void Postfix() => WildsCoopMod.Instance?.OnMenuPostFade();

        [HarmonyPrefix]
        public static void Prefix(TitleScreenManager __instance)
        {
            var gfxController = (TitleAnimationController)AccessTools.Field(typeof(TitleScreenManager), "_gfxController").GetValue(__instance);

            WildsCoopMod.Instance?.OnMenuPreFade(__instance, gfxController);
        }
    }
}
