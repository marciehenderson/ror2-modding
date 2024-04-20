using BotanistMod.Survivors.Botanist.Achievements;
using RoR2;
using UnityEngine;

namespace BotanistMod.Survivors.Botanist
{
    public static class BotanistUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                BotanistMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(BotanistMasteryAchievement.identifier),
                BotanistSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
