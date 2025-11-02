using System;
using System.Collections.Generic;
using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Debrief
{
    /// <summary>
    /// 结算界面Patch
    /// </summary>
    [HarmonyPatch(typeof(ClosureView), "Awake")]
    public static class PatchClosureViewAwake
    {
        private static void AddExtraCharacterRenderer(Transform transform)
        {
            // 开启相机渲染
            ModBehaviour.ExtraCamera.Start();

            // var firstImage = __instance.transform.Find("Image");
            // if (firstImage != null)
            // {
            //     ModBehaviour.ExtraCamera.SetBackgroundColor(firstImage.GetComponent<Image>().color);
            // }
            
            
            var containerGO = new GameObject("ExtraCharacterRenderer");
            var container = containerGO.AddComponent<RectTransform>();
            // 设置父级和锚点
            container.SetParent(transform); 
            container.anchorMin = new Vector2(1f, 0.5f);  // 左下锚点：右侧(1)，垂直中心(0.5)
            container.anchorMax = new Vector2(1f, 0.5f);  // 右上锚点：右侧(1)，垂直中心(0.5)

            // 设置轴心点为右侧中心，这样旋转和缩放会以右侧为中心
            container.pivot = new Vector2(1f, 0.5f);
            container.anchoredPosition = Vector2.zero;

            container.sizeDelta = new Vector2(512, 512);
            
            var mapImage = containerGO.AddComponent<RawImage>();
            mapImage.texture = ModBehaviour.ExtraCamera.CharacterTexture;
        }

        private static TextMeshProUGUI durationText;
        public static void SetDurationText(TimeSpan duration)
        {
            if (durationText == null)
            {
                return;
            }
            durationText.text = TranslatableText.T(LanguageKey.Duration, duration.Days, duration.Hours, duration.Minutes);
        }
        private static void AddDurationTextComponent(ClosureView __instance)
        {
            var reasonOfDeath = __instance.transform.Find("Content/ReasonOfDeath");
            if (reasonOfDeath == null)
            {
                return;
            }
            var siblingIndex = reasonOfDeath.GetSiblingIndex();
            AddDurationTextComponent(reasonOfDeath.transform.parent);
            durationText.transform.SetSiblingIndex(siblingIndex + 1);
        }

        private static void AddDurationTextComponent(Transform transform)
        {
            durationText = Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, transform);
            durationText.gameObject.name = "DurationText";
            durationText.enableAutoSizing = true;
            durationText.fontSizeMin = 25;
            durationText.fontSizeMax = 40;
        }
        
        private static TextMeshProUGUI totalRewardText;
        public static void SetTotalRewardText(float totalReward)
        {
            if (totalRewardText == null)
            {
                return;
            }
            totalRewardText.text = TranslatableText.T(LanguageKey.TotalReward, totalReward);
        }

        private static RectTransform itemStackContainerRect;
        private static ItemStackUI[] itemStackUIs;
        
        /// <summary>
        /// 更新物品堆栈显示
        /// </summary>
        public static void SetItemStack(List<ItemStack> itemStacks)
        {
            if (itemStackContainerRect == null)
            {
                return;
            }
            
            // 清空现有的物品堆栈UI
            if (itemStackUIs != null)
            {
                foreach (var ui in itemStackUIs)
                {
                    if (ui != null) Object.Destroy(ui.gameObject);
                }
            }
            
            int itemCount = itemStacks.Count;
            if (itemCount == 0) return;
            
            // 计算需要的行数（每行5个物品）
            int rows = (itemCount + 4) / 5; // 向上取整
            itemStackUIs = new ItemStackUI[itemCount];
            
            // 创建行容器
            for (int row = 0; row < rows; row++)
            {
                // 创建水平行容器
                var rowGO = new GameObject($"ItemRow{row}");
                var rowRect = rowGO.AddComponent<RectTransform>();
                rowRect.SetParent(itemStackContainerRect, false);
                rowRect.sizeDelta = new Vector2(600, 100);
                
                // 设置行布局
                var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
                rowLayout.childAlignment = TextAnchor.MiddleCenter;
                rowLayout.childControlWidth = false;
                rowLayout.childControlHeight = false;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;
                rowLayout.spacing = 10;
                
                // 当前行的物品数量（最多5个）
                int itemsInRow = Math.Min(5, itemCount - row * 5);
                
                for (int col = 0; col < itemsInRow; col++)
                {
                    int itemIndex = row * 5 + col;
                    if (itemIndex >= itemCount) break;
                    
                    var itemStackGO = new GameObject($"ItemStack{itemIndex}");
                    var itemStackRect = itemStackGO.AddComponent<RectTransform>();
                    itemStackRect.SetParent(rowRect, false);
                    itemStackRect.sizeDelta = new Vector2(100, 100);
            
                    // 设置布局元素
                    var layoutElement = itemStackGO.AddComponent<LayoutElement>();
                    layoutElement.preferredWidth = 100;
                    layoutElement.preferredHeight = 100;
                    layoutElement.flexibleWidth = 0;
                    layoutElement.flexibleHeight = 0;
                    
                    var itemStackUI = itemStackGO.AddComponent<ItemStackUI>();
                    itemStackUI.SetItemStack(itemStacks[itemIndex]);
                    itemStackUIs[itemIndex] = itemStackUI;
                }
            }
        }
        
        private static RectTransform totalRewardContainerRect;
        
        private static void AddTotalRewardContainer(ClosureView __instance)
        {
            var expBarContainer = __instance.transform.Find("Content/ExpBarContainer");
            if (expBarContainer == null)
            {
                return;
            }
            var expBarIdx = expBarContainer.GetSiblingIndex();
            var expBarRect = expBarContainer.GetComponent<RectTransform>();
            if (expBarRect == null)
            {
                return;
            }
            AddTotalRewardContainer(expBarContainer.transform.parent, expBarRect.sizeDelta.x);
            totalRewardContainerRect.SetSiblingIndex(expBarIdx);
            
        }

        private static void AddTotalRewardContainer(Transform transform, float width)
        {
            var totalRewardContainer = new GameObject("TotalRewardContainer");
            totalRewardContainerRect = totalRewardContainer.AddComponent<RectTransform>();
            totalRewardContainerRect.SetParent(transform);
            totalRewardContainerRect.sizeDelta = new Vector2(width, 200);
            totalRewardContainerRect.anchorMin = new Vector2(0.5f, 1f);
            totalRewardContainerRect.anchorMax = new Vector2(0.5f, 1f);
            totalRewardContainerRect.pivot = new Vector2(0.5f, 1f);
            totalRewardContainerRect.anchoredPosition = Vector2.zero;
            
            var containerLayoutElement = totalRewardContainer.AddComponent<LayoutElement>();
            containerLayoutElement.preferredHeight = 200;
            containerLayoutElement.flexibleWidth = 1;
            
            // 为totalRewardContainerRect添加垂直布局组
            var verticalLayout = totalRewardContainer.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            
            // 添加内容尺寸适配器
            var contentFitter = totalRewardContainer.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            totalRewardText = Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, totalRewardContainerRect);
            totalRewardText.gameObject.name = "TotalRewardText"; 
            totalRewardText.enableAutoSizing = true;
            totalRewardText.fontSizeMin = 30;
            totalRewardText.fontSizeMax = 40;
            totalRewardText.alignment = TextAlignmentOptions.Center;
            
            // 为totalRewardText添加布局元素，使其在垂直布局中正确排列
            var textLayoutElement = totalRewardText.gameObject.AddComponent<LayoutElement>();
            textLayoutElement.preferredHeight = 50;
            textLayoutElement.flexibleHeight = 0;
            
            var itemStackContainerGO = new GameObject("DebriefItemContainer");
            itemStackContainerRect = itemStackContainerGO.AddComponent<RectTransform>();
            itemStackContainerRect.SetParent(totalRewardContainerRect);
            
            // 设置容器布局
            itemStackContainerRect.anchorMin = new Vector2(0.5f, 1f);
            itemStackContainerRect.anchorMax = new Vector2(0.5f, 1f);
            itemStackContainerRect.pivot = new Vector2(0.5f, 1f);
            itemStackContainerRect.localScale = Vector3.one;
            
            // 调整容器大小以适应竖直布局
            itemStackContainerRect.sizeDelta = new Vector2(600, 300);  // 增加高度以容纳多行
            itemStackContainerRect.anchoredPosition = new Vector2(0, -60);
            
            // 为itemStackContainerRect添加布局元素
            var itemContainerLayoutElement = itemStackContainerGO.AddComponent<LayoutElement>();
            itemContainerLayoutElement.preferredHeight = 300;
            itemContainerLayoutElement.flexibleHeight = 1;
            
            // 添加滚动视图
            var scrollRect = itemStackContainerGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // 添加遮罩
            var mask = itemStackContainerGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // 添加背景
            var background = itemStackContainerGO.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.5f); // 半透明黑色
            
            // 创建内容容器
            var contentGO = new GameObject("ItemStackContent");
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.SetParent(itemStackContainerRect, false);
            
            // 设置内容容器的锚点和大小
            contentRect.anchorMin = new Vector2(0f, 1f);  // 左下锚点：左侧(0)，顶部(1)
            contentRect.anchorMax = new Vector2(1f, 1f);  // 右上锚点：右侧(1)，顶部(1)
            contentRect.pivot = new Vector2(0.5f, 1f);    // 轴心点：顶部中心
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f); // 宽度自适应，高度自适应
            
            // 将滚动视图的视口和内容设置正确
            scrollRect.viewport = itemStackContainerRect;
            scrollRect.content = contentRect;
            
            // 为内容容器添加垂直布局组
            var itemVerticalLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            itemVerticalLayout.childAlignment = TextAnchor.UpperCenter;  // 顶部居中对齐
            itemVerticalLayout.childControlWidth = false;
            itemVerticalLayout.childControlHeight = false;
            itemVerticalLayout.childForceExpandWidth = false;
            itemVerticalLayout.childForceExpandHeight = false;
            itemVerticalLayout.spacing = 10;
            
            // 添加内容尺寸适配
            var itemContentFitter = contentGO.AddComponent<ContentSizeFitter>();
            itemContentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        #region 左侧列容器
        private static RectTransform leftContentRect;
        
        /// <summary>
        /// 创建左侧列容器
        /// </summary>
        private static void AddLeftContentContainer(Transform transform)
        {
            var containerGO = new GameObject("LeftContent");
            leftContentRect = containerGO.AddComponent<RectTransform>();
            
            // 设置父级和锚点，使其固定在左侧并占满高度
            leftContentRect.SetParent(transform);
            leftContentRect.anchorMin = new Vector2(0f, 0f);  // 左下锚点：左侧(0)，底部(0)
            leftContentRect.anchorMax = new Vector2(0f, 1f);  // 右上锚点：左侧(0)，顶部(1)
            leftContentRect.pivot = new Vector2(0f, 1f);      // 轴心点：左上角
            leftContentRect.anchoredPosition = new Vector2(20f, -20f); // 距离左上角20像素
            leftContentRect.sizeDelta = new Vector2(500f, 0f); // 宽度500，高度自适应
            
            // 添加垂直布局组
            var verticalLayout = containerGO.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            
            // 添加内容尺寸适配器
            var contentFitter = containerGO.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        #endregion

        #region 击杀记录容器
        private static RectTransform killRecordContainerRect;
        private static KillRecordUI[] killRecordUIs;
        
        /// <summary>
        /// 更新击杀记录显示
        /// </summary>
        public static void UpdateKillRecords(Dictionary<string, KillRecord> records)
        {
            if (killRecordContainerRect == null)
            {
                return;
            }
            
            // 清空现有的击杀记录UI
            if (killRecordUIs != null)
            {
                foreach (var ui in killRecordUIs)
                {
                    if (ui != null) Object.Destroy(ui.gameObject);
                }
            }
            
            // 创建新的击杀记录UI数组
            int recordCount = records.Values.Count;
            killRecordUIs = new KillRecordUI[recordCount];
            
            int index = 0;
            foreach (var killRecord in records.Values)
            {
                var killRecordGO = new GameObject("KillRecord");
                var killRecordRect = killRecordGO.AddComponent<RectTransform>();
                killRecordRect.SetParent(killRecordContainerRect, false);
                killRecordRect.sizeDelta = new Vector2(480, 50); // 宽度略小于容器
                
                var killRecordUI = killRecordGO.AddComponent<KillRecordUI>();
                killRecordUI.SetKillRecord(killRecord);
                killRecordUIs[index++] = killRecordUI;
            }
        }
        
        /// <summary>
        /// 创建击杀记录容器作为LeftContent的上半部分子容器
        /// </summary>
        private static void AddKillRecordContainer(Transform transform)
        {
            var containerGO = new GameObject("KillRecordContainer");
            killRecordContainerRect = containerGO.AddComponent<RectTransform>();
            
            // 设置父级为LeftContent容器
            if (leftContentRect != null)
            {
                killRecordContainerRect.SetParent(leftContentRect, false);
            }
            else
            {
                killRecordContainerRect.SetParent(transform);
            }
            
            killRecordContainerRect.anchorMin = new Vector2(0f, 1f);  // 左下锚点：左侧(0)，顶部(1)
            killRecordContainerRect.anchorMax = new Vector2(1f, 1f);  // 右上锚点：右侧(1)，顶部(1)
            killRecordContainerRect.pivot = new Vector2(0.5f, 1f);    // 轴心点：顶部中心
            killRecordContainerRect.anchoredPosition = Vector2.zero;
            killRecordContainerRect.sizeDelta = new Vector2(0f, 300f); // 宽度自适应，固定高度300
            
            // 添加垂直布局组
            var verticalLayout = containerGO.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            
            // 添加内容尺寸适配器
            var contentFitter = containerGO.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 添加滚动视图
            var scrollRect = containerGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = killRecordContainerRect;
            scrollRect.content = killRecordContainerRect;
            
            // 添加遮罩
            var mask = containerGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // 添加背景
            var background = containerGO.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.5f); // 半透明黑色
            
            // 添加标题
            var titleGO = new GameObject("Title");
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.SetParent(killRecordContainerRect, false);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -5f);
            
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "击杀记录";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 18;
            titleText.color = Color.white;
        }
        #endregion

        #region 任务记录容器
        private static RectTransform questRecordContainerRect;
        private static FinishedQuestUI[] questRecordUIs;
        
        /// <summary>
        /// 更新任务记录显示
        /// </summary>
        public static void UpdateQuestRecords(List<FinishedQuest> records)
        {
            if (questRecordContainerRect == null)
            {
                return;
            }
            
            // 清空现有的任务记录UI
            if (questRecordUIs != null)
            {
                foreach (var ui in questRecordUIs)
                {
                    if (ui != null) Object.Destroy(ui.gameObject);
                }
            }
            
            // 创建新的任务记录UI数组
            int recordCount = records.Count;
            questRecordUIs = new FinishedQuestUI[recordCount];
            
            int index = 0;
            foreach (var questRecord in records)
            {
                var questRecordGO = new GameObject("QuestRecord");
                var questRecordRect = questRecordGO.AddComponent<RectTransform>();
                questRecordRect.SetParent(questRecordContainerRect, false);
                questRecordRect.sizeDelta = new Vector2(480, 50); // 宽度略小于容器
                
                var questRecordUI = questRecordGO.AddComponent<FinishedQuestUI>();
                questRecordUI.SetQuestData(questRecord);
                questRecordUIs[index++] = questRecordUI;
            }
        }
        
        /// <summary>
        /// 创建任务记录容器作为LeftContent的下半部分子容器
        /// </summary>
        private static void AddQuestRecordContainer(Transform transform)
        {
            var containerGO = new GameObject("QuestRecordContainer");
            questRecordContainerRect = containerGO.AddComponent<RectTransform>();
            
            // 设置父级为LeftContent容器
            if (leftContentRect != null)
            {
                questRecordContainerRect.SetParent(leftContentRect, false);
            }
            else
            {
                questRecordContainerRect.SetParent(transform);
            }
            
            questRecordContainerRect.anchorMin = new Vector2(0f, 0f);  // 左下锚点：左侧(0)，底部(0)
            questRecordContainerRect.anchorMax = new Vector2(1f, 1f);  // 右上锚点：右侧(1)，顶部(1)
            questRecordContainerRect.pivot = new Vector2(0.5f, 1f);    // 轴心点：顶部中心
            questRecordContainerRect.anchoredPosition = Vector2.zero;
            questRecordContainerRect.sizeDelta = new Vector2(0f, 0f); // 宽度和高度自适应
            
            // 添加垂直布局组
            var verticalLayout = containerGO.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            
            // 添加内容尺寸适配器
            var contentFitter = containerGO.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 添加滚动视图
            var scrollRect = containerGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = questRecordContainerRect;
            scrollRect.content = questRecordContainerRect;
            
            // 添加遮罩
            var mask = containerGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // 添加背景
            var background = containerGO.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.5f); // 半透明黑色
            
            // 添加标题
            var titleGO = new GameObject("Title");
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.SetParent(questRecordContainerRect, false);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -5f);
            
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "任务记录";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 18;
            titleText.color = Color.white;
        }
        #endregion

        public static void Prefix(ClosureView __instance)
        {
            AddExtraCharacterRenderer(__instance.transform);
            
            AddDurationTextComponent(__instance);
            
            AddTotalRewardContainer(__instance);
            
            // 先创建左侧列容器
            AddLeftContentContainer(__instance.transform);
            
            // 然后创建击杀记录容器（作为LeftContent的上半部分）
            AddKillRecordContainer(__instance.transform);
            
            // 最后创建任务记录容器（作为LeftContent的下半部分）
            AddQuestRecordContainer(__instance.transform);
        }

        
    }
}