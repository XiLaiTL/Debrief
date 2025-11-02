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
            PatchClosureViewAwake.SetTotalRewardText(totalValue);
            
            var duration = GameClock.Now - ModBehaviour.EnterLevelTime;
            PatchClosureViewAwake.SetDurationText(duration);

            var mostValueItems = ModBehaviour.MostValueItems();
            PatchClosureViewAwake.SetItemStack(mostValueItems);
            
            PatchClosureViewAwake.UpdateKillRecords(ModBehaviour.KillRecords);
            
            PatchClosureViewAwake.UpdateQuestRecords(ModBehaviour.GetFinishedQuests());
        }
    }
}