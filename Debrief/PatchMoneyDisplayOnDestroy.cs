using Duckov.UI;
using HarmonyLib;

namespace Debrief
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