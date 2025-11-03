using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov.Economy;
using Duckov.MiniMaps;
using Duckov.Quests;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Debrief
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public const string ModName = "Debrief";
        private const string Id = "XiLaiTL." + ModName;

        /// <summary>
        /// 进入关卡时玩家身上的总价值
        /// </summary>
        public static float EnterLevelTotalValue;

        /// <summary>
        /// 进入关卡时的时间
        /// </summary>
        public static TimeSpan EnterLevelTime;
        
        public static Dictionary<int, ItemStack> EnterLevelItemCountMap = new Dictionary<int, ItemStack>();
        
        public static string CurrentSceneName;
        
        public readonly static ExtraCamera ExtraCamera = new ExtraCamera();
        
        public static Dictionary<string, KillRecord> KillRecords = new Dictionary<string, KillRecord>();
        
        public static List<Task> EnterLevelFinishedTasks = new List<Task>();
        public static List<Quest> EnterLevelFinishedQuests = new List<Quest>();
        
        private Harmony harmony;
        
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

        public static List<Quest> PlayerQuestsFinished()
        {
            var quests = new List<Quest>();
            var questManager = QuestManager.Instance;
            if (questManager == null) return quests;
            foreach (var quest in questManager.ActiveQuests)
            {
                if (quest.AreTasksFinished())
                {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        public static List<Task> PlayerTasksFinished()
        {
            var tasks = new List<Task>();
            var questManager = QuestManager.Instance;
            if (questManager == null) return tasks;
            foreach (var quest in questManager.ActiveQuests)
            {
                if (quest.Tasks == null) continue;
                foreach (var task in quest.Tasks)
                {
                    if (task.IsFinished())
                    {
                        tasks.Add(task);
                    }
                }
            }
            return tasks;
        }
        
        

        public static List<Item> PlayerTotalItems()
        {
            var allBelongsToPlayer = new List<Item>();
            var characterInventory =  LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
            if (characterInventory != null)
            {
                foreach (var item in characterInventory)
                {
                    if (item != null)
                    {
                        allBelongsToPlayer.Add(item);
                    }
                }
            }
            var characterSlot = LevelManager.Instance?.MainCharacter?.CharacterItem?.Slots;
            if (characterSlot != null)
            {
                foreach (var slot in characterSlot)
                {
                    if (slot != null)
                    {
                        var item = slot.Content;
                        if (item != null)
                        {
                            allBelongsToPlayer.Add(item);
                        }
                    }
                }
            }
            
            var petProxyInventory = LevelManager.Instance?.PetProxy?.Inventory;
            if (petProxyInventory != null)
            {
                foreach (var item in petProxyInventory)
                {
                    if (item != null)
                    {
                        allBelongsToPlayer.Add(item);
                    }
                }
            }
        
            var allItems = new List<Item>();
            foreach (var item in allBelongsToPlayer)
            {
                var inventory = item.Inventory;
                if (inventory != null)
                {
                    foreach (var item2 in inventory)
                    {
                        if (item2 != null)
                        {
                            allItems.Add(item2);
                        }
                    }
                }
                var slots = item.Slots;
                if (slots != null)
                {
                    foreach (var slot in slots)
                    {
                        if (slot != null)
                        {
                            var item2 = slot.Content;
                            if (item2 != null)
                            {
                                allItems.Add(item2);
                            }
                        }
                    }
                }
            }
            allItems.AddRange(allBelongsToPlayer);
            return allItems;
        }
        
        public static Dictionary<int, ItemStack> ItemCountMap(List<Item> items)
        {
            var countMap = new Dictionary<int, ItemStack>();
            foreach (var item in items)
            {
                    if (countMap.ContainsKey(item.TypeID))
                    {
                        countMap[item.TypeID].StackCount += item.StackCount;
                    }
                    else
                    {
                        countMap[item.TypeID] = ItemStack.FromItem(item);
                    }
            }
            return countMap;
        }
        
        public static List<ItemStack> MostValueItems()
        {
            var totalItems = PlayerTotalItems();
            var currentItemCountMap = ItemCountMap(totalItems);
            // 创建键的副本，避免在枚举时修改字典
            var keys = new List<int>(currentItemCountMap.Keys);
            foreach (var key in keys)
            {
                if (EnterLevelItemCountMap.TryGetValue(key, out var value))
                {
                    currentItemCountMap[key].StackCount -= value.StackCount;
                }
                if (currentItemCountMap[key].StackCount <= 0)
                {
                    currentItemCountMap.Remove(key);
                }
            }
            
            var mostValueItems = new List<ItemStack>();
            
            // 首先筛选出ItemValueLevel为LightRed或Red的物品
            var lightRedAndRedItems = new List<ItemStack>();
            foreach (var itemStack in currentItemCountMap.Values)
            {
                if (itemStack.ItemValueLevel == ItemValueLevel.LightRed || itemStack.ItemValueLevel == ItemValueLevel.Red)
                {
                    lightRedAndRedItems.Add(itemStack);
                }
            }
            
            // 添加LightRed和Red物品到结果列表
            mostValueItems.AddRange(lightRedAndRedItems);
            
            // 如果数量少于5个，添加其他最值钱的物品
            if (mostValueItems.Count < 5)
            {
                // 获取除了LightRed和Red之外的所有物品
                var otherItems = new List<ItemStack>();
                foreach (var itemStack in currentItemCountMap.Values)
                {
                    if (itemStack.ItemValueLevel != ItemValueLevel.LightRed && itemStack.ItemValueLevel != ItemValueLevel.Red)
                    {
                        otherItems.Add(itemStack);
                    }
                }
                
                // 按价值排序
                otherItems.Sort((a, b) => b.GetTotalValue().CompareTo(a.GetTotalValue()));
                
                // 添加足够数量的其他物品，使总数达到5个
                int remainingCount = Math.Min(5 - mostValueItems.Count, otherItems.Count);
                mostValueItems.AddRange(otherItems.GetRange(0, remainingCount));
            }
            
            // 按价值排序最终结果
            mostValueItems.Sort((a, b) => b.GetTotalValue().CompareTo(a.GetTotalValue()));
            return mostValueItems;
        }

        public static List<FinishedQuest> GetFinishedQuests()
        {
            var curFinishedQuests = PlayerQuestsFinished();
            var curFinishedTasks = PlayerTasksFinished();
            var finishedQuests = new List<FinishedQuest>();
            foreach (var quest in curFinishedQuests)
            {
                if (!EnterLevelFinishedQuests.Contains(quest))
                {
                    var finishedTasks = new List<string>();
                    foreach (var task in quest.Tasks )
                    {
                        if (curFinishedTasks.Contains(task) && !EnterLevelFinishedTasks.Contains(task))
                        {
                            finishedTasks.Add(task.Description);
                        }
                    }
                    var finishedQuest = new FinishedQuest(quest.DisplayName, finishedTasks);
                    finishedQuests.Add(finishedQuest);
                }
            }
            return finishedQuests;
        }

        private void OnDead(Health health, DamageInfo damageInfo)
        {
            if (health == null)
            {
                return;
            }
            var killer = damageInfo.fromCharacter;
            var victim = damageInfo.toDamageReceiver.health.TryGetCharacter();

            if (killer == null || !killer.IsMainCharacter || victim == null || victim.IsMainCharacter)
            {
                return;
            }

            var time = GameClock.Now;
            var weaponItem = ItemAssetsCollection.GetPrefab(damageInfo.fromWeaponItemID);
            var weaponSprite = weaponItem != null 
                ? weaponItem.Icon 
                : MapMarkerManager.Icons.Find((Sprite e) => e != null && e.name == "swords");
            
            var victimName = "Unknown";
            Sprite? victimSprite = null;
            if (victim.characterPreset != null)
            {
                victimName = victim.characterPreset.DisplayName;
                victimSprite = victim.characterPreset.GetCharacterIcon();
            }
            
            if (KillRecords.TryGetValue(victimName, out var record))
            {
                record.KillCount++;
            }
            else
            {
                KillRecords[victimName] = new KillRecord(weaponSprite, victimSprite, victimName, 1, time);
            }
            
        }
        
        private void OnAfterSceneInitialize(SceneLoadingContext context)
        {
            ExtraCamera.CheckAndSetup();
            QuickStatsView.CreateView();
        }

        private void Update()
        {
            ExtraCamera.Update();
            
            // 检测O键按下
            if (Input.GetKeyDown(KeyCode.L))
            {
                QuickStatsView.Instance.ToggleView();
            }
        }

        private void OnStartedLoadingScene(SceneLoadingContext context)
        {
            if (CurrentSceneName == "Base")
            {
                // 如果之前在基地，说明要进入关卡了，记录进入关卡时玩家身上的总价值和时间
                EnterLevelTotalValue = PlayerTotalValue();
                EnterLevelTime = GameClock.Now;
                var totalItems = PlayerTotalItems();
                EnterLevelItemCountMap = ItemCountMap(totalItems);
                EnterLevelFinishedTasks = PlayerTasksFinished();
                EnterLevelFinishedQuests = PlayerQuestsFinished();
                KillRecords.Clear();
            }
            CurrentSceneName = context.sceneName;
            if (CurrentSceneName.Contains("Base") )
            {
                // 如果进入基地，说明要退出关卡
                ExtraCamera.Stop();
            }
        }

        private void OnEnable()
        {
            Debug.Log("Debrief Mod OnEnable");

            ItemValueUtils.Setup();
            SpriteUtils.Setup();
            
            SceneLoader.onStartedLoadingScene += OnStartedLoadingScene;
            SceneLoader.onAfterSceneInitialize += OnAfterSceneInitialize;
            Health.OnDead += OnDead;
            
            harmony = new Harmony(Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnDisable()
        {
            SceneLoader.onStartedLoadingScene -= OnStartedLoadingScene;
            SceneLoader.onAfterSceneInitialize -= OnAfterSceneInitialize;
            Health.OnDead -= OnDead;
            
            harmony.UnpatchAll(Id);
        }
    }
}