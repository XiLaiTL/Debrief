using System;
using ItemStatsSystem;
using UnityEngine;

namespace Debrief
{
    [Serializable]
    public class ItemStack
    {
        public int TypeID { get; }
        public int StackCount { get; set; }
        public Sprite Icon { get; }
        public string DisplayName { get; }
        public int Value { get; }
        public ItemValueLevel ItemValueLevel { get; }

        public int GetTotalValue()
        {
            return Value * StackCount / 2;
        }

        public ItemStack(int typeID, int stackCount, Sprite icon, string displayName, int value,
            ItemValueLevel? itemValueLevel)
        {
            TypeID = typeID;
            StackCount = stackCount;
            Icon = icon;
            DisplayName = displayName;
            Value = value;
            ItemValueLevel = itemValueLevel ?? ItemValueUtils.CalculateItemValueLevel(value);
        }

        public static ItemStack FromItem(Item item)
        {
            return new ItemStack(item.TypeID, item.StackCount, item.Icon, item.DisplayName, item.Value,
                ItemValueUtils.GetItemValueLevel(item));
        }
    }
}