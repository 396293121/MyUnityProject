using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 测试场景UI管理器
/// 基于Phaser项目中的UI系统，管理HUD、菜单等界面元素
/// </summary>
public class TestSceneUIManager : MonoBehaviour
{
    #region 配置引用
    [Header("配置")]
    [SerializeField] private HUDConfig hudConfig;
    [SerializeField] private TestSceneConfig sceneConfig;
    #endregion
    
    #region UI引用
    [Header("HUD元素")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    
    [Header("技能栏")]
    [SerializeField] private Transform skillBarParent;
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private Image[] skillIcons;
    [SerializeField] private TextMeshProUGUI[] skillCooldownTexts;
    
    [Header("小地图")]
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Transform minimapPlayerIcon;
    [SerializeField] private Transform minimapEnemyParent;
    
    [Header("菜单界面")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("通知系统")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Queue<string> notificationQueue = new Queue<string>();
    
    [Header("调试UI")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private Toggle debugToggle;
    #endregion
    
    #region 私有字段
    private TestSceneEventBus eventBus;
    private Character playerCharacter;
    private Camera minimapCamera;
    
    // UI状态
    private bool isHUDVisible = true;
    private bool isPauseMenuOpen = false;
    private bool isDebugUIVisible = false;
    
    // 动画相关
    private Coroutine notificationCoroutine;
    private Coroutine healthBarAnimationCoroutine;
    private Coroutine manaBarAnimationCoroutine;
    private Coroutine experienceBarAnimationCoroutine;
    
    // 技能冷却
    private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();
    private Dictionary<int, float> skillMaxCooldowns = new Dictionary<int, float>();
    
    // 小地图
    private List<Transform> minimapEnemyIcons = new List<Transform>();
    private RenderTexture minimapRenderTexture;
    
    // 调试信息
    private float debugUpdateTimer = 0f;
    private int frameCount = 0;
    private float fps = 0f;
    #endregion
    
    #region Unity生命周期
    private void Awake()
    {
        // 初始化UI组件
        InitializeUIComponents();
    }
    
    private void Start()
    {
        // 设置初始UI状态
        SetupInitialUIState();
    }
    
    private void Update()
    {
        // 更新技能冷却
        UpdateSkillCooldowns();
        
        // 更新小地图
        UpdateMinimap();
        
        // 更新调试信息
        UpdateDebugInfo();
        
        // 处理通知队列
        ProcessNotificationQueue();
    }
    
    private void OnDestroy()
    {
        // 清理资源
        CleanupResources();
    }
    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    public void Initialize(HUDConfig config, TestSceneEventBus eventSystem)
    {
        hudConfig = config;
        eventBus = eventSystem;
        
        // 注册事件监听器
        RegisterEventListeners();
        
        // 设置UI配置
        ApplyHUDConfiguration();
        
        Debug.Log("[TestSceneUIManager] UI管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUIComponents()
    {
        // 确保所有必要的UI组件都已分配
        if (hudCanvas == null)
            hudCanvas = GetComponentInChildren<Canvas>();
        
        // 初始化技能按钮事件
        SetupSkillButtons();
        
        // 初始化菜单按钮事件
        SetupMenuButtons();
        
        // 初始化调试UI
        SetupDebugUI();
        
        // 创建小地图渲染纹理
        SetupMinimap();
    }
    
    /// <summary>
    /// 设置初始UI状态
    /// </summary>
    private void SetupInitialUIState()
    {
        // 显示HUD
        SetHUDVisible(true);
        
        // 隐藏暂停菜单
        SetPauseMenuVisible(false);
        
        // 隐藏调试UI
        SetDebugUIVisible(false);
        
        // 隐藏通知面板
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }
    
    /// <summary>
    /// 应用HUD配置
    /// </summary>
    private void ApplyHUDConfiguration()
    {
        if (hudConfig == null) return;
        
        // 应用生命值配置
        if (healthBar != null && hudConfig.healthBarConfig != null)
        {
            healthBar.fillRect.GetComponent<Image>().color = hudConfig.healthBarConfig.fillColor;
        }
        
        // 应用魔法值配置
        if (manaBar != null && hudConfig.manaBarConfig != null)
        {
            manaBar.fillRect.GetComponent<Image>().color = hudConfig.manaBarConfig.fillColor;
        }
        
        // 应用经验值配置
        if (experienceBar != null && hudConfig.experienceBarConfig != null)
        {
            experienceBar.fillRect.GetComponent<Image>().color = hudConfig.experienceBarConfig.fillColor;
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
        
        // 玩家事件
        eventBus.OnPlayerHealthChanged += UpdateHealthBar;
        eventBus.OnPlayerManaChanged += UpdateManaBar;
        eventBus.OnPlayerLevelUp += UpdateLevel;
        eventBus.OnPlayerExperienceGained += UpdateExperience;
        eventBus.OnPlayerDeath += OnPlayerDeath;
        eventBus.OnPlayerSkillUsed += OnSkillUsed;
        
        // 敌人事件
        eventBus.OnEnemySpawned += OnEnemySpawned;
        eventBus.OnEnemyDeath += OnEnemyDeath;
        eventBus.OnAllEnemiesDefeated += OnAllEnemiesDefeated;
        
        // UI事件
        eventBus.OnGamePaused += OnGamePaused;
        eventBus.OnUINotification += ShowNotification;
        
        Debug.Log("[TestSceneUIManager] 事件监听器注册完成");
    }
    
    /// <summary>
    /// 注销事件监听器
    /// </summary>
    private void UnregisterEventListeners()
    {
        if (eventBus == null) return;
        
        // 玩家事件
        eventBus.OnPlayerHealthChanged -= UpdateHealthBar;
        eventBus.OnPlayerManaChanged -= UpdateManaBar;
        eventBus.OnPlayerLevelUp -= UpdateLevel;
        eventBus.OnPlayerExperienceGained -= UpdateExperience;
        eventBus.OnPlayerDeath -= OnPlayerDeath;
        eventBus.OnPlayerSkillUsed -= OnSkillUsed;
        
        // 敌人事件
        eventBus.OnEnemySpawned -= OnEnemySpawned;
        eventBus.OnEnemyDeath -= OnEnemyDeath;
        eventBus.OnAllEnemiesDefeated -= OnAllEnemiesDefeated;
        
        // UI事件
        eventBus.OnGamePaused -= OnGamePaused;
        eventBus.OnUINotification -= ShowNotification;
    }
    #endregion
    
    #region HUD更新方法
    /// <summary>
    /// 设置玩家角色引用
    /// </summary>
    public void SetPlayerCharacter(Character character)
    {
        playerCharacter = character;
        
        if (playerCharacter != null)
        {
            // 初始化HUD显示
            UpdateHealthBar(playerCharacter.maxHealth, playerCharacter.currentHealth);
            UpdateManaBar(playerCharacter.maxMana, playerCharacter.currentMana);
            UpdateLevel(playerCharacter.level);
            UpdateExperience(playerCharacter.experience);
        }
    }
    
    /// <summary>
    /// 更新生命值条
    /// </summary>
    private void UpdateHealthBar(float oldHealth, float newHealth)
    {
        if (healthBar == null || playerCharacter == null) return;
        
        float healthPercentage = newHealth / playerCharacter.maxHealth;
        
        // 停止之前的动画
        if (healthBarAnimationCoroutine != null)
        {
            StopCoroutine(healthBarAnimationCoroutine);
        }
        
        // 开始新的动画
        healthBarAnimationCoroutine = StartCoroutine(AnimateBar(healthBar, healthPercentage, hudConfig?.healthBarConfig?.animationDuration ?? 0.3f));
        
        // 更新文本
        if (healthText != null)
        {
            healthText.text = $"{(int)newHealth}/{(int)playerCharacter.maxHealth}";
        }
    }
    
    /// <summary>
    /// 更新魔法值条
    /// </summary>
    private void UpdateManaBar(float oldMana, float newMana)
    {
        if (manaBar == null || playerCharacter == null) return;
        
        float manaPercentage = newMana / playerCharacter.maxMana;
        
        // 停止之前的动画
        if (manaBarAnimationCoroutine != null)
        {
            StopCoroutine(manaBarAnimationCoroutine);
        }
        
        // 开始新的动画
        manaBarAnimationCoroutine = StartCoroutine(AnimateBar(manaBar, manaPercentage, hudConfig?.manaBarConfig?.animationDuration ?? 0.3f));
        
        // 更新文本
        if (manaText != null)
        {
            manaText.text = $"{(int)newMana}/{(int)playerCharacter.maxMana}";
        }
    }
    
    /// <summary>
    /// 更新等级显示
    /// </summary>
    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }
    }
    
    /// <summary>
    /// 更新经验值显示
    /// </summary>
    private void UpdateExperience(int experience)
    {
        if (experienceBar == null || playerCharacter == null) return;
        
        int expForNextLevel = playerCharacter.GetExperienceForNextLevel();
        int currentLevelExp = playerCharacter.GetCurrentLevelExperience();
        
        float expPercentage = (float)currentLevelExp / expForNextLevel;
        
        // 停止之前的动画
        if (experienceBarAnimationCoroutine != null)
        {
            StopCoroutine(experienceBarAnimationCoroutine);
        }
        
        // 开始新的动画
        experienceBarAnimationCoroutine = StartCoroutine(AnimateBar(experienceBar, expPercentage, hudConfig?.experienceBarConfig?.animationDuration ?? 0.3f));
        
        // 更新文本
        if (experienceText != null)
        {
            experienceText.text = $"{currentLevelExp}/{expForNextLevel}";
        }
    }
    
    /// <summary>
    /// 进度条动画
    /// </summary>
    private IEnumerator AnimateBar(Slider bar, float targetValue, float duration)
    {
        float startValue = bar.value;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            bar.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        
        bar.value = targetValue;
    }
    #endregion
    
    #region 技能栏管理
    /// <summary>
    /// 设置技能按钮
    /// </summary>
    private void SetupSkillButtons()
    {
        if (skillButtons == null) return;
        
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int skillIndex = i; // 闭包变量
            if (skillButtons[i] != null)
            {
                skillButtons[i].onClick.AddListener(() => OnSkillButtonClicked(skillIndex));
            }
        }
    }
    
    /// <summary>
    /// 技能按钮点击事件
    /// </summary>
    private void OnSkillButtonClicked(int skillIndex)
    {
        // 检查冷却时间
        if (IsSkillOnCooldown(skillIndex))
        {
            ShowNotification($"技能 {skillIndex + 1} 冷却中");
            return;
        }
        
        // 触发技能使用事件
        eventBus?.TriggerEvent("SkillInput", skillIndex);
    }
    
    /// <summary>
    /// 技能使用事件处理
    /// </summary>
    private void OnSkillUsed(string skillName)
    {
        // 解析技能索引（简化处理）
        if (int.TryParse(skillName.Replace("skill_", ""), out int skillIndex))
        {
            StartSkillCooldown(skillIndex, 5f); // 默认5秒冷却
        }
    }
    
    /// <summary>
    /// 开始技能冷却
    /// </summary>
    private void StartSkillCooldown(int skillIndex, float cooldownTime)
    {
        skillCooldowns[skillIndex] = cooldownTime;
        skillMaxCooldowns[skillIndex] = cooldownTime;
        
        // 设置按钮不可交互
        if (skillIndex < skillButtons.Length && skillButtons[skillIndex] != null)
        {
            skillButtons[skillIndex].interactable = false;
        }
    }
    
    /// <summary>
    /// 检查技能是否在冷却中
    /// </summary>
    private bool IsSkillOnCooldown(int skillIndex)
    {
        return skillCooldowns.ContainsKey(skillIndex) && skillCooldowns[skillIndex] > 0f;
    }
    
    /// <summary>
    /// 更新技能冷却
    /// </summary>
    private void UpdateSkillCooldowns()
    {
        var keysToRemove = new List<int>();
        
        foreach (var kvp in new Dictionary<int, float>(skillCooldowns))
        {
            int skillIndex = kvp.Key;
            float cooldown = kvp.Value;
            
            cooldown -= Time.deltaTime;
            
            if (cooldown <= 0f)
            {
                // 冷却结束
                keysToRemove.Add(skillIndex);
                
                // 恢复按钮可交互
                if (skillIndex < skillButtons.Length && skillButtons[skillIndex] != null)
                {
                    skillButtons[skillIndex].interactable = true;
                }
                
                // 清除冷却文本
                if (skillIndex < skillCooldownTexts.Length && skillCooldownTexts[skillIndex] != null)
                {
                    skillCooldownTexts[skillIndex].text = "";
                }
            }
            else
            {
                // 更新冷却时间
                skillCooldowns[skillIndex] = cooldown;
                
                // 更新冷却文本
                if (skillIndex < skillCooldownTexts.Length && skillCooldownTexts[skillIndex] != null)
                {
                    skillCooldownTexts[skillIndex].text = Mathf.Ceil(cooldown).ToString();
                }
            }
        }
        
        // 移除已完成的冷却
        foreach (int key in keysToRemove)
        {
            skillCooldowns.Remove(key);
            skillMaxCooldowns.Remove(key);
        }
    }
    #endregion
    
    #region 小地图系统
    /// <summary>
    /// 设置小地图
    /// </summary>
    private void SetupMinimap()
    {
        if (minimapImage == null) return;
        
        // 创建渲染纹理
        minimapRenderTexture = new RenderTexture(256, 256, 16);
        minimapImage.texture = minimapRenderTexture;
        
        // 查找小地图摄像机
        minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera")?.GetComponent<Camera>();
        if (minimapCamera != null)
        {
            minimapCamera.targetTexture = minimapRenderTexture;
        }
    }
    
    /// <summary>
    /// 更新小地图
    /// </summary>
    private void UpdateMinimap()
    {
        if (minimapCamera == null || playerCharacter == null) return;
        
        // 更新小地图摄像机位置
        Vector3 playerPos = playerCharacter.transform.position;
        minimapCamera.transform.position = new Vector3(playerPos.x, minimapCamera.transform.position.y, playerPos.z);
        
        // 更新玩家图标位置
        if (minimapPlayerIcon != null)
        {
            minimapPlayerIcon.position = new Vector3(playerPos.x, minimapPlayerIcon.position.y, playerPos.z);
        }
    }
    
    /// <summary>
    /// 敌人生成事件处理
    /// </summary>
    private void OnEnemySpawned(Enemy enemy)
    {
        // 在小地图上添加敌人图标
        if (minimapEnemyParent != null && hudConfig?.minimapConfig?.enemyIconPrefab != null)
        {
            GameObject enemyIcon = Instantiate(hudConfig.minimapConfig.enemyIconPrefab, minimapEnemyParent);
            enemyIcon.transform.position = enemy.transform.position;
            minimapEnemyIcons.Add(enemyIcon.transform);
        }
    }
    
    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    private void OnEnemyDeath(Enemy enemy)
    {
        // 从小地图移除敌人图标
        for (int i = minimapEnemyIcons.Count - 1; i >= 0; i--)
        {
            if (minimapEnemyIcons[i] == null)
            {
                minimapEnemyIcons.RemoveAt(i);
            }
        }
    }
    #endregion
    
    #region 菜单系统
    /// <summary>
    /// 设置菜单按钮
    /// </summary>
    private void SetupMenuButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
    }
    
    /// <summary>
    /// 设置暂停菜单可见性
    /// </summary>
    public void SetPauseMenuVisible(bool visible)
    {
        isPauseMenuOpen = visible;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(visible);
        }
        
        // 暂停/恢复游戏
        Time.timeScale = visible ? 0f : 1f;
        
        // 触发暂停事件
        eventBus?.TriggerGamePaused(visible);
    }
    
    /// <summary>
    /// 切换暂停菜单
    /// </summary>
    public void TogglePauseMenu()
    {
        SetPauseMenuVisible(!isPauseMenuOpen);
    }
    
    /// <summary>
    /// 继续游戏按钮
    /// </summary>
    private void OnResumeButtonClicked()
    {
        SetPauseMenuVisible(false);
    }
    
    /// <summary>
    /// 设置按钮
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        // TODO: 打开设置界面
        ShowNotification("设置功能待实现");
    }
    
