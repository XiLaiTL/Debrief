using Duckov.UI;
using HarmonyLib;
using UnityEngine;

namespace Debrief
{
    /// <summary>
    /// 针对ClosureView.ShowAndReturnTask方法的Prefix Patch
    /// </summary>
    ///
    /// 但是好像在这时开启相机不起作用
    [HarmonyPatch(typeof(ClosureView))]
    public static class PatchClosureViewShowAndReturnTask
    {
        /// <summary>
        /// 针对第一个ShowAndReturnTask方法的Prefix
        /// </summary>
        [HarmonyPatch("ShowAndReturnTask", typeof(float))]
        [HarmonyPrefix]
        public static bool PrefixShowAndReturnTask1(float duration = 0.5f)
        {
            Debug.Log($"[Debrief] ShowAndReturnTask called with duration: {duration}");
            
            ModBehaviour.ExtraCamera.Open();
            ModBehaviour.ExtraCamera.Start();
            return true;
        }

        /// <summary>
        /// 针对第二个ShowAndReturnTask方法的Prefix
        /// </summary>
        [HarmonyPatch("ShowAndReturnTask", typeof(DamageInfo), typeof(float))]
        [HarmonyPrefix]
        public static bool PrefixShowAndReturnTask2(DamageInfo dmgInfo, float duration = 0.5f)
        {
            Debug.Log($"[Debrief] ShowAndReturnTask called with DamageInfo and duration: {duration}");
            
            ModBehaviour.ExtraCamera.Open();
            ModBehaviour.ExtraCamera.Start();
            return true;
        }
    }
}