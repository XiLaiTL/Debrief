using ItemStatsSystem;
using UnityEngine;

namespace Debrief
{
    /// <summary>
    /// from ItemLevelAndSearchSoundMod
    /// 物品价值等级对应的颜色
    /// </summary>
    public static class ItemValueUtils
    {
        public static Color White;
        public static Color Green;
        public static Color Blue;
        public static Color Purple;
        public static Color Orange;
        public static Color LightRed;
        public static Color Red;

        public static void Setup()
        {
            ColorUtility.TryParseHtmlString("#FFFFFF00", out White);
            ColorUtility.TryParseHtmlString("#7cff7c40", out Green);
            ColorUtility.TryParseHtmlString("#7cd5ff40", out Blue);
            ColorUtility.TryParseHtmlString("#d0acff40", out Purple);
            ColorUtility.TryParseHtmlString("#ffdc2496", out Orange);
            ColorUtility.TryParseHtmlString("#ff585896", out LightRed);
            ColorUtility.TryParseHtmlString("#bb000096", out Red);
        }
        
        public static ItemValueLevel GetItemValueLevel(Item item)
        {
            if (item == null)
            {
                return ItemValueLevel.White;
            }
            // 除2得到售价
            var value = item.Value / 2f;

            if (item.Tags.Contains("Bullet"))
            {
                // 子弹特殊处理
                if (item.DisplayQuality != DisplayQuality.None)
                {
                    return item.DisplayQuality == DisplayQuality.Orange ?
                        // 6级特种弹
                        ItemValueLevel.LightRed :
                        // 有官方稀有度的子弹，使用官方的稀有度
                        ParseDisplayQuality(item.DisplayQuality);
                }

                if (item.Quality == 1)
                {
                    // 生锈弹
                    return ItemValueLevel.White;
                }
                if (item.Quality == 2)
                {
                    // 普通弹
                    return ItemValueLevel.Green;
                }
                // 剩下的都是特殊子弹，根据30一组计算稀有度，最高到橙色
                var bulletLevel = CalculateItemValueLevel((int)(value * 30));
                if (bulletLevel > ItemValueLevel.Orange)
                {
                    return ItemValueLevel.Orange;
                }
                return bulletLevel;
            }

            if (item.Tags.Contains("Equipment"))
            {
                // 装备特殊处理
                if (item.Tags.Contains("Special"))
                {
                    if (item.name.Contains("StormProtection"))
                    {
                        // 风暴系列的装备稀有度直接使用官方的
                        return (ItemValueLevel) (item.Quality - 1);
                    }
                    var quality = item.Quality - 2;
                    if (quality > 6)
                    {
                        return ItemValueLevel.Red;
                    }
                    if (quality < 0)
                    {
                        return ItemValueLevel.White;
                    }
                    return (ItemValueLevel) quality;
                }
                else
                {
                    // 非特殊装备
                    if (item.Quality <= 7)
                    {
                        // 7以内的装备按官方稀有度计算
                        return (ItemValueLevel) (item.Quality - 1);
                    }
                    return CalculateItemValueLevel((int)value);
                }
            }

            if (item.Tags.Contains("Accessory"))
            {
                // 配件特殊处理
                if (item.Quality <= 7)
                {
                    return (ItemValueLevel) (item.Quality - 1);
                }

                return ParseDisplayQuality(item.DisplayQuality);
            }

            if (item.TypeID == 862 || item.TypeID == 1238)
            {
                // 带火AK-47、MF-毒液的价格和普通是一样的，特殊处理下
                return ItemValueLevel.Orange;
            }
            
            // 物品价值
            var itemValueLevel = CalculateItemValueLevel((int)value);

            // 官方的物品稀有度和物品价值取最大值
            var displayQuality = ParseDisplayQuality(item.DisplayQuality);

            if (displayQuality > itemValueLevel)
            {
                itemValueLevel = displayQuality;
            }
            return itemValueLevel;
        }

        public static ItemValueLevel CalculateItemValueLevel(int value)
        {
            if (value >= 10000)
            {
                // 范围内53个道具
                return ItemValueLevel.Red;
            }
            else if (value >= 5000)
            {
                // 范围内84个道具
                return ItemValueLevel.LightRed;
            }
            else if (value >= 2500)
            {
                // 范围内90个道具
                return ItemValueLevel.Orange;
            }
            else if (value >= 1200)
            {
                // 范围内146个道具
                return ItemValueLevel.Purple;
            }
            else if (value >= 600)
            {
                // 范围内177个道具
                return ItemValueLevel.Blue;
            }
            else if (value >= 200)
            {
                // 范围内253个道具
                return ItemValueLevel.Green;
            }
            else
            {
                // 范围内376个道具
                return ItemValueLevel.White;
            }
        }

        public static ItemValueLevel ParseDisplayQuality(DisplayQuality displayQuality)
        {
            switch (displayQuality)
            {
                case DisplayQuality.None:
                case DisplayQuality.White:
                    return ItemValueLevel.White;
                case DisplayQuality.Green:
                    return ItemValueLevel.Green;
                case DisplayQuality.Blue:
                    return ItemValueLevel.Blue;
                case DisplayQuality.Purple:
                    return ItemValueLevel.Purple;
                case DisplayQuality.Orange:
                    return ItemValueLevel.Orange;
                case DisplayQuality.Red:
                case DisplayQuality.Q7:
                case DisplayQuality.Q8:
                    return ItemValueLevel.Red;
                default:
                    return ItemValueLevel.White;
            }
        }

        public static Color GetItemValueLevelColor(ItemValueLevel level)
        {
            switch (level)
            {
                case ItemValueLevel.Red:
                    return Red;
                case ItemValueLevel.LightRed:
                    return LightRed;
                case ItemValueLevel.Orange:
                    return Orange;
                case ItemValueLevel.Purple:
                    return Purple;
                case ItemValueLevel.Blue:
                    return Blue;
                case ItemValueLevel.Green:
                    return Green;
                case ItemValueLevel.White:
                    return White;
                default:
                    return White;
            }
        }
    }
}