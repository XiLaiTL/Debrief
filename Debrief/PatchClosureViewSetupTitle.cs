using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debrief
{
    [HarmonyPatch(typeof(ClosureView), "SetupTitle")]
    public static class PatchClosureViewSetupTitle
    {
        
        public static void Prefix(ClosureView __instance, bool dead)
        {

            var totalValue = (int)(ModBehaviour.PlayerTotalValue() - ModBehaviour.EnterLevelTotalValue);
            QuickStatsView.SetTotalRewardText(totalValue);
            
            var duration = GameClock.Now - ModBehaviour.EnterLevelTime;
            QuickStatsView.SetDurationText(duration);

            var mostValueItems = ModBehaviour.MostValueItems();
            QuickStatsView.SetItemStack(mostValueItems);
            
            QuickStatsView.UpdateKillRecords(ModBehaviour.KillRecords);
            
            QuickStatsView.UpdateQuestRecords(ModBehaviour.GetFinishedQuests());
        }
    }
}