    /// <summary>
    /// 主菜单按钮
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        // 触发返回主菜单事件
        eventBus?.TriggerEvent("ReturnToMainMenu", null);
    }
    
    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    
    /// <summary>
    /// 游戏暂停事件处理
    /// </summary>
    private void OnGamePaused(bool isPaused)
    {
        // 可以在这里添加额外的暂停逻辑
    }
    #endregion
    
    #region 通知系统
    /// <summary>
    /// 显示通知
    /// </summary>
    public void ShowNotification(string message)
    {
        notificationQueue.Enqueue(message);
    }
    
    /// <summary>
    /// 处理通知队列
    /// </summary>
    private void ProcessNotificationQueue()
    {
        if (notificationQueue.Count > 0 && notificationCoroutine == null)
        {
            string message = notificationQueue.Dequeue();
            notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
        }
    }
    
    /// <summary>
    /// 显示通知协程
    /// </summary>
    private IEnumerator ShowNotificationCoroutine(string message)
    {
        if (notificationPanel == null || notificationText == null) yield break;
        
        // 设置通知文本
        notificationText.text = message;
        notificationPanel.SetActive(true);
        
        // 淡入动画
        yield return StartCoroutine(FadeUI(notificationPanel, 0f, 1f, 0.3f));
        
        // 显示时间
        yield return new WaitForSeconds(2f);
        
        // 淡出动画
        yield return StartCoroutine(FadeUI(notificationPanel, 1f, 0f, 0.3f));
        
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }
    
    /// <summary>
    /// UI淡入淡出动画
    /// </summary>
    private IEnumerator FadeUI(GameObject uiObject, float startAlpha, float endAlpha, float duration)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiObject.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    #endregion
    
    #region 调试UI
    /// <summary>
    /// 设置调试UI
    /// </summary>
    private void SetupDebugUI()
    {
        if (debugToggle != null)
        {
            debugToggle.onValueChanged.AddListener(SetDebugUIVisible);
        }
    }
    
    /// <summary>
    /// 设置调试UI可见性
    /// </summary>
    public void SetDebugUIVisible(bool visible)
    {
        isDebugUIVisible = visible;
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (!isDebugUIVisible || debugText == null) return;
        
        debugUpdateTimer += Time.deltaTime;
        frameCount++;
        
        // 每秒更新一次FPS
        if (debugUpdateTimer >= 1f)
        {
            fps = frameCount / debugUpdateTimer;
            frameCount = 0;
            debugUpdateTimer = 0f;
            
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }
        }
        
        // 更新调试信息
        if (playerCharacter != null)
        {
            string debugInfo = $"玩家位置: {playerCharacter.transform.position}\n";
            debugInfo += $"玩家生命值: {playerCharacter.currentHealth}/{playerCharacter.maxHealth}\n";
            debugInfo += $"玩家魔法值: {playerCharacter.currentMana}/{playerCharacter.maxMana}\n";
            debugInfo += $"玩家等级: {playerCharacter.level}\n";
            debugInfo += $"活跃敌人数: {GameObject.FindObjectsOfType<Enemy>().Length}\n";
            
            debugText.text = debugInfo;
        }
    }
    #endregion
    
    #region 事件处理
    /// <summary>
    /// 玩家死亡事件处理
    /// </summary>
    private void OnPlayerDeath()
    {
        ShowNotification("玩家死亡！");
        
        // 可以在这里添加死亡UI逻辑
    }
    
    /// <summary>
    /// 所有敌人被消灭事件处理
    /// </summary>
    private void OnAllEnemiesDefeated()
    {
        ShowNotification("所有敌人已被消灭！");
        
        // 可以在这里添加胜利UI逻辑
    }
    #endregion
    
    #region 公共方法
    /// <summary>
    /// 更新UI（供外部调用）
    /// </summary>
    public void UpdateUI()
    {
        // 这个方法可以为空，因为Update()已经处理了所有UI更新
        // 或者可以用于强制更新UI状态
    }
    
    /// <summary>
    /// 设置HUD可见性
    /// </summary>
    public void SetHUDVisible(bool visible)
    {
        isHUDVisible = visible;
        
        if (hudCanvas != null)
        {
            hudCanvas.gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 切换HUD可见性
    /// </summary>
    public void ToggleHUD()
    {
        SetHUDVisible(!isHUDVisible);
    }
    #endregion
    
    #region 清理方法
    /// <summary>
    /// 清理资源
    /// </summary>
    private void CleanupResources()
    {
        // 注销事件监听器
        UnregisterEventListeners();
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 清理渲染纹理
        if (minimapRenderTexture != null)
        {
            minimapRenderTexture.Release();
            minimapRenderTexture = null;
        }
        
        // 清理通知队列
        notificationQueue.Clear();
        
        // 清理技能冷却
        skillCooldowns.Clear();
        skillMaxCooldowns.Clear();
        
        Debug.Log("[TestSceneUIManager] 资源清理完成");
    }
    #endregion
}