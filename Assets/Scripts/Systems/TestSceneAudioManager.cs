using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 测试场景音频管理器
/// 基于Phaser项目中的音频系统，管理背景音乐、音效和环境音
/// </summary>
public class TestSceneAudioManager : MonoBehaviour
{
    #region 配置引用
    [Header("配置")]
    [SerializeField] private TestSceneConfig sceneConfig;
    #endregion
    
    #region 音频组件
    [Header("音频源")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioSource[] sfxAudioSources;
    [SerializeField] private AudioSource uiAudioSource;
    #endregion
    
    #region 音频设置
    [Header("音频设置")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float ambientVolume = 0.5f;
    [SerializeField] private float uiVolume = 0.6f;
    [SerializeField] private bool isMuted = false;
    #endregion
    
    #region 私有字段
    private TestSceneEventBus eventBus;
    
    // 音频资源缓存
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, SoundEffectConfig> soundEffectConfigs = new Dictionary<string, SoundEffectConfig>();
    
    // 音频播放状态
    private string currentMusicName = "";
    private bool isMusicPlaying = false;
    private bool isMusicPaused = false;
    private float musicFadeSpeed = 1f;
    
    // 音效管理
    private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private Dictionary<string, float> lastSfxPlayTimes = new Dictionary<string, float>();
    
    // 环境音管理
    private string currentAmbientName = "";
    private bool isAmbientPlaying = false;
    
    // 音频淡入淡出
    private Coroutine musicFadeCoroutine;
    private Coroutine ambientFadeCoroutine;
    
    // 3D音频支持
    private Dictionary<string, AudioSource> spatialAudioSources = new Dictionary<string, AudioSource>();
    
    // 音频混合器组
    private const float MIN_VOLUME_DB = -80f;
    private const float MAX_VOLUME_DB = 0f;
    #endregion
    
    #region Unity生命周期
    private void Awake()
    {
        // 初始化音频组件
        InitializeAudioComponents();
    }
    
    private void Start()
    {
        // 设置初始音频状态
        SetupInitialAudioState();
    }
    
    private void Update()
    {
        // 更新音频系统
        UpdateAudioSystem();
    }
    
    private void OnDestroy()
    {
        // 清理音频资源
        CleanupAudioResources();
    }
    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 初始化音频管理器
    /// </summary>
    public void Initialize(TestSceneConfig config, TestSceneEventBus eventSystem)
    {
        sceneConfig = config;
        eventBus = eventSystem;
        
        // 注册事件监听器
        RegisterEventListeners();
        
        // 加载音频配置
        LoadAudioConfiguration();
        
        // 预加载音频资源
        PreloadAudioClips();
        
        Debug.Log("[TestSceneAudioManager] 音频管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化音频组件
    /// </summary>
    private void InitializeAudioComponents()
    {
        // 创建音乐音频源
        if (musicAudioSource == null)
        {
            GameObject musicObj = new GameObject("MusicAudioSource");
            musicObj.transform.SetParent(transform);
            musicAudioSource = musicObj.AddComponent<AudioSource>();
            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;
        }
        
        // 创建环境音频源
        if (ambientAudioSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientAudioSource");
            ambientObj.transform.SetParent(transform);
            ambientAudioSource = ambientObj.AddComponent<AudioSource>();
            ambientAudioSource.loop = true;
            ambientAudioSource.playOnAwake = false;
        }
        
        // 创建UI音频源
        if (uiAudioSource == null)
        {
            GameObject uiObj = new GameObject("UIAudioSource");
            uiObj.transform.SetParent(transform);
            uiAudioSource = uiObj.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }
        
        // 创建音效音频源池
        InitializeSfxAudioSources();
    }
    
    /// <summary>
    /// 初始化音效音频源池
    /// </summary>
    private void InitializeSfxAudioSources()
    {
        int sfxSourceCount = 8; // 默认8个音效源
        
        if (sfxAudioSources == null || sfxAudioSources.Length == 0)
        {
            sfxAudioSources = new AudioSource[sfxSourceCount];
            
            for (int i = 0; i < sfxSourceCount; i++)
            {
                GameObject sfxObj = new GameObject($"SfxAudioSource_{i}");
                sfxObj.transform.SetParent(transform);
                sfxAudioSources[i] = sfxObj.AddComponent<AudioSource>();
                sfxAudioSources[i].playOnAwake = false;
            }
        }
        
        // 初始化可用音效源队列
        availableSfxSources.Clear();
        foreach (var source in sfxAudioSources)
        {
            availableSfxSources.Enqueue(source);
        }
    }
    
    /// <summary>
    /// 设置初始音频状态
    /// </summary>
    private void SetupInitialAudioState()
    {
        // 应用音量设置
        ApplyVolumeSettings();
        
        // 设置音频源属性
        SetupAudioSourceProperties();
    }
    
    /// <summary>
    /// 加载音频配置
    /// </summary>
    private void LoadAudioConfiguration()
    {
        if (sceneConfig?.soundEffects == null) return;
        
        // 设置音量
        if (sceneConfig.audioConfig != null)
        {
            masterVolume = sceneConfig.audioConfig.masterVolume;
            musicVolume = sceneConfig.audioConfig.musicVolume;
            sfxVolume = sceneConfig.audioConfig.sfxVolume;
            uiVolume = sceneConfig.audioConfig.voiceVolume;
        }
        
        // 加载音效配置
        foreach (var sfxConfig in sceneConfig.soundEffects)
        {
            if (!string.IsNullOrEmpty(sfxConfig.soundName))
            {
                soundEffectConfigs[sfxConfig.soundName] = sfxConfig;
            }
        }
        
        Debug.Log($"[TestSceneAudioManager] 加载了 {soundEffectConfigs.Count} 个音效配置");
    }
    
    /// <summary>
    /// 预加载音频资源
    /// </summary>
    private void PreloadAudioClips()
    {
        if (sceneConfig == null) return;
        
        // 预加载背景音乐
        if (sceneConfig.backgroundMusic != null)
        {
            string musicName = sceneConfig.backgroundMusic.name;
            audioClipCache[musicName] = sceneConfig.backgroundMusic;
        }
        
        // 预加载音效
        foreach (var sfxConfig in sceneConfig.soundEffects)
        {
            if (sfxConfig.audioClip != null)
            {
                audioClipCache[sfxConfig.soundName] = sfxConfig.audioClip;
            }
        }
        
        Debug.Log($"[TestSceneAudioManager] 预加载了 {audioClipCache.Count} 个音频资源");
    }
    
    /// <summary>
    /// 设置音频源属性
    /// </summary>
    private void SetupAudioSourceProperties()
    {
        // 设置音乐音频源
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume * masterVolume;
            musicAudioSource.pitch = 1f;
            musicAudioSource.spatialBlend = 0f; // 2D音频
        }
        
        // 设置环境音频源
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientVolume * masterVolume;
            ambientAudioSource.pitch = 1f;
            ambientAudioSource.spatialBlend = 0f; // 2D音频
        }
        
        // 设置UI音频源
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = uiVolume * masterVolume;
            uiAudioSource.pitch = 1f;
            uiAudioSource.spatialBlend = 0f; // 2D音频
        }
        
        // 设置音效音频源
        foreach (var sfxSource in sfxAudioSources)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
                sfxSource.pitch = 1f;
                sfxSource.spatialBlend = 0f; // 默认2D音频
            }
        }
    }
    #endregion
    
    #region 音乐播放方法
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayMusic(string musicName, bool loop = true, float fadeInTime = 1f)
    {
        if (string.IsNullOrEmpty(musicName) || musicAudioSource == null) return;
        
        // 如果已经在播放相同的音乐，则不重复播放
        if (currentMusicName == musicName && isMusicPlaying) return;
        
        // 获取音频剪辑
        AudioClip musicClip = GetAudioClip(musicName);
        if (musicClip == null)
        {
            Debug.LogWarning($"[TestSceneAudioManager] 未找到音乐: {musicName}");
            return;
        }
        
        // 停止当前音乐
        if (isMusicPlaying)
        {
            StopMusic(fadeInTime * 0.5f);
        }
        
        // 设置新音乐
        musicAudioSource.clip = musicClip;
        musicAudioSource.loop = loop;
        currentMusicName = musicName;
        
        // 开始播放并淡入
        if (fadeInTime > 0f)
        {
            StartCoroutine(FadeInMusic(fadeInTime));
        }
        else
        {
            musicAudioSource.volume = musicVolume * masterVolume;
            musicAudioSource.Play();
            isMusicPlaying = true;
        }
        
        Debug.Log($"[TestSceneAudioManager] 播放音乐: {musicName}");
        eventBus?.TriggerEvent("MusicPlay", musicName);
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopMusic(float fadeOutTime = 1f)
    {
        if (!isMusicPlaying || musicAudioSource == null) return;
        
        if (fadeOutTime > 0f)
        {
            StartCoroutine(FadeOutMusic(fadeOutTime));
        }
        else
        {
            musicAudioSource.Stop();
            isMusicPlaying = false;
            currentMusicName = "";
        }
        
        eventBus?.TriggerEvent("MusicStop", null);
    }
    
    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseMusic()
    {
        if (isMusicPlaying && musicAudioSource != null)
        {
            musicAudioSource.Pause();
            isMusicPaused = true;
        }
    }
    
    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeMusic()
    {
        if (isMusicPaused && musicAudioSource != null)
        {
            musicAudioSource.UnPause();
            isMusicPaused = false;
        }
    }
    
    /// <summary>
    /// 音乐淡入协程
    /// </summary>
    private IEnumerator FadeInMusic(float fadeTime)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicAudioSource.volume = 0f;
        musicAudioSource.Play();
        isMusicPlaying = true;
        
        float targetVolume = musicVolume * masterVolume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            musicAudioSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
        
        musicAudioSource.volume = targetVolume;
        musicFadeCoroutine = null;
    }
    
    /// <summary>
    /// 音乐淡出协程
    /// </summary>
    private IEnumerator FadeOutMusic(float fadeTime)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        float startVolume = musicAudioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        musicAudioSource.volume = 0f;
        musicAudioSource.Stop();
        isMusicPlaying = false;
        currentMusicName = "";
        musicFadeCoroutine = null;
    }
    #endregion
    
    #region 音效播放方法
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySoundEffect(string sfxName, float volume = 1f, float pitch = 1f, bool is3D = false, Vector3 position = default)
    {
        if (string.IsNullOrEmpty(sfxName)) return;
        
        // 检查音效冷却时间
        if (IsOnCooldown(sfxName)) return;
        
        // 获取音频剪辑
        AudioClip sfxClip = GetAudioClip(sfxName);
        if (sfxClip == null)
        {
            Debug.LogWarning($"[TestSceneAudioManager] 未找到音效: {sfxName}");
            return;
        }
        
        // 获取可用的音效源
        AudioSource sfxSource = GetAvailableSfxSource();
        if (sfxSource == null)
        {
            Debug.LogWarning("[TestSceneAudioManager] 没有可用的音效源");
            return;
        }
        
        // 应用音效配置
        ApplySoundEffectConfig(sfxSource, sfxName, volume, pitch, is3D, position);
        
        // 播放音效
        sfxSource.clip = sfxClip;
        sfxSource.Play();
        
        // 记录播放时间
        lastSfxPlayTimes[sfxName] = Time.time;
        
        // 启动协程来回收音效源
        StartCoroutine(RecycleSfxSource(sfxSource, sfxClip.length));
        
        Debug.Log($"[TestSceneAudioManager] 播放音效: {sfxName}");
        eventBus?.TriggerEvent("SoundEffectPlay", sfxName);
    }
    
    /// <summary>
    /// 播放UI音效
    /// </summary>
    public void PlayUISound(string sfxName, float volume = 1f)
    {
        if (string.IsNullOrEmpty(sfxName) || uiAudioSource == null) return;
        
        AudioClip sfxClip = GetAudioClip(sfxName);
        if (sfxClip == null) return;
        
        uiAudioSource.volume = volume * uiVolume * masterVolume;
        uiAudioSource.PlayOneShot(sfxClip);
    }
    
    /// <summary>
    /// 获取可用的音效源
    /// </summary>
    private AudioSource GetAvailableSfxSource()
    {
        // 优先使用队列中的可用源
        if (availableSfxSources.Count > 0)
        {
            AudioSource source = availableSfxSources.Dequeue();
            activeSfxSources.Add(source);
            return source;
        }
        
        // 查找已停止播放的源
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                AudioSource source = activeSfxSources[i];
                activeSfxSources.RemoveAt(i);
                activeSfxSources.Add(source);
                return source;
            }
        }
        
        // 如果所有源都在使用，强制使用最早的源
        if (activeSfxSources.Count > 0)
        {
            AudioSource source = activeSfxSources[0];
            activeSfxSources.RemoveAt(0);
            activeSfxSources.Add(source);
            source.Stop();
            return source;
        }
        
        return null;
    }
    
    /// <summary>
    /// 应用音效配置
    /// </summary>
    private void ApplySoundEffectConfig(AudioSource source, string sfxName, float volume, float pitch, bool is3D, Vector3 position)
    {
        // 应用基础设置
        source.volume = volume * sfxVolume * masterVolume;
        source.pitch = pitch;
        source.loop = false;
        
        // 应用3D音频设置
        if (is3D)
        {
            source.spatialBlend = 1f;
            source.transform.position = position;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 20f;
        }
        else
        {
            source.spatialBlend = 0f;
        }
        
        // 应用特定音效配置
        if (soundEffectConfigs.TryGetValue(sfxName, out SoundEffectConfig config))
        {
            source.volume *= config.volume;
            source.pitch *= config.pitch;
            
            if (config.is3D)
            {
                source.spatialBlend = 1f;
                source.minDistance = config.minDistance;
                source.maxDistance = config.maxDistance;
            }
        }
    }
    
    /// <summary>
    /// 回收音效源协程
    /// </summary>
    private IEnumerator RecycleSfxSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f); // 额外等待0.1秒确保播放完成
        
        if (activeSfxSources.Contains(source))
        {
            activeSfxSources.Remove(source);
            availableSfxSources.Enqueue(source);
        }
    }
    
    /// <summary>
    /// 检查音效是否在冷却中
    /// </summary>
    private bool IsOnCooldown(string sfxName)
    {
        if (!lastSfxPlayTimes.ContainsKey(sfxName)) return false;
        
        float cooldownTime = 0.1f; // 默认冷却时间
        
        if (soundEffectConfigs.TryGetValue(sfxName, out SoundEffectConfig config))
        {
            cooldownTime = config.cooldownTime;
        }
        
        return Time.time - lastSfxPlayTimes[sfxName] < cooldownTime;
    }
    #endregion
    
    #region 环境音播放方法
    /// <summary>
    /// 播放环境音
    /// </summary>
    public void PlayAmbientSound(string ambientName, bool loop = true, float fadeInTime = 2f)
    {
        if (string.IsNullOrEmpty(ambientName) || ambientAudioSource == null) return;
        
        // 如果已经在播放相同的环境音，则不重复播放
        if (currentAmbientName == ambientName && isAmbientPlaying) return;
        
        AudioClip ambientClip = GetAudioClip(ambientName);
        if (ambientClip == null)
        {
            Debug.LogWarning($"[TestSceneAudioManager] 未找到环境音: {ambientName}");
            return;
        }
        
        // 停止当前环境音
        if (isAmbientPlaying)
        {
            StopAmbientSound(fadeInTime * 0.5f);
        }
        
        // 设置新环境音
        ambientAudioSource.clip = ambientClip;
        ambientAudioSource.loop = loop;
        currentAmbientName = ambientName;
        
        // 开始播放并淡入
        if (fadeInTime > 0f)
        {
            StartCoroutine(FadeInAmbient(fadeInTime));
        }
        else
        {
            ambientAudioSource.volume = ambientVolume * masterVolume;
            ambientAudioSource.Play();
            isAmbientPlaying = true;
        }
        
        Debug.Log($"[TestSceneAudioManager] 播放环境音: {ambientName}");
    }
    
    /// <summary>
    /// 停止环境音
    /// </summary>
    public void StopAmbientSound(float fadeOutTime = 2f)
    {
        if (!isAmbientPlaying || ambientAudioSource == null) return;
        
        if (fadeOutTime > 0f)
        {
            StartCoroutine(FadeOutAmbient(fadeOutTime));
        }
        else
        {
            ambientAudioSource.Stop();
            isAmbientPlaying = false;
            currentAmbientName = "";
        }
    }
    
    /// <summary>
    /// 环境音淡入协程
    /// </summary>
    private IEnumerator FadeInAmbient(float fadeTime)
    {
        if (ambientFadeCoroutine != null)
        {
            StopCoroutine(ambientFadeCoroutine);
        }
        
        ambientAudioSource.volume = 0f;
        ambientAudioSource.Play();
        isAmbientPlaying = true;
        
        float targetVolume = ambientVolume * masterVolume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            ambientAudioSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
        
        ambientAudioSource.volume = targetVolume;
        ambientFadeCoroutine = null;
    }
    
    /// <summary>
    /// 环境音淡出协程
    /// </summary>
    private IEnumerator FadeOutAmbient(float fadeTime)
    {
        if (ambientFadeCoroutine != null)
        {
            StopCoroutine(ambientFadeCoroutine);
        }
        
        float startVolume = ambientAudioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        ambientAudioSource.volume = 0f;
        ambientAudioSource.Stop();
        isAmbientPlaying = false;
        currentAmbientName = "";
        ambientFadeCoroutine = null;
    }
    #endregion
    
    #region 音量控制方法
    /// <summary>
    /// 设置主音量
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        eventBus?.TriggerEvent("VolumeChanged", masterVolume);
    }
    
    /// <summary>
    /// 设置音乐音量
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (var source in sfxAudioSources)
        {
            if (source != null)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }
    }
    
    /// <summary>
    /// 设置环境音音量
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// 设置UI音量
    /// </summary>
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = uiVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// 静音/取消静音
    /// </summary>
    public void SetMuted(bool muted)
    {
        isMuted = muted;
        ApplyVolumeSettings();
    }
    
    /// <summary>
    /// 应用音量设置
    /// </summary>
    private void ApplyVolumeSettings()
    {
        float volumeMultiplier = isMuted ? 0f : 1f;
        
        if (musicAudioSource != null)
            musicAudioSource.volume = musicVolume * masterVolume * volumeMultiplier;
        
        if (ambientAudioSource != null)
            ambientAudioSource.volume = ambientVolume * masterVolume * volumeMultiplier;
        
        if (uiAudioSource != null)
            uiAudioSource.volume = uiVolume * masterVolume * volumeMultiplier;
        
        foreach (var source in sfxAudioSources)
        {
            if (source != null)
                source.volume = sfxVolume * masterVolume * volumeMultiplier;
        }
    }
    #endregion
    
    #region 辅助方法
    /// <summary>
    /// 获取音频剪辑
    /// </summary>
    private AudioClip GetAudioClip(string clipName)
    {
        if (audioClipCache.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }
        
        // 尝试从Resources文件夹加载
        clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        if (clip != null)
        {
            audioClipCache[clipName] = clip;
            return clip;
        }
        
        return null;
    }
    
    /// <summary>
    /// 更新音频系统
    /// </summary>
    private void UpdateAudioSystem()
    {
        // 清理已停止的音效源
        CleanupInactiveSfxSources();
        
        // 检查音乐播放状态
        if (isMusicPlaying && musicAudioSource != null && !musicAudioSource.isPlaying && !isMusicPaused)
        {
            // 音乐意外停止
            isMusicPlaying = false;
            currentMusicName = "";
        }
    }
    
    /// <summary>
    /// 清理非活跃的音效源
    /// </summary>
    private void CleanupInactiveSfxSources()
    {
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                AudioSource source = activeSfxSources[i];
                activeSfxSources.RemoveAt(i);
                availableSfxSources.Enqueue(source);
            }
        }
    }
    #endregion
    
    #region 事件系统
    /// <summary>
    /// 注册事件监听器
    /// </summary>
    private void RegisterEventListeners()
    {
        if (eventBus == null) return;
        
        // 游戏状态事件
        eventBus.OnGamePaused += OnGamePaused;
        eventBus.OnGameStart += OnGameStart;
        eventBus.OnGameEnd += OnGameEnd;
        
        // 玩家事件
        eventBus.OnPlayerAttack += OnPlayerAttack;
        eventBus.OnPlayerDeath += OnPlayerDeath;
        eventBus.OnPlayerSkillUsed += OnPlayerSkillUsed;
        
        // 敌人事件
        eventBus.OnEnemyDeath += OnEnemyDeath;
        eventBus.OnEnemyAttack += OnEnemyAttack;
        
        Debug.Log("[TestSceneAudioManager] 事件监听器注册完成");
    }
    
    /// <summary>
    /// 注销事件监听器
    /// </summary>
    private void UnregisterEventListeners()
    {
        if (eventBus == null) return;
        
        eventBus.OnGamePaused -= OnGamePaused;
        eventBus.OnGameStart -= OnGameStart;
        eventBus.OnGameEnd -= OnGameEnd;
        eventBus.OnPlayerAttack -= OnPlayerAttack;
        eventBus.OnPlayerDeath -= OnPlayerDeath;
        eventBus.OnPlayerSkillUsed -= OnPlayerSkillUsed;
        eventBus.OnEnemyDeath -= OnEnemyDeath;
        eventBus.OnEnemyAttack -= OnEnemyAttack;
    }
    
    /// <summary>
    /// 游戏暂停事件处理
    /// </summary>
    private void OnGamePaused(bool isPaused)
    {
        if (isPaused)
        {
            PauseMusic();
        }
        else
        {
            ResumeMusic();
        }
    }
    
    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    private void OnGameStart()
    {
        // 播放背景音乐
        if (sceneConfig?.backgroundMusic != null)
        {
            PlayMusic(sceneConfig.backgroundMusic.name);
        }
    }
    
    /// <summary>
    /// 游戏结束事件处理
    /// </summary>
    private void OnGameEnd()
    {
        StopMusic(1f);
        StopAmbientSound(1f);
    }
    
    /// <summary>
    /// 玩家攻击事件处理
    /// </summary>
    private void OnPlayerAttack()
    {
        PlaySoundEffect("player_attack");
    }
    
    /// <summary>
    /// 玩家死亡事件处理
    /// </summary>
    private void OnPlayerDeath()
    {
        PlaySoundEffect("player_death");
    }
    
    /// <summary>
    /// 玩家技能使用事件处理
    /// </summary>
    private void OnPlayerSkillUsed(string skillName)
    {
        PlaySoundEffect($"skill_{skillName}");
    }
    
    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    private void OnEnemyDeath(Enemy enemy)
    {
        PlaySoundEffect("enemy_death", 1f, 1f, true, enemy.transform.position);
    }
    
    /// <summary>
    /// 敌人攻击事件处理
    /// </summary>
    private void OnEnemyAttack(Enemy enemy)
    {
        PlaySoundEffect("enemy_attack", 1f, 1f, true, enemy.transform.position);
    }
    #endregion
    
    #region 公共方法
    /// <summary>
    /// 获取音频统计信息
    /// </summary>
    public string GetAudioStats()
    {
        string stats = "音频系统统计:\n";
        stats += $"主音量: {masterVolume:F2}\n";
        stats += $"音乐音量: {musicVolume:F2}\n";
        stats += $"音效音量: {sfxVolume:F2}\n";
        stats += $"环境音音量: {ambientVolume:F2}\n";
        stats += $"静音状态: {(isMuted ? "是" : "否")}\n";
        stats += $"当前音乐: {currentMusicName}\n";
        stats += $"当前环境音: {currentAmbientName}\n";
        stats += $"活跃音效源: {activeSfxSources.Count}\n";
        stats += $"可用音效源: {availableSfxSources.Count}\n";
        stats += $"缓存音频数: {audioClipCache.Count}\n";
        
        return stats;
    }
    
    /// <summary>
    /// 检查音乐是否正在播放
    /// </summary>
    public bool IsMusicPlaying()
    {
        return isMusicPlaying;
    }
    
    /// <summary>
    /// 检查环境音是否正在播放
    /// </summary>
    public bool IsAmbientPlaying()
    {
        return isAmbientPlaying;
    }
    
    /// <summary>
    /// 获取当前音乐名称
    /// </summary>
    public string GetCurrentMusicName()
    {
        return currentMusicName;
    }
    #endregion
    
    #region 清理方法
    /// <summary>
    /// 清理音频资源
    /// </summary>
    private void CleanupAudioResources()
    {
        // 注销事件监听器
        UnregisterEventListeners();
        
        // 停止所有音频
        StopMusic(0f);
        StopAmbientSound(0f);
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 清理音效源
        foreach (var source in sfxAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
        
        // 清理缓存
        audioClipCache.Clear();
        soundEffectConfigs.Clear();
        lastSfxPlayTimes.Clear();
        availableSfxSources.Clear();
        activeSfxSources.Clear();
        spatialAudioSources.Clear();
        
        Debug.Log("[TestSceneAudioManager] 音频资源清理完成");
    }
    #endregion
}