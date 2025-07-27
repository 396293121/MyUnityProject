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
    
  
}
#endif