using System;

namespace FeatureSplitscreen
{
    public class LocalUserAdd
    {
        // attributes

        // methods
        public static void LocalUserManagerUpdate(On.RoR2.LocalUserManager.orig_Update orig)
        {
            orig();
        }
    }
}