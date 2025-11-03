using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Duckov.Utilities;

namespace Debrief
{
    public class KillRecordUI : MonoBehaviour
    {
        private Image weaponIconImage;
        private Image victimIconImage;
        private TextMeshProUGUI victimNameText;
        private TextMeshProUGUI killCountText;
        private HorizontalLayoutGroup layoutGroup;

        // 常量
        private const int FontSize = 24;


        private void Awake()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// 初始化UI结构
        /// </summary>
        private void InitializeUI()
        {
            // 设置基础属性
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            
            // 创建水平布局组
            CreateLayoutGroup();
            
            // 创建各个UI元素
            CreatePlaceHolder();
            CreateWeaponIcon();
            CreateVictimIcon();
            CreateVictimNameText();
            CreateKillCountText();
        }



        private void CreateLayoutGroup()
        {
            layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 8;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            
            // 添加内容尺寸适配器
            ContentSizeFitter sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreatePlaceHolder()
        {
            GameObject placeHolderObj = new GameObject("PlaceHolder");
            placeHolderObj.transform.SetParent(transform, false);
            Image placeHolderImage = placeHolderObj.AddComponent<Image>();
            placeHolderImage.color = Color.clear;
            placeHolderImage.preserveAspect = false;
            
            // 添加 LayoutElement
            var layoutElement = placeHolderObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 40;
            layoutElement.preferredHeight = 40;
            layoutElement.minWidth = 40;
            layoutElement.minHeight = 40;
        }

        private void CreateWeaponIcon()
        {
            GameObject weaponIconObj = new GameObject("WeaponIcon");
            weaponIconObj.transform.SetParent(transform, false);
            weaponIconImage = weaponIconObj.AddComponent<Image>();
    
            // 添加 LayoutElement
            var layoutElement = weaponIconObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 40;
            layoutElement.preferredHeight = 40;
            layoutElement.minWidth = 40;
            layoutElement.minHeight = 40;
    
            weaponIconImage.preserveAspect = true; // 建议改为 true 保持比例
        }

        private void CreateVictimIcon()
        {
            GameObject victimIconObj = new GameObject("VictimIcon");
            victimIconObj.transform.SetParent(transform, false);
            victimIconImage = victimIconObj.AddComponent<Image>();
    
            // 添加 LayoutElement
            var layoutElement = victimIconObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 28;
            layoutElement.preferredHeight = 28;
            layoutElement.minWidth = 28;
            layoutElement.minHeight = 28;
    
            victimIconImage.preserveAspect = true;
        }

        private void CreateVictimNameText()
        {
            victimNameText = CreateTextComponent("VictimName", new Vector2(0, 0), FontSize, Color.white);
            victimNameText.rectTransform.sizeDelta = new Vector2(120, 24);
            victimNameText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        private void CreateKillCountText()
        {
            killCountText = CreateTextComponent("KillCount", new Vector2(0, 0), FontSize, Color.yellow);
            killCountText.rectTransform.sizeDelta = new Vector2(40, 24);
            killCountText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        public void SetKillRecord(KillRecord killRecord)
        {
            gameObject.SetActive(true);
            
            // 设置武器图标
            if (killRecord.WeaponSprite != null)
            {
                weaponIconImage.sprite = killRecord.WeaponSprite;
                weaponIconImage.gameObject.SetActive(true);
            }
            else
            {
                weaponIconImage.gameObject.SetActive(false);
            }

            // 设置受害者图标
            if (killRecord.VictimSprite != null)
            {
                victimIconImage.sprite = killRecord.VictimSprite;
                victimIconImage.gameObject.SetActive(true);
            }
            else
            {
                victimIconImage.gameObject.SetActive(false);
            }
            // 设置受害者名字
            victimNameText.text = GetDisplayName(killRecord.Victim);

            // 设置击杀数（数量为1时不显示）
            if (killRecord.KillCount > 1)
            {
                killCountText.text = "X " + killRecord.KillCount;
                // 根据击杀数设置文本颜色
                killCountText.color = ItemValueUtils.GetKillCountColor(killRecord.KillCount);
                killCountText.gameObject.SetActive(true);
            }
            else
            {
                killCountText.gameObject.SetActive(false);
            }
        }

        private TextMeshProUGUI CreateTextComponent(string name, Vector2 position, int fontSize, Color color)
        {
            return Utils.CreateTextComponent(transform, name, position, fontSize, color);
        }
        
        private string GetDisplayName(string originalName)
        {
            // 限制名字长度为15个字符，超出部分用省略号代替
            if (string.IsNullOrEmpty(originalName))
            {
                return "Unknown";
            }
            
            if (originalName.Length > 15)
            {
                return originalName.Substring(0, 12) + "...";
            }
            
            return originalName;
        }
    }
}