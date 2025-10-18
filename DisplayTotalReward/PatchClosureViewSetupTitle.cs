using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace DisplayTotalReward
{
    [HarmonyPatch(typeof(ClosureView), "SetupTitle")]
    public static class PatchClosureViewSetupTitle
    {
        public static void Prefix(ClosureView __instance, bool dead)
        {
            var textObj = __instance.transform.Find("Content/TotalRewardText");
            if (textObj != null)
            {
                int totalValue = (int)(ModBehaviour.PlayerTotalValue() - ModBehaviour.EnterLevelTotalValue);
                textObj.GetComponent<TextMeshProUGUI>().text = ModBehaviour.GetTotalRewardText(totalValue);
            }

            var durationTextObj = __instance.transform.Find("Content/DurationText");
            if (durationTextObj != null)
            {
                var durationText = durationTextObj.GetComponent<TextMeshProUGUI>();
                var duration = GameClock.Now - ModBehaviour.EnterLevelTime;
                
                durationText.text = ModBehaviour.GetDurationText(duration);
            }
        }
    }
}