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
        private TextMeshProUGUI[] descriptionTexts;
        private RectTransform container;

        // 配置常量
        private const float CONTAINER_WIDTH = 400f;
        private const float CONTAINER_HEIGHT = 200f;
        private const int TITLE_FONT_SIZE = 16;
        private const int DESCRIPTION_FONT_SIZE = 12;
        private const float INDENT_AMOUNT = 20f;
        private const float LINE_SPACING = 5f;

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
            container = gameObject.AddComponent<RectTransform>();
            container.sizeDelta = new Vector2(CONTAINER_WIDTH, CONTAINER_HEIGHT);
            container.anchorMin = new Vector2(0.5f, 0.5f);
            container.anchorMax = new Vector2(0.5f, 0.5f);
            container.pivot = new Vector2(0.5f, 0.5f);
        }

        private void CreateTitleText()
        {
            titleText = Utils.CreateTextComponent(transform, "QuestTitle", 
                new Vector2(0f, CONTAINER_HEIGHT / 2f - 20f), 
                TITLE_FONT_SIZE, Color.white);
            
            var rt = titleText.rectTransform;
            rt.sizeDelta = new Vector2(CONTAINER_WIDTH - 20f, 30f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
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
            if (descriptionTexts != null)
            {
                foreach (var text in descriptionTexts)
                {
                    if (text != null) Destroy(text.gameObject);
                }
            }

            // 创建描述文本
            if (finishedQuest.Tasks != null && finishedQuest.Tasks.Count > 0)
            {
                descriptionTexts = new TextMeshProUGUI[finishedQuest.Tasks.Count];
                
                for (int i = 0; i < finishedQuest.Tasks.Count; i++)
                {
                    // 计算位置：从标题下方开始，每行向下偏移
                    float yPosition = CONTAINER_HEIGHT / 2f - 40f - (i * (DESCRIPTION_FONT_SIZE + LINE_SPACING));
                    
                    descriptionTexts[i] = Utils.CreateTextComponent(transform, $"Description{i}", 
                        new Vector2(INDENT_AMOUNT, yPosition), 
                        DESCRIPTION_FONT_SIZE, Color.gray);
                    
                    var rt = descriptionTexts[i].rectTransform;
                    rt.sizeDelta = new Vector2(CONTAINER_WIDTH - INDENT_AMOUNT - 10f, DESCRIPTION_FONT_SIZE + 5f);
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0f, 1f);

                    // 设置文本内容
                    descriptionTexts[i].text = finishedQuest.Tasks[i];
                }
            }
        }

        /// <summary>
        /// 设置容器大小
        /// </summary>
        public void SetContainerSize(float width, float height)
        {
            if (container != null)
            {
                container.sizeDelta = new Vector2(width, height);
                UpdateTextPositions();
            }
        }

        /// <summary>
        /// 更新文本位置
        /// </summary>
        private void UpdateTextPositions()
        {
            if (titleText != null)
            {
                var titleRt = titleText.rectTransform;
                titleRt.anchoredPosition = new Vector2(0f, container.sizeDelta.y / 2f - 20f);
            }

            if (descriptionTexts != null)
            {
                for (int i = 0; i < descriptionTexts.Length; i++)
                {
                    if (descriptionTexts[i] != null)
                    {
                        float yPosition = container.sizeDelta.y / 2f - 40f - (i * (DESCRIPTION_FONT_SIZE + LINE_SPACING));
                        descriptionTexts[i].rectTransform.anchoredPosition = new Vector2(INDENT_AMOUNT, yPosition);
                    }
                }
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void OnDestroy()
        {
            if (descriptionTexts != null)
            {
                foreach (var text in descriptionTexts)
                {
                    if (text != null) Destroy(text.gameObject);
                }
                descriptionTexts = null;
            }
        }
    }
}