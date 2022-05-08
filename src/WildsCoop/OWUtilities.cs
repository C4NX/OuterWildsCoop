using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Reflection;

namespace WildsCoop
{
    public static class OWUtilities
    {
        public static GameObject CloneMainButton(string newText, int newOrder = -1)
        {
            GameObject buttonToClone = GameObject.Find("Button-Options");
            GameObject newButton = GameObject.Instantiate(buttonToClone);
            newButton.transform.SetParent(buttonToClone.transform.parent);

            GameObject.Destroy(newButton.GetComponent<SubmitActionMenu>());
            GameObject.Destroy(newButton.GetComponentInChildren<LocalizedText>());

            if (newOrder != -1)
                newButton.transform.SetSiblingIndex(newOrder);
            if (newText != null)
            {
                Text componentText = newButton.GetComponentInChildren<Text>();
                if (componentText != null)
                {
                    componentText.text = newText;
                }
                else
                    MelonDebug.Error($"CloneMainButton, Cannot find a button component Text in child of '{newButton.name}'");
            }

            return newButton;
        }

        public static void ButtonResetClickEvent(GameObject gameObject, UnityAction newOnClick)
        {
            var bt = gameObject.GetComponent<Button>();
            if(bt != null)
            {
                bt.onClick.RemoveAllListeners();
                bt.onClick.AddListener(newOnClick);
            }
            else
                MelonDebug.Error($"ButtonOneClickEvent, Cannot find a button component in '{bt.name}'");
        }

        public static void AddButtonToFadeGroup(GameObject buttonGameObject, TitleAnimationController titleAnimationController)
        {
            FieldInfo fadeControllersField = AccessTools.Field(typeof(TitleAnimationController), "_buttonFadeControllers");
            CanvasGroup buttonCanvas = buttonGameObject.GetComponent<CanvasGroup>();

            var controller = new CanvasGroupFadeController { group = buttonCanvas };
            var controllersArray = ((CanvasGroupFadeController[])fadeControllersField.GetValue(titleAnimationController))
                        .AddItem(controller)
                        .OrderBy((e)=>e.group.transform.GetSiblingIndex())
                        .ToArray();

            fadeControllersField.SetValue(titleAnimationController, controllersArray);
        }

        public static bool IsPlaying() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SolarSystem";
    }
}
