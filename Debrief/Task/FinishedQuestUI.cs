using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Debrief
{
    public class FinishedQuestUI : MonoBehaviour
    {
        // UI组件引用
        private TextMeshProUGUI titleText;
        private VerticalLayoutGroup layoutGroup;
        private ContentSizeFitter contentFitter;
        private RectTransform container;

        // 配置常量
        private const int TITLE_FONT_SIZE = 24;
        private const int DESCRIPTION_FONT_SIZE = 22;
        private const float SPACING = 5f;
        private const float PADDING = 10f;

        public void Awake()
        {
            InitializeUI();
        }

        /// <summary>
        /// 初始化UI结构
        /// </summary>
        private void InitializeUI()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            CreateContainer();
            CreateTitleText();
        }

        private void CreateContainer()
        {
            container = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            container.anchorMin = new Vector2(0f, 1f);
            container.anchorMax = new Vector2(1f, 1f);
            container.pivot = new Vector2(0.5f, 1f);
            container.anchoredPosition = Vector2.zero;

            // 添加垂直布局组
            layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = SPACING;
            layoutGroup.padding = new RectOffset((int)PADDING, (int)PADDING, (int)PADDING, (int)PADDING);
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;

            // 添加内容尺寸适配器
            contentFitter = gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreateTitleText()
        {
            titleText = Utils.CreateTextComponent(transform, "QuestTitle", 
                Vector2.zero, 
                TITLE_FONT_SIZE, Color.white);
            
            var rt = titleText.rectTransform;
            rt.sizeDelta = new Vector2(0f, 30f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            // 为标题添加布局元素
            var layoutElement = titleText.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;
            layoutElement.flexibleHeight = 0f;
        }

        /// <summary>
        /// 设置任务数据
        /// </summary>
        public void SetQuestData(FinishedQuest finishedQuest)
        {
            if (finishedQuest == null || string.IsNullOrEmpty(finishedQuest.Quest))
            {
                gameObject.SetActive(false);
                return;
            }

            try
            {
                gameObject.SetActive(true);
                ApplyQuestData(finishedQuest);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FinishedQuestUI] Failed to set quest data: {e.Message}");
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 应用任务数据到UI（使用FinishedQuest类型）
        /// </summary>
        private void ApplyQuestData(FinishedQuest finishedQuest)
        {
            // 设置标题
            titleText.text = finishedQuest.Quest;

            // 清理现有的描述文本
            var childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.gameObject != titleText.gameObject && child.name.StartsWith("Description"))
                {
                    Destroy(child.gameObject);
                }
            }

            // 创建描述文本
            if (finishedQuest.Tasks != null && finishedQuest.Tasks.Count > 0)
            {
                for (int i = 0; i < finishedQuest.Tasks.Count; i++)
                {
                    CreateDescriptionText(i, finishedQuest.Tasks[i]);
                }
            }

            // 强制布局重建
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }

        private void CreateDescriptionText(int index, string taskText)
        {
            var descriptionText = Utils.CreateTextComponent(transform, $"Description{index}", 
                Vector2.zero, 
                DESCRIPTION_FONT_SIZE, Color.gray);
            
            var rt = descriptionText.rectTransform;
            rt.sizeDelta = new Vector2(0f, DESCRIPTION_FONT_SIZE + 5f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            // 为描述文本添加布局元素
            var layoutElement = descriptionText.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DESCRIPTION_FONT_SIZE + 5f;
            layoutElement.flexibleHeight = 0f;

            // 设置文本内容
            descriptionText.text = $"• {taskText}";
            descriptionText.alignment = TextAlignmentOptions.Left;

            // 确保描述文本在标题之后
            descriptionText.transform.SetSiblingIndex(index + 1);
        }

        /// <summary>
        /// 设置容器大小（现在由布局系统自动处理）
        /// </summary>
        public void SetContainerSize(float width, float height)
        {
            // 现在由布局系统自动处理大小，此方法可以保留但不再需要手动更新位置
            if (container != null)
            {
                // 如果需要固定宽度，可以设置
                if (width > 0)
                {
                    container.sizeDelta = new Vector2(width, 0f);
                }
                // 高度由布局系统自动计算
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void OnDestroy()
        {
            // 清理所有描述文本
            var childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.gameObject != titleText.gameObject && child.name.StartsWith("Description"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}