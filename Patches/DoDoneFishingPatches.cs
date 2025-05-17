using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;
using System.Collections.Generic; // Ensure this is included for List<T>

namespace StackEverythingRedux.Patches
{
    internal class DoDoneFishingPatches
    {
        private static List<SObject> tackles = new List<SObject>(); // Fixed initialization

        public static void Prefix(FishingRod __instance)
        {
            tackles = __instance.GetTackle();
        }

        public static void Postfix(FishingRod __instance)
        {
            if (__instance.attachments is null || __instance.attachments?.Count <= 1)
            {
                return;
            }

            int i = 1;
            foreach (SObject tackle in tackles)
            {
                if (tackle != null && __instance.attachments[i] == null)
                {
                    if (tackle.Stack > 1)
                    {
                        tackle.Stack--;
                        tackle.uses.Value = 0;
                        __instance.attachments[i] = tackle;

                        string displayedMessage = new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086")).message;
                        _ = Game1.hudMessages.Remove(Game1.hudMessages.FirstOrDefault(item => item.message == displayedMessage));
                    }
                }
                i++;
            }
        }
    }
}
