using System.IO;
using System.Reflection;
using Duckov.Utilities;
using TMPro;
using UnityEngine;

namespace Debrief
{
    public static class Utils
    {
        public static void PrintSceneHierarchy(GameObject current)
        {
            PrintTransformHierarchy(current.transform, "");
        }

        private static void PrintTransformHierarchy(Transform current, string prefix)
        {
            // 输出当前Transform
            Debug.Log(prefix + current.name + " (Active: " + current.gameObject.activeInHierarchy + ")");
        
            // 更新前缀用于子对象
            var newPrefix = prefix + "  ";
        
            // 递归处理所有子对象
            for (int i = 0; i < current.childCount; i++)
            {
                PrintTransformHierarchy(current.GetChild(i), newPrefix);
            }
        }

        /// <summary>
        /// 创建文本组件的辅助方法
        /// </summary>
        public static TextMeshProUGUI CreateTextComponent(Transform parent, string name, Vector2 position, int fontSize, Color color)
        {
            var textComponent = Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, parent);
            textComponent.gameObject.name = name;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            
            var rt = textComponent.rectTransform;
            rt.localPosition = position;
            
            return textComponent;
        }
        
        // 获取当前dll的所在目录 by (F_G_O)
        public static string GetDllDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        
        public static T GetOrAddComponent<T>(this GameObject uo) where T : Component
        {
            var component = uo.GetComponent<T>();
            if (component == null)
            {
                component = uo.AddComponent<T>();
            }
            return component;
        }

        
        public static GameObject GetOrAddGameObject(this Transform transform, string name)
        {
            var node = transform.Find(name);
            if (node == null)
            {
                node = new GameObject(name).transform;
                node.SetParent(transform, false);
            }
            return node.gameObject;
        }
        
        public static T GetOrInstantiate<T>(this Transform transform, string name, T prefab) where T : Component
        {
            var node = transform.Find(name);
            if (node == null)
            {
                var obj = Object.Instantiate(prefab, transform);
                obj.gameObject.name = name;
                return obj;
            }
            return node.GetComponent<T>();
        }

    }
}