#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

/// <summary>
/// Unity常用组件的扩展自定义Inspector集合
/// 为更多Unity自带组件添加中文标签和详细注释
/// </summary>
namespace CustomInspectors.Extended
{
    /// <summary>
    /// SpriteRenderer组件的自定义Inspector
    /// 为2D精灵渲染器属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(SpriteRenderer))]
    public class SpriteRendererInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            SpriteRenderer spriteRenderer = (SpriteRenderer)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("精灵渲染器 (SpriteRenderer)", "用于渲染2D精灵图像的组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 精灵设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("精灵设置", EditorStyles.boldLabel);
            
            // 创建自定义样式的小图标提示
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要显示的精灵图像资源", helpStyle);
            spriteRenderer.sprite = (Sprite)EditorGUILayout.ObjectField(
                "Sprite (精灵)", 
                spriteRenderer.sprite, 
                typeof(Sprite), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 精灵的颜色和透明度", helpStyle);
            spriteRenderer.color = EditorGUILayout.ColorField("Color (颜色)", spriteRenderer.color);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 翻转设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("翻转设置", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 水平翻转精灵图像", helpStyle);
            spriteRenderer.flipX = EditorGUILayout.Toggle("Flip X (水平翻转)", spriteRenderer.flipX);
            
            EditorGUILayout.LabelField("ℹ️ 垂直翻转精灵图像", helpStyle);
            spriteRenderer.flipY = EditorGUILayout.Toggle("Flip Y (垂直翻转)", spriteRenderer.flipY);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 渲染设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("渲染设置", EditorStyles.boldLabel);
            
            // EditorGUILayout.LabelField("ℹ️ 用于渲染的材质，影响着色器效果", helpStyle);
            // spriteRenderer.material = (Material)EditorGUILayout.ObjectField(
            //     "Material (材质)", 
            //     spriteRenderer.material, 
            //     typeof(Material), 
            //     false);
            
            EditorGUILayout.LabelField("ℹ️ 渲染层级，数值越大越靠前显示", helpStyle);
            spriteRenderer.sortingOrder = EditorGUILayout.IntField("Sorting Order (排序层级)", spriteRenderer.sortingOrder);
            
            EditorGUILayout.LabelField("ℹ️ 排序层名称，用于分组管理渲染顺序", helpStyle);
            string[] sortingLayerNames = SortingLayer.layers.Length > 0 ? 
                System.Array.ConvertAll(SortingLayer.layers, layer => layer.name) : 
                new string[] { "Default" };
            int currentIndex = System.Array.IndexOf(sortingLayerNames, spriteRenderer.sortingLayerName);
            if (currentIndex < 0) currentIndex = 0;
            
            int newIndex = EditorGUILayout.Popup("Sorting Layer (排序层)", currentIndex, sortingLayerNames);
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < sortingLayerNames.Length)
            {
                spriteRenderer.sortingLayerName = sortingLayerNames[newIndex];
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 精灵信息
            if (spriteRenderer.sprite != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("精灵信息", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("精灵名称", spriteRenderer.sprite.name);
                EditorGUILayout.Vector2Field("精灵尺寸", new Vector2(spriteRenderer.sprite.rect.width, spriteRenderer.sprite.rect.height));
                EditorGUILayout.FloatField("像素单位 (Pixels Per Unit)", spriteRenderer.sprite.pixelsPerUnit);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(spriteRenderer);
            }
        }
    }
    
    /// <summary>
    /// AudioSource组件的自定义Inspector
    /// 为音频源属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(AudioSource))]
    public class AudioSourceInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            AudioSource audioSource = (AudioSource)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("音频源 (AudioSource)", "播放音频剪辑的组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 音频剪辑
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("音频设置", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要播放的音频剪辑文件", helpStyle);
            audioSource.clip = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip (音频剪辑)", 
                audioSource.clip, 
                typeof(AudioClip), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 音频输出的混音器组", helpStyle);
            audioSource.outputAudioMixerGroup = (UnityEngine.Audio.AudioMixerGroup)EditorGUILayout.ObjectField(
                "Output (输出组)", 
                audioSource.outputAudioMixerGroup, 
                typeof(UnityEngine.Audio.AudioMixerGroup), 
                false);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 播放控制
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("播放控制", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 是否在游戏开始时自动播放", helpStyle);
            audioSource.playOnAwake = EditorGUILayout.Toggle("Play On Awake (开始时播放)", audioSource.playOnAwake);
            
            EditorGUILayout.LabelField("ℹ️ 是否循环播放音频", helpStyle);
            audioSource.loop = EditorGUILayout.Toggle("Loop (循环播放)", audioSource.loop);
            
            EditorGUILayout.LabelField("ℹ️ 音频播放的优先级 (0-256，数值越高优先级越高)", helpStyle);
            audioSource.priority = EditorGUILayout.IntSlider("Priority (优先级)", audioSource.priority, 0, 256);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 音量和音调
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("音量和音调", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 音频播放的音量大小 (0-1)", helpStyle);
            audioSource.volume = EditorGUILayout.Slider("Volume (音量)", audioSource.volume, 0f, 1f);
            
            EditorGUILayout.LabelField("ℹ️ 音频播放的音调高低 (-3到3，1为正常音调)", helpStyle);
            audioSource.pitch = EditorGUILayout.Slider("Pitch (音调)", audioSource.pitch, -3f, 3f);
            
            EditorGUILayout.LabelField("ℹ️ 立体声声像位置 (-1左声道，0中央，1右声道)", helpStyle);
            audioSource.panStereo = EditorGUILayout.Slider("Stereo Pan (立体声声像)", audioSource.panStereo, -1f, 1f);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 3D音效设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3D音效设置", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 3D音效的空间混合程度 (0为2D，1为完全3D)", helpStyle);
            audioSource.spatialBlend = EditorGUILayout.Slider("Spatial Blend (空间混合)", audioSource.spatialBlend, 0f, 1f);
            
            if (audioSource.spatialBlend > 0f)
            {
                EditorGUILayout.LabelField("ℹ️ 音频开始衰减的距离", helpStyle);
                audioSource.minDistance = EditorGUILayout.FloatField("Min Distance (最小距离)", audioSource.minDistance);
                
                EditorGUILayout.LabelField("ℹ️ 音频完全衰减的距离", helpStyle);
                audioSource.maxDistance = EditorGUILayout.FloatField("Max Distance (最大距离)", audioSource.maxDistance);
                
                EditorGUILayout.LabelField("ℹ️ 音量衰减的计算方式", helpStyle);
                audioSource.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("Volume Rolloff (衰减模式)", audioSource.rolloffMode);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 运行时信息
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("运行时信息", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Is Playing (正在播放)", audioSource.isPlaying);
                
                if (audioSource.clip != null)
                {
                    EditorGUILayout.Slider("播放进度", audioSource.time, 0f, audioSource.clip.length);
                }
                EditorGUI.EndDisabledGroup();
                
                // 播放控制按钮
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("播放"))
                {
                    audioSource.Play();
                }
                if (GUILayout.Button("暂停"))
                {
                    audioSource.Pause();
                }
                if (GUILayout.Button("停止"))
                {
                    audioSource.Stop();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            
            // 音频剪辑信息
            if (audioSource.clip != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("音频剪辑信息", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("剪辑名称", audioSource.clip.name);
                EditorGUILayout.FloatField("时长 (秒)", audioSource.clip.length);
                EditorGUILayout.IntField("采样率 (Hz)", audioSource.clip.frequency);
                EditorGUILayout.IntField("声道数", audioSource.clip.channels);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(audioSource);
            }
        }
    }
    
    /// <summary>
    /// Camera组件的自定义Inspector
    /// 为摄像机属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Camera))]
    public class CameraInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Camera camera = (Camera)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("摄像机 (Camera)", "渲染场景到屏幕或纹理的组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 渲染设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("渲染设置", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 摄像机清除标志，决定如何清除上一帧的内容", helpStyle);
            camera.clearFlags = (CameraClearFlags)EditorGUILayout.EnumPopup("Clear Flags (清除标志)", camera.clearFlags);
            
            if (camera.clearFlags == CameraClearFlags.SolidColor)
            {
                EditorGUILayout.LabelField("ℹ️ 用于清除屏幕的纯色背景", helpStyle);
                camera.backgroundColor = EditorGUILayout.ColorField("Background (背景颜色)", camera.backgroundColor);
            }
            
            EditorGUILayout.LabelField("ℹ️ 摄像机渲染的层级遮罩", helpStyle);
            camera.cullingMask = EditorGUILayout.MaskField("Culling Mask (剔除遮罩)", camera.cullingMask, UnityEditorInternal.InternalEditorUtility.layers);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 投影设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("投影设置", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 摄像机投影模式：透视投影或正交投影", helpStyle);
            camera.orthographic = EditorGUILayout.Toggle("Orthographic (正交投影)", camera.orthographic);
            
            if (camera.orthographic)
            {
                EditorGUILayout.LabelField("ℹ️ 正交投影的视野大小", helpStyle);
                camera.orthographicSize = EditorGUILayout.FloatField("Size (正交尺寸)", camera.orthographicSize);
            }
            else
            {
                EditorGUILayout.LabelField("ℹ️ 透视投影的视野角度 (度)", helpStyle);
                camera.fieldOfView = EditorGUILayout.Slider("Field of View (视野角度)", camera.fieldOfView, 1f, 179f);
            }
            
            EditorGUILayout.LabelField("ℹ️ 近裁剪平面距离", helpStyle);
            camera.nearClipPlane = EditorGUILayout.FloatField("Near (近裁剪面)", camera.nearClipPlane);
            
            EditorGUILayout.LabelField("ℹ️ 远裁剪平面距离", helpStyle);
            camera.farClipPlane = EditorGUILayout.FloatField("Far (远裁剪面)", camera.farClipPlane);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 视口设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("视口设置", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 摄像机在屏幕上的渲染区域 (X, Y, Width, Height)", helpStyle);
            camera.rect = EditorGUILayout.RectField("Viewport Rect (视口矩形)", camera.rect);
            
            EditorGUILayout.LabelField("ℹ️ 摄像机渲染深度，数值越高越后渲染", helpStyle);
            camera.depth = EditorGUILayout.FloatField("Depth (深度)", camera.depth);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 渲染路径
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("渲染路径", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 摄像机使用的渲染路径", helpStyle);
            camera.renderingPath = (RenderingPath)EditorGUILayout.EnumPopup("Rendering Path (渲染路径)", camera.renderingPath);
            
            EditorGUILayout.LabelField("ℹ️ 目标纹理，如果设置则渲染到纹理而不是屏幕", helpStyle);
            camera.targetTexture = (RenderTexture)EditorGUILayout.ObjectField(
                "Target Texture (目标纹理)", 
                camera.targetTexture, 
                typeof(RenderTexture), 
                false);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 摄像机信息
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("摄像机信息", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Aspect Ratio (宽高比)", camera.aspect);
            EditorGUILayout.Vector2Field("Pixel Size (像素尺寸)", new Vector2(camera.pixelWidth, camera.pixelHeight));
            
            if (camera.targetTexture == null)
            {
                EditorGUILayout.TextField("渲染目标", "屏幕");
            }
            else
            {
                EditorGUILayout.TextField("渲染目标", camera.targetTexture.name);
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(camera);
            }
        }
    }
}
#endif