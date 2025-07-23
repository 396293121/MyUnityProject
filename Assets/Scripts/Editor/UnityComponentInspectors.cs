#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity常用组件的自定义Inspector集合
/// 为Unity自带组件添加中文标签和详细注释
/// </summary>
namespace CustomInspectors
{
    /// <summary>
    /// Transform组件的自定义Inspector
    /// 为位置、旋转、缩放属性添加中文标签和说明
    /// </summary>
    // [CustomEditor(typeof(Transform))]
    // public class TransformInspector : OdinEditor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         Transform transform = (Transform)target;
            
    //         EditorGUILayout.Space();
            
    //         // 标题
    //         SirenixEditorGUI.Title("Transform (变换组件)", "控制物体在3D空间中的位置、旋转和缩放", TextAlignment.Left, true);
            
    //         EditorGUILayout.Space();
            
    //         // 位置
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Position (位置)", EditorStyles.boldLabel);
    //         var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
    //         EditorGUILayout.LabelField("ℹ️ 物体在世界坐标系中的位置坐标 (X, Y, Z)", helpStyle);
    //         transform.position = EditorGUILayout.Vector3Field("Position (世界位置)", transform.position);
            
    //         if (transform.parent != null)
    //         {
    //             transform.localPosition = EditorGUILayout.Vector3Field("Local Position (本地位置)", transform.localPosition);
    //         }
    //         EditorGUILayout.EndVertical();
            
    //         EditorGUILayout.Space();
            
    //         // 旋转
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Rotation (旋转)", EditorStyles.boldLabel);
    //         EditorGUILayout.LabelField("ℹ️ 物体的旋转角度，以欧拉角表示 (X, Y, Z 轴旋转度数)", helpStyle);
            
    //         Vector3 eulerAngles = transform.eulerAngles;
    //         Vector3 newEulerAngles = EditorGUILayout.Vector3Field("Euler Angles (世界旋转)", eulerAngles);
    //         if (newEulerAngles != eulerAngles)
    //         {
    //             transform.eulerAngles = newEulerAngles;
    //         }
            
    //         if (transform.parent != null)
    //         {
    //             Vector3 localEulerAngles = transform.localEulerAngles;
    //             Vector3 newLocalEulerAngles = EditorGUILayout.Vector3Field("Local Euler Angles (本地旋转)", localEulerAngles);
    //             if (newLocalEulerAngles != localEulerAngles)
    //             {
    //                 transform.localEulerAngles = newLocalEulerAngles;
    //             }
    //         }
    //         EditorGUILayout.EndVertical();
            
    //         EditorGUILayout.Space();
            
    //         // 缩放
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Scale (缩放)", EditorStyles.boldLabel);
    //         EditorGUILayout.LabelField("ℹ️ 物体的缩放比例，1为原始大小 (X, Y, Z 轴缩放倍数)", helpStyle);
    //         transform.localScale = EditorGUILayout.Vector3Field("Local Scale (本地缩放)", transform.localScale);
    //         EditorGUILayout.EndVertical();
            
    //         EditorGUILayout.Space();
            
    //         // 层级信息
    //         if (transform.childCount > 0 || transform.parent != null)
    //         {
    //             EditorGUILayout.BeginVertical("box");
    //             EditorGUILayout.LabelField("Hierarchy (层级信息)", EditorStyles.boldLabel);
                
    //             if (transform.parent != null)
    //             {
    //                 EditorGUILayout.LabelField($"Parent (父物体): {transform.parent.name}");
    //             }
                
    //             if (transform.childCount > 0)
    //             {
    //                 EditorGUILayout.LabelField($"Child Count (子物体数量): {transform.childCount}");
    //             }
                
    //             EditorGUILayout.EndVertical();
    //         }
            
    //         if (GUI.changed)
    //         {
    //             EditorUtility.SetDirty(transform);
    //         }
    //     }
    // }
    
    /// <summary>
    /// Rigidbody2D组件的自定义Inspector
    /// 为2D刚体物理属性添加中文标签和说明
    /// </summary>
    // [CustomEditor(typeof(Rigidbody2D))]
    // public class Rigidbody2DInspector : OdinEditor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         Rigidbody2D rb = (Rigidbody2D)target;
            
    //         EditorGUILayout.Space();
            
    //         // 标题
    //         SirenixEditorGUI.Title("Rigidbody2D (2D刚体组件)", "控制物体的2D物理行为和运动", TextAlignment.Left, true);
            
