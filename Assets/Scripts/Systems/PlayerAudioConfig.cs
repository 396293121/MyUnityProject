using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家音频配置组件
/// 提供可配置的音效系统，支持拖入音频文件、设置音量等参数
/// 优化音效播放的扩展性和性能
/// </summary>
[System.Serializable]
public class AudioClipConfig
{
    [LabelText("音频文件")]
    [Required("请拖入音频文件")]
    public AudioClip audioClip;

    [LabelText("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [LabelText("音调")]
    [Range(0.5f, 2f)]
    public float pitch = 1f;

    [LabelText("播放延迟")]
    [Range(0f, 2f)]
    [SuffixLabel("秒")]
    public float delay = 0f;

    [LabelText("随机音调范围")]
    [Range(0f, 0.5f)]
    [InfoBox("为音效添加随机音调变化，增加真实感")]
    public float pitchVariation = 0f;

    [LabelText("冷却时间")]
    [Range(0f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("防止音效播放过于频繁")]
    public float cooldown = 0f;
    [LabelText("是否循环播放")]
    public bool isLoop = false;
}
public class PlayerAudioConfig : MonoBehaviour
{
    public static PlayerAudioConfig Instance { get; private set; }
  [TabGroup("高级设置", "音频源设置")]
    [BoxGroup("高级设置/音频源设置/音效文件")]
    [LabelText("音效文件")]
    [InfoBox("所有角色怪物的音效文件")]
    public AudioDataConfig audioDataConfig;   
     [TabGroup("高级设置", "音频源设置")]
    [BoxGroup("高级设置/音频源设置/音频源")]
    [LabelText("专用音频源")]
    [InfoBox("如果不指定，将使用AudioManager的默认音频源")]
    public AudioSource dedicatedAudioSource;

    [BoxGroup("高级设置/音频源设置/音频源")]
    [LabelText("使用3D音效")]
    [InfoBox("启用3D空间音效，音量会根据距离变化")]
    public bool use3DAudio = false;

    [BoxGroup("高级设置/音频源设置/音频源")]
    [LabelText("最大播放距离")]
    [ShowIf("use3DAudio")]
    [Range(1f, 50f)]
    public float maxDistance = 20f;

    [TabGroup("高级设置", "性能优化")]
    [BoxGroup("高级设置/性能优化/缓存")]
    [LabelText("启用音效缓存")]
    [InfoBox("缓存音效配置以提高性能")]
    public bool enableCaching = true;

    [BoxGroup("高级设置/性能优化/缓存")]
    [LabelText("最大同时播放数")]
    [Range(1, 10)]
    [InfoBox("限制同时播放的音效数量")]
    public int maxConcurrentSounds = 5;

    [TabGroup("调试", "调试信息")]
    [BoxGroup("调试/调试信息/日志")]
    [LabelText("启用调试日志")]
    public bool enableDebugLog = false;

    [BoxGroup("调试/调试信息/统计")]
    [LabelText("显示播放统计")]
    [ReadOnly]
    [ShowInInspector]
    private int totalSoundsPlayed = 0;

    // 内部缓存和状态管理
    private Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();
    private Dictionary<string, AudioClipConfig> cachedConfigs = new Dictionary<string, AudioClipConfig>();
    private List<AudioSource> activeSources = new List<AudioSource>();
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioConfig();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// 初始化音频配置
    /// </summary>
    private void InitializeAudioConfig()
    {
        // if (enableCaching)
        // {
        //     CacheAudioConfigs();
        // }

        // 如果启用3D音效但没有专用音频源，创建一个
        if (use3DAudio && dedicatedAudioSource == null)
        {
            GameObject audioSourceObj = new GameObject("PlayerAudioSource");
            audioSourceObj.transform.SetParent(transform);
            audioSourceObj.transform.localPosition = Vector3.zero;

            dedicatedAudioSource = audioSourceObj.AddComponent<AudioSource>();
            dedicatedAudioSource.spatialBlend = 1f; // 3D音效
            dedicatedAudioSource.maxDistance = maxDistance;
            dedicatedAudioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    /// <summary>
    /// 缓存音频配置
    /// </summary>
    // private void CacheAudioConfigs()
    // {
    //     classSoundConfigs.Values.ForEach(string name =>
    //     {
    //         cachedConfigs[config.name] = config;
    //     });
    //     cachedConfigs["jump"] = jumpSound;
    //     cachedConfigs["attack"] = attackSound;
    //     cachedConfigs["hurt"] = hurtSound;
    //     cachedConfigs["death"] = deathSound;
    //     cachedConfigs["lowHealth"] = lowHealthWarningSound;
    //     cachedConfigs["levelUp"] = levelUpSound;
    // }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="volumeMultiplier">音量倍数</param>
    /// <param name="pitchMultiplier">音调倍数</param>
    public void PlaySound(string soundName, AudioCategory category=default, float volumeMultiplier = 1f, float pitchMultiplier = 1f)

    {  

            AudioClipConfig config = null;
        if (category != AudioCategory.Default)
        {
            config = audioDataConfig.soundCategories.Find((entry) => entry.categoryType == category)?.soundEntries.Find((entry) => entry.soundName == soundName)?.config;

        }
        else
        {
                     Debug.Log("大司2");
            config = audioDataConfig.classSoundConfigs.Find((entry) => entry.className == soundName)?.sound;

        }

        if (config?.audioClip == null)
        {
            if (enableDebugLog)
                Debug.LogWarning($"[PlayerAudioConfig] 找不到音效配置: {soundName}");
            return;
        }

        // 检查冷却时间
        if (IsOnCooldown(soundName, config.cooldown))
        {
            if (enableDebugLog)
                Debug.Log($"[PlayerAudioConfig] 音效 {soundName} 仍在冷却中");
            return;
        }

        // 检查最大同时播放数
        if (activeSources.Count >= maxConcurrentSounds)
        {
            // 停止最早播放的音效
            if (activeSources.Count > 0)
            {
                activeSources[0].Stop();
                activeSources.RemoveAt(0);
            }
        }

        // 播放音效
        if (config.delay > 0f)
        {

            StartCoroutine(PlaySoundWithDelay(config, volumeMultiplier, pitchMultiplier, soundName));
        }
        else
        {
            PlaySoundImmediate(config, volumeMultiplier, pitchMultiplier, soundName);
        }
    }



    /// <summary>
    /// 立即播放音效
    /// </summary>
    private void PlaySoundImmediate(AudioClipConfig config, float volumeMultiplier, float pitchMultiplier, string soundName)
    {
        AudioSource audioSource = GetAudioSource();
        if (audioSource == null) return;

        // 配置音频源
        audioSource.clip = config.audioClip;
        audioSource.volume = config.volume * volumeMultiplier;

        // 添加音调变化
        float finalPitch = config.pitch * pitchMultiplier;
        if (config.pitchVariation > 0f)
        {
            finalPitch += Random.Range(-config.pitchVariation, config.pitchVariation);
        }
        audioSource.pitch = finalPitch;
        audioSource.loop = config.isLoop;
        // 播放音效
        audioSource.Play();

        // 记录播放时间和统计
        lastPlayTimes[soundName] = Time.time;
        totalSoundsPlayed++;

        // 添加到活跃音源列表
        if (!activeSources.Contains(audioSource))
        {
            activeSources.Add(audioSource);
        }

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerAudioConfig] 播放音效: {soundName}, 音量: {audioSource.volume:F2}, 音调: {audioSource.pitch:F2}");
        }

        // 启动协程来清理完成的音源
        StartCoroutine(CleanupAudioSource(audioSource, config.audioClip.length / finalPitch));
    }

    /// <summary>
    /// 延迟播放音效
    /// </summary>
    private System.Collections.IEnumerator PlaySoundWithDelay(AudioClipConfig config, float volumeMultiplier, float pitchMultiplier, string soundName)
    {
        yield return new WaitForSeconds(config.delay);
        PlaySoundImmediate(config, volumeMultiplier, pitchMultiplier, soundName);
    }

    /// <summary>
    /// 清理音频源
    /// </summary>
    private System.Collections.IEnumerator CleanupAudioSource(AudioSource audioSource, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f); // 额外等待0.1秒确保播放完成
       // 保留至少一个专用音频源
        if (audioSource != dedicatedAudioSource && activeSources.Count > 1) 
        {
            activeSources.Remove(audioSource);
            Destroy(audioSource);
        }
        // if (activeSources.Contains(audioSource))
        // {
        //     activeSources.Remove(audioSource);
        // }
    }

    /// <summary>
    /// 获取音频源
    /// </summary>
    private AudioSource GetAudioSource()
    {
       // 优先使用专用音频源
        if (dedicatedAudioSource != null && !dedicatedAudioSource.isPlaying) 
        {
            return dedicatedAudioSource;
        }

        // 动态创建临时音频源（最多保留5个）
        var newSource = gameObject.AddComponent<AudioSource>();
        newSource.spatialBlend = use3DAudio ? 1f : 0f;
        activeSources.Add(newSource);
        return newSource;
    }


    /// <summary>
    /// 检查是否在冷却时间内
    /// </summary>
    private bool IsOnCooldown(string soundName, float cooldown)
    {
        if (cooldown <= 0f) return false;

        if (lastPlayTimes.ContainsKey(soundName))
        {
            return Time.time - lastPlayTimes[soundName] < cooldown;
        }

        return false;
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var audioSource in activeSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        activeSources.Clear();
    }

    /// <summary>
    /// 设置全局音量
    /// </summary>
    public void SetGlobalVolume(float volume)
    {
        if (dedicatedAudioSource != null)
        {
            dedicatedAudioSource.volume = volume;
        }
    }






}