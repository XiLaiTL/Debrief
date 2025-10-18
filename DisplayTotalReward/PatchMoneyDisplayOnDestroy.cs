using Duckov.UI;
using HarmonyLib;

namespace DisplayTotalReward
{
    [HarmonyPatch(typeof(MoneyDisplay), "OnDestroy")]
    public class PatchMoneyDisplayOnDestroy
    {
        public static void Prefix(MoneyDisplay __instance)
        {
            PatchMoneyDisplayOnEnable.Unregister(__instance);
        }
    }
}