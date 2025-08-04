using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音频管理器 - 统一管理游戏中的音效和背景音乐
/// 从原Phaser项目的音频系统迁移而来
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }
    
    [Header("音频源组件")]
    public AudioSource musicSource;     // 背景音乐音频源
    
    [Header("音量设置")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    
    [Header("默认背景音乐")]
    public AudioClip defaultBackgroundMusic;
    
    // 音频字典，用于快速查找
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    
    // 当前播放的背景音乐
    private AudioClip currentMusic;
    private bool isMusicFading = false;
    
    // 音频配置（从原项目的AudioConfig.js迁移）
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
            InitializeBGMManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
            InitializeEventListeners();
    }
    private void OnDestroy()
    {
        CleanupEventListeners();
    }

    #region 地图切换事件处理
    /// <summary>
    /// 初始化事件监听器
    /// </summary>
    private void InitializeEventListeners()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete += OnMapTransitionComplete;
            MapManager.Instance.OnMapLoaded += OnMapLoaded;
        }
        else
        {
            Debug.LogWarning("[BGMManager] MapManager实例未找到，无法订阅地图切换事件");
        }
    }

    /// <summary>
    /// 清理事件监听器
    /// </summary>
    private void CleanupEventListeners()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete -= OnMapTransitionComplete;
            MapManager.Instance.OnMapLoaded -= OnMapLoaded;
            Debug.Log("[BGMManager] 已取消订阅地图切换完成事件和地图加载事件");
        }
    }

    /// <summary>
    /// 地图加载完成事件处理（用于初次加载）
    /// </summary>
    /// <param name="areaId">区域ID</param>
    private void OnMapLoaded(string areaId)
    {
        
        // 初次地图加载时播放背景音乐
        LoadAndPlayAreaMusic();
    }

    /// <summary>
    /// 地图切换完成事件处理
    /// </summary>
    /// <param name="targetAreaId">目标区域ID</param>
    private void OnMapTransitionComplete(PortalConfig targetPortalConfig)
    {
        
        // 停止当前背景音乐并淡出
        if (musicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }
        
        // 加载并播放新区域的背景音乐
        LoadAndPlayAreaMusic();
    }

    /// <summary>
    /// 加载并播放区域背景音乐
    /// </summary>
    /// <param name="areaId">区域ID</param>
    private void LoadAndPlayAreaMusic()
    {
        try
        {
            // 从MapManager获取当前区域配置
            if (MapManager.Instance?.CurrentSceneArea != null)
            {
                var currentArea = MapManager.Instance.CurrentSceneArea;
                
                // 检查区域是否有特定的背景音乐配置
                if (currentArea.backgroundMusic != null)
                {
                    
                    // 延迟播放新音乐，确保淡出完成
                    StartCoroutine(PlayMusicAfterDelay(currentArea.backgroundMusic, 1.5f));
                }
                else
                {
                     StartCoroutine(PlayMusicAfterDelay(defaultBackgroundMusic, 1.5f));

                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BGMManager] 加载区域背景音乐时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 延迟播放音乐协程
    /// </summary>
    /// <param name="musicClip">音乐片段</param>
    /// <param name="delay">延迟时间</param>
    private IEnumerator PlayMusicAfterDelay(AudioClip musicClip, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (musicClip != null)
        {
            PlayMusic(musicClip, musicVolume, true); // 淡入播放
        }
    }
    #endregion
    /// <summary>
    /// 初始化音频管理器
    /// </summary>
    private void InitializeBGMManager()
    {
        // 创建音频源组件（如果不存在）
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        
        // 应用音量设置
        UpdateVolumes();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[BGMManager] 音频管理器初始化完成");
        }
    }
    
    /// <summary>
    /// 播放背景音乐（带音量和循环参数）
    /// </summary>
    public void PlayMusic(string musicName, float volume, bool loop)
    {
        if (!audioClips.ContainsKey(musicName))
        {
            Debug.LogWarning($"[BGMManager] 找不到背景音乐: {musicName}");
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
            Debug.Log($"[BGMManager] 播放背景音乐: {musicName}, 音量: {volume}, 循环: {loop}");
        }
    }
    /// <summary>
    /// 播放背景音乐（带音频剪辑、音量和循环参数）
    /// </summary>
    public void PlayMusic(AudioClip music, float volume, bool loop)
    {
        if (currentMusic == music && musicSource.isPlaying)
        {
            return;
        }
        musicSource.clip = music;
        musicSource.volume = volume * masterVolume;
        musicSource.loop = loop;
        musicSource.Play();
        currentMusic = music;
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
        // if (!audioClips.ContainsKey(sfxName))
        // {
        //     Debug.LogWarning($"[BGMManager] 找不到音效: {sfxName}");
        //     return;
        // }
        
        // AudioClip clip = audioClips[sfxName];
        // AudioConfig config = audioConfigs.ContainsKey(sfxName) ? audioConfigs[sfxName] : new AudioConfig();
      
        // // 如果有延迟，使用协程播放
        // if (config.delay > 0f)
        // {
        //     StartCoroutine(PlaySFXWithDelay(clip, config, volumeMultiplier, pitchMultiplier));
        // }
        // else
        // {
        //     PlaySFXImmediate(clip, config, volumeMultiplier, pitchMultiplier);
        // }
    }
    
    
    
    /// <summary>
    /// 播放语音
    /// </summary>
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