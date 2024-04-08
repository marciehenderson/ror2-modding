using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Facepunch.Steamworks;
using GameModeWave;
using IL.RoR2.ContentManagement;
using IL.RoR2.UI;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Text;
using static GameModeWave.ArtifactBase;
using Mono.Security.Authenticode;
using RoR2.UI;
using EntityStates.AffixVoid;
using EntityStates.ArtifactShell;
using Newtonsoft.Json.Utilities;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using Rewired;
using System.Threading;
using R2API.Utils;
using TMPro;
using UnityEngine.SceneManagement;
using RoR2.Navigation;
using MonoMod.Cil;
using System.Reflection.Emit;
using EntityStates.Missions.ArtifactWorld.TrialController;
using System.Runtime.Serialization;

/***
description: RoR2 Plugin for Adding a Wave-Based Game-Mode
author: Marcie Henderson
since: 20240405
***/
namespace GameModeWave
{
    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    public class GameModeWave : BaseUnityPlugin
    {
        // Plugin GUID and other metadata
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MarcieHenderson";
        public const string PluginName = "GameModeWave";
        public const string PluginVersion = "0.0.4"; // change with each commit

        // Specify GameModeArtifact class attributes and methods
        class GameModeArtifact : ArtifactBase
        {
            public static ConfigEntry<int> TimesToPrintMessageOnStart;
            public override string ArtifactName => "Artifact of Waves";
            public override string ArtifactLangTokenName => "ARTIFACT_OF_WAVES";
            public override string ArtifactDescription => "When enabled, allow setting of first stage in run.";
            public override Sprite ArtifactEnabledIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            public override Sprite ArtifactDisabledIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            public override void Init(ConfigFile config)
            {
                CreateConfig(config);
                CreateLang();
                CreateArtifact();
                Hooks();
            }
            private void CreateConfig(ConfigFile config)
            {
                TimesToPrintMessageOnStart = config.Bind<int>("Artifact: " + ArtifactName, "Times to Print Message in Chat", 1, "How many times should a message be printed to the chat on run start?");
            }
            public override void Hooks()
            {
                // Set hooks
                Run.onRunStartGlobal += PrintMessageToChat; // message indicating artifact is enabled
                // Run.onRunStartGlobal += Effect;
                IL.RoR2.Run.Start += Effect;
            }
            // Effects of artifact (currently just prints message to chat)
            private void PrintMessageToChat(Run run)
            {
                if(NetworkServer.active && ArtifactEnabled)
                {
                    for(int i = 0; i < TimesToPrintMessageOnStart.Value; i++)
                    {
                        Chat.AddMessage("~~~ waves artifact has been enabled ~~~");
                    }
                }
            }
            // Effect of artifact, allows selection of first stage
            // based on code written by KingEnderBrine for the
            // ProperSave mod.
            private void Effect(ILContext il) {
                var c = new ILCursor(il);
                c.EmitDelegate<Func<bool>>(() =>
                {
                    // Check if the artifact has been enabled
                    if (ArtifactEnabled)
                    {
                        // Sets next stage to the currently selected stage
                        try
                        {
                            var instance = Run.instance;
                            #pragma warning disable Publicizer001
                            instance.OnRuleBookUpdated(instance.networkRuleBookComponent);
                            #pragma warning restore Publicizer001
                            instance.seed = instance.GenerateSeedForNewRun();
                            instance.selectedDifficulty = DifficultyIndex.Hard;
                            instance.fixedTime = 0;
                            instance.shopPortalCount = 0;
                            #pragma warning disable Publicizer001
                            instance.runStopwatch = new Run.RunStopwatch
                            {
                                offsetFromFixedTime = 0,
                                isPaused = false
                            };
                            var rng = FormatterServices.GetUninitializedObject(typeof(Xoroshiro128Plus)) as Xoroshiro128Plus;
                            instance.runRNG = rng;
                            instance.nextStageRng = rng;
                            instance.stageRngGenerator = rng;
                            instance.GenerateStageRNG();
                            
                            instance.allowNewParticipants = true;
                            #pragma warning restore Publicizer001
                            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
                            var onlyInstancesList = NetworkUser.readOnlyInstancesList;
                            for (int index = 0; index < onlyInstancesList.Count; ++index)
                            {
                                instance.OnUserAdded(onlyInstancesList[index]);
                            }
                            #pragma warning disable Publicizer001
                            instance.allowNewParticipants = false;
                            #pragma warning restore Publicizer001
                            instance.stageClearCount = 0;
                            instance.RecalculateDifficultyCoefficent();
                            instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName(startingSceneCode);
                            NetworkManager.singleton.ServerChangeScene(startingSceneCode);
                            Log.Debug("Requested " + startingSceneCode + " scene.");
                            #pragma warning disable Publicizer001
                            instance.BuildUnlockAvailability();
                            #pragma warning restore Publicizer001
                            instance.BuildDropTable();
                            if (onRunStartGlobalDelegate.GetValue(null) is MulticastDelegate onRunStartGlobal && onRunStartGlobal != null)
                            {
                                foreach (var handler in onRunStartGlobal.GetInvocationList())
                                {
                                    handler.Method.Invoke(handler.Target, new object[] { instance });
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Log.Warning("Unable to set starting stage to " + startingSceneCode + " scene.");
                            Log.Warning(e);
                        }
                    }
                    return ArtifactEnabled;
                });
                c.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, c.Next);
                c.Emit(Mono.Cecil.Cil.OpCodes.Ret);
            }
        }
        /******************************************************************/
        // Define persistent objects here
        // Initialize list of artifacts
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        // Initialize lobby ui controller
        private RoR2.UI.CharacterSelectController lobbyUI = null;
        // Initialize lobbyButton and child GameObject
        private GameObject lobbyButton = null;
        GameObject lobbyButtonChild = null;
        //
        private static int startingSceneIndex = 0;
        private static String startingSceneCode = "ancientloft";
        //
        private static readonly FieldInfo onRunStartGlobalDelegate = typeof(Run).GetField(nameof(Run.onRunStartGlobal), BindingFlags.NonPublic | BindingFlags.Static);
        /******************************************************************/
        // Runs at game initialization - use for initializing mod
        public void Awake()
        {
            // Start logger
            Log.Init(Logger);

            // Modded artifact startup code
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }

            // Add Hooks
            // adds new lobby ui button
            On.RoR2.UI.CharacterSelectController.Awake += CreateLobbyUI;
            // disables pod drop animation on run start
            On.RoR2.Stage.Start += (orig, self) =>
            {
                self.SetFieldValue("usePod", false);
                orig(self);
            };
        }

        // Runs on each frame - use for continuous processes in mod
        private void Update()
        {
            // add stuff here
            
        }

        // Clean up note - not sure if this works yet
        private void OnDestroy()
        {
            // Remove Hooks
            On.RoR2.UI.CharacterSelectController.Awake -= CreateLobbyUI;
        }
        // Custom methods
        // Validate artifact method
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;
            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }
        // Create lobby ui elements
        private void CreateLobbyUI(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            // reset stage select option
            startingSceneIndex = 0;
            startingSceneCode = "";
            lobbyUI = self;
            // Try to load ui components, and catch/log on error
            try
            {
                lobbyButton = new GameObject("StageSelectButton");
                lobbyButton.transform.SetParent(lobbyUI.transform);
                RectTransform rectTransform = lobbyButton.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero; // bottom left
                rectTransform.anchorMax = Vector2.one; // top right
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                
                // Make visual component of button
                UnityEngine.UI.Image lobbyButtonImage = lobbyButton.AddComponent<UnityEngine.UI.Image>();
                lobbyButtonImage.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUILaunchButton.png").WaitForCompletion();
                // Position the button
                lobbyButtonImage.rectTransform.anchorMin = new Vector2((float)0.510, (float)0.411);
                lobbyButtonImage.rectTransform.anchorMax = new Vector2((float)0.515, (float)0.413);
                // Add text to button
                lobbyButtonChild = new GameObject("StageSelectText");
                lobbyButtonChild.transform.SetParent(lobbyButtonImage.transform);
                TextMeshProUGUI lobbyButtonChildText = lobbyButtonChild.AddComponent<TextMeshProUGUI>();
                lobbyButtonChildText.name = "LobbyButtonText";
                lobbyButtonChildText.text = "Default";
                lobbyButtonChildText.color = UnityEngine.Color.white;
                lobbyButtonChildText.fontSize = 3;
                lobbyButtonChildText.autoSizeTextContainer = true;
                lobbyButtonChildText.rectTransform.anchorMin = Vector2.zero;
                lobbyButtonChildText.rectTransform.anchorMax = Vector2.one;
                lobbyButtonChildText.ForceMeshUpdate();
                // Add functionality to button
                UnityEngine.UI.Button lobbyButtonButton = lobbyButton.AddComponent<UnityEngine.UI.Button>();
                lobbyButtonButton.onClick.AddListener(LobbyButtonClick);
                // Log addition for debugging
                Log.Debug("Completed adding lobby buttons");
                Log.Debug(lobbyUI.transform.position.x + " " + lobbyUI.transform.position.y + " " + lobbyUI.transform.position.z);
            }
            catch (Exception e)
            {
                // Log ui loading errors
                Log.Warning("Failed while adding lobby buttons");
                Log.Error(e);
            }
            // Run other code
            orig(self);
        }
        private void LobbyButtonClick()
        {
            // Log button click event for debugging
            Log.Debug("Lobby button clicked.");
            // Open stage selector ui
            String lobbyButtonString = "";
            switch (startingSceneIndex)
            {
                case 0:
                    lobbyButtonString = "Aphelian Sanctuary";
                    startingSceneIndex++;
                    startingSceneCode = "ancientloft";
                    break;
                case 1:
                    lobbyButtonString = "Titanic Plains";
                    startingSceneIndex++;
                    startingSceneCode = "golemplains";
                    break;
                case 2:
                    lobbyButtonString = "Distant Roost";
                    startingSceneIndex++;
                    startingSceneCode = "blackbeach";
                    break;
                case 3:
                    lobbyButtonString = "Wetland Aspect";
                    startingSceneIndex++;
                    startingSceneCode = "foggyswamp";
                    break;
                case 4:
                    lobbyButtonString = "Rallypoint Delta";
                    startingSceneIndex++;
                    startingSceneCode = "frozenwall";
                    break;
                case 5:
                    lobbyButtonString = "Abyssal Depths";
                    startingSceneIndex++;
                    startingSceneCode = "dampcavesimple";
                    break;
                case 6:
                    lobbyButtonString = "Abandoned Aqueduct";
                    startingSceneIndex++;
                    startingSceneCode = "goolake";
                    break;
                case 7:
                    lobbyButtonString = "Sundered Grove";
                    startingSceneIndex++;
                    startingSceneCode = "rootjungle";
                    break;
                case 8:
                    lobbyButtonString = "Siren's Call";
                    startingSceneIndex++;
                    startingSceneCode = "shipgraveyard";
                    break;
                case 9:
                    lobbyButtonString = "Siphoned Forest";
                    startingSceneIndex++;
                    startingSceneCode = "snowyforest";
                    break;
                case 10:
                    lobbyButtonString = "Sulfur Pools";
                    startingSceneIndex++;
                    startingSceneCode = "sulfurpools";
                    break;
                case 11:
                    lobbyButtonString = "Scorched Acres";
                    startingSceneIndex++;
                    startingSceneCode = "wispgraveyard";
                    break;
                case 12:
                    lobbyButtonString = "Sky Meadow";
                    startingSceneIndex = 0;
                    startingSceneCode = "skymeadow";
                    break;
            }
            TextMeshProUGUI lobbyButtonText = lobbyButtonChild.GetComponent<TextMeshProUGUI>();
            lobbyButtonText.text = lobbyButtonString;
            lobbyButtonText.ForceMeshUpdate();
            Log.Debug(lobbyButtonString + " code: " + startingSceneCode + " index: " + startingSceneIndex);
        }
    }
}