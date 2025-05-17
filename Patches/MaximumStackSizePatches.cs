namespace StackEverythingRedux.Patches
{
    internal class MaximumStackSizePatches
    {
        public static bool Prefix(ref int __result)
        {
            __result = StackEverythingRedux.Config.MaxStackingNumber;

            return false;
        }
    }
}
