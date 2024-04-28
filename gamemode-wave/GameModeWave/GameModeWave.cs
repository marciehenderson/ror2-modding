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
using System.Diagnostics.Tracing;
using System.IO;
using System.Collections;
using System.Data;
using HarmonyLib;

/***
description: RoR2 Plugin for Adding a Wave-Based Game-Mode
author: Marcie Henderson
since: 20240405
todo: Improve GUI, Add equipment loadout for selection
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

    // This is the main declaration of the plugin class.
    public class GameModeWave : BaseUnityPlugin
    {
        // Plugin GUID and other metadata
        #region metadata
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MarcieHenderson";
        public const string PluginName = "GameModeWave";
        public const string PluginVersion = "0.0.7"; // change with each commit
        #endregion metadata
        // Specify GameModeArtifact class attributes and methods
        class GameModeArtifact : ArtifactBase
        {
            public static ConfigFile config;
            public static ConfigEntry<int> TimesToPrintMessageOnStart;
            public override string ArtifactName => "Artifact of Waves";
            public override string ArtifactLangTokenName => "ARTIFACT_OF_WAVES";
            public override string ArtifactDescription => "When enabled, allow setting of first stage in run.";
            public override Sprite ArtifactEnabledIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            public override Sprite ArtifactDisabledIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            // Variables
            #region variables
            private static Dictionary<string, RoR2.SpawnCard> DisabledInteractableSpawnCards = new Dictionary<string, RoR2.SpawnCard>();
            #endregion variables
            // Constants
            #region constants
            private const float BOSS_SPAWN_PERIOD = 60F;
            private const float SPAWN_INCREASE_PERIOD = 300F;
            private const float SUPER_BOSS_START = 20F;
            // lists of boss reference codes
            private readonly string[] BOSS_SPAWNCARD_REFERENCE = 
            {
                "RoR2/Base/Beetle/cscBeetleQueen.asset",
                "RoR2/Base/ClayBoss/cscClayBoss.asset",
                "RoR2/Base/Gravekeeper/cscGravekeeper.asset",
                "RoR2/Base/Grandparent/cscGrandparent.asset",
                "RoR2/Base/ImpBoss/cscImpBoss.asset",
                "RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset",
                "RoR2/Base/Titan/cscTitanBlackBeach.asset",
                "RoR2/Base/Vagrant/cscVagrant.asset",
                "RoR2/Base/MagmaWorm/cscMagmaWorm.asset",
                "RoR2/Base/Nullifier/cscNullifier.asset",
                //"RoR2/DLC1/MajorAndMinorConstruct/cscMajorConstruct.asset", //unused boss
            };
            private readonly string[] SUPER_SPAWNCARD_REFERENCE =
            {
                "RoR2/Base/ElectricWorm/cscElectricWorm.asset",
                "RoR2/Base/RoboBallBoss/cscSuperRoboBallBoss.asset",
                "RoR2/Base/Titan/cscTitanGold.asset",
                "RoR2/Base/Scav/cscScavBoss.asset",
                "RoR2/DLC1/VoidJailer/cscVoidJailer.asset",
                "RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrab.asset",
                "RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/cscBrotherIT.asset",
            };
            private readonly Dictionary<string, string> ENABLED_INTERACTABLE_SPAWN_CARDS = new()
            {
                {"copy_common", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset").WaitForCompletion().name},
                {"copy_rare", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset").WaitForCompletion().name},
                {"copy_legend", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion().name},
                {"copy_boss", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion().name},
                //{"scrapper", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset").WaitForCompletion().name},
                //{"turret", Addressables.LoadAssetAsync<RoR2.SpawnCard>("RoR2/Base/Drones/iscBrokenTurret1.asset").WaitForCompletion().name},
            };
            #endregion constants
            public override void Init(ConfigFile config)
            {
                GameModeArtifact.config = config;
                CreateConfig(GameModeArtifact.config);
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
                // Set all artifact hooks
                #region artifacthooks
                Run.onRunStartGlobal += ChatController; // Sends message indicating artifact is enabled
                On.RoR2.Run.Start += StartILCondition; // Determin whether to override start behaviour
                On.RoR2.Run.Update += RunController; // Change overall run behaviour
                On.RoR2.GlobalEventManager.OnCharacterDeath += DeathController; // Do stuff on death events
                RoR2.SceneDirector.onGenerateInteractableCardSelection += InteractableController0;
                On.RoR2.SceneDirector.GenerateInteractableCardSelection += InteractableController1; // Get information from the interactable card selection
                On.RoR2.DirectorCard.IsAvailable += CardController; // Change the behavior of card selection for interactables
                On.EntityStates.Duplicator.Duplicating.OnEnter += DuplicatorController; // Modify 3d printer behaviour
                On.RoR2.SceneDirector.PlaceTeleporter += TeleporterController; // Delete any spawned teleporters
                // Remove artifact hooks
                 On.RoR2.Run.OnDestroy += (orig, self) =>
                {
                    IL.RoR2.Run.Start -= StartController; // Allow normal run start for non-artifact runs
                };
                #endregion artifacthooks
            }
            // Artifact behaviour controllers
            #region artifactcontrollers
            // Sends message to chat on run start
            private void ChatController(Run run)
            {
                if(NetworkServer.active && ArtifactEnabled)
                {
                    for(int i = 0; i < TimesToPrintMessageOnStart.Value; i++)
                    {
                        Chat.AddMessage("~~~ waves artifact has been enabled ~~~");
                    }
                }
            }
            // Ensures run start behaviour is not overridden on a normal run
            private void StartILCondition(On.RoR2.Run.orig_Start orig, RoR2.Run self)
            {
                if(ArtifactEnabled)
                {
                    IL.RoR2.Run.Start += StartController; // Change run start behaviour
                }
                orig(self);
            }
            // Selection of the stage is based on code written by KingEnderBrine for the ProperSave mod.
            private void StartController(ILContext il) {
                var c = new ILCursor(il);
                c.EmitDelegate<Func<bool>>(() =>
                {
                    previousTime = 0; // Reset timer for regular boss spawning
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
                        rng.ResetSeed(instance.seed); // set rng seed to instance seed
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
                    return ArtifactEnabled;
                });
                c.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, c.Next);
                c.Emit(Mono.Cecil.Cil.OpCodes.Ret);
            }
            // Method for changing behaviour of a run
            private void RunController(On.RoR2.Run.orig_Update orig, RoR2.Run self)
            {
                if(ArtifactEnabled)
                {
                    // spawn boss
                    if(self.time - previousTime > BOSS_SPAWN_PERIOD)
                    {
                        previousTime = self.time;
                        // increase number of spawns
                        var spawnNumber = (int)(self.time / SPAWN_INCREASE_PERIOD) + 1;
                        for (var i = 0; i < spawnNumber; i++)
                        {
                            var cardGroup = BOSS_SPAWNCARD_REFERENCE;
                            // add chance of spawning super bosses after period of time
                            if(self.time > BOSS_SPAWN_PERIOD * SUPER_BOSS_START)
                            {
                                cardGroup.Concat(SUPER_SPAWNCARD_REFERENCE);
                            }
                            var cardReference = cardGroup[UnityEngine.Random.RandomRangeInt(0, cardGroup.Length)];
                            try
                            {
                                RoR2.CharacterSpawnCard bossSpawnCard = Addressables.LoadAssetAsync<RoR2.CharacterSpawnCard>(cardReference).WaitForCompletion();
                                // Get player's current transform value
                                Transform playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                                // Get valid ground nodes for spawning a boss within a specified range from the player
                                List<RoR2.Navigation.NodeGraph.NodeIndex> validSpawnNodes = RoR2.SceneInfo.instance.groundNodes.FindNodesInRange(playerTransform.position, 30F, 200F, RoR2.HullMask.BeetleQueen);
                                // Randomly select one of the valid nodes
                                RoR2.SceneInfo.instance.groundNodes.GetNodePosition(validSpawnNodes[UnityEngine.Random.RandomRangeInt(0, validSpawnNodes.Count)], out Vector3 spawnPosition);
                                // Create the placement rule for spawning the boss
                                RoR2.DirectorPlacementRule placementRule = new()
                                {
                                    placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                                    position = spawnPosition,
                                };
                                Log.Debug("Loaded spawn card and created placement rule.");
                                try
                                {
                                    RoR2.DirectorSpawnRequest spawnRequest = new(bossSpawnCard, placementRule, self.runRNG);
                                    Quaternion quaternion = playerTransform.rotation;
                                    spawnRequest.ignoreTeamMemberLimit = true;
                                    spawnRequest.teamIndexOverride = TeamIndex.Monster;
                                    RoR2.SpawnCard.SpawnResult spawnResult = bossSpawnCard.DoSpawn(placementRule.targetPosition, quaternion, spawnRequest);
                                    Log.Debug("Spawned entity at position: " + placementRule.targetPosition.ToString());
                                }
                                catch(Exception e)
                                {
                                    Log.Warning("Unable to spawn entity at position: " + placementRule.targetPosition.ToString());
                                    Log.Warning(e);
                                }
                            }
                            catch(Exception e)
                            {
                                Log.Warning("Failed to load spawn card and/or create placement rule");
                                Log.Warning(e);
                            }
                            // if a super boss is spawned, dont spawn any more
                            if(SUPER_SPAWNCARD_REFERENCE.Contains(cardReference))
                            {
                                break;// end looping
                            }
                        }             
                    }
                    // run rest of code from other sources
                    orig(self);
                }
            }
            // Method for adding function on character deaths
            private void DeathController(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport report)
            {
                if(ArtifactEnabled)
                {
                    try
                    {
                        // on end of enemy team member instance
                        if
                        (
                            report.victimTeamIndex == TeamIndex.Monster ||
                            report.victimTeamIndex == TeamIndex.Void ||
                            report.victimTeamIndex == TeamIndex.Lunar
                        )
                        {
                            // equation for calculating appropriate reward chances
                            var rfMax = 4F;
                            var rewardFactor = 
                            (report.victimIsBoss?1:0)     * 1.6F +
                            (report.victimIsChampion?1:0) * 1.0F +
                            (report.victimIsElite?1:0)    * 0.8F +
                                                            0.6F ;
                            // check if reward should drop
                            if(RoR2.Util.CheckRoll(rewardFactor/rfMax*100F, report.attackerBody.master))
                            {
                                // choose reward tier (common=0, rare=1, legendary=2)
                                var tierIndex = 0;
                                for(var i = 0; i <= tierIndex; i++)
                                {
                                    if(RoR2.Util.CheckRoll(rewardFactor/rfMax*100F, report.attackerBody.master) && tierIndex < 2)
                                    {
                                        // upgrade tier on success with a limit of two upgrades
                                        tierIndex++;
                                    }
                                }
                                if(tierIndex > 1)
                                {
                                    // flip a coin to see whether to give boss, or legendary scrap
                                    if(RoR2.Util.CheckRoll(50F))
                                    {
                                        tierIndex = 3;
                                    }
                                }
                                // drop item
                                Transform itemTransform = new();
                                try
                                {
                                    itemTransform = report.victimBody.master.GetBodyObject().transform;
                                }
                                catch(Exception e)
                                {
                                    // drop scrap near player if the victim has an issue with their body
                                    itemTransform = RoR2.PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                                    Log.Debug("Using default transform for dropping scrap");
                                    Log.Warning(e);
                                }
                                RoR2.ItemIndex itemIndex = new();
                                switch (tierIndex)
                                {
                                    case 1:
                                        itemIndex = Addressables.LoadAssetAsync<RoR2.ItemDef>("RoR2/Base/Scrap/ScrapGreen.asset").WaitForCompletion().itemIndex;
                                        break;
                                    case 2:
                                        itemIndex = Addressables.LoadAssetAsync<RoR2.ItemDef>("RoR2/Base/Scrap/ScrapRed.asset").WaitForCompletion().itemIndex;
                                        break;
                                    case 3:
                                        itemIndex = Addressables.LoadAssetAsync<RoR2.ItemDef>("RoR2/Base/Scrap/ScrapYellow.asset").WaitForCompletion().itemIndex;
                                        break;
                                    default:
                                        // also case 0
                                        itemIndex = Addressables.LoadAssetAsync<RoR2.ItemDef>("RoR2/Base/Scrap/ScrapWhite.asset").WaitForCompletion().itemIndex;
                                        break;
                                }
                                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemIndex), itemTransform.position, itemTransform.forward * 20F);
                                Log.Debug("Item Dropped: " + itemIndex);
                            }
                        }
                        // back to other code
                        orig(self, report);
                    }
                    catch(Exception e)
                    {
                        Log.Warning("Error while dropping specified pickup.");
                        Log.Warning(e);
                    }
                }
            }
            // Methods for gathering disabled interactables based on the ContentDisabler mod by William758
            private void InteractableController0 (RoR2.SceneDirector sceneDirector, RoR2.DirectorCardCategorySelection dccs)
            {
                Log.Debug("InteractableController0 - Start");
                if(ArtifactEnabled)
                {
                    foreach(var category in dccs.categories)
                    {
                        foreach(var card in category.cards)
                        {
                            try
                            {
                                var spawnCard = card.spawnCard;
                                // Check the spawn card name against the dictionary of enabled names
                                if(!ENABLED_INTERACTABLE_SPAWN_CARDS.ContainsValue(spawnCard.name))
                                {
                                    // If an interactable should not be spawned, add it to
                                    // the disabled interactable spawn cards dictionary
                                    if(!DisabledInteractableSpawnCards.ContainsKey(spawnCard.name))
                                    {
                                        // Only follow through with the addition if it hasn't been added yet
                                        DisabledInteractableSpawnCards.Add(spawnCard.name, spawnCard);
                                        Log.Debug("Added " + spawnCard.name + " to the disabled interactables list.");
                                    }
                                    else
                                    {
                                        Log.Debug(spawnCard.name + " already exists in the disabled interactables list.");
                                    }
                                }
                                else
                                {
                                    // Modify the behaviour of the enabled interactable spawn cards
                                }
                            }
                            catch(Exception e)
                            {
                                Log.Warning("Unable to read spawn card from category: " + category.name);
                                Log.Warning(e);
                            }
                        }
                    }
                }
                Log.Debug("InteractableController0 - End");
            }
            private WeightedSelection<RoR2.DirectorCard> InteractableController1 (On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, RoR2.SceneDirector self)
            {
                return orig(self);
            }
            // Method for intervening in the card selection process
            private bool CardController (On.RoR2.DirectorCard.orig_IsAvailable orig, RoR2.DirectorCard self)
            {
                // Only change behaviour if the artifact has been enabled
                if(ArtifactEnabled)
                {
                    try
                    {
                        if(DisabledInteractableSpawnCards.ContainsKey(self.spawnCard.name))
                        {
                            // If the interactable has been disabled tell the director it isn't available
                            return false;
                        }
                        else if(ENABLED_INTERACTABLE_SPAWN_CARDS.ContainsValue(self.spawnCard.name))
                        {
                            // If the interactable should be used, return true
                            return true;
                        }
                    }
                    catch(Exception e)
                    {
                        Log.Warning("Unable to intervene in card availability process.");
                        Log.Warning(e);
                    }
                }
                // Return default calculated result
                return orig(self);
            }
            // Method for changing the behaviour of interactable 3d printers based on the PrintToInventory mod by Moffein
            private void DuplicatorController(On.EntityStates.Duplicator.Duplicating.orig_OnEnter orig, EntityStates.Duplicator.Duplicating self)
            {
                // Run regular interaction behaviour
                orig(self);
                // Add behaviour if the artifact is enabled
                if(ArtifactEnabled)
                {
                    // If the network server is active
                    if(UnityEngine.Networking.NetworkServer.active)
                    {
                        #pragma warning disable Publicizer001
                        var interaction = self.GetComponent<RoR2.PurchaseInteraction>();
                        var behaviour = self.GetComponent<RoR2.ShopTerminalBehavior>();
                        #pragma warning restore Publicizer001
                        if(interaction && behaviour)
                        {
                            var body = interaction.lastActivator.GetComponent<RoR2.CharacterBody>();
                            if(body && body.inventory)
                            {
                                #pragma warning disable Publicizer001
                                var pickup = RoR2.PickupCatalog.GetPickupDef(behaviour.pickupIndex);
                                if(pickup != null && pickup.itemIndex != RoR2.ItemIndex.None)
                                {
                                    body.inventory.GiveItem(pickup.itemIndex, 1);
                                    behaviour.SetHasBeenPurchased(true);
                                    self.hasDroppedDroplet = true;
                                    self.hasStartedCooking = true;
                                    try
                                    {
                                        // If the context token can be parsed then change the number of remaining uses
                                        if(int.TryParse(interaction.displayNameToken, out int remaining))
                                        {
                                            int remainingNew = remaining - 1;
                                            if(remainingNew > 0)
                                            {
                                                interaction.contextToken = "Remaining Uses: " + remainingNew.ToString();
                                                interaction.displayNameToken = remainingNew.ToString();
                                                Log.Debug("Updated 3D printer uses.");
                                            }
                                            else
                                            {
                                                interaction.contextToken = "Remaining Uses: 5";
                                                interaction.displayNameToken = "5";
                                                Log.Debug("Reset 3D printer uses.");
                                                try
                                                {
                                                    switch(UnityEngine.Random.RandomRangeInt(0, 4))
                                                    {
                                                        default:
                                                            behaviour.dropTable = Addressables.LoadAssetAsync<RoR2.BasicPickupDropTable>("RoR2/Base/Duplicator/dtDuplicatorTier1.asset").WaitForCompletion();
                                                            interaction.costType = RoR2.CostTypeIndex.WhiteItem;
                                                            break;
                                                        case 1:
                                                            behaviour.dropTable = Addressables.LoadAssetAsync<RoR2.BasicPickupDropTable>("RoR2/Base/DuplicatorLarge/dtDuplicatorTier2.asset").WaitForCompletion();
                                                            interaction.costType = RoR2.CostTypeIndex.GreenItem;
                                                            break;
                                                        case 2:
                                                            behaviour.dropTable = Addressables.LoadAssetAsync<RoR2.BasicPickupDropTable>("RoR2/Base/DuplicatorMilitary/dtDuplicatorTier3.asset").WaitForCompletion();
                                                            interaction.costType = RoR2.CostTypeIndex.RedItem;
                                                            break;
                                                        case 3:
                                                            behaviour.dropTable = Addressables.LoadAssetAsync<RoR2.BasicPickupDropTable>("RoR2/Base/DuplicatorWild/dtDuplicatorWild.asset").WaitForCompletion();
                                                            interaction.costType = RoR2.CostTypeIndex.BossItem;
                                                            break;
                                                    }
                                                }
                                                catch(Exception e)
                                                {
                                                    Log.Warning("Error while randomizing the printer's tier");
                                                    Log.Warning(e);
                                                }
                                                try
                                                {
                                                    // change item that can be duplicated
                                                    behaviour.SetPickupIndex(behaviour.dropTable.GenerateDrop(behaviour.rng), false);
                                                    behaviour.UpdatePickupDisplayAndAnimations();
                                                    Log.Debug("A 3D printer has changed.");
                                                }
                                                catch(Exception e)
                                                {
                                                    Log.Warning("Unable to change printer item");
                                                    Log.Warning(e);
                                                }
                                            }   
                                        }
                                        else
                                        {
                                            interaction.contextToken = "Remaining Uses: 4";
                                            interaction.displayNameToken = "4";
                                            Log.Debug("Initialized 3d printer.");
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        Log.Warning("Error updating printer interaction.");
                                        Log.Warning(e);
                                    }
                                }
                                #pragma warning restore Publicizer001
                            }
                        }
                    }
                }
            }
            // Method for conditionally removing the teleporter from the scene
            private void TeleporterController(On.RoR2.SceneDirector.orig_PlaceTeleporter orig, RoR2.SceneDirector self)
            {
                // Allow teleporter to be spawned as usual
                orig(self);
                // If artifact is enabled, then delete the teleporter from the scene
                if(ArtifactEnabled)
                {
                    // Try to destroy the teleporter gameobject
                    try
                    {
                        UnityEngine.GameObject.Destroy(self.teleporterInstance);
                        Log.Debug("Teleporter Removed");
                    }
                    catch(Exception e)
                    {
                        Log.Warning("Unable to remove teleporter GameObject.");
                        Log.Warning(e);
                    }
                    
                }
            }
            #endregion artifactcontrollers
        }
        // Define persistent objects here
        #region persistant
        // Initialize list of artifacts
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        // Initialize lobby ui controller
        private RoR2.UI.CharacterSelectController lobbyUI = null;
        // Initialize lobbyButton and child GameObject
        private GameObject lobbyButton = null;
        GameObject lobbyButtonChild = null;
        // Initialize run starting scene variables
        private static int startingSceneIndex = 0;
        private static string startingSceneCode = "ancientloft";
        private readonly Dictionary<string, (string, string)> STAGE_SCENE_REFERENCE = new()
        {
            {"ancientloft", ("Aphelian Sanctuary", "RoR2/DLC1/ancientloft/texAncientLoftPreview.png")},
            {"blackbeach", ("Distant Roost 1", "RoR2/Base/blackbeach/texBlackbeachPreview.png")},
            {"blackbeach2", ("Distant Roost 2", "RoR2/Base/blackbeach/texBlackbeachPreview.png")},
            {"dampcavesimple", ("Abyssal Depths", "RoR2/Base/dampcavesimple/texDampcavePreview.png")},
            {"foggyswamp", ("Wetland Aspect", "RoR2/Base/foggyswamp/texFoggyswampPreview.png")},
            {"frozenwall", ("Rallypoint Delta", "RoR2/Base/frozenwall/texFrozenwallPreview.png")},
            {"golemplains", ("Titanic Plains 1", "RoR2/Base/golemplains/texGolemplainsPreview.png")},
            {"golemplains2", ("Titanic Plains 2", "RoR2/Base/golemplains/texGolemplainsPreview.png")},
            {"goolake", ("Abandoned Aqueduct", "RoR2/Base/goolake/texGoolakePreview.png")},
            {"rootjungle", ("Sundered Grove", "RoR2/Base/rootjungle/texRootjunglePreview.png")},
            {"shipgraveyard", ("Siren's Call", "RoR2/Base/shipgraveyard/texShipgraveyardPreview.png")},
            {"skymeadow", ("Sky Meadow", "RoR2/Base/skymeadow/texSkymeadowPreview.png")},
            {"snowyforest", ("Siphoned Forest", "RoR2/DLC1/snowyforest/texSnowyforestPreview.jpg")},
            {"sulfurpools", ("Sulfur Pools", "RoR2/DLC1/sulfurpools/texSulfurPoolsPreview.png")},
            {"wispgraveyard", ("Scorched Acres", "RoR2/Base/wispgraveyard/texWispgraveyardPreview.png")},
        };
        // Initialize field info used in replacing the code at run start
        private static readonly FieldInfo onRunStartGlobalDelegate = typeof(Run).GetField(nameof(Run.onRunStartGlobal), BindingFlags.NonPublic | BindingFlags.Static);
        private static float previousTime = 0;
        #endregion persistant
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
            On.RoR2.UI.RuleCategoryController.SetData += LobbyController;
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
        // Custom methods
        #region methods
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
            startingSceneCode = "ancientloft";
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
                lobbyButtonImage.rectTransform.anchorMin = new Vector2((float)0.511, (float)0.411);
                lobbyButtonImage.rectTransform.anchorMax = new Vector2((float)0.513, (float)0.413);
                // Add text to button
                lobbyButtonChild = new GameObject("StageSelectText");
                lobbyButtonChild.transform.SetParent(lobbyButtonImage.transform);
                TextMeshProUGUI lobbyButtonChildText = lobbyButtonChild.AddComponent<TextMeshProUGUI>();
                lobbyButtonChildText.name = "LobbyButtonText";
                lobbyButtonChildText.text = "Aphelian Sanctuary";
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
            startingSceneIndex++;
            switch (startingSceneIndex)
            {
                default:
                    lobbyButtonString = "Aphelian Sanctuary";
                    startingSceneCode = "ancientloft";
                    break;
                case 1:
                    lobbyButtonString = "Titanic Plains";
                    startingSceneCode = "golemplains";
                    break;
                case 2:
                    lobbyButtonString = "Distant Roost";
                    startingSceneCode = "blackbeach";
                    break;
                case 3:
                    lobbyButtonString = "Wetland Aspect";
                    startingSceneCode = "foggyswamp";
                    break;
                case 4:
                    lobbyButtonString = "Rallypoint Delta";
                    startingSceneCode = "frozenwall";
                    break;
                case 5:
                    lobbyButtonString = "Abyssal Depths";
                    startingSceneCode = "dampcavesimple";
                    break;
                case 6:
                    lobbyButtonString = "Abandoned Aqueduct";
                    startingSceneCode = "goolake";
                    break;
                case 7:
                    lobbyButtonString = "Sundered Grove";
                    startingSceneCode = "rootjungle";
                    break;
                case 8:
                    lobbyButtonString = "Siren's Call";
                    startingSceneCode = "shipgraveyard";
                    break;
                case 9:
                    lobbyButtonString = "Siphoned Forest";
                    startingSceneCode = "snowyforest";
                    break;
                case 10:
                    lobbyButtonString = "Sulfur Pools";
                    startingSceneCode = "sulfurpools";
                    break;
                case 11:
                    lobbyButtonString = "Scorched Acres";
                    startingSceneCode = "wispgraveyard";
                    break;
                case 12:
                    lobbyButtonString = "Sky Meadow";
                    startingSceneIndex = -1;
                    startingSceneCode = "skymeadow";
                    break;
            }
            TextMeshProUGUI lobbyButtonText = lobbyButtonChild.GetComponent<TextMeshProUGUI>();
            lobbyButtonText.text = lobbyButtonString;
            lobbyButtonText.ForceMeshUpdate();
            Log.Debug(lobbyButtonString + " code: " + startingSceneCode + " index: " + startingSceneIndex);
        }
        // Better lobby ui
        private void LobbyController(On.RoR2.UI.RuleCategoryController.orig_SetData orig, RoR2.UI.RuleCategoryController self, RoR2.RuleCategoryDef categoryDef, RoR2.RuleChoiceMask availability, RoR2.RuleBook rulebook)
        {
            Log.Debug("Attempting to add new rule.");
            try
            {
                RoR2.RuleCategoryDef ruleCategory = new()
                {
                    displayToken = "Stages",
                    subtitleToken = "Choose Your Destination",
                    color = UnityEngine.Color.white,
                    ruleCategoryType = RuleCatalog.RuleCategoryType.StripVote,
                };
                RoR2.RuleDef rule = new("stageoptions", "Options");
                List<RoR2.RuleChoiceDef> ruleChoices = new();
                foreach(var pair in STAGE_SCENE_REFERENCE)
                {
                    ruleChoices.Add(new RoR2.RuleChoiceDef()
                    {
                        tooltipNameToken = pair.Value.Item1,
                        tooltipBodyToken = pair.Key,
                        sprite = Addressables.LoadAssetAsync<UnityEngine.Sprite>(pair.Value.Item2).WaitForCompletion(),
                        ruleDef = rule,
                    });
                }
                rule.category = ruleCategory;
                rule.choices = ruleChoices;
                rule.defaultChoiceIndex = 0;
                // tell rule category controller to also display the new rule
                rulebook
                Log.Debug("Added new rule successfully");
            }
            catch(Exception e)
            {
                Log.Warning("Error adding stage selection to rules");
                Log.Warning(e);
            }
            
            orig(self, categoryDef, availability, rulebook);
        }
        #endregion methods
    }
}