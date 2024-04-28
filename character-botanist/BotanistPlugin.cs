using BepInEx;
using BotanistMod.Survivors.Botanist;
using R2API.Utils;

/*
* @description: A mod for the game 'Risk of Rain 2' which
* introduces a new playable character known as 'Botanist'.
* @author: Marcie Henderson
* @since: 20240415
* @note: Based on HenryMod tutorial for creating characters.
* @todo: Create base skills for Botanist (Special)
*/
// @history: 
/* [v001]
* V0.0.1 - Asset bundle with custom model and textures loads
* properly. Fixed miscellaneous runtime errors, thus allowing
* the bare-bones character to be playable.
*/
/* [v002]
* V0.0.2 - Secondary skill is modified to be a throwable pot
* that applies a debuff on hit. Skill naming still needs to
* be changes. Tweaks will improve behaviour once other skills
* are complete.
*/
/* [v003]
* V0.0.3 - Throw Pot is now primary skill. Secondary skill is
* an aoe attack called 'Swing Shovel' that pushes away all 
* nearby foes.
*/
/* [v004]
* V0.0.4 - Tweaked existing skills. Added the utility skill 
* called 'Water Hop' which gives Botanist a useful option
* for mobility. Also removes any existing debuffs like the
* cleanse effect.
*/
namespace BotanistMod
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class BotanistPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.MarcieHenderson.BotanistMod";
        public const string MODNAME = "BotanistMod";
        public const string MODVERSION = "0.0.4";
        public const string DEVELOPER_PREFIX = "MARCIEHENDERSON";

        public static BotanistPlugin instance;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new BotanistSurvivor().Initialize();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }
    }
}
