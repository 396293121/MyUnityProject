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
}

[ShowOdinSerializedPropertiesInInspector]
public class PlayerAudioConfig : MonoBehaviour
{
    [TabGroup("音效配置", "移动音效")]
    [BoxGroup("音效配置/移动音效/跳跃")]
    [LabelText("跳跃音效")]
    public AudioClipConfig jumpSound = new AudioClipConfig { volume = 0.8f, pitch = 1f };
    
    [TabGroup("音效配置", "战斗音效")]
    [BoxGroup("音效配置/战斗音效/攻击")]
    [LabelText("普通攻击音效")]
    public AudioClipConfig attackSound = new AudioClipConfig { volume = 0.9f, pitch = 1f, pitchVariation = 0.1f };
    
    [BoxGroup("音效配置/战斗音效/受伤")]
    [LabelText("受伤音效")]
    public AudioClipConfig hurtSound = new AudioClipConfig { volume = 1f, pitch = 1f, cooldown = 0.5f };
    
    [BoxGroup("音效配置/战斗音效/死亡")]
    [LabelText("死亡音效")]
    public AudioClipConfig deathSound = new AudioClipConfig { volume = 1f, pitch = 0.8f };
    
    [TabGroup("音效配置", "状态音效")]
    [BoxGroup("音效配置/状态音效/生命值")]
    [LabelText("低血量警告音效")]
    public AudioClipConfig lowHealthWarningSound = new AudioClipConfig { volume = 0.7f, pitch = 1.2f, cooldown = 2f };
    
    [BoxGroup("音效配置/状态音效/升级")]
    [LabelText("升级音效")]
    public AudioClipConfig levelUpSound = new AudioClipConfig { volume = 1f, pitch = 1f };
    
    [TabGroup("音效配置", "技能音效")]
    [BoxGroup("音效配置/技能音效/战士技能")]
    [LabelText("重击音效")]
    public AudioClipConfig heavySlashSound = new AudioClipConfig { volume = 1f, pitch = 0.9f, delay = 0.3f };
    
    [BoxGroup("音效配置/技能音效/战士技能")]
    [LabelText("旋风斩音效")]
    public AudioClipConfig whirlwindSound = new AudioClipConfig { volume = 0.9f, pitch = 1.1f, delay = 0.4f };
    
    [BoxGroup("音效配置/技能音效/战士技能")]
    [LabelText("战斗怒吼音效")]
    public AudioClipConfig battleCrySound = new AudioClipConfig { volume = 1f, pitch = 0.8f };
    
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
        InitializeAudioConfig();
    }
    
    /// <summary>
    /// 初始化音频配置
    /// </summary>
    private void InitializeAudioConfig()
    {
        if (enableCaching)
        {
            CacheAudioConfigs();
        }
        
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
    private void CacheAudioConfigs()
    {
        cachedConfigs["jump"] = jumpSound;
        cachedConfigs["attack"] = attackSound;
        cachedConfigs["hurt"] = hurtSound;
        cachedConfigs["death"] = deathSound;
        cachedConfigs["lowHealth"] = lowHealthWarningSound;
        cachedConfigs["levelUp"] = levelUpSound;
        cachedConfigs["heavySlash"] = heavySlashSound;
        cachedConfigs["whirlwind"] = whirlwindSound;
        cachedConfigs["battleCry"] = battleCrySound;
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="volumeMultiplier">音量倍数</param>
    /// <param name="pitchMultiplier">音调倍数</param>
    public void PlaySound(string soundName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        AudioClipConfig config = GetAudioConfig(soundName);
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
        
        if (activeSources.Contains(audioSource))
        {
            activeSources.Remove(audioSource);
        }
    }
    
    /// <summary>
    /// 获取音频源
    /// </summary>
    private AudioSource GetAudioSource()
    {
        if (dedicatedAudioSource != null)
        {
            return dedicatedAudioSource;
        }
        
        // 使用AudioManager的音频源
        if (AudioManager.Instance != null)
        {
            // 这里可以扩展为从AudioManager获取可用的音频源
            return AudioManager.Instance.sfxSource;
        }
        
        Debug.LogWarning("[PlayerAudioConfig] 没有可用的音频源");
        return null;
    }
    
    /// <summary>
    /// 获取音频配置
    /// </summary>
    private AudioClipConfig GetAudioConfig(string soundName)
    {
        if (enableCaching && cachedConfigs.ContainsKey(soundName))
        {
            return cachedConfigs[soundName];
        }
        
        // 直接访问配置
        switch (soundName.ToLower())
        {
            case "jump": return jumpSound;
            case "attack": return attackSound;
            case "hurt": return hurtSound;
            case "death": return deathSound;
            case "lowhealth": return lowHealthWarningSound;
            case "levelup": return levelUpSound;
            case "heavyslash": return heavySlashSound;
            case "whirlwind": return whirlwindSound;
            case "battlecry": return battleCrySound;
            default: return null;
        }
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
    
    [TabGroup("调试", "测试按钮")]
    [BoxGroup("调试/测试按钮/音效测试")]
    [Button("测试跳跃音效")]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void TestJumpSound() => PlaySound("jump");
    
    [BoxGroup("调试/测试按钮/音效测试")]
    [Button("测试攻击音效")]
    [GUIColor(1f, 0.6f, 0.4f)]
    private void TestAttackSound() => PlaySound("attack");
    
    [BoxGroup("调试/测试按钮/音效测试")]
    [Button("测试受伤音效")]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void TestHurtSound() => PlaySound("hurt");
    
    [BoxGroup("调试/测试按钮/控制")]
    [Button("停止所有音效")]
    [GUIColor(0.8f, 0.8f, 0.8f)]
    private void TestStopAllSounds() => StopAllSounds();
    
    [BoxGroup("调试/测试按钮/统计")]
    [Button("重置播放统计")]
    [GUIColor(0.6f, 1f, 0.6f)]
    private void ResetStats() => totalSoundsPlayed = 0;
    


}