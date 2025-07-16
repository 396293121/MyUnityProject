using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音频管理器 - 统一管理游戏中的音效和背景音乐
/// 从原Phaser项目的音频系统迁移而来
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("音频源组件")]
    public AudioSource musicSource;     // 背景音乐音频源
    public AudioSource sfxSource;       // 音效音频源
    public AudioSource voiceSource;     // 语音音频源
    
    [Header("音量设置")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    
    [Header("音频资源")]
    public AudioClip[] backgroundMusics;
    public AudioClip[] characterSounds;
    public AudioClip[] enemySounds;
    public AudioClip[] uiSounds;
    public AudioClip[] environmentSounds;
    
    // 音频字典，用于快速查找
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    
    // 当前播放的背景音乐
    private AudioClip currentMusic;
    private bool isMusicFading = false;
    
    // 音效池，用于同时播放多个音效
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int maxSfxSources = 10;
    
    // 音频配置（从原项目的AudioConfig.js迁移）
    private Dictionary<string, AudioConfig> audioConfigs = new Dictionary<string, AudioConfig>();
        /// <summary>
    /// 自动初始化InputManager - 在场景加载前自动创建
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化音频管理器
    /// </summary>
    private void InitializeAudioManager()
    {
        // 创建音频源组件（如果不存在）
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }
        
        // 初始化音效池
        InitializeSfxPool();
        
        // 加载音频资源到字典
        LoadAudioClips();
        
        // 初始化音频配置
        InitializeAudioConfigs();
        
        // 应用音量设置
        UpdateVolumes();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[AudioManager] 音频管理器初始化完成");
        }
    }
    
    /// <summary>
    /// 初始化音效池
    /// </summary>
    private void InitializeSfxPool()
    {
        for (int i = 0; i < maxSfxSources; i++)
        {
            GameObject sfxObject = new GameObject($"SFX_Source_{i}");
            sfxObject.transform.SetParent(transform);
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }
    
    /// <summary>
    /// 加载音频资源到字典
    /// </summary>
    private void LoadAudioClips()
    {
        // 加载背景音乐
        if (backgroundMusics != null)
        {
            foreach (var clip in backgroundMusics)
            {
                if (clip != null)
                {
                    audioClips[clip.name] = clip;
                }
            }
        }
        
        // 加载角色音效
        if (characterSounds != null)
        {
            foreach (var clip in characterSounds)
            {
                if (clip != null)
                {
                    audioClips[clip.name] = clip;
                }
            }
        }
        
        // 加载敌人音效
        if (enemySounds != null)
        {
            foreach (var clip in enemySounds)
            {
                if (clip != null)
                {
                    audioClips[clip.name] = clip;
                }
            }
        }
        
        // 加载UI音效
        if (uiSounds != null)
        {
            foreach (var clip in uiSounds)
            {
                if (clip != null)
                {
                    audioClips[clip.name] = clip;
                }
            }
        }
        
        // 加载环境音效
        if (environmentSounds != null)
        {
            foreach (var clip in environmentSounds)
            {
                if (clip != null)
                {
                    audioClips[clip.name] = clip;
                }
            }
        }

        // 调试：检查 button_click 是否被加载
        if (audioClips.ContainsKey("button_click"))
        {
            Debug.Log($"[AudioManager] 'button_click' AudioClip 已加载: {audioClips["button_click"].name}");
        }
        else
        {
            Debug.LogWarning("[AudioManager] 'button_click' AudioClip 未加载。请确保它已添加到 AudioManager 的 UI Sounds 数组中。");
        }
    }
    
    /// <summary>
    /// 初始化音频配置（从原项目AudioConfig.js迁移）
    /// </summary>
    private void InitializeAudioConfigs()
    {
        // 战士音效配置
        audioConfigs["warrior_attack"] = new AudioConfig { volume = 0.8f, pitch = 1f, delay = 0f };
        audioConfigs["warrior_heavy_slash"] = new AudioConfig { volume = 1f, pitch = 0.9f, delay = 0.3f };
        audioConfigs["warrior_whirlwind"] = new AudioConfig { volume = 0.9f, pitch = 1.1f, delay = 0.4f };
        audioConfigs["warrior_battle_cry"] = new AudioConfig { volume = 1f, pitch = 0.8f, delay = 0f };
        
        // 法师音效配置
        audioConfigs["mage_fireball"] = new AudioConfig { volume = 0.7f, pitch = 1f, delay = 0.5f };
        audioConfigs["mage_lightning"] = new AudioConfig { volume = 0.9f, pitch = 1.2f, delay = 0.3f };
        audioConfigs["mage_heal"] = new AudioConfig { volume = 0.6f, pitch = 1f, delay = 0.4f };
        audioConfigs["mage_teleport"] = new AudioConfig { volume = 0.8f, pitch = 1.3f, delay = 0f };
        
        // 射手音效配置
        audioConfigs["archer_shoot"] = new AudioConfig { volume = 0.6f, pitch = 1f, delay = 0.2f };
        audioConfigs["archer_multi_shot"] = new AudioConfig { volume = 0.8f, pitch = 1.1f, delay = 0.3f };
        audioConfigs["archer_piercing_shot"] = new AudioConfig { volume = 0.7f, pitch = 0.9f, delay = 0.2f };
        audioConfigs["archer_explosive_arrow"] = new AudioConfig { volume = 1f, pitch = 0.8f, delay = 0.2f };
        
        // 野猪音效配置
        audioConfigs["wild_boar_charge"] = new AudioConfig { volume = 0.9f, pitch = 0.8f, delay = 0f };
        audioConfigs["wild_boar_attack"] = new AudioConfig { volume = 0.8f, pitch = 1f, delay = 0f };
        audioConfigs["wild_boar_enrage"] = new AudioConfig { volume = 1f, pitch = 0.7f, delay = 0f };
        audioConfigs["wild_boar_death"] = new AudioConfig { volume = 0.9f, pitch = 0.9f, delay = 0f };
        
        // UI音效配置
        audioConfigs["button_click"] = new AudioConfig { volume = 0.6f, pitch = 1f, delay = 0f };
        audioConfigs["ui_hover"] = new AudioConfig { volume = 0.3f, pitch = 1.2f, delay = 0f };
        audioConfigs["ui_confirm"] = new AudioConfig { volume = 0.7f, pitch = 1f, delay = 0f };
        audioConfigs["ui_cancel"] = new AudioConfig { volume = 0.6f, pitch = 0.8f, delay = 0f };
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayMusic(string musicName, bool fadeIn = true)
    {
        if (!audioClips.ContainsKey(musicName))
        {
            Debug.LogWarning($"[AudioManager] 找不到背景音乐: {musicName}");
            return;
        }
        
        AudioClip newMusic = audioClips[musicName];
        
        // 如果是同一首音乐，不需要切换
        if (currentMusic == newMusic && musicSource.isPlaying)
        {
            return;
        }
        
        if (fadeIn && musicSource.isPlaying)
        {
            StartCoroutine(FadeMusic(newMusic));
        }
        else
        {
            musicSource.clip = newMusic;
            musicSource.Play();
            currentMusic = newMusic;
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[AudioManager] 播放背景音乐: {musicName}");
        }
    }
    
    /// <summary>
    /// 播放背景音乐（带音量和循环参数）
    /// </summary>
    public void PlayMusic(string musicName, float volume, bool loop)
    {
        if (!audioClips.ContainsKey(musicName))
        {
            Debug.LogWarning($"[AudioManager] 找不到背景音乐: {musicName}");
            return;
        }
        
        AudioClip newMusic = audioClips[musicName];
        
        // 如果是同一首音乐，不需要切换
        if (currentMusic == newMusic && musicSource.isPlaying)
        {
            return;
        }
        
        musicSource.clip = newMusic;
        musicSource.volume = volume * masterVolume;
        musicSource.loop = loop;
        musicSource.Play();
        currentMusic = newMusic;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[AudioManager] 播放背景音乐: {musicName}, 音量: {volume}, 循环: {loop}");
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(string sfxName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        if (!audioClips.ContainsKey(sfxName))
        {
            Debug.LogWarning($"[AudioManager] 找不到音效: {sfxName}");
            return;
        }
        
        AudioClip clip = audioClips[sfxName];
        AudioConfig config = audioConfigs.ContainsKey(sfxName) ? audioConfigs[sfxName] : new AudioConfig();
      
        // 如果有延迟，使用协程播放
        if (config.delay > 0f)
        {
            StartCoroutine(PlaySFXWithDelay(clip, config, volumeMultiplier, pitchMultiplier));
        }
        else
        {
            PlaySFXImmediate(clip, config, volumeMultiplier, pitchMultiplier);
        }
    }
    
    /// <summary>
    /// 立即播放音效
    /// </summary>
    private void PlaySFXImmediate(AudioClip clip, AudioConfig config, float volumeMultiplier, float pitchMultiplier)
    {
        AudioSource availableSource = GetAvailableSfxSource();
        if (availableSource != null)
        {
            availableSource.clip = clip;
            float finalVolume = config.volume * sfxVolume * masterVolume * volumeMultiplier;
            availableSource.volume = finalVolume;
            Debug.Log($"[AudioManager] Playing SFX: {clip.name}, Final Volume: {finalVolume}, Config Volume: {config.volume}, SFX Volume: {sfxVolume}, Master Volume: {masterVolume}, Multiplier: {volumeMultiplier}");
            availableSource.pitch = config.pitch * pitchMultiplier;
            availableSource.Play();
        }
    }
    
    /// <summary>
    /// 延迟播放音效
    /// </summary>
    private IEnumerator PlaySFXWithDelay(AudioClip clip, AudioConfig config, float volumeMultiplier, float pitchMultiplier)
    {
        yield return new WaitForSeconds(config.delay);
        PlaySFXImmediate(clip, config, volumeMultiplier, pitchMultiplier);
    }
    
    /// <summary>
    /// 播放语音
    /// </summary>
    public void PlayVoice(string voiceName, float volumeMultiplier = 1f)
    {
        if (!audioClips.ContainsKey(voiceName))
        {
            Debug.LogWarning($"[AudioManager] 找不到语音: {voiceName}");
            return;
        }
        
        voiceSource.clip = audioClips[voiceName];
        voiceSource.volume = voiceVolume * masterVolume * volumeMultiplier;
        voiceSource.Play();
    }
    
    /// <summary>
    /// 获取可用的音效源
    /// </summary>
    private AudioSource GetAvailableSfxSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // 如果没有可用的，使用第一个（会打断当前播放的音效）
        return sfxPool[0];
    }
    
    /// <summary>
    /// 音乐淡入淡出
    /// </summary>
    private IEnumerator FadeMusic(AudioClip newMusic)
    {
        if (isMusicFading) yield break;
        
        isMusicFading = true;
        float fadeTime = 1f;
        float originalVolume = musicSource.volume;
        
        // 淡出当前音乐
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(originalVolume, 0f, t / fadeTime);
            yield return null;
        }
        
        // 切换音乐
        musicSource.clip = newMusic;
        musicSource.Play();
        currentMusic = newMusic;
        
        // 淡入新音乐
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, t / fadeTime);
            yield return null;
        }
        
        musicSource.volume = musicVolume * masterVolume;
        isMusicFading = false;
    }
    
    /// <summary>
    /// 音乐淡出
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        if (isMusicFading) yield break;
        
        isMusicFading = true;
        float fadeTime = 1f;
        float originalVolume = musicSource.volume;
        
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(originalVolume, 0f, t / fadeTime);
            yield return null;
        }
        
        musicSource.Stop();
        currentMusic = null;
        isMusicFading = false;
    }
    
    /// <summary>
    /// 更新音量设置
    /// </summary>
    public void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        
        // 更新音效池中所有音源的音量
        foreach (var source in sfxPool)
        {
            if (source.isPlaying)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }
        
        if (voiceSource != null)
        {
            voiceSource.volume = voiceVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// 设置主音量
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// 设置音乐音量
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// 设置语音音量
    /// </summary>
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// 暂停所有音频
    /// </summary>
    public void PauseAll()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        
        foreach (var source in sfxPool)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
        
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Pause();
        }
    }
    
    /// <summary>
    /// 恢复所有音频
    /// </summary>
    public void ResumeAll()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
        
        foreach (var source in sfxPool)
        {
            source.UnPause();
        }
        
        if (voiceSource != null)
        {
            voiceSource.UnPause();
        }
    }
    
    /// <summary>
    /// 停止所有音频
    /// </summary>
    public void StopAll()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        
        foreach (var source in sfxPool)
        {
            source.Stop();
        }
        
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
        
        currentMusic = null;
    }
}

/// <summary>
/// 音频配置结构
/// </summary>
[System.Serializable]
public struct AudioConfig
{
    public float volume;
    public float pitch;
    public float delay;
    
    public AudioConfig(float volume = 1f, float pitch = 1f, float delay = 0f)
    {
        this.volume = volume;
        this.pitch = pitch;
        this.delay = delay;
    }
}