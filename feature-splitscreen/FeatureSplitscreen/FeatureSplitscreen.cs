using BepInEx;
using Facepunch.Steamworks;
using IL.RoR2.ContentManagement;
using IL.RoR2.UI;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
///*
/// @description: an implementation of a local 
/// splitscreen coop feature for Risk of Rain 2
/// @author: MarcieHenderson
/// @since: 20240428
namespace FeatureSplitscreen
{
    // dependency declaration
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    // main class declaration
    public class FeatureSplitscreen : BaseUnityPlugin
    {
        // Plugin GUID based on author name, plugin name, and the current plugin version
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MarcieHenderson";
        public const string PluginName = "FeatureSplitscreen";
        public const string PluginVersion = "0.0.0";
        // Global variables
        
        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Initialize logger
            Log.Init(Logger);
            // Generate hooks
            // Hook onto LobbyInit methods
            On.RoR2.LobbyManager.OnMultiplayerMenuEnabled += LobbyInit.OnMultiplayerMenuEnabled;
            On.RoR2.UI.MainMenu.MultiplayerMenuController.Awake += LobbyInit.MultiplayerMenuController;
            On.RoR2.UI.LobbyUserList.Awake += (orig, self) => {
                orig(self);
            };
        }
        // The Update() method is run on every frame of the game.
        private void Update()
        {
            // Do regular tasks
        }
    }
}
