using System;
using System.Reflection;
using Duckov.Economy;
using HarmonyLib;
using SodaCraft.Localizations;
using UnityEngine;

namespace DisplayTotalReward
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Id = "Spuddy.DisplayTotalReward";

        /// <summary>
        /// 进入关卡时玩家身上的总价值
        /// </summary>
        public static float EnterLevelTotalValue;

        /// <summary>
        /// 进入关卡时的时间
        /// </summary>
        public static TimeSpan EnterLevelTime;
        public static string CurrentSceneName;

        private Harmony harmony;

        public static string GetTotalRewardText(int value)
        {
            switch (LocalizationManager.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return $"本局收益：{value:n0}";
                case SystemLanguage.Japanese:
                    return $"今回の獲得：{value:n0}";
                case SystemLanguage.German:
                    return $"Gesamtertrag: {value:n0}";
                case SystemLanguage.Russian:
                    return $"Общий результат: {value:n0}";
                case SystemLanguage.Spanish:
                    return $"Total de la partida: {value:n0}";
                case SystemLanguage.Korean:
                    return $"이번 경기 수익: {value:n0}";
                case SystemLanguage.French:
                    return $"Gain total: {value:n0}";
                case SystemLanguage.Portuguese:
                    return $"Total da partida: {value:n0}";
                case SystemLanguage.English:
                default:
                    return $"Match Total: {value:n0}";
            }
        }

        public static string GetDurationText(TimeSpan duration)
        {
            switch (LocalizationManager.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return $"时长：{duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.ChineseTraditional:
                    return $"時長：{duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.Japanese:
                    return $"時間: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.German:
                    return $"Dauer: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.Russian:
                    return $"Продолжительность: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.Spanish:
                    return $"Duración: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.Korean:
                    return $"시간: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.French:
                    return $"Durée: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.Portuguese:
                    return $"Duração: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
                case SystemLanguage.English:
                default:
                    return $"Duration: {duration.Days:D2}:{duration.Hours:D2}:{duration.Minutes:D2}";
            }
        }

        /// <summary>
        /// 计算玩家和宠物身上的总价值
        /// </summary>
        /// <returns></returns>
        public static float PlayerTotalValue()
        {
            float value = 0;
            // 玩家本身也是个Item，直接调用GetTotalRawValue()即可
            var characterItem = LevelManager.Instance?.MainCharacter?.CharacterItem;
            if (characterItem != null)
            {
                value += characterItem.GetTotalRawValue() / 2;
            }
            var petInventory = LevelManager.Instance?.PetProxy?.Inventory?.Content;
            if (petInventory != null)
            {
                foreach (var item in petInventory)
                {
                    if (item != null)
                    {
                        value += item.GetTotalRawValue() / 2;
                    }
                }
            }
            // 玩家的金钱也算在总价值中
            value += EconomyManager.Money;
            return value;
        }

        private void OnEnable()
        {
            Debug.Log("ItemLevelAndSearchSoundMod OnEnable");

            SceneLoader.onStartedLoadingScene += OnStartedLoadingScene;

            harmony = new Harmony(Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnStartedLoadingScene(SceneLoadingContext context)
        {
            Debug.Log($"OnAfterLevelInitialized");
            if (CurrentSceneName == "Base")
            {
                // 如果之前在基地，说明要进入关卡了，记录进入关卡时玩家身上的总价值和时间
                EnterLevelTotalValue = PlayerTotalValue();
                EnterLevelTime = GameClock.Now;
            }
            CurrentSceneName = context.sceneName;
        }

        private void OnDisable()
        {
            SceneLoader.onStartedLoadingScene -= OnStartedLoadingScene;
            harmony.UnpatchAll(Id);
        }
    }
}
