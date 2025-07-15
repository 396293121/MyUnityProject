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
    [CustomEditor(typeof(Canvas))]
    public class CanvasInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Canvas canvas = (Canvas)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Canvas (画布组件)", "UI元素的渲染容器，控制UI的显示方式", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 渲染设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Render Settings (渲染设置)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 渲染模式：Screen Space-Overlay屏幕覆盖，Screen Space-Camera屏幕相机，World Space世界空间", helpStyle);
            canvas.renderMode = (RenderMode)EditorGUILayout.EnumPopup("Render Mode (渲染模式)", canvas.renderMode);
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                EditorGUILayout.LabelField("ℹ️ 用于渲染UI的相机，如果为空则使用主相机", helpStyle);
                canvas.worldCamera = (Camera)EditorGUILayout.ObjectField(
                    "Render Camera (渲染相机)", 
                    canvas.worldCamera, 
                    typeof(Camera), 
                    true);
            }
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                EditorGUILayout.LabelField("ℹ️ UI平面距离相机的距离", helpStyle);
                canvas.planeDistance = EditorGUILayout.FloatField("Plane Distance (平面距离)", canvas.planeDistance);
            }
            
            EditorGUILayout.LabelField("ℹ️ 渲染层级，数值越大越靠前显示", helpStyle);
            canvas.sortingOrder = EditorGUILayout.IntField("Sort Order (排序层级)", canvas.sortingOrder);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 其他设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Additional Settings (其他设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 像素完美渲染，确保UI元素在像素级别对齐", helpStyle);
            canvas.pixelPerfect = EditorGUILayout.Toggle("Pixel Perfect (像素完美)", canvas.pixelPerfect);
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(canvas);
            }
        }
    }
    
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
    /// Animator组件的自定义Inspector
    /// 为动画控制器属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Animator))]
    public class AnimatorInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Animator animator = (Animator)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Animator (动画控制器)", "控制角色和物体的动画播放", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 控制器设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Controller Settings (动画控制器设置)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 动画控制器资源，定义了动画状态机和过渡条件", helpStyle);
            animator.runtimeAnimatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                "Controller (控制器)", 
                animator.runtimeAnimatorController, 
                typeof(RuntimeAnimatorController), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 角色的Avatar配置，用于人形角色的动画重定向", helpStyle);
            animator.avatar = (Avatar)EditorGUILayout.ObjectField(
                "Avatar (Avatar配置)", 
                animator.avatar, 
                typeof(Avatar), 
                false);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 播放设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Playback Settings (播放设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 动画更新模式：Normal-正常更新，AnimatePhysics-物理更新，UnscaledTime-不受时间缩放影响", helpStyle);
            animator.updateMode = (AnimatorUpdateMode)EditorGUILayout.EnumPopup("Update Mode (更新模式)", animator.updateMode);
            
            EditorGUILayout.LabelField("ℹ️ 动画剔除模式：AlwaysAnimate-总是播放，CullUpdateTransforms-剔除时只更新变换，CullCompletely-完全剔除", helpStyle);
            animator.cullingMode = (AnimatorCullingMode)EditorGUILayout.EnumPopup("Culling Mode (剔除模式)", animator.cullingMode);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 运行时信息
            if (Application.isPlaying && animator.runtimeAnimatorController != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Runtime Info (运行时信息)", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                
                // 当前状态信息
                if (animator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    EditorGUILayout.TextField("Current State (当前状态)", stateInfo.IsName("") ? "未知" : "播放中");
                    EditorGUILayout.Slider("Progress (播放进度)", stateInfo.normalizedTime % 1f, 0f, 1f);
                }
                
                EditorGUILayout.Toggle("In Transition (是否在过渡)", animator.IsInTransition(0));
                
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                // 参数列表
                if (animator.parameters.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Parameters (动画参数)", EditorStyles.boldLabel);
                    
                    foreach (AnimatorControllerParameter param in animator.parameters)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(param.name, GUILayout.Width(120));
                        
                        EditorGUI.BeginDisabledGroup(true);
                        switch (param.type)
                        {
                            case AnimatorControllerParameterType.Bool:
                                EditorGUILayout.Toggle(animator.GetBool(param.name));
                                break;
                            case AnimatorControllerParameterType.Float:
                                EditorGUILayout.FloatField(animator.GetFloat(param.name));
                                break;
                            case AnimatorControllerParameterType.Int:
                                EditorGUILayout.IntField(animator.GetInteger(param.name));
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                EditorGUILayout.LabelField("触发器");
                                break;
                        }
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(animator);
            }
        }
    }
    
    /// <summary>
    /// Image组件的自定义Inspector
    /// 为UI图像组件属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Image))]
    public class ImageInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Image image = (Image)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Image (图像组件)", "显示2D图像的UI组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 图像设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Image Settings (图像设置)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要显示的图像资源", helpStyle);
            image.sprite = (Sprite)EditorGUILayout.ObjectField(
                "Source Image (源图像)", 
                image.sprite, 
                typeof(Sprite), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 图像颜色和透明度", helpStyle);
            image.color = EditorGUILayout.ColorField("Color (颜色)", image.color);
            
            EditorGUILayout.LabelField("ℹ️ 材质，用于特殊渲染效果", helpStyle);
            image.material = (Material)EditorGUILayout.ObjectField(
                "Material (材质)", 
                image.material, 
                typeof(Material), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 是否接收光线投射事件（点击检测）", helpStyle);
            image.raycastTarget = EditorGUILayout.Toggle("Raycast Target (射线检测目标)", image.raycastTarget);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 图像类型设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Image Type Settings (图像类型设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 图像类型：Simple简单，Sliced切片，Tiled平铺，Filled填充", helpStyle);
            image.type = (Image.Type)EditorGUILayout.EnumPopup("Image Type (图像类型)", image.type);
            
            if (image.type == Image.Type.Simple)
            {
                EditorGUILayout.LabelField("ℹ️ 保持图像原始尺寸比例", helpStyle);
                image.preserveAspect = EditorGUILayout.Toggle("Preserve Aspect (保持宽高比)", image.preserveAspect);
            }
            else if (image.type == Image.Type.Filled)
            {
                EditorGUILayout.LabelField("ℹ️ 填充方法：Horizontal水平，Vertical垂直，Radial90径向90度，Radial180径向180度，Radial360径向360度", helpStyle);
                image.fillMethod = (Image.FillMethod)EditorGUILayout.EnumPopup("Fill Method (填充方法)", image.fillMethod);
                
                EditorGUILayout.LabelField("ℹ️ 填充量，0为空，1为满", helpStyle);
                image.fillAmount = EditorGUILayout.Slider("Fill Amount (填充量)", image.fillAmount, 0f, 1f);
                
                if (image.fillMethod == Image.FillMethod.Radial90 || image.fillMethod == Image.FillMethod.Radial180 || image.fillMethod == Image.FillMethod.Radial360)
                {
                    EditorGUILayout.LabelField("ℹ️ 顺时针填充", helpStyle);
                    image.fillClockwise = EditorGUILayout.Toggle("Clockwise (顺时针)", image.fillClockwise);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(image);
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
    
    /// <summary>
    /// Text组件的自定义Inspector
    /// 为UI文本组件属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(Text))]
    public class TextInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Text text = (Text)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("Text (文本组件)", "显示文本内容的UI组件", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 文本内容
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Text Content (文本内容)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要显示的文本内容", helpStyle);
            text.text = EditorGUILayout.TextArea(text.text, GUILayout.Height(60));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 字体设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Font Settings (字体设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 字体资源", helpStyle);
            text.font = (Font)EditorGUILayout.ObjectField(
                "Font (字体)", 
                text.font, 
                typeof(Font), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 字体样式：Normal正常，Bold粗体，Italic斜体，BoldAndItalic粗斜体", helpStyle);
            text.fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style (字体样式)", text.fontStyle);
            
            EditorGUILayout.LabelField("ℹ️ 字体大小", helpStyle);
            text.fontSize = EditorGUILayout.IntField("Font Size (字体大小)", text.fontSize);
            
            EditorGUILayout.LabelField("ℹ️ 行间距", helpStyle);
            text.lineSpacing = EditorGUILayout.FloatField("Line Spacing (行间距)", text.lineSpacing);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 外观设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Appearance Settings (外观设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 文本颜色", helpStyle);
            text.color = EditorGUILayout.ColorField("Color (颜色)", text.color);
            
            EditorGUILayout.LabelField("ℹ️ 材质，用于特殊渲染效果", helpStyle);
            text.material = (Material)EditorGUILayout.ObjectField(
                "Material (材质)", 
                text.material, 
                typeof(Material), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 是否接收光线投射事件（点击检测）", helpStyle);
            text.raycastTarget = EditorGUILayout.Toggle("Raycast Target (射线检测目标)", text.raycastTarget);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 对齐设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Alignment Settings (对齐设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 文本对齐方式", helpStyle);
            text.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment (对齐方式)", text.alignment);
            
            EditorGUILayout.LabelField("ℹ️ 水平文本溢出处理：Wrap换行，Overflow溢出", helpStyle);
            text.horizontalOverflow = (HorizontalWrapMode)EditorGUILayout.EnumPopup("Horizontal Overflow (水平溢出)", text.horizontalOverflow);
            
            EditorGUILayout.LabelField("ℹ️ 垂直文本溢出处理：Truncate截断，Overflow溢出", helpStyle);
            text.verticalOverflow = (VerticalWrapMode)EditorGUILayout.EnumPopup("Vertical Overflow (垂直溢出)", text.verticalOverflow);
            
            EditorGUILayout.LabelField("ℹ️ 自动调整文本大小以适应容器", helpStyle);
            text.resizeTextForBestFit = EditorGUILayout.Toggle("Best Fit (最佳适应)", text.resizeTextForBestFit);
            
            if (text.resizeTextForBestFit)
            {
                EditorGUILayout.LabelField("ℹ️ 最小字体大小", helpStyle);
                text.resizeTextMinSize = EditorGUILayout.IntField("Min Size (最小尺寸)", text.resizeTextMinSize);
                
                EditorGUILayout.LabelField("ℹ️ 最大字体大小", helpStyle);
                text.resizeTextMaxSize = EditorGUILayout.IntField("Max Size (最大尺寸)", text.resizeTextMaxSize);
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(text);
            }
        }
    }
    
    /// <summary>
    /// TextMeshProUGUI组件的自定义Inspector
    /// 为TextMeshPro UI文本组件属性添加中文标签和说明
    /// </summary>
    [CustomEditor(typeof(TextMeshProUGUI))]
    public class TextMeshProUGUIInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            TextMeshProUGUI tmpText = (TextMeshProUGUI)target;
            
            EditorGUILayout.Space();
            
            // 标题
            SirenixEditorGUI.Title("TextMeshPro - Text (UI) (TMP文本组件)", "高级文本渲染组件，支持富文本和特效", TextAlignment.Left, true);
            
            EditorGUILayout.Space();
            
            // 文本内容
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Text Input (文本输入)", EditorStyles.boldLabel);
            
            var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            EditorGUILayout.LabelField("ℹ️ 要显示的文本内容，支持富文本标签", helpStyle);
            tmpText.text = EditorGUILayout.TextArea(tmpText.text, GUILayout.Height(60));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 字体设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Font Settings (字体设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ TextMeshPro字体资源", helpStyle);
            tmpText.font = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Font Asset (字体资源)", 
                tmpText.font, 
                typeof(TMP_FontAsset), 
                false);
            
            EditorGUILayout.LabelField("ℹ️ 字体样式：Normal正常，Bold粗体，Italic斜体等", helpStyle);
            tmpText.fontStyle = (FontStyles)EditorGUILayout.EnumFlagsField("Font Style (字体样式)", tmpText.fontStyle);
            
            EditorGUILayout.LabelField("ℹ️ 字体大小", helpStyle);
            tmpText.fontSize = EditorGUILayout.FloatField("Font Size (字体大小)", tmpText.fontSize);
            
            EditorGUILayout.LabelField("ℹ️ 自动调整字体大小", helpStyle);
            tmpText.enableAutoSizing = EditorGUILayout.Toggle("Auto Size (自动调整大小)", tmpText.enableAutoSizing);
            
            if (tmpText.enableAutoSizing)
            {
                EditorGUILayout.LabelField("ℹ️ 最小字体大小", helpStyle);
                tmpText.fontSizeMin = EditorGUILayout.FloatField("Min (最小尺寸)", tmpText.fontSizeMin);
                
                EditorGUILayout.LabelField("ℹ️ 最大字体大小", helpStyle);
                tmpText.fontSizeMax = EditorGUILayout.FloatField("Max (最大尺寸)", tmpText.fontSizeMax);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 外观设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Appearance (外观设置)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 文本颜色", helpStyle);
            tmpText.color = EditorGUILayout.ColorField("Vertex Color (顶点颜色)", tmpText.color);
            
            EditorGUILayout.LabelField("ℹ️ 颜色渐变", helpStyle);
            tmpText.enableVertexGradient = EditorGUILayout.Toggle("Color Gradient (颜色渐变)", tmpText.enableVertexGradient);
            
            EditorGUILayout.LabelField("ℹ️ 是否接收光线投射事件（点击检测）", helpStyle);
            tmpText.raycastTarget = EditorGUILayout.Toggle("Raycast Target (射线检测目标)", tmpText.raycastTarget);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 对齐和换行设置
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Alignment & Wrapping (对齐和换行)", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("ℹ️ 文本对齐方式", helpStyle);
            tmpText.alignment = (TextAlignmentOptions)EditorGUILayout.EnumPopup("Text Alignment (文本对齐)", tmpText.alignment);
            
            EditorGUILayout.LabelField("ℹ️ 自动换行", helpStyle);
            tmpText.enableWordWrapping = EditorGUILayout.Toggle("Wrapping (自动换行)", tmpText.enableWordWrapping);
            
            EditorGUILayout.LabelField("ℹ️ 文本溢出模式：Overflow溢出，Ellipsis省略号，Masking遮罩，Truncate截断，ScrollRect滚动，Page分页，Linked链接", helpStyle);
            tmpText.overflowMode = (TextOverflowModes)EditorGUILayout.EnumPopup("Overflow (溢出模式)", tmpText.overflowMode);
            
            EditorGUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(tmpText);
            }
        }
    }
}
#endif