    //         EditorGUILayout.Space();
    //         // 基本属性
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Basic Properties (基本属性)", EditorStyles.boldLabel);
            
    //         var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
    //         EditorGUILayout.LabelField("ℹ️ 物体类型决定了物理引擎如何处理这个物体", helpStyle);
    //         rb.bodyType = (RigidbodyType2D)EditorGUILayout.EnumPopup("Body Type (物体类型)", rb.bodyType);
            
    //         if (rb.bodyType == RigidbodyType2D.Dynamic)
    //         {
    //             EditorGUILayout.LabelField("ℹ️ 物体的质量，影响碰撞和力的作用效果", helpStyle);
    //             rb.mass = EditorGUILayout.FloatField("Mass (质量)", rb.mass);
                
    //             EditorGUILayout.LabelField("ℹ️ 线性阻力，模拟空气阻力等效果", helpStyle);
    //             rb.drag = EditorGUILayout.FloatField("Linear Drag (线性阻力)", rb.drag);
                
    //             EditorGUILayout.LabelField("ℹ️ 角度阻力，影响旋转运动的衰减", helpStyle);
    //             rb.angularDrag = EditorGUILayout.FloatField("Angular Drag (角度阻力)", rb.angularDrag);
                
    //             EditorGUILayout.LabelField("ℹ️ 重力缩放，1为正常重力，0为无重力", helpStyle);
    //             rb.gravityScale = EditorGUILayout.FloatField("Gravity Scale (重力缩放)", rb.gravityScale);
    //         }
            
    //         EditorGUILayout.EndVertical();
            
    //         EditorGUILayout.Space();
            
    //         // 约束
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Constraints (运动约束)", EditorStyles.boldLabel);
    //         EditorGUILayout.LabelField("ℹ️ 限制物体在特定轴向上的移动或旋转", helpStyle);
            
    //         rb.freezeRotation = EditorGUILayout.Toggle("Freeze Rotation (冻结旋转)", rb.freezeRotation);
    //         rb.constraints = (RigidbodyConstraints2D)EditorGUILayout.EnumFlagsField("Constraints (位置约束)", rb.constraints);
            
    //         EditorGUILayout.EndVertical();
            
    //         EditorGUILayout.Space();
            
    //         // 运行时信息
    //         if (Application.isPlaying)
    //         {
    //             EditorGUILayout.BeginVertical("box");
    //             EditorGUILayout.LabelField("Runtime Info (运行时信息)", EditorStyles.boldLabel);
                
    //             EditorGUI.BeginDisabledGroup(true);
    //             EditorGUILayout.Vector2Field("Velocity (当前速度)", rb.velocity);
    //             EditorGUILayout.FloatField("Angular Velocity (当前角速度)", rb.angularVelocity);
    //             EditorGUI.EndDisabledGroup();
                
    //             EditorGUILayout.EndVertical();
    //         }
            
    //         if (GUI.changed)
    //         {
    //             EditorUtility.SetDirty(rb);
    //         }
    //     }
    // }
    
    /// <summary>
    /// BoxCollider2D组件的自定义Inspector
    /// 为2D盒子碰撞器属性添加中文标签和说明
    /// </summary>
    // [CustomEditor(typeof(BoxCollider2D))]
    // public class BoxCollider2DInspector : OdinEditor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         BoxCollider2D collider = (BoxCollider2D)target;
            
    //         EditorGUILayout.Space();
            
    //         // 标题
    //         SirenixEditorGUI.Title("BoxCollider2D (2D盒子碰撞器)", "矩形形状的2D碰撞检测组件", TextAlignment.Left, true);
            
    //         EditorGUILayout.Space();
            
    //         // 使用Unity原生的Inspector绘制
    //         DrawDefaultInspector();
            
    //         EditorGUILayout.Space();
            
    //         // 信息显示
    //         EditorGUILayout.BeginVertical("box");
    //         EditorGUILayout.LabelField("Collider Info (碰撞器信息)", EditorStyles.boldLabel);
            
    //         var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
    //         EditorGUILayout.LabelField("ℹ️ 碰撞器的世界坐标信息", helpStyle);
            
    //         EditorGUI.BeginDisabledGroup(true);
    //         EditorGUILayout.Vector2Field("World Center (世界中心)", collider.bounds.center);
    //         EditorGUILayout.Vector2Field("World Size (世界尺寸)", collider.bounds.size);
    //         EditorGUI.EndDisabledGroup();
            
