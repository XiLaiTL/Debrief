using System;
using System.Collections.Generic;
using Duckov.MiniMaps;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debrief
{
    /// <summary>
    ///     快速统计视图，按L键唤出
    /// </summary>
    public class QuickStatsView : MonoBehaviour
    {
        private const int SubTitleFontSize = 32;

        private Canvas canvas;
        private RectTransform backgroundRect;
        private RectTransform contentRect;
        private TextMeshProUGUI titleText;

        /// <summary>
        ///     获取或创建QuickStatsView实例
        /// </summary>
        public static QuickStatsView Instance { get; private set; }

        /// <summary>
        ///     切换视图显示状态
        /// </summary>
        public void ToggleView()
        {
            if (Instance)
            {
                if (Instance.canvas)
                {
                    Instance.canvas.enabled = !Instance.canvas.enabled;
                }

                if (Instance.canvas.enabled)
                {
                    CreateComponents();
                    SetupComponents();
                }
            }
            else
            {
                Debug.LogWarning("QuickStatsView Instance Not Found");
            }
        }

        /// <summary>
        ///     创建视图
        /// </summary>
        public static void CreateView()
        {
            if (Instance == null)
            {
                var viewGO = new GameObject("QuickStatsView");
                Instance = viewGO.AddComponent<QuickStatsView>();
                Instance.Initialize();
                viewGO.transform.SetParent(LevelManager.Instance.transform, false);
                Debug.Log("QuickStatsView Created");
            }
        }


        private void CreateComponents()
        {
            AddExtraCharacterRenderer(backgroundRect);

            AddDurationTextComponent(contentRect);

            AddTotalRewardContainer(contentRect, 500);

            // 先创建左侧列容器
            AddLeftContentContainer(backgroundRect);

            // 然后创建击杀记录容器（作为LeftContent的上半部分）
            AddKillRecordContainer(backgroundRect);

            // 最后创建任务记录容器（作为LeftContent的下半部分）
            AddQuestRecordContainer(backgroundRect);
        }

        private void SetupComponents()
        {
            var totalValue = (int)(ModBehaviour.PlayerTotalValue());
            SetTotalRewardText(totalValue);

            var duration = GameClock.Now;
            SetDurationText(duration);

            var mostValueItems = ModBehaviour.MostValueItems();
            SetItemStack(mostValueItems);
            var defaultIcon = MapMarkerManager.Icons.Find((Sprite e) => e != null && e.name == "swords");
            var killRecordTest = new Dictionary<string, KillRecord>()
            {
                { "拾荒者", new KillRecord(defaultIcon, null, "拾荒者", 10, GameClock.Now) },
                {
                    "雇佣兵",
                    new KillRecord(defaultIcon, GameplayDataSettings.UIStyle.PmcCharacterIcon, "雇佣兵", 2, GameClock.Now)
                },
                {
                    "雇佣兵1",
                    new KillRecord(defaultIcon, GameplayDataSettings.UIStyle.PmcCharacterIcon, "雇佣兵1", 2, GameClock.Now)
                },
                {
                    "雇佣兵2",
                    new KillRecord(defaultIcon, GameplayDataSettings.UIStyle.PmcCharacterIcon, "雇佣兵2", 2, GameClock.Now)
                },
                {
                    "雇佣兵3",
                    new KillRecord(defaultIcon, GameplayDataSettings.UIStyle.PmcCharacterIcon, "雇佣兵3", 2, GameClock.Now)
                },
                {
                    "矮鸭",
                    new KillRecord(defaultIcon, GameplayDataSettings.UIStyle.BossCharacterIcon, "矮鸭", 1, GameClock.Now)
                }
            };

            UpdateKillRecords(killRecordTest);

            var questRecordTest = new List<FinishedQuest>()
            {
                new FinishedQuest("任务1", new List<string>()
                {
                    "完成任务1", "完成任务2"
                }),
                new FinishedQuest("任务2", new List<string>()
                {
                    "完成任务1", "完成任务2"
                }),
            };

            UpdateQuestRecords(questRecordTest);
        }


        /// <summary>
        ///     初始化视图
        /// </summary>
        private void Initialize()
        {
            // 创建Canvas
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 高优先级，确保显示在最前面

            // 添加Graphic Raycaster以支持UI交互
            gameObject.AddComponent<GraphicRaycaster>();

            // 创建背景
            var backgroundGO = new GameObject("Background");
            backgroundRect = backgroundGO.AddComponent<RectTransform>();
            backgroundRect.SetParent(transform, false);

            // 设置背景位置和大小（屏幕中央）
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            // 设置背景颜色
            var backgroundImage = backgroundGO.AddComponent<Image>();
            ColorUtility.TryParseHtmlString("#0D0D0DFF", out var backgroundColor);
            backgroundImage.color = backgroundColor;

            // 创建中间内容容器
            var contentGO = new GameObject("Content");
            contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.SetParent(backgroundRect, false);

            // 设置内容容器位置和大小：正中间，高度100%，宽度三分之一
            contentRect.anchorMin = new Vector2(0.1f, 0f); // 左侧锚点：三分之一宽度
            contentRect.anchorMax = new Vector2(0.9f, 1f); // 右侧锚点：三分之二宽度
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // 添加垂直布局组
            var verticalLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);

            // 添加内容尺寸适配器
            var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 创建标题
            var titleGO = new GameObject("Title");
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.SetParent(contentRect, false); // 将标题放在内容容器内

            // 标题位置：在内容容器内顶部
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, 0f);
            titleRect.sizeDelta = new Vector2(0f, 60f);

            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "快速统计";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 42;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;

            // 为标题添加布局元素
            var titleLayoutElement = titleGO.AddComponent<LayoutElement>();
            titleLayoutElement.preferredHeight = 60;
            titleLayoutElement.flexibleHeight = 0;

            // 初始隐藏视图
            canvas.enabled = false;
        }

        private void Update()
        {
            // 如果视图可见，每帧更新统计信息
            if (canvas)
                if (canvas.enabled)
                {
                }
        }

        /// <summary>
        ///     显示视图
        /// </summary>
        public void Show()
        {
            if (canvas != null) canvas.enabled = true;
        }

        /// <summary>
        ///     隐藏视图
        /// </summary>
        public void Hide()
        {
            if (canvas != null) canvas.enabled = false;
        }


        public static void AddExtraCharacterRenderer(Transform transform)
        {
            // 开启相机渲染
            ModBehaviour.ExtraCamera.Start();

            var containerGO = new GameObject("ExtraCharacterRenderer");
            var container = containerGO.AddComponent<RectTransform>();
            // 设置父级和锚点
            container.SetParent(transform);
            container.anchorMin = new Vector2(1f, 0.5f); // 左下锚点：右侧(1)，垂直中心(0.5)
            container.anchorMax = new Vector2(1f, 0.5f); // 右上锚点：右侧(1)，垂直中心(0.5)

            // 设置轴心点为右侧中心，这样旋转和缩放会以右侧为中心
            container.pivot = new Vector2(1f, 0.5f);
            container.anchoredPosition = new Vector2(-100f,0);

            container.sizeDelta = new Vector2(512, 512);

            var mapImage = containerGO.AddComponent<RawImage>();
            mapImage.texture = ModBehaviour.ExtraCamera.CharacterTexture;
        }

        public static TextMeshProUGUI durationText;

        public static void SetDurationText(TimeSpan duration)
        {
            if (durationText == null) return;
            durationText.text =
                TranslatableText.T(LanguageKey.Duration, duration.Days, duration.Hours, duration.Minutes);
        }


        public static void AddDurationTextComponent(Transform transform)
        {
            durationText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, transform);
            durationText.gameObject.name = "DurationText";
            durationText.enableAutoSizing = true;
            durationText.fontSizeMin = 25;
            durationText.fontSizeMax = 40;
            durationText.alignment = TextAlignmentOptions.Center;
        }

        private static TextMeshProUGUI totalRewardText;
        private static TextMeshProUGUI totalRewardValueText; // 新增：用于显示金额的文本组件

        public static void SetTotalRewardText(float totalReward)
        {
            if (totalRewardText == null || totalRewardValueText == null) return;

            // 左边显示本地化文本（不带金额）
            totalRewardText.text = TranslatableText.T(LanguageKey.TotalReward, "");

            // 右边显示金额，并根据ItemValueUtils.CalculateItemValueLevel(totalReward/3)改变颜色
            totalRewardValueText.text = totalReward.ToString("n0");

            // 计算价值等级并设置颜色
            var valueLevel = ItemValueUtils.CalculateItemValueLevel((int)(totalReward / 3));
            if (valueLevel != ItemValueLevel.White)
            {
                var color = ItemValueUtils.GetItemValueLevelColor(valueLevel);
                totalRewardValueText.color = color;
            }
        }

        private static RectTransform itemStackContainerRect;
        private static ItemStackUI[] itemStackUIs;

        /// <summary>
        ///     更新物品堆栈显示
        /// </summary>
        public static void SetItemStack(List<ItemStack> itemStacks)
        {
            if (itemStackContainerRect == null) return;

            // 清空现有的物品堆栈UI
            if (itemStackUIs != null)
                foreach (var ui in itemStackUIs)
                    if (ui != null)
                        Destroy(ui.gameObject);

            var itemCount = itemStacks.Count;
            if (itemCount == 0)
            {
                // 隐藏物品堆栈容器
                itemStackContainerRect.gameObject.SetActive(false);
                return;
            }

            // 计算需要的行数（每行5个物品）
            var rows = (itemCount + 4) / 5; // 向上取整
            itemStackUIs = new ItemStackUI[itemCount];

            // 创建行容器
            for (var row = 0; row < rows; row++)
            {
                // 创建水平行容器
                var rowGO = new GameObject($"ItemRow{row}");
                var rowRect = rowGO.AddComponent<RectTransform>();
                rowRect.SetParent(itemStackContainerRect, false);
                rowRect.sizeDelta = new Vector2(500, 125);
                
                // 设置布局元素
                var rowLayoutElement = rowGO.AddComponent<LayoutElement>();
                rowLayoutElement.preferredHeight = 125;
                rowLayoutElement.flexibleHeight = 1;

                // 设置行布局
                var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
                rowLayout.childAlignment = TextAnchor.MiddleCenter;
                rowLayout.childControlWidth = false;
                rowLayout.childControlHeight = false;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;
                rowLayout.spacing = 10;
                
                // 当前行的物品数量（最多5个）
                var itemsInRow = Math.Min(5, itemCount - row * 5);

                Debug.Log($"Row {row}: {itemsInRow} items");
                for (var col = 0; col < itemsInRow; col++)
                {
                    var itemIndex = row * 5 + col;
                    if (itemIndex >= itemCount) break;

                    Debug.Log($"Col {row}: {itemIndex} items: {itemStacks[itemIndex].DisplayName}");
                    
                    var itemStackGO = new GameObject($"ItemStack{itemIndex}");
                    var itemStackRect = itemStackGO.AddComponent<RectTransform>();
                    itemStackRect.SetParent(rowRect, false);
                    itemStackRect.sizeDelta = new Vector2(90, 125);

                    // 设置布局元素
                    var layoutElement = itemStackGO.AddComponent<LayoutElement>();
                    layoutElement.preferredWidth = 90;
                    layoutElement.preferredHeight = 125;
                    layoutElement.flexibleWidth = 0;
                    layoutElement.flexibleHeight = 1;

                    var itemStackUI = itemStackGO.AddComponent<ItemStackUI>();
                    itemStackUI.SetItemStack(itemStacks[itemIndex]);
                    itemStackUIs[itemIndex] = itemStackUI;
                }
            }
        }

        public static RectTransform totalRewardContainerRect;


        public static void AddTotalRewardContainer(Transform transform, float width)
        {
            
            var totalRewardContainer = new GameObject("TotalRewardContainer");
            totalRewardContainerRect = totalRewardContainer.AddComponent<RectTransform>();
            totalRewardContainerRect.SetParent(transform);
            totalRewardContainerRect.anchorMin = new Vector2(0.5f, 1f);
            totalRewardContainerRect.anchorMax = new Vector2(0.5f, 1f);
            totalRewardContainerRect.pivot = new Vector2(0.5f, 1f);
            totalRewardContainerRect.anchoredPosition = Vector2.zero;
            totalRewardContainerRect.sizeDelta = new Vector2(width, 100);
            
            var containerLayoutElement = totalRewardContainer.AddComponent<LayoutElement>();
            containerLayoutElement.preferredHeight = 100;
            containerLayoutElement.flexibleWidth = 0;
            containerLayoutElement.preferredWidth = width;

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

            // 创建水平布局容器用于收益文本
            var rewardTextContainer = new GameObject("RewardTextContainer");
            var rewardTextContainerRect = rewardTextContainer.AddComponent<RectTransform>();
            rewardTextContainerRect.SetParent(totalRewardContainerRect);

            // 为水平容器添加布局元素
            var rewardTextLayoutElement = rewardTextContainer.AddComponent<LayoutElement>();
            rewardTextLayoutElement.preferredHeight = 50;
            rewardTextLayoutElement.flexibleHeight = 0;

            // 为水平容器添加水平布局组
            var horizontalLayout = rewardTextContainer.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;

            // 创建左侧文本（本地化文本）
            var leftTextGO = new GameObject("TotalRewardLeftText");
            var leftTextRect = leftTextGO.AddComponent<RectTransform>();
            leftTextRect.SetParent(rewardTextContainerRect);

            totalRewardText = leftTextGO.AddComponent<TextMeshProUGUI>();
            totalRewardText.gameObject.name = "TotalRewardText";
            totalRewardText.enableAutoSizing = true;
            totalRewardText.fontSizeMin = 30;
            totalRewardText.fontSizeMax = 40;
            totalRewardText.alignment = TextAlignmentOptions.Right;
            totalRewardText.color = Color.white;

            // 为左侧文本添加布局元素
            var leftTextLayout = leftTextGO.AddComponent<LayoutElement>();
            leftTextLayout.preferredWidth = 200;
            leftTextLayout.flexibleWidth = 0;

            // 创建右侧文本（金额显示）
            var rightTextGO = new GameObject("TotalRewardValueText");
            var rightTextRect = rightTextGO.AddComponent<RectTransform>();
            rightTextRect.SetParent(rewardTextContainerRect);

            totalRewardValueText = rightTextGO.AddComponent<TextMeshProUGUI>();
            totalRewardValueText.gameObject.name = "TotalRewardValueText";
            totalRewardValueText.enableAutoSizing = true;
            totalRewardValueText.fontSizeMin = 30;
            totalRewardValueText.fontSizeMax = 40;
            totalRewardValueText.alignment = TextAlignmentOptions.Left;
            totalRewardValueText.color = Color.white;

            // 为右侧文本添加布局元素
            var rightTextLayout = rightTextGO.AddComponent<LayoutElement>();
            rightTextLayout.preferredWidth = 150;
            rightTextLayout.flexibleWidth = 0;

            // 原有的itemStackContainerRect创建代码保持不变
            var itemStackContainerGO = new GameObject("DebriefItemContainer");
            itemStackContainerRect = itemStackContainerGO.AddComponent<RectTransform>();
            itemStackContainerRect.SetParent(totalRewardContainerRect);

            // 设置容器布局
            itemStackContainerRect.anchorMin = new Vector2(0.5f, 1f);
            itemStackContainerRect.anchorMax = new Vector2(0.5f, 1f);
            itemStackContainerRect.pivot = new Vector2(0.5f, 1f);
            itemStackContainerRect.localScale = Vector3.one;

            // 调整容器大小以适应竖直布局
            itemStackContainerRect.sizeDelta = new Vector2(width, 250); // 增加高度以容纳多行
            itemStackContainerRect.anchoredPosition = new Vector2(0, 0);

            var backgroundImage = totalRewardContainer.AddComponent<Image>();
            backgroundImage.sprite = SpriteUtils.BgWhite;
            backgroundImage.preserveAspect = false;
            ColorUtility.TryParseHtmlString("#373737FF", out var color);
            backgroundImage.color = color;
            
            // 为itemStackContainerRect添加布局元素
            var itemContainerLayoutElement = itemStackContainerGO.AddComponent<LayoutElement>();
            itemContainerLayoutElement.preferredHeight = 250;
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

            // 将滚动视图的视口和内容设置正确
            scrollRect.viewport = itemStackContainerRect;
            scrollRect.content = itemStackContainerRect;

            // 为内容容器添加垂直布局组
            var itemVerticalLayout = itemStackContainerGO.AddComponent<VerticalLayoutGroup>();
            itemVerticalLayout.childAlignment = TextAnchor.UpperCenter; // 顶部居中对齐
            itemVerticalLayout.childControlWidth = false;
            itemVerticalLayout.childControlHeight = false;
            itemVerticalLayout.childForceExpandWidth = false;
            itemVerticalLayout.childForceExpandHeight = false;
            itemVerticalLayout.spacing = 0;

            // 添加内容尺寸适配
            var itemContentFitter = itemStackContainerGO.AddComponent<ContentSizeFitter>();
            itemContentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        #region 左侧列容器

        private static RectTransform leftContentRect;

        /// <summary>
        ///     创建左侧列容器
        /// </summary>
        public static void AddLeftContentContainer(Transform transform)
        {
            var containerGO = new GameObject("LeftContent");
            leftContentRect = containerGO.AddComponent<RectTransform>();

            // 设置父级和锚点，使其固定在左侧并占满高度
            leftContentRect.SetParent(transform);
            leftContentRect.anchorMin = new Vector2(0f, 0f); // 左下锚点：左侧(0)，底部(0)
            leftContentRect.anchorMax = new Vector2(0f, 1f); // 右上锚点：左侧(0)，顶部(1)
            leftContentRect.pivot = new Vector2(0f, 1f); // 轴心点：左上角
            leftContentRect.anchoredPosition = new Vector2(50f, -100f); // 距离左上角20像素
            leftContentRect.sizeDelta = new Vector2(480f, 0f); // 宽度480，高度自适应

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
        ///     更新击杀记录显示
        /// </summary>
        public static void UpdateKillRecords(Dictionary<string, KillRecord> records)
        {
            if (killRecordContainerRect == null) return;

            // 清空现有的击杀记录UI
            if (killRecordUIs != null)
                foreach (var ui in killRecordUIs)
                    if (ui != null)
                        Destroy(ui.gameObject);

            // 创建新的击杀记录UI数组
            var recordCount = records.Values.Count;
            killRecordUIs = new KillRecordUI[recordCount];
            if (KillRecordTitleText)
            {
                KillRecordTitleText.gameObject.SetActive(recordCount > 0);
            }

            var index = 0;
            foreach (var killRecord in records.Values)
            {
                var killRecordGO = new GameObject("KillRecord");
                var killRecordRect = killRecordGO.AddComponent<RectTransform>();
                killRecordRect.SetParent(killRecordContainerRect, false);
                killRecordRect.sizeDelta = new Vector2(480, 40); // 宽度略小于容器
                var killRecordBackground = killRecordGO.AddComponent<Image>();
                // 检查是否为Boss角色图标，如果是则设置背景为淡红色
                if (killRecord.VictimSprite == GameplayDataSettings.UIStyle.BossCharacterIcon)
                {
                    killRecordBackground.color = ItemValueUtils.LightRed; // 淡红色
                    killRecordBackground.sprite = SpriteUtils.BarWhite;
                    killRecordBackground.preserveAspect = false;
                }
                else
                {
                    killRecordBackground.color = Color.clear;
                }

                var layoutElement = killRecordGO.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 480;
                layoutElement.preferredHeight = 50;
                layoutElement.flexibleWidth = 0;
                layoutElement.flexibleHeight = 1;

                var killRecordUI = killRecordGO.AddComponent<KillRecordUI>();
                killRecordUI.SetKillRecord(killRecord);
                killRecordUIs[index++] = killRecordUI;
            }
        }

        /// <summary>
        ///     创建击杀记录容器作为LeftContent的上半部分子容器
        /// </summary>
        public static TextMeshProUGUI KillRecordTitleText;
        public static void AddKillRecordContainer(Transform transform)
        {
            var containerGO = new GameObject("KillRecordContainer");
            killRecordContainerRect = containerGO.AddComponent<RectTransform>();

            // 设置父级为LeftContent容器
            if (leftContentRect != null)
                killRecordContainerRect.SetParent(leftContentRect, false);
            else
                killRecordContainerRect.SetParent(transform);

            killRecordContainerRect.anchorMin = new Vector2(0f, 1f); // 左下锚点：左侧(0)，顶部(1)
            killRecordContainerRect.anchorMax = new Vector2(1f, 1f); // 右上锚点：右侧(1)，顶部(1)
            killRecordContainerRect.pivot = new Vector2(0.5f, 1f); // 轴心点：顶部中心
            killRecordContainerRect.anchoredPosition = Vector2.zero;
            killRecordContainerRect.sizeDelta = new Vector2(0f, Screen.height/2f); // 宽度自适应，固定高度300

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

            KillRecordTitleText = titleGO.AddComponent<TextMeshProUGUI>();
            KillRecordTitleText.text = "击杀记录";
            KillRecordTitleText.alignment = TextAlignmentOptions.Center;
            KillRecordTitleText.fontSize = SubTitleFontSize;
            KillRecordTitleText.color = Color.white;
        }

        #endregion

        #region 任务记录容器

        private static RectTransform questRecordContainerRect;
        private static FinishedQuestUI[] questRecordUIs;

        /// <summary>
        ///     更新任务记录显示
        /// </summary>
        public static void UpdateQuestRecords(List<FinishedQuest> records)
        {
            if (questRecordContainerRect == null) return;

            // 清空现有的任务记录UI
            if (questRecordUIs != null)
                foreach (var ui in questRecordUIs)
                    if (ui != null)
                        Destroy(ui.gameObject);

            // 创建新的任务记录UI数组
            var recordCount = records.Count;
            questRecordUIs = new FinishedQuestUI[recordCount];
            if (QuestRecordTitleText)
            {
                QuestRecordTitleText.gameObject.SetActive(recordCount > 0);
            }
            var index = 0;
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
        ///     创建任务记录容器作为LeftContent的下半部分子容器
        /// </summary>
        public static TextMeshProUGUI QuestRecordTitleText;
        public static void AddQuestRecordContainer(Transform transform)
        {
            var containerGO = new GameObject("QuestRecordContainer");
            questRecordContainerRect = containerGO.AddComponent<RectTransform>();

            // 设置父级为LeftContent容器
            if (leftContentRect != null)
                questRecordContainerRect.SetParent(leftContentRect, false);
            else
                questRecordContainerRect.SetParent(transform);

            questRecordContainerRect.anchorMin = new Vector2(0f, 0f); // 左下锚点：左侧(0)，底部(0)
            questRecordContainerRect.anchorMax = new Vector2(1f, 1f); // 右上锚点：右侧(1)，顶部(1)
            questRecordContainerRect.pivot = new Vector2(0.5f, 1f); // 轴心点：顶部中心
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

            QuestRecordTitleText = titleGO.AddComponent<TextMeshProUGUI>();
            QuestRecordTitleText.text = "任务记录";
            QuestRecordTitleText.alignment = TextAlignmentOptions.Center;
            QuestRecordTitleText.fontSize = SubTitleFontSize;
            QuestRecordTitleText.color = Color.white;
        }

        #endregion
    }
}