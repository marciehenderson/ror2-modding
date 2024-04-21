using System;
using BotanistMod.Modules;
using BotanistMod.Survivors.Botanist.Achievements;

namespace BotanistMod.Survivors.Botanist
{
    public static class BotanistTokens
    {
        public static void Init()
        {
            AddBotanistTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Botanist.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddBotanistTokens()
        {
            string prefix = BotanistSurvivor.BOTANIST_PREFIX;

            string desc = "Botanist is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Throw Pot is good for crowd-control and inflicting debuffs." + Environment.NewLine + Environment.NewLine
             + "< ! > Swing Shovel is good for keeping foes at bay or pushing them into hazards." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so she left, having grown all she could.";
            string outroFailure = "..and so she vanished, no longer able to grow.";

            Language.Add(prefix + "NAME", "Botanist");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Friend of Flora");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Botanist passive");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Sample text.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_POT_NAME", "Pot");
            Language.Add(prefix + "PRIMARY_POT_DESCRIPTION", Tokens.agilePrefix + $"Throws a pot for <style=cIsDamage>{100f * BotanistStaticValues.gunDamageCoefficient}% damage</style>, and inflicts a slowing debuff.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_SHOVEL_NAME", "Shovel");
            Language.Add(prefix + "SECONDARY_SHOVEL_DESCRIPTION", Tokens.agilePrefix + $"Swing in a circle for <style=cIsDamage>{100f * BotanistStaticValues.swordDamageCoefficient}% damage</style>, and push foes away.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_ROLL_NAME", "Roll");
            Language.Add(prefix + "UTILITY_ROLL_DESCRIPTION", "Roll a short distance, gaining <style=cIsUtility>300 armor</style>. <style=cIsUtility>You cannot be hit during the roll.</style>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BOMB_NAME", "Bomb");
            Language.Add(prefix + "SPECIAL_BOMB_DESCRIPTION", $"Throw a bomb for <style=cIsDamage>{100f * BotanistStaticValues.bombDamageCoefficient}% damage</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(BotanistMasteryAchievement.identifier), "Botanist: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(BotanistMasteryAchievement.identifier), "As Botanist, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