    //         EditorGUILayout.EndVertical();
            
    //         if (GUI.changed)
    //         {
    //             EditorUtility.SetDirty(collider);
    //         }
    //     }
    // }
    
    // /// <summary>
    /// Canvas组件的自定义Inspector
    /// 为Canvas画布属性添加中文标签和说明
    /// </summary>

    /// <summary>
    /// Camera组件的自定义Inspector
    /// 为相机属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Camera))]
    public class CameraInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Camera camera = (Camera)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Camera (相机组件)", "控制场景的视角和渲染设置", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 投影设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Projection Settings (投影设置)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 投影模式：Perspective透视投影，Orthographic正交投影", helpStyle);
            camera.orthographic = EditorGUILayout.Toggle("Orthographic (正交投影)", camera.orthographic);
            
            if (camera.orthographic)
            {
                EditorGUILayout.LabelField("ℹ️ 正交相机的视野大小", helpStyle);
                camera.orthographicSize = EditorGUILayout.FloatField("Size (正交大小)", camera.orthographicSize);
            }
            else
            {
                EditorGUILayout.LabelField("ℹ️ 透视相机的视野角度（度数）", helpStyle);
                camera.fieldOfView = EditorGUILayout.Slider("Field of View (视野角度)", camera.fieldOfView, 1f, 179f);
            }
            
            EditorGUILayout.LabelField("ℹ️ 近裁剪平面距离，小于此距离的物体不会被渲染", helpStyle);
            camera.nearClipPlane = EditorGUILayout.FloatField("Near (近裁剪面)", camera.nearClipPlane);
            
            EditorGUILayout.LabelField("ℹ️ 远裁剪平面距离，大于此距离的物体不会被渲染", helpStyle);
            camera.farClipPlane = EditorGUILayout.FloatField("Far (远裁剪面)", camera.farClipPlane);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 渲染设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Rendering Settings (渲染设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 相机的渲染优先级，数值越小越先渲染", helpStyle);
            camera.depth = EditorGUILayout.FloatField("Depth (渲染深度)", camera.depth);
            
            EditorGUILayout.LabelField("ℹ️ 相机渲染的层级遮罩", helpStyle);
            camera.cullingMask = EditorGUILayout.MaskField("Culling Mask (渲染层级)", camera.cullingMask, UnityEditorInternal.InternalEditorUtility.layers);
            
            EditorGUILayout.LabelField("ℹ️ 背景清除方式：Skybox天空盒，Solid Color纯色，Depth Only仅深度，Nothing不清除", helpStyle);
            camera.clearFlags = (CameraClearFlags)EditorGUILayout.EnumPopup("Clear Flags (清除标志)", camera.clearFlags);
            
            if (camera.clearFlags == CameraClearFlags.SolidColor)
            {
                EditorGUILayout.LabelField("ℹ️ 背景纯色", helpStyle);
                camera.backgroundColor = EditorGUILayout.ColorField("Background (背景颜色)", camera.backgroundColor);
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(camera);
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
            SirenixEditorGUI.Title("AudioSource (音频源组件)", "播放音频剪辑的组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 音频剪辑
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Audio Clip (音频剪辑)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要播放的音频文件", helpStyle);
            audioSource.clip = (AudioClip)EditorGUILayout.ObjectField(
                "AudioClip (音频剪辑)", 
                audioSource.clip, 
                typeof(AudioClip), 
                false);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 播放设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Playback Settings (播放设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 游戏开始时自动播放", helpStyle);
            audioSource.playOnAwake = EditorGUILayout.Toggle("Play On Awake (启动时播放)", audioSource.playOnAwake);
            
            EditorGUILayout.LabelField("ℹ️ 循环播放音频", helpStyle);
            audioSource.loop = EditorGUILayout.Toggle("Loop (循环播放)", audioSource.loop);
            
            EditorGUILayout.LabelField("ℹ️ 音频优先级，0为最高优先级，256为最低", helpStyle);
            audioSource.priority = EditorGUILayout.IntSlider("Priority (优先级)", audioSource.priority, 0, 256);
            
            EditorGUILayout.LabelField("ℹ️ 音量大小，0为静音，1为最大音量", helpStyle);
            audioSource.volume = EditorGUILayout.Slider("Volume (音量)", audioSource.volume, 0f, 1f);
            
            EditorGUILayout.LabelField("ℹ️ 音调，1为正常速度，2为双倍速度，0.5为半速", helpStyle);
            audioSource.pitch = EditorGUILayout.Slider("Pitch (音调)", audioSource.pitch, -3f, 3f);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 3D音频设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3D Sound Settings (3D音频设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 空间混合：0为2D音频，1为3D音频", helpStyle);
            audioSource.spatialBlend = EditorGUILayout.Slider("Spatial Blend (空间混合)", audioSource.spatialBlend, 0f, 1f);
            
            if (audioSource.spatialBlend > 0f)
            {
                EditorGUILayout.LabelField("ℹ️ 最小听到距离", helpStyle);
                audioSource.minDistance = EditorGUILayout.FloatField("Min Distance (最小距离)", audioSource.minDistance);
                
                EditorGUILayout.LabelField("ℹ️ 最大听到距离", helpStyle);
                audioSource.maxDistance = EditorGUILayout.FloatField("Max Distance (最大距离)", audioSource.maxDistance);
            }
            
            EditorGUILayout.EndVertical();
            
            // 运行时信息
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Runtime Info (运行时信息)", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Is Playing (正在播放)", audioSource.isPlaying);
                if (audioSource.clip != null)
                {
                    EditorGUILayout.Slider("Time (播放时间)", audioSource.time, 0f, audioSource.clip.length);
                }
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
    /// Button组件的自定义Inspector
    /// 为UI按钮组件属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Button))]
    public class ButtonInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Button button = (Button)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Button (按钮组件)", "可交互的UI按钮组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 交互设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Interactable Settings (交互设置)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 按钮是否可以交互", helpStyle);
            button.interactable = EditorGUILayout.Toggle("Interactable (可交互)", button.interactable);
            
            EditorGUILayout.LabelField("ℹ️ 过渡效果：None无，ColorTint颜色变化，SpriteSwap精灵切换，Animation动画", helpStyle);
            button.transition = (Selectable.Transition)EditorGUILayout.EnumPopup("Transition (过渡效果)", button.transition);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 导航设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Navigation Settings (导航设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 键盘/手柄导航模式：None无，Horizontal水平，Vertical垂直，Automatic自动，Explicit明确指定", helpStyle);
            Navigation nav = button.navigation;
            nav.mode = (Navigation.Mode)EditorGUILayout.EnumPopup("Navigation (导航模式)", nav.mode);
            button.navigation = nav;
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 颜色过渡设置
            if (button.transition == Selectable.Transition.ColorTint)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Color Transition (颜色过渡)", EditorStyles.boldLabel);
                
                ColorBlock colors = button.colors;
                EditorGUILayout.LabelField("ℹ️ 正常状态颜色", helpStyle);
                colors.normalColor = EditorGUILayout.ColorField("Normal Color (正常颜色)", colors.normalColor);
                
                EditorGUILayout.LabelField("ℹ️ 高亮状态颜色（鼠标悬停）", helpStyle);
                colors.highlightedColor = EditorGUILayout.ColorField("Highlighted Color (高亮颜色)", colors.highlightedColor);
                
                EditorGUILayout.LabelField("ℹ️ 按下状态颜色", helpStyle);
                colors.pressedColor = EditorGUILayout.ColorField("Pressed Color (按下颜色)", colors.pressedColor);
                
                EditorGUILayout.LabelField("ℹ️ 禁用状态颜色", helpStyle);
                colors.disabledColor = EditorGUILayout.ColorField("Disabled Color (禁用颜色)", colors.disabledColor);
                
                EditorGUILayout.LabelField("ℹ️ 颜色过渡倍数", helpStyle);
                colors.colorMultiplier = EditorGUILayout.Slider("Color Multiplier (颜色倍数)", colors.colorMultiplier, 1f, 5f);
                
                EditorGUILayout.LabelField("ℹ️ 过渡持续时间", helpStyle);
                colors.fadeDuration = EditorGUILayout.Slider("Fade Duration (过渡时间)", colors.fadeDuration, 0f, 2f);
                
                button.colors = colors;
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            
            // 事件设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Events (事件)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("ℹ️ 按钮点击时触发的事件", helpStyle);
            
            // 使用Unity默认的UnityEvent绘制器
            SerializedObject serializedObject = new SerializedObject(button);
            SerializedProperty onClickProperty = serializedObject.FindProperty("m_OnClick");
            EditorGUILayout.PropertyField(onClickProperty, new GUIContent("On Click (点击事件)"));
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(button);
            }
        }
    }
    
}
#endif