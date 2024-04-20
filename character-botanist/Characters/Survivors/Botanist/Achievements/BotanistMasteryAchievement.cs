using RoR2;
using BotanistMod.Modules.Achievements;

namespace BotanistMod.Survivors.Botanist.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class BotanistMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = BotanistSurvivor.BOTANIST_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = BotanistSurvivor.BOTANIST_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => BotanistSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}