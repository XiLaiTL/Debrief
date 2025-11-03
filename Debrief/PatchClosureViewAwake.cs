using Duckov.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Debrief
{
    /// <summary>
    /// 结算界面Patch
    /// </summary>
    [HarmonyPatch(typeof(ClosureView), "Awake")]
    public static class PatchClosureViewAwake
    {
        private static void AddDurationTextComponent(ClosureView __instance)
        {
            var reasonOfDeath = __instance.transform.Find("Content/ReasonOfDeath");
            if (reasonOfDeath == null)
            {
                return;
            }
            var siblingIndex = reasonOfDeath.GetSiblingIndex();
            QuickStatsView.AddDurationTextComponent(reasonOfDeath.transform.parent);
            QuickStatsView.durationText.transform.SetSiblingIndex(siblingIndex + 1);
        }
        
        private static void AddTotalRewardContainer(ClosureView __instance)
        {
            var expBarContainer = __instance.transform.Find("Content/ExpBarContainer");
            if (expBarContainer == null)
            {
                return;
            }
            var expBarIdx = expBarContainer.GetSiblingIndex();
            var expBarRect = expBarContainer.Find("ExpBar").GetComponent<Image>().GetComponent<RectTransform>();
            if (expBarRect == null)
            {
                return;
            }
            QuickStatsView.AddTotalRewardContainer(expBarContainer.transform.parent, expBarRect.sizeDelta.x);
            QuickStatsView.totalRewardContainerRect.SetSiblingIndex(expBarIdx);
            
        }

        
        public static void Prefix(ClosureView __instance)
        {
            QuickStatsView.AddExtraCharacterRenderer(__instance.transform);
            
            AddDurationTextComponent(__instance);
            
            AddTotalRewardContainer(__instance);
            
            // 先创建左侧列容器
            QuickStatsView.AddLeftContentContainer(__instance.transform);
            
            // 然后创建击杀记录容器（作为LeftContent的上半部分）
            QuickStatsView.AddKillRecordContainer(__instance.transform);
            
            // 最后创建任务记录容器（作为LeftContent的下半部分）
            QuickStatsView.AddQuestRecordContainer(__instance.transform);
        }

        
    }
}