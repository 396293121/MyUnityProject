using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 场景类型枚举
/// </summary>
public enum SceneType
{
    MainMenu,       // 主菜单
    GameWorld,      // 游戏世界
    Battle,         // 战斗场景
    Town,           // 城镇
    Dungeon,        // 地牢
    Loading         // 加载场景
}

/// <summary>
/// 场景信息结构
/// </summary>
[System.Serializable]
public struct SceneInfo
{
    public string sceneName;        // 场景名称
    public SceneType sceneType;     // 场景类型
    public string displayName;      // 显示名称
    public bool isAdditive;         // 是否为附加场景
    public float loadingDelay;      // 加载延迟
}

/// <summary>
/// 场景控制器 - 管理游戏场景的加载、切换和状态管理
/// 从原Phaser项目的SceneManager.js迁移而来
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    
    [Header("场景配置")]
    public List<SceneInfo> sceneInfos = new List<SceneInfo>();
    public string loadingSceneName = "LoadingScene";
    public float minLoadingTime = 2f;           // 最小加载时间
    public bool showLoadingProgress = true;     // 显示加载进度
    
    [Header("过渡效果")]
    public GameObject fadePanel;                // 淡入淡出面板
    public float fadeTime = 1f;                 // 淡入淡出时间
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("音频设置")]
    public bool playTransitionSound = true;     // 播放过渡音效
    public string transitionSoundName = "scene_transition";
    
    [Header("调试")]
    public bool debugMode = false;
    
    // 当前场景信息
    private SceneInfo currentSceneInfo;
    private string currentSceneName;
    private SceneType currentSceneType;
    
    // 加载状态
    private bool isLoading = false;
    private float loadingProgress = 0f;
    private AsyncOperation currentLoadOperation;
    
    // 场景历史
    private Stack<string> sceneHistory = new Stack<string>();
    private int maxHistorySize = 10;
    
    // 事件
    public static event Action<string> OnSceneLoadStarted;
    public static event Action<string, float> OnSceneLoadProgress;
    public static event Action<string> OnSceneLoadCompleted;
    public static event Action<string, string> OnSceneChanged;
    
    // 场景数据传递
    private Dictionary<string, object> sceneData = new Dictionary<string, object>();
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneController();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 获取当前场景信息
        UpdateCurrentSceneInfo();
        
        // 初始化淡入淡出面板
        if (fadePanel != null)
        {
            fadePanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 更新加载进度
        UpdateLoadingProgress();
        
        // 处理输入（调试用）
        if (debugMode)
        {
            HandleDebugInput();
        }
    }
    
    /// <summary>
    /// 初始化场景控制器
    /// </summary>
    private void InitializeSceneController()
    {
        // 设置默认场景信息
        if (sceneInfos.Count == 0)
        {
            SetupDefaultScenes();
        }
        
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        if (debugMode)
        {
            Debug.Log("[SceneController] 场景控制器初始化完成");
        }
    }
    
    /// <summary>
    /// 设置默认场景配置
    /// </summary>
    private void SetupDefaultScenes()
    {
        sceneInfos.Add(new SceneInfo
        {
            sceneName = "MainMenu",
            sceneType = SceneType.MainMenu,
            displayName = "主菜单",
            isAdditive = false,
            loadingDelay = 0f
        });
        
        sceneInfos.Add(new SceneInfo
        {
            sceneName = "GameWorld",
            sceneType = SceneType.GameWorld,
            displayName = "游戏世界",
            isAdditive = false,
            loadingDelay = 1f
        });
        
        sceneInfos.Add(new SceneInfo
        {
            sceneName = "BattleScene",
            sceneType = SceneType.Battle,
            displayName = "战斗",
            isAdditive = false,
            loadingDelay = 0.5f
        });
    }
    
    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string sceneName, bool useLoadingScreen = true)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[SceneController] 正在加载场景，无法加载 {sceneName}");
            return;
        }
        
        SceneInfo sceneInfo = GetSceneInfo(sceneName);
        if (string.IsNullOrEmpty(sceneInfo.sceneName))
        {
            Debug.LogError($"[SceneController] 找不到场景信息: {sceneName}");
            return;
        }
        
        // 添加到历史记录
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            AddToHistory(currentSceneName);
        }
        
        if (useLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneInfo));
        }
        else
        {
            StartCoroutine(LoadSceneDirectly(sceneInfo));
        }
    }
    
    /// <summary>
    /// 通过场景类型加载场景
    /// </summary>
    public void LoadSceneByType(SceneType sceneType, bool useLoadingScreen = true)
    {
        SceneInfo sceneInfo = GetSceneInfoByType(sceneType);
        if (!string.IsNullOrEmpty(sceneInfo.sceneName))
        {
            LoadScene(sceneInfo.sceneName, useLoadingScreen);
        }
        else
        {
            Debug.LogError($"[SceneController] 找不到场景类型: {sceneType}");
        }
    }
    
    /// <summary>
    /// 使用加载屏幕加载场景
    /// </summary>
    private IEnumerator LoadSceneWithLoadingScreen(SceneInfo targetScene)
    {
        isLoading = true;
        loadingProgress = 0f;
        
        // 播放过渡音效
        if (playTransitionSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(transitionSoundName);
        }
        
        // 淡出当前场景
        yield return StartCoroutine(FadeOut());
        
        // 触发加载开始事件
        OnSceneLoadStarted?.Invoke(targetScene.sceneName);
        
        // 加载加载场景
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync(loadingSceneName);
        yield return loadingSceneOp;
        
        // 淡入加载场景
        yield return StartCoroutine(FadeIn());
        
        // 等待最小加载时间
        float startTime = Time.time;
        
        // 预加载目标场景
        currentLoadOperation = SceneManager.LoadSceneAsync(targetScene.sceneName);
        currentLoadOperation.allowSceneActivation = false;
        
        // 等待加载完成或最小时间
        while (currentLoadOperation.progress < 0.9f || (Time.time - startTime) < minLoadingTime)
        {
            loadingProgress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            OnSceneLoadProgress?.Invoke(targetScene.sceneName, loadingProgress);
            yield return null;
        }
        
        // 等待额外延迟
        if (targetScene.loadingDelay > 0)
        {
            yield return new WaitForSeconds(targetScene.loadingDelay);
        }
        
        // 淡出加载场景
        yield return StartCoroutine(FadeOut());
        
        // 激活目标场景
        currentLoadOperation.allowSceneActivation = true;
        yield return currentLoadOperation;
        
        // 更新当前场景信息
        UpdateCurrentSceneInfo(targetScene);
        
        // 淡入目标场景
        yield return StartCoroutine(FadeIn());
        
        // 完成加载
        CompleteSceneLoad(targetScene.sceneName);
    }
    
    /// <summary>
    /// 直接加载场景
    /// </summary>
    private IEnumerator LoadSceneDirectly(SceneInfo targetScene)
    {
        isLoading = true;
        loadingProgress = 0f;
        
        // 播放过渡音效
        if (playTransitionSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(transitionSoundName);
        }
        
        // 淡出当前场景
        yield return StartCoroutine(FadeOut());
        
        // 触发加载开始事件
        OnSceneLoadStarted?.Invoke(targetScene.sceneName);
        
        // 等待额外延迟
        if (targetScene.loadingDelay > 0)
        {
            yield return new WaitForSeconds(targetScene.loadingDelay);
        }
        
        // 加载场景
        if (targetScene.isAdditive)
        {
            currentLoadOperation = SceneManager.LoadSceneAsync(targetScene.sceneName, LoadSceneMode.Additive);
        }
        else
        {
            currentLoadOperation = SceneManager.LoadSceneAsync(targetScene.sceneName);
        }
        
        // 等待加载完成
        while (!currentLoadOperation.isDone)
        {
            loadingProgress = currentLoadOperation.progress;
            OnSceneLoadProgress?.Invoke(targetScene.sceneName, loadingProgress);
            yield return null;
        }
        
        // 更新当前场景信息
        if (!targetScene.isAdditive)
        {
            UpdateCurrentSceneInfo(targetScene);
        }
        
        // 淡入新场景
        yield return StartCoroutine(FadeIn());
        
        // 完成加载
        CompleteSceneLoad(targetScene.sceneName);
    }
    
    /// <summary>
    /// 完成场景加载
    /// </summary>
    private void CompleteSceneLoad(string sceneName)
    {
        isLoading = false;
        loadingProgress = 1f;
        currentLoadOperation = null;
        
        // 触发加载完成事件
        OnSceneLoadCompleted?.Invoke(sceneName);
        
        if (debugMode)
        {
            Debug.Log($"[SceneController] 场景加载完成: {sceneName}");
        }
    }
    
    /// <summary>
    /// 卸载附加场景
    /// </summary>
    public void UnloadAdditiveScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            StartCoroutine(UnloadSceneCoroutine(sceneName));
        }
    }
    
    /// <summary>
    /// 卸载场景协程
    /// </summary>
    private IEnumerator UnloadSceneCoroutine(string sceneName)
    {
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
        yield return unloadOp;
        
        if (debugMode)
        {
            Debug.Log($"[SceneController] 场景卸载完成: {sceneName}");
        }
    }
    
    /// <summary>
    /// 返回上一个场景
    /// </summary>
    public void GoBack()
    {
        if (sceneHistory.Count > 0)
        {
            string previousScene = sceneHistory.Pop();
            LoadScene(previousScene, false);
        }
        else
        {
            Debug.LogWarning("[SceneController] 没有可返回的场景");
        }
    }
    
    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            LoadScene(currentSceneName, false);
        }
    }
    
    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;
        
        fadePanel.SetActive(true);
        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                canvasGroup.alpha = fadeCurve.Evaluate(t);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;
        
        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                canvasGroup.alpha = 1f - fadeCurve.Evaluate(t);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        
        fadePanel.SetActive(false);
    }
    
    /// <summary>
    /// 更新加载进度
    /// </summary>
    private void UpdateLoadingProgress()
    {
        if (currentLoadOperation != null && showLoadingProgress)
        {
            loadingProgress = currentLoadOperation.progress;
            OnSceneLoadProgress?.Invoke(currentSceneName, loadingProgress);
        }
    }
    
    /// <summary>
    /// 处理调试输入
    /// </summary>
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadSceneByType(SceneType.MainMenu);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadSceneByType(SceneType.GameWorld);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadSceneByType(SceneType.Battle);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GoBack();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ReloadCurrentScene();
        }
    }
    
    /// <summary>
    /// 获取场景信息
    /// </summary>
    private SceneInfo GetSceneInfo(string sceneName)
    {
        foreach (var info in sceneInfos)
        {
            if (info.sceneName == sceneName)
            {
                return info;
            }
        }
        return new SceneInfo(); // 返回空结构
    }
    
    /// <summary>
    /// 通过类型获取场景信息
    /// </summary>
    private SceneInfo GetSceneInfoByType(SceneType sceneType)
    {
        foreach (var info in sceneInfos)
        {
            if (info.sceneType == sceneType)
            {
                return info;
            }
        }
        return new SceneInfo(); // 返回空结构
    }
    
    /// <summary>
    /// 更新当前场景信息
    /// </summary>
    private void UpdateCurrentSceneInfo(SceneInfo? newSceneInfo = null)
    {
        if (newSceneInfo.HasValue)
        {
            currentSceneInfo = newSceneInfo.Value;
            currentSceneName = currentSceneInfo.sceneName;
            currentSceneType = currentSceneInfo.sceneType;
        }
        else
        {
            Scene activeScene = SceneManager.GetActiveScene();
            currentSceneName = activeScene.name;
            currentSceneInfo = GetSceneInfo(currentSceneName);
            currentSceneType = currentSceneInfo.sceneType;
        }
    }
    
    /// <summary>
    /// 添加到历史记录
    /// </summary>
    private void AddToHistory(string sceneName)
    {
        if (sceneHistory.Count >= maxHistorySize)
        {
            // 移除最旧的记录
            var tempStack = new Stack<string>();
            for (int i = 0; i < maxHistorySize - 1; i++)
            {
                if (sceneHistory.Count > 0)
                {
                    tempStack.Push(sceneHistory.Pop());
                }
            }
            sceneHistory.Clear();
            while (tempStack.Count > 0)
            {
                sceneHistory.Push(tempStack.Pop());
            }
        }
        
        sceneHistory.Push(sceneName);
    }
    
    /// <summary>
    /// 场景加载完成回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string previousScene = currentSceneName;
        UpdateCurrentSceneInfo();
        
        // 触发场景切换事件
        OnSceneChanged?.Invoke(previousScene, scene.name);
        
        if (debugMode)
        {
            Debug.Log($"[SceneController] 场景已加载: {scene.name} (模式: {mode})");
        }
    }
    
    /// <summary>
    /// 场景卸载完成回调
    /// </summary>
    private void OnSceneUnloaded(Scene scene)
    {
        if (debugMode)
        {
            Debug.Log($"[SceneController] 场景已卸载: {scene.name}");
        }
    }
    
    /// <summary>
    /// 设置场景数据
    /// </summary>
    public void SetSceneData(string key, object value)
    {
        sceneData[key] = value;
    }
    
    /// <summary>
    /// 获取场景数据
    /// </summary>
    public T GetSceneData<T>(string key, T defaultValue = default(T))
    {
        if (sceneData.ContainsKey(key) && sceneData[key] is T)
        {
            return (T)sceneData[key];
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 清除场景数据
    /// </summary>
    public void ClearSceneData()
    {
        sceneData.Clear();
    }
    
    /// <summary>
    /// 获取当前场景名称
    /// </summary>
    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }
    
    /// <summary>
    /// 获取当前场景类型
    /// </summary>
    public SceneType GetCurrentSceneType()
    {
        return currentSceneType;
    }
    
    /// <summary>
    /// 获取加载进度
    /// </summary>
    public float GetLoadingProgress()
    {
        return loadingProgress;
    }
    
    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading()
    {
        return isLoading;
    }
    
    /// <summary>
    /// 获取场景历史数量
    /// </summary>
    public int GetHistoryCount()
    {
        return sceneHistory.Count;
    }
    
    private void OnDestroy()
    {
        // 取消监听场景事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}