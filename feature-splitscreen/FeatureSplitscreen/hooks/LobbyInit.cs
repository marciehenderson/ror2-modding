using System;
using AK.Wwise;
using EntityStates.BrotherMonster;
using R2API.Utils;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeatureSplitscreen
{
    public class LobbyInit
    {
        // attributes
        
        // methods
        public static void OnMultiplayerMenuEnabled(On.RoR2.LobbyManager.orig_OnMultiplayerMenuEnabled orig, RoR2.LobbyManager self, Action<RoR2.UserID> onLobbyLeave)
        {
            // run standard multiplayer lobby init
            orig(self, onLobbyLeave);
            // debug message to notify when a multiplayer lobby has been enabled
            Log.Debug("Entered a multiplayer lobby.");
        }
        public static void MultiplayerMenuController(On.RoR2.UI.MainMenu.MultiplayerMenuController.orig_Awake orig, RoR2.UI.MainMenu.MultiplayerMenuController self)
        {
            orig(self);
            Log.Debug("Mutiplayer menu controller is awake");
            try
            {
                // make new gameObject for controlling feature enabling/settings
                UnityEngine.GameObject containerObj = new UnityEngine.GameObject("Settings Container");
                // add positioning component
                UnityEngine.RectTransform containerRct = containerObj.AddComponent<UnityEngine.RectTransform>();
                containerRct.SetParent(self.lobbyUserList.contentArea.GetComponent<UnityEngine.RectTransform>());
                // size layout element
                UnityEngine.UI.LayoutElement containerElm = containerObj.AddComponent<UnityEngine.UI.LayoutElement>();
                containerElm.preferredHeight = 50.0F;
                containerElm.flexibleWidth = 1.0F;
                // control layout of subsequent children
                UnityEngine.UI.VerticalLayoutGroup containerLyt = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                // padding
                containerLyt.padding.left = 10;
                containerLyt.padding.right = 10;
                containerLyt.padding.top = 10;
                containerLyt.padding.bottom = 10;
                // element spacing
                containerLyt.spacing = 4;
                // element alignment
                containerLyt.childAlignment = UnityEngine.TextAnchor.UpperCenter;
                // element sizing controls
                containerLyt.childControlWidth = true;
                containerLyt.childControlHeight = true;
                containerLyt.childScaleWidth = false;
                containerLyt.childScaleHeight = false;
                containerLyt.childForceExpandWidth = false;
                containerLyt.childForceExpandHeight = false;
                // make gameObjects for container children
                UnityEngine.GameObject enableObj = new UnityEngine.GameObject("Enable Button");
                UnityEngine.RectTransform enableRct = enableObj.AddComponent<UnityEngine.RectTransform>();
                // set parent to container
                enableRct.SetParent(containerRct);
                // visual component
                UnityEngine.UI.Image enableImg = enableObj.AddComponent<UnityEngine.UI.Image>();
                enableImg.sprite = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<UnityEngine.Sprite>("RoR2/Base/UI/texUICleanButton.png").WaitForCompletion();
                // button component
                UnityEngine.UI.Button enableBtn = enableObj.AddComponent<UnityEngine.UI.Button>();
                // onClick listener
                enableBtn.onClick.AddListener(() => 
                {
                    // log event
                    Log.Debug("Enable Button Clicked");
                    // add local player to lobby
                    // Rewired.Player newPlayer = new Rewired.Player();
                    // RoR2.LocalUserManager.AddUser();
                });
                // layout control
                UnityEngine.UI.LayoutElement enableElm = enableObj.AddComponent<UnityEngine.UI.LayoutElement>();
                enableElm.preferredWidth = 100.0F;
                enableElm.preferredHeight = 25.0F;
                Log.Debug("Success");
            }
            catch(Exception e)
            {
                Log.Warning("Failure");
                Log.Warning(e);
            }
        }
    }
}