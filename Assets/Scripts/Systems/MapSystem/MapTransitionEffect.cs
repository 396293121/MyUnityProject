using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 地图切换特效管理器 - 处理场景切换时的视觉效果
/// 支持多种切换效果：淡入淡出、滑动、缩放等
/// </summary>
public class MapTransitionEffect : MonoBehaviour
{
    #region 单例模式
    private static MapTransitionEffect _instance;
    public static MapTransitionEffect Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MapTransitionEffect>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MapTransitionEffect");
                    _instance = go.AddComponent<MapTransitionEffect>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 配置
    [TitleGroup("切换特效配置")]

    [FoldoutGroup("切换特效配置/基础设置")]
    [LabelText("默认切换时长")]
    [Range(0.1f, 5f)]
    public float defaultTransitionDuration = 1f;

    [FoldoutGroup("切换特效配置/基础设置")]
    [LabelText("启用音效")]
    public bool enableSoundEffects = true;

    [FoldoutGroup("切换特效配置/UI设置", expanded: false)]
    [LabelText("Canvas预制体")]
    [AssetsOnly]
    public GameObject canvasPrefab;

    [FoldoutGroup("切换特效配置/UI设置")]
    [LabelText("淡入淡出面板")]
    [AssetsOnly]
    public GameObject fadePanelPrefab;

    [FoldoutGroup("切换特效配置/UI设置")]
    [LabelText("加载界面预制体")]
    [AssetsOnly]
    public GameObject loadingScreenPrefab;
    #endregion

    #region 音效配置
    [TitleGroup("音效配置")]
    [FoldoutGroup("音效配置/切换音效", expanded: false)]
    [LabelText("淡入淡出音效")]
    [AssetsOnly]
    public AudioClip fadeTransitionSound;

    [FoldoutGroup("音效配置/切换音效")]
    [LabelText("滑动切换音效")]
    [AssetsOnly]
    public AudioClip slideTransitionSound;

    [FoldoutGroup("音效配置/切换音效")]
    [LabelText("缩放切换音效")]
    [AssetsOnly]
    public AudioClip scaleTransitionSound;

    [FoldoutGroup("音效配置/切换音效")]
    [LabelText("传送音效")]
    [AssetsOnly]
    public AudioClip teleportSound;

    [FoldoutGroup("音效配置/切换音效")]
    [LabelText("音效音量")]
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    #endregion

