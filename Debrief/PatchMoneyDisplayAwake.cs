using System.Reflection;
using Duckov.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debrief
{
    [HarmonyPatch(typeof(MoneyDisplay), "Awake")]
    public class PatchMoneyDisplayAwake
    {
        public static void Prefix(MoneyDisplay __instance)
        {
            var text = (TextMeshProUGUI) typeof(MoneyDisplay).GetField("text", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            void CreateSpace()
            {
                GameObject space = new GameObject("Space");
                space.AddComponent<LayoutElement>().preferredWidth = 10;
                space.transform.SetParent(text.transform.parent);
            }

            CreateSpace();

            var totalRewardText = UnityEngine.Object.Instantiate(text, text.transform.parent);
            totalRewardText.gameObject.name = "TotalRewardText";
        }
    }
}