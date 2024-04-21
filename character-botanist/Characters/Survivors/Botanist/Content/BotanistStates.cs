using BotanistMod.Survivors.Botanist.SkillStates;

namespace BotanistMod.Survivors.Botanist
{
    public static class BotanistStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SwingShovel));

            Modules.Content.AddEntityState(typeof(ThrowPot));

            Modules.Content.AddEntityState(typeof(WaterHop));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