    #region 运行时状态
    [TitleGroup("运行时状态")]
    [FoldoutGroup("运行时状态/状态信息", expanded: false)]
    [LabelText("正在切换")]
    [ReadOnly]
    [ShowInInspector]
    private bool isTransitioning = false;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("UI Canvas")]
    [ReadOnly]
    [ShowInInspector]
    private Canvas uiCanvas;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("淡入淡出面板")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject fadePanel;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("加载界面")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject loadingScreen;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("音频源")]
    [ReadOnly]
    [ShowInInspector]
    private AudioSource audioSource;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        InitializeSingleton();
        InitializeComponents();
    }

    private void Start()
    {
        SetupTransitionUI();
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化单例
    /// </summary>
    private void InitializeSingleton()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取或添加音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        Debug.Log("[MapTransitionEffect] 组件初始化完成");
    }

    /// <summary>
    /// 设置切换UI
    /// </summary>
    private void SetupTransitionUI()
    {
        // 创建UI Canvas
        if (canvasPrefab != null && uiCanvas == null)
        {
            GameObject canvasGO = Instantiate(canvasPrefab);
            uiCanvas = canvasGO.GetComponent<Canvas>();
            if (uiCanvas != null)
            {
                uiCanvas.sortingOrder = 1000; // 确保在最上层
                DontDestroyOnLoad(canvasGO);
            }
        }

        // 创建淡入淡出面板
        if (fadePanelPrefab != null && fadePanel == null)
        {
            fadePanel = Instantiate(fadePanelPrefab, uiCanvas != null ? uiCanvas.transform : transform);
            fadePanel.SetActive(false);
        }

        // 创建加载界面
        if (loadingScreenPrefab != null && loadingScreen == null)
        {
            loadingScreen = Instantiate(loadingScreenPrefab, uiCanvas != null ? uiCanvas.transform : transform);
            loadingScreen.SetActive(false);
        }

        Debug.Log("[MapTransitionEffect] 切换UI设置完成");
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 开始切换特效
    /// </summary>
    public void StartTransition( float duration = -1f, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MapTransitionEffect] 正在进行切换，请稍后再试");
            return;
        }

        if (duration < 0)
            duration = defaultTransitionDuration;
        StartCoroutine(ExecuteTransition( duration, onComplete));
    }

    /// <summary>
    /// 开始切换特效（使用默认设置）
    /// </summary>
    public void StartTransition(System.Action onComplete = null)
    {
        StartTransition(defaultTransitionDuration, onComplete);
    }

    /// <summary>
    /// 立即完成当前切换
    /// </summary>
    public void CompleteTransition()
    {
        if (isTransitioning)
        {
            StopAllCoroutines();
            isTransitioning = false;
            HideAllTransitionUI();
        }
    }

    /// <summary>
    /// 检查是否正在切换
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    #endregion

    #region 切换执行
    /// <summary>
    /// 执行切换特效
    /// </summary>
    private IEnumerator ExecuteTransition(float duration, System.Action onComplete)
    {
        isTransitioning = true;
        Debug.Log($"[MapTransitionEffect] 开始执行切换特效: , 时长: {duration}s");

        // 播放音效
        PlayTransitionSound();

        yield return StartCoroutine(ExecuteFadeTransition(duration));

        isTransitioning = false;
        onComplete?.Invoke();
        
        Debug.Log($"[MapTransitionEffect] 切换特效完成:");
    }

    /// <summary>
    /// 执行淡入淡出切换
    /// </summary>
    private IEnumerator ExecuteFadeTransition(float duration)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 淡入淡出面板未设置");
            yield break;
        }

        Image fadeImage = fadePanel.GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 淡入淡出面板缺少Image组件");
            yield break;
        }

        fadePanel.SetActive(true);
        
        float halfDuration = duration * 0.5f;

        // 淡出（变黑）
        yield return StartCoroutine(FadeImage(fadeImage, 0f, 1f, halfDuration));
        
        // 等待一小段时间
        yield return new WaitForSeconds(0.1f);
        
        // 淡入（变透明）
        yield return StartCoroutine(FadeImage(fadeImage, 1f, 0f, halfDuration));
        
        fadePanel.SetActive(false);
    }

    /// <summary>
    /// 执行滑动切换
    /// </summary>
    private IEnumerator ExecuteSlideTransition(float duration)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 滑动面板未设置");
            yield break;
        }

        RectTransform panelRect = fadePanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 滑动面板缺少RectTransform组件");
            yield break;
        }

        fadePanel.SetActive(true);
        
        float screenWidth = Screen.width;
        Vector3 startPos = new Vector3(-screenWidth, 0, 0);
        Vector3 centerPos = Vector3.zero;
        Vector3 endPos = new Vector3(screenWidth, 0, 0);
        
        float halfDuration = duration * 0.5f;

        // 从左侧滑入
        panelRect.anchoredPosition = startPos;
        yield return StartCoroutine(MovePanel(panelRect, startPos, centerPos, halfDuration));
        
        // 等待一小段时间
        yield return new WaitForSeconds(0.1f);
        
        // 向右侧滑出
        yield return StartCoroutine(MovePanel(panelRect, centerPos, endPos, halfDuration));
        
        fadePanel.SetActive(false);
    }

    /// <summary>
    /// 执行缩放切换
    /// </summary>
    private IEnumerator ExecuteScaleTransition(float duration)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 缩放面板未设置");
            yield break;
        }

        Transform panelTransform = fadePanel.transform;
        fadePanel.SetActive(true);
        
        float halfDuration = duration * 0.5f;

        // 从小到大
        yield return StartCoroutine(ScalePanel(panelTransform, Vector3.zero, Vector3.one, halfDuration));
        
        // 等待一小段时间
        yield return new WaitForSeconds(0.1f);
        
        // 从大到小
        yield return StartCoroutine(ScalePanel(panelTransform, Vector3.one, Vector3.zero, halfDuration));
        
        fadePanel.SetActive(false);
    }

    /// <summary>
    /// 执行传送切换
    /// </summary>
    private IEnumerator ExecuteTeleportTransition(float duration)
    {
        // 传送效果：快速闪烁
        if (fadePanel == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 传送面板未设置");
            yield break;
        }

        Image fadeImage = fadePanel.GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 传送面板缺少Image组件");
            yield break;
        }

        fadePanel.SetActive(true);
        
        // 快速闪烁效果
        int flashCount = 3;
        float flashDuration = duration / (flashCount * 2);
        
        for (int i = 0; i < flashCount; i++)
        {
            // 闪白
            fadeImage.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            
            // 变透明
            fadeImage.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(flashDuration);
        }
        
        fadePanel.SetActive(false);
    }

    /// <summary>
    /// 执行加载切换
    /// </summary>
    private IEnumerator ExecuteLoadingTransition(float duration)
    {
        if (loadingScreen == null)
        {
            Debug.LogWarning("[MapTransitionEffect] 加载界面未设置");
            yield break;
        }

        loadingScreen.SetActive(true);
        
        // 显示加载界面指定时间
        yield return new WaitForSeconds(duration);
        
        loadingScreen.SetActive(false);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 淡入淡出图片
    /// </summary>
    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = image.color;
        startColor.a = startAlpha;
        Color endColor = startColor;
        endColor.a = endAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        image.color = endColor;
    }

    /// <summary>
    /// 移动面板
    /// </summary>
    private IEnumerator MovePanel(RectTransform panelRect, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            panelRect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        panelRect.anchoredPosition = endPos;
    }

    /// <summary>
    /// 缩放面板
    /// </summary>
    private IEnumerator ScalePanel(Transform panelTransform, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            panelTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        panelTransform.localScale = endScale;
    }

    /// <summary>
    /// 播放切换音效
    /// </summary>
    private void PlayTransitionSound()
    {
        if (!enableSoundEffects || audioSource == null)
            return;

        AudioClip soundToPlay = null;

          soundToPlay = fadeTransitionSound;
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay, soundVolume);
        }
    }

    /// <summary>
    /// 隐藏所有切换UI
    /// </summary>
    private void HideAllTransitionUI()
    {
        if (fadePanel != null)
            fadePanel.SetActive(false);
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
    #endregion

    #region 调试工具
    [TitleGroup("调试工具")]
    [FoldoutGroup("调试工具/测试操作", expanded: false)]
    [Button("测试淡入淡出", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void TestFadeTransition()
    {
        if (Application.isPlaying)
        {
            StartTransition(2f, () => Debug.Log("淡入淡出测试完成"));
        }
    }

 

    [FoldoutGroup("调试工具/调试操作")]
    [Button("输出状态信息", ButtonSizes.Medium)]
    [GUIColor(0.9f, 0.9f, 0.9f)]
    public void LogStateInfo()
    {
        Debug.Log($"[MapTransitionEffect] 状态信息:");
        Debug.Log($"  正在切换: {isTransitioning}");
        Debug.Log($"  默认切换时长: {defaultTransitionDuration}s");
        Debug.Log($"  启用音效: {enableSoundEffects}");
        Debug.Log($"  UI Canvas: {(uiCanvas != null ? "已设置" : "未设置")}");
        Debug.Log($"  淡入淡出面板: {(fadePanel != null ? "已设置" : "未设置")}");
        Debug.Log($"  加载界面: {(loadingScreen != null ? "已设置" : "未设置")}");
    }
    #endregion
}