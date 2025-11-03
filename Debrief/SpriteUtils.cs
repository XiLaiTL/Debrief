using System.IO;
using UnityEngine;

namespace Debrief
{
    public static class SpriteUtils
    {
        public static Sprite? BgWhite;
        public static Sprite? BgVoid;
        public static Sprite? BarWhite;

        public static void Setup()
        {
            BgWhite = LoadSprite("BgWhite");
            BgVoid = LoadSprite("BgVoid");
            BarWhite = LoadSprite("BarWhite");
        }
        
        public static Sprite? LoadSprite(string fileName)
        {
            var dllDir = Path.Combine(Utils.GetDllDirectory(), "Icons");
            var exeDir = Path.Combine(Application.streamingAssetsPath, ModBehaviour.ModName, "Icons");
            Directory.CreateDirectory(exeDir);
    
            var dllPath = Path.Combine(dllDir, fileName + ".png");
            var exePath = Path.Combine(exeDir, fileName + ".png");
    
            byte[] iconBytes;
            Texture2D iconTexture;
    
            // 优先尝试加载exe路径的文件
            if (File.Exists(exePath))
            {
                iconBytes = File.ReadAllBytes(exePath);
                iconTexture = new Texture2D(2560, 2560);
                if (iconTexture.LoadImage(iconBytes))
                {
                    Debug.Log($"{ModBehaviour.ModName}: 纹理加载成功 = {exePath}");
                    return Sprite.Create(iconTexture, new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), 
                        new Vector2(iconTexture.width / 2.0f, iconTexture.height / 2.0f));
                }
        
                Debug.LogError($"{ModBehaviour.ModName}: 纹理加载失败，尝试回退 = {exePath}");
            }
    
            // 回退到dll路径的文件
            if (File.Exists(dllPath))
            {
                iconBytes = File.ReadAllBytes(dllPath);
                iconTexture = new Texture2D(2560, 2560);
                if (iconTexture.LoadImage(iconBytes))
                {
                    Debug.Log($"{ModBehaviour.ModName}: 纹理加载成功 = {dllPath}");
                    return Sprite.Create(iconTexture, new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), 
                        new Vector2(iconTexture.width / 2.0f, iconTexture.height / 2.0f));
                }
            }
    
            Debug.LogError($"{ModBehaviour.ModName}: 文件不存在或加载失败 = {dllPath}");
            return null;
        }
    }
}