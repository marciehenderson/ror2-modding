using BotanistMod.Survivors.Botanist.SkillStates;

namespace BotanistMod.Survivors.Botanist
{
    public static class BotanistStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Shoot));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
