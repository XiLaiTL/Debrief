using System;
using System.Collections.Generic;
using SodaCraft.Localizations;
using UnityEngine;

namespace Debrief
{
    public enum LanguageKey
    {
        TotalReward,
        Duration
    }
    
    public class TranslatableText
    {
        private static readonly Dictionary<LanguageKey, Dictionary<SystemLanguage, string>> LocalizationMap =
            new Dictionary<LanguageKey, Dictionary<SystemLanguage, string>>
            {
                {
                    LanguageKey.TotalReward, new Dictionary<SystemLanguage, string>
                    {
                        { SystemLanguage.ChineseSimplified, "本局收益：" },
                        { SystemLanguage.ChineseTraditional, "本局收益：" },
                        { SystemLanguage.Japanese, "今回の獲得：" },
                        { SystemLanguage.German, "Gesamtertrag: " },
                        { SystemLanguage.Russian, "Общий результат: " },
                        { SystemLanguage.Spanish, "Total de la partida:" },
                        { SystemLanguage.Korean, "이번 경기 수익: " },
                        { SystemLanguage.French, "Gain total: " },
                        { SystemLanguage.Portuguese, "Total da partida: " },
                        { SystemLanguage.English, "Match Total: " }
                    }
                },
                {
                    LanguageKey.Duration, new Dictionary<SystemLanguage, string>
                    {
                        { SystemLanguage.ChineseSimplified, "时长：{0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.ChineseTraditional, "時長：{0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.Japanese, "時間: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.German, "Dauer: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.Russian, "Продолжительность: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.Spanish, "Duración: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.Korean, "시간: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.French, "Durée: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.Portuguese, "Duração: {0:D2}:{1:D2}:{2:D2}" },
                        { SystemLanguage.English, "Duration: {0:D2}:{1:D2}:{2:D2}" }
                    }
                }
                
            };

        // 主要的格式化函数
        public static string T(LanguageKey key, params object[] args)
        {
            if (!LocalizationMap.TryGetValue(key, out var languageMap))
            {
                Debug.LogWarning($"Localization key not found: {key}");
                return $"#{key}#";
            }

            var currentLanguage = LocalizationManager.CurrentLanguage;

            // 如果当前语言没有对应的翻译，回退到英语
            if (!languageMap.TryGetValue(currentLanguage, out string format))
            {
                if (!languageMap.TryGetValue(SystemLanguage.English, out format))
                {
                    // 如果英语也没有，使用第一个可用的语言
                    using var enumerator = languageMap.Values.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        format = enumerator.Current;
                    }
                    else
                    {
                        return $"#{key}#";
                    }
                }
            }

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"Format error for key '{key}': {ex.Message}");
                return format; // 返回原始格式字符串
            }
        }
    }
}