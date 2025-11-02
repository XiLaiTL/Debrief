using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debrief
{
    public class ItemStackUI : MonoBehaviour
    {
        // UI组件引用
        private Image background;
        private Image iconImage;
        private TextMeshProUGUI stackCountText;
        private TextMeshProUGUI nameText;
        private TextMeshProUGUI totalValueText;

        // 配置常量
        private const float BACKGROUND_SIZE = 100f;
        private const float ICON_SCALE = 0.8f;
        private const float CORNER_RADIUS = 10f;

        // 缓存字体避免重复加载
        private static Font _arialFont;
        private static Font ArialFont => _arialFont ??= Resources.GetBuiltinResource<Font>("Arial.ttf");

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
            
            CreateBackground();
            CreateIcon();
            CreateStackCountText();
            CreateNameText();
            CreateTotalValueText();
        }

        private void CreateBackground()
        {
            var backgroundGO = new GameObject("Background");
            backgroundGO.transform.SetParent(transform, false);
            background = backgroundGO.AddComponent<Image>();
            
            // 设置RectTransform
            var rt = background.rectTransform;
            rt.sizeDelta = new Vector2(BACKGROUND_SIZE, BACKGROUND_SIZE);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = Vector3.zero;
            
            // 设置圆角材质（简化版）
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // 默认背景色
        }

        private void CreateIcon()
        {
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(transform, false);
            iconImage = iconGO.AddComponent<Image>();
            iconImage.preserveAspect = true;
            
            var rt = iconImage.rectTransform;
            rt.sizeDelta = new Vector2(BACKGROUND_SIZE * ICON_SCALE, BACKGROUND_SIZE * ICON_SCALE);
            rt.localPosition = Vector3.zero;
        }

        private void CreateStackCountText()
        {
            stackCountText = CreateTextComponent("StackCount", 
                new Vector2(-5f, 5f), 
                12, 
                Color.white);
            
            var rt = stackCountText.rectTransform;
            rt.sizeDelta = new Vector2(30f, 20f);
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
        }

        private void CreateNameText()
        {
            nameText = CreateTextComponent("Name",
                new Vector2(0f, -15f),
                10,
                Color.white);
            
            var rt = nameText.rectTransform;
            rt.sizeDelta = new Vector2(BACKGROUND_SIZE - 10f, 20f);
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 1f);
        }

        private void CreateTotalValueText()
        {
            totalValueText = CreateTextComponent("TotalValue",
                new Vector2(0f, 15f),
                12,
                Color.yellow);
            
            var rt = totalValueText.rectTransform;
            rt.sizeDelta = new Vector2(BACKGROUND_SIZE - 10f, 20f);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
        }

        /// <summary>
        /// 设置物品堆栈数据
        /// </summary>
        public void SetItemStack(ItemStack? itemStack)
        {
            if (itemStack == null)
            {
                gameObject.SetActive(false);
                return;
            }

            try
            {
                gameObject.SetActive(true);
                ApplyItemStackData(itemStack);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ItemStackUI] Failed to set item stack: {e.Message}");
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 应用物品堆栈数据到UI
        /// </summary>
        private void ApplyItemStackData(ItemStack itemStack)
        {
            // 设置背景颜色
            background.color = ItemValueUtils.GetItemValueLevelColor(itemStack.ItemValueLevel);
            

            // 设置图标
            if (itemStack.Icon != null)
            {
                iconImage.sprite = itemStack.Icon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.color = Color.clear;
            }

            // 设置堆叠数量（只有>1时显示）
            stackCountText.text = itemStack.StackCount > 1 ? itemStack.StackCount.ToString() : string.Empty;
            stackCountText.gameObject.SetActive(itemStack.StackCount > 1);

            // 设置名称
            nameText.text = GetDisplayName(itemStack);

            // 设置总价值
            totalValueText.text = itemStack.GetTotalValue().ToString("N0");
        }

        /// <summary>
        /// 获取显示名称（带安全处理）
        /// </summary>
        private string GetDisplayName(ItemStack itemStack)
        {
            if (string.IsNullOrEmpty(itemStack.DisplayName))
                return "Unknown Item";
            
            // 名称长度限制
            return itemStack.DisplayName.Length > 15 
                ? itemStack.DisplayName.Substring(0, 12) + "..." 
                : itemStack.DisplayName;
        }

        private TextMeshProUGUI CreateTextComponent(string name, Vector2 position, int fontSize, Color color)
        {
            return Utils.CreateTextComponent(transform, name, position, fontSize, color);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void OnDestroy()
        {
            // // 清理自定义创建的资源
            // if (iconImage != null && iconImage.sprite != null)
            // {
            //     // 如果是动态创建的sprite需要销毁
            //     if (!iconImage.sprite.name.Contains("UI Sprite"))
            //         DestroyImmediate(iconImage.sprite);
            // }
        }
    }
}