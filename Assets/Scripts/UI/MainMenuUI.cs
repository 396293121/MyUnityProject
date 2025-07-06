using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单UI控制器
/// 从Phaser项目的MainMenuScene.js迁移而来
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Button startGameButton;
    public Button settingsButton;
    public Button exitGameButton;
    public Image backgroundImage;
    public Image logoImage;
    
    [Header("配置设置")]
    public string uiConfigName = "MainMenuUI";
    
    // 从配置系统加载的配置
    private UIConfigData uiConfig;
    
    // 音频和动画设置（从配置加载）
    private bool playBackgroundMusic = true;
    private bool playButtonSounds = true;
    private float fadeInTime = 1.0f;
    private float buttonHoverScale = 1.1f;
    private float buttonClickScale = 0.95f;
    
    private AudioSource audioSource;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeMainMenu();
    }
    
    /// <summary>
    /// 初始化主菜单
    /// </summary>
    void InitializeMainMenu()
    {
        if (isInitialized) return;
        
        // 从配置管理器加载UI配置
        LoadUIConfig();
        
        // 设置按钮事件
        SetupButtonEvents();
        
        // 播放背景音乐
        if (playBackgroundMusic)
        {
            PlayBackgroundMusic();
        }
        
        // 设置游戏状态
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.MainMenu);
        }
        
        // 淡入效果
        StartCoroutine(FadeInMenu());
        
        isInitialized = true;
        
        Debug.Log("[MainMenuUI] 主菜单初始化完成");
    }
    
    /// <summary>
    /// 从配置管理器加载UI配置
    /// </summary>
    private void LoadUIConfig()
    {
        if (ConfigManager.Instance != null)
        {
            uiConfig = ConfigManager.Instance.GetUIConfig(uiConfigName);
            
            if (uiConfig != null)
            {
                // 应用配置值
                fadeInTime = uiConfig.fadeInTime;
                buttonHoverScale = uiConfig.buttonHoverScale;
                buttonClickScale = uiConfig.buttonClickScale;
                playBackgroundMusic = uiConfig.playBackgroundMusic;
                playButtonSounds = uiConfig.playButtonSounds;
                
                Debug.Log($"[MainMenuUI] 已加载UI配置: {uiConfigName}");
            }
            else
            {
                Debug.LogWarning($"[MainMenuUI] 未找到UI配置: {uiConfigName}，使用默认值");
            }
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] ConfigManager实例不存在，使用默认值");
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    void SetupButtonEvents()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            AddButtonEffects(startGameButton);
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonEffects(settingsButton);
        }
        
        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(OnExitGameClicked);
            AddButtonEffects(exitGameButton);
        }
    }
    
    /// <summary>
    /// 添加按钮效果
    /// </summary>
    void AddButtonEffects(Button button)
    {
        // 添加按钮悬停和点击效果
        var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 鼠标进入事件
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one * buttonHoverScale;
            PlayButtonSound("button_select");
        });
        eventTrigger.triggers.Add(pointerEnter);
        
        // 鼠标离开事件
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        eventTrigger.triggers.Add(pointerExit);
        
        // 鼠标按下事件
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one * buttonClickScale;
        });
        eventTrigger.triggers.Add(pointerDown);
        
        // 鼠标抬起事件
        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one * buttonHoverScale;
        });
        eventTrigger.triggers.Add(pointerUp);
    }
    
    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    public void OnStartGameClicked()
    {
        PlayButtonSound("button_click");
        
        Debug.Log("[MainMenuUI] 开始游戏");
        
        // 停止背景音乐
        StopBackgroundMusic();
        
        // 加载下一个场景（默认为角色选择场景）
        string nextScene = "CharacterSelectScene";
        if (uiConfig != null && !string.IsNullOrEmpty(uiConfig.nextScene))
        {
            nextScene = uiConfig.nextScene;
        }
        LoadScene(nextScene);
    }
    
    /// <summary>
    /// 设置按钮点击事件
    /// </summary>
    public void OnSettingsClicked()
    {
        PlayButtonSound("button_click");
        
        Debug.Log("[MainMenuUI] 打开设置");
        
        // 显示设置面板
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanel("Settings");
        }
    }
    
    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
   public void OnExitGameClicked()
    {
        PlayButtonSound("button_click");
        
        Debug.Log("[MainMenuUI] 退出游戏");
        
        // 显示确认对话框
        if (UIManager.Instance != null)
        {
            // 使用配置中的对话框文本，如果没有则使用默认文本
            string dialogText = "确定要退出游戏吗？";
            if (uiConfig != null && uiConfig.dialogTexts != null && uiConfig.dialogTexts.ContainsKey("ExitConfirm"))
            {
                dialogText = uiConfig.dialogTexts["ExitConfirm"];
            }
            
            UIManager.Instance.ShowConfirmDialog(
                dialogText,
                () => {
                    // 确认退出
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.QuitGame();
                    }
                    else
                    {
                        Application.Quit();
                    }
                },
                null // 取消不做任何操作
            );
        }
        else
        {
            // 直接退出
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Application.Quit();
            }
        }
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    void PlayBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            // 获取背景音乐名称，如果配置中有指定则使用配置中的名称
            string musicKey = "menu_music";
            if (uiConfig != null && !string.IsNullOrEmpty(uiConfig.backgroundMusicKey))
            {
                musicKey = uiConfig.backgroundMusicKey;
            }
            
            var audioConfig = ConfigManager.Instance?.GetAudioConfig(musicKey);
            if (audioConfig != null)
            {
                AudioManager.Instance.PlayMusic(musicKey, audioConfig.volume, audioConfig.loop);
            }
            else
            {
                AudioManager.Instance.PlayMusic(musicKey, 0.6f, true);
            }
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    void StopBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
    
    /// <summary>
    /// 播放按钮音效
    /// </summary>
    void PlayButtonSound(string soundKey)
    {
        if (!playButtonSounds) return;
        
        if (AudioManager.Instance != null)
        {
            // 如果配置中有按钮音效映射，则使用映射后的音效键
            string actualSoundKey = soundKey;
            if (uiConfig != null && uiConfig.buttonSoundMappings != null && uiConfig.buttonSoundMappings.ContainsKey(soundKey))
            {
                actualSoundKey = uiConfig.buttonSoundMappings[soundKey];
            }
            
            var audioConfig = ConfigManager.Instance?.GetAudioConfig(actualSoundKey);
            if (audioConfig != null)
            {
                AudioManager.Instance.PlaySFX(actualSoundKey, audioConfig.volume);
            }
            else
            {
                AudioManager.Instance.PlaySFX(actualSoundKey, 0.6f);
            }
        }
    }
    
    /// <summary>
    /// 加载场景
    /// </summary>
    void LoadScene(string sceneName)
    {
        // 如果配置中指定了下一个场景，则使用配置中的场景名称
        string nextScene = sceneName;
        if (uiConfig != null && !string.IsNullOrEmpty(uiConfig.nextScene))
        {
            nextScene = uiConfig.nextScene;
        }
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }
    
    /// <summary>
    /// 淡入菜单效果
    /// </summary>
    System.Collections.IEnumerator FadeInMenu()
    {
        // 设置初始透明度
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        // 获取淡入时间，如果配置中有指定则使用配置中的值
        float actualFadeInTime = fadeInTime;
        
        // 淡入效果
        float elapsedTime = 0f;
        while (elapsedTime < actualFadeInTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / actualFadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    void OnDestroy()
    {
        // 清理事件监听
        if (startGameButton != null) startGameButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (exitGameButton != null) exitGameButton.onClick.RemoveAllListeners();
    }
}