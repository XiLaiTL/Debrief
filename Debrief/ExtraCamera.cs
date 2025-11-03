using UnityEngine;

namespace Debrief
{
    public class ExtraCamera
    {
        
        private Camera _mainCharacterCamera;
        
        public RenderTexture CharacterTexture = new RenderTexture(1024, 1024, 24);
        
        
        /// <summary>
        /// 是否正在渲染相机
        /// </summary>
        private bool _cameraRendering = false;

        public void Start()
        {
            _cameraRendering = true;
        }
        
        public void Stop()
        {
            _cameraRendering = false;
        }

        // 不起作用
        public void Open()
        {
            if (_mainCharacterCamera)
            {
                _mainCharacterCamera.enabled = true;
                _mainCharacterCamera.gameObject.SetActive(true);
            }
        }

        // 不起作用
        public void Close()
        {
            if (_mainCharacterCamera)
            {
                _mainCharacterCamera.enabled = false;
                _mainCharacterCamera.gameObject.SetActive(false);
            }
        }
        
        private void Setup()
        {
            // 找到玩家
            var playerTransform = LevelManager.Instance.MainCharacter.characterModel.transform;
            if (playerTransform != null)
            {
                Debug.Log($"Player found: {playerTransform.name}");
        
                // 创建特写相机
                var cameraGO = playerTransform.GetOrAddGameObject("CharacterCloseUpCamera");
                _mainCharacterCamera = cameraGO.GetOrAddComponent<Camera>();
    
                // 设置相机属性 - 保持透视用于特写
                _mainCharacterCamera.orthographic = false;
                _mainCharacterCamera.fieldOfView = 25f; // 较小的FOV减少畸变，更适合特写
                _mainCharacterCamera.nearClipPlane = 0.01f; // 重要：避免裁剪太近的物体
                _mainCharacterCamera.farClipPlane = 5f;
                _mainCharacterCamera.clearFlags = CameraClearFlags.SolidColor;
                ColorUtility.TryParseHtmlString("#0D0D0DFF", out var background);
                _mainCharacterCamera.backgroundColor = Color.clear; //background; 

                // 创建Render Texture
                _mainCharacterCamera.targetTexture = CharacterTexture;
                
                // 设置相机跟随玩家
                cameraGO.transform.parent = playerTransform;
        
                // 设置相机只渲染角色层级
                int characterLayer = LevelManager.Instance.MainCharacter.gameObject.layer;
                _mainCharacterCamera.cullingMask = 1 << characterLayer;
        
                // 优化相机位置 - 人物前方特写
                cameraGO.transform.localPosition = new Vector3(-2f, 0.7f, 3.7f); // 调整到人物前方偏上
                cameraGO.transform.localRotation = Quaternion.Euler(0f, 145f, 0f); // 稍微俯视，面向人物
                
            }
        }
        
        public void CheckAndSetup()
        {
            if (_mainCharacterCamera == null)
            {
                Setup();
            }
        }

        public void Update()
        {
            if (_mainCharacterCamera)
            {
                if (_cameraRendering)
                {
                    _mainCharacterCamera.Render();
                }
            }
        }
        
        public void SetBackgroundColor(Color color)
        {
            _mainCharacterCamera.backgroundColor = color;
        }
    }
}