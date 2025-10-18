using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using UnityEngine;

namespace DisplayTotalReward
{
    /// <summary>
    /// 结算界面Patch
    /// </summary>
    [HarmonyPatch(typeof(ClosureView), "Awake")]
    public static class PatchClosureViewAwake
    {
        public static void Prefix(ClosureView __instance)
        {
            Transform expBarContainer = __instance.transform.Find("Content/ExpBarContainer");

            if (expBarContainer != null)
            {
                var text = Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, expBarContainer.transform.parent);
                text.gameObject.name = "TotalRewardText"; 
                text.enableAutoSizing = true;
                text.fontSizeMin = 40;
                text.fontSizeMax = 80;
                int expBarIdx = expBarContainer.GetSiblingIndex();
                text.transform.SetSiblingIndex(expBarIdx);

                var durationText = Object.Instantiate(text, text.transform.parent);
                durationText.gameObject.name = "DurationText";
                durationText.enableAutoSizing = true;
                durationText.fontSizeMin = 40;
                durationText.fontSizeMax = 60;
                durationText.transform.SetSiblingIndex(expBarIdx);
            }
        }
    }
}