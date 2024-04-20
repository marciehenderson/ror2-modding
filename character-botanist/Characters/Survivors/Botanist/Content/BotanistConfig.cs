using BepInEx.Configuration;
using BotanistMod.Modules;

namespace BotanistMod.Survivors.Botanist
{
    public static class BotanistConfig
    {
        public static ConfigEntry<bool> someConfigBool;
        public static ConfigEntry<float> someConfigFloat;
        public static ConfigEntry<float> someConfigFloatWithCustomRange;

        public static void Init()
        {
            string section = "Botanist";

            someConfigBool = Config.BindAndOptions(
                section,
                "someConfigBool",
                true,
                "this creates a bool config, and a checkbox option in risk of options");

            someConfigFloat = Config.BindAndOptions(
                section,
                "someConfigfloat",
                5f);//blank description will default to just the name

            someConfigFloatWithCustomRange = Config.BindAndOptions(
                section,
                "someConfigfloat2",
                5f,
                0,
                50,
                "if a custom range is not passed in, a float will default to a slider with range 0-20. risk of options only has sliders");
        }
    }
}
