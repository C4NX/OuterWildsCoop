using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.UI
{
    public class TitleMenu
    {
        private TitleScreenManager titleScreenManager;

        public TitleMenu(TitleScreenManager titleScreenManager)
        {
            this.titleScreenManager = titleScreenManager;
        }

        /// <summary>
        /// Resume your last outer wilds game
        /// </summary>
        /// <returns>If the resume action has been successfully run</returns>
        public bool ResumeGame()
        {
            var resumeAction = (SubmitActionLoadScene)AccessTools.Field(typeof(TitleScreenManager), "_resumeGameAction").GetValue(titleScreenManager);
            resumeAction?.Submit();
            //TODO: Check that !
            return resumeAction != null;
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        public void NewGame()
        {
            var newGameAction = (SubmitActionLoadScene)AccessTools.Field(typeof(TitleScreenManager), "_newGameAction").GetValue(titleScreenManager);
            newGameAction.Submit();
        }
    }
}
