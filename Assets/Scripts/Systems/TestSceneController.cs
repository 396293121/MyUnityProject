using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 增强版测试场景控制器
/// 基于Phaser项目TestScene.js完全重构
/// 使用ScriptableObject配置驱动，实现高可维护性和可配置性
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class TestSceneController : MonoBehaviour
{
    #region 配置引用
    [TabGroup("配置", "统一配置")]
    [FoldoutGroup("配置/统一配置/ScriptableObject配置", expanded: true)]
    [LabelText("统一场景配置")]
    [Required("必须指定统一场景配置文件")]
    [AssetsOnly]
    [SerializeField] private UnifiedSceneConfig unifiedConfig;
    
    [TabGroup("配置", "场景引用")]
    [FoldoutGroup("配置/场景引用/核心组件", expanded: true)]
    [LabelText("主摄像机")]
    [Required("必须指定主摄像机")]
    [SerializeField] private Camera mainCamera;
    
    [FoldoutGroup("配置/场景引用/核心组件")]
    [LabelText("地图容器")]
    [InfoBox("地图预制体的父对象，如果为空则使用当前对象")]
    [SerializeField] private GameObject mapContainer;
    
    [TabGroup("配置", "调试设置")]
    [FoldoutGroup("配置/调试设置/调试选项", expanded: true)]
    [LabelText("调试模式")]
    [InfoBox("启用后会在控制台输出详细的调试信息")]
    [SerializeField] private bool debugMode = false;
    
    [FoldoutGroup("配置/调试设置/调试选项", expanded: true)]
    [LabelText("显示调试UI")]
    [InfoBox("在游戏中显示调试信息界面")]
    [SerializeField] private bool showDebugUI = false;
    
    [FoldoutGroup("配置/调试设置/调试选项")]
    [LabelText("启用音效")]
    [InfoBox("控制是否播放音效")]
    [SerializeField] private bool enableAudioEffects = true;
    #endregion
    
    #region 私有字段
    [TabGroup("状态", "游戏对象")]
    [FoldoutGroup("状态/游戏对象/实例引用", expanded: true)]
    [LabelText("当前玩家")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject currentPlayer;
    
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("活跃敌人列表")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("所有敌人列表")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    private List<GameObject> enemies = new List<GameObject>();
    
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("敌人控制器列表")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true)]
    private List<Enemy> enemyControllers = new List<Enemy>();
    
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("当前地图")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject currentMap;
    
    [TabGroup("状态", "系统管理器")]
    [FoldoutGroup("状态/系统管理器/核心管理器", expanded: true)]
    [LabelText("敌人系统管理器")]
    [ReadOnly]
    [ShowInInspector]
    private TestSceneEnemySystem enemySystem;
    
    [FoldoutGroup("状态/系统管理器/核心管理器")]
    [LabelText("UI管理器")]
    [ReadOnly]
    [ShowInInspector]
    private TestSceneUIManager uiManager;
    
    
    [TabGroup("状态", "组件引用")]
    [FoldoutGroup("状态/组件引用/控制器组件", expanded: true)]
    [LabelText("玩家控制器")]
    [ReadOnly]
    [ShowInInspector]
    private PlayerController playerController;
    
    [FoldoutGroup("状态/组件引用/控制器组件")]
    [LabelText("玩家角色组件")]
    [ReadOnly]
    [ShowInInspector]
    private Character playerCharacter;
    
    [FoldoutGroup("状态/组件引用/控制器组件")]
    [LabelText("摄像机跟随组件")]
    [ReadOnly]
    [ShowInInspector]
    private CameraFollow cameraFollow;
    
    [TabGroup("状态", "场景状态")]
    [FoldoutGroup("状态/场景状态/初始化状态", expanded: true)]
    [LabelText("场景已初始化")]
    [ReadOnly]
    [ShowInInspector]
    private bool isSceneInitialized = false;
    
    [FoldoutGroup("状态/场景状态/游戏状态", expanded: true)]
    [LabelText("游戏暂停")]
    [ReadOnly]
    [ShowInInspector]
    private bool isGamePaused = false;
    
    [FoldoutGroup("状态/场景状态/游戏状态")]
    [LabelText("玩家死亡")]
    [ReadOnly]
    [ShowInInspector]
    private bool isPlayerDead = false;
    
    [FoldoutGroup("状态/场景状态/时间统计", expanded: true)]
    [LabelText("场景开始时间")]
    [ReadOnly]
    [ShowInInspector]
    [SuffixLabel("秒")]
    private float sceneStartTime;
    
    [TabGroup("状态", "角色设置")]
    [FoldoutGroup("状态/角色设置/角色选择", expanded: true)]
    [LabelText("选择的角色类型")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("从PlayerPrefs中读取的角色类型")]
    private string selectedCharacterType;
    
    [TabGroup("状态", "事件系统")]
    [FoldoutGroup("状态/事件系统/事件总线", expanded: true)]
    [LabelText("测试场景事件总线")]
    [ReadOnly]
    [ShowInInspector]
    private TestSceneEventBus eventBus;
    
    [TabGroup("状态", "调试信息")]
    [FoldoutGroup("状态/调试信息/性能统计", expanded: true)]
    [LabelText("调试更新计时器")]
    [ReadOnly]
    [ShowInInspector]
    [SuffixLabel("秒")]
    [ProgressBar(0, 1, ColorGetter = "GetDebugTimerColor")]
    private float debugUpdateTimer = 0f;
    
    [FoldoutGroup("状态/调试信息/性能统计")]
    [LabelText("帧计数")]
    [ReadOnly]
    [ShowInInspector]
    private int frameCount = 0;
    
    [FoldoutGroup("状态/调试信息/性能统计")]
    [LabelText("当前FPS")]
    [ReadOnly]
    [ShowInInspector]
    [SuffixLabel("帧/秒")]
    [PropertyRange(0, 120)]
    private float fps = 0f;
    #endregion
    
    #region Odin Inspector 控制面板
    [TabGroup("控制面板", "场景管理")]
    [FoldoutGroup("控制面板/场景管理/场景控制", expanded: true)]
    [Button("重新初始化场景", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    [EnableIf("isSceneInitialized")]
    private void ReinitializeScene()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(ReinitializeSceneCoroutine());
        }
    }
    
    [FoldoutGroup("控制面板/场景管理/场景控制")]
    [Button("清理场景", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.8f)]
    [EnableIf("isSceneInitialized")]
    private void ForceCleanupScene()
    {
        if (Application.isPlaying)
        {
            CleanupTestScene();
        }
    }
    
    [FoldoutGroup("控制面板/场景管理/场景控制")]
    [Button("切换暂停状态", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.8f, 1f)]
    [EnableIf("isSceneInitialized")]
    private void ForceTogglePause()
    {
        if (Application.isPlaying)
        {
            TogglePause();
        }
    }
    
    [TabGroup("控制面板", "敌人管理")]
    [FoldoutGroup("控制面板/敌人管理/敌人控制", expanded: true)]
    [Button("生成野猪", ButtonSizes.Medium)]
    [GUIColor(1f, 0.9f, 0.7f)]
    [EnableIf("CanSpawnEnemy")]
    private void SpawnWildBoar()
    {
        if (Application.isPlaying && currentPlayer != null)
        {
            Vector3 spawnPos = currentPlayer.transform.position + Vector3.right * 3f;
            CreateEnemyAtPosition("wild_boar", spawnPos);
        }
    }
    
    [FoldoutGroup("控制面板/敌人管理/敌人控制")]
    [Button("清除所有敌人", ButtonSizes.Medium)]
    [GUIColor(1f, 0.7f, 0.7f)]
    [EnableIf("HasActiveEnemies")]
    private void ClearAllEnemies()
    {
        if (Application.isPlaying)
        {
            enemySystem?.ClearAllEnemies();
            foreach (var enemy in enemies.ToList())
            {
                if (enemy != null)
                {
                    DestroyImmediate(enemy);
                }
            }
            enemies.Clear();
            enemyControllers.Clear();
            activeEnemies.Clear();
        }
    }
    
    [TabGroup("控制面板", "调试工具")]
    [FoldoutGroup("控制面板/调试工具/调试控制", expanded: true)]
    [Button("切换调试模式", ButtonSizes.Medium)]
    [GUIColor(0.9f, 0.9f, 0.9f)]
    private void ToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"[TestSceneController] 调试模式: {(debugMode ? "开启" : "关闭")}");
    }
    
    [FoldoutGroup("控制面板/调试工具/调试控制")]
    [Button("切换调试UI", ButtonSizes.Medium)]
    [GUIColor(0.9f, 0.9f, 0.9f)]
    [EnableIf("debugMode")]
    private void ToggleDebugUI()
    {
        showDebugUI = !showDebugUI;
    }
    
    [FoldoutGroup("控制面板/调试工具/调试控制")]
    [Button("输出场景信息", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    private void LogSceneInfo()
    {
        Debug.Log($"[场景信息] 玩家: {(currentPlayer != null ? currentPlayer.name : "无")}");
        Debug.Log($"[场景信息] 敌人数量: {enemies.Count}");
        Debug.Log($"[场景信息] 场景状态: {(isSceneInitialized ? "已初始化" : "未初始化")}");
        Debug.Log($"[场景信息] 游戏状态: {(isGamePaused ? "暂停" : "运行中")}");
    }
    #endregion
    
    #region Odin Inspector 属性访问器
    [TabGroup("实时状态", "场景状态")]
    [FoldoutGroup("实时状态/场景状态/基本信息", expanded: true)]
    [LabelText("场景运行时间")]
    [ShowInInspector]
    [ReadOnly]
    [SuffixLabel("秒")]
    [ProgressBar(0, 300, ColorGetter = "GetSceneTimeColor")]
    private float SceneRunTime => isSceneInitialized ? Time.time - sceneStartTime : 0f;
    
    [FoldoutGroup("实时状态/场景状态/基本信息")]
    [LabelText("当前FPS")]
    [ShowInInspector]
    [ReadOnly]
    [SuffixLabel("帧/秒")]
    [ProgressBar(0, 120, ColorGetter = "GetFPSColor")]
    private float CurrentFPS => fps;
    
    [FoldoutGroup("实时状态/场景状态/基本信息")]
    [LabelText("时间缩放")]
    [ShowInInspector]
    [ReadOnly]
    [PropertyRange(0, 2)]
    [ProgressBar(0, 2, ColorGetter = "GetTimeScaleColor")]
    private float TimeScale => Time.timeScale;
    
    [TabGroup("实时状态", "游戏对象统计")]
    [FoldoutGroup("实时状态/游戏对象统计/数量统计", expanded: true)]
    [LabelText("活跃敌人数量")]
    [ShowInInspector]
    [ReadOnly]
    [PropertyRange(0, 20)]
    [ProgressBar(0, 20, ColorGetter = "GetEnemyCountColor")]
    private int ActiveEnemyCount => activeEnemies.Count(e => e != null);
    
    [FoldoutGroup("实时状态/游戏对象统计/数量统计")]
    [LabelText("总敌人数量")]
    [ShowInInspector]
    [ReadOnly]
    [PropertyRange(0, 20)]
    private int TotalEnemyCount => enemies.Count;
    
    [FoldoutGroup("实时状态/游戏对象统计/数量统计")]
    [LabelText("敌人控制器数量")]
    [ShowInInspector]
    [ReadOnly]
    [PropertyRange(0, 20)]
    private int EnemyControllerCount => enemyControllers.Count(e => e != null);
    
    [TabGroup("实时状态", "玩家信息")]
    [FoldoutGroup("实时状态/玩家信息/角色状态", expanded: true)]
    [LabelText("玩家是否存在")]
    [ShowInInspector]
    [ReadOnly]
    private bool HasPlayer => currentPlayer != null;
    
    [FoldoutGroup("实时状态/玩家信息/角色状态")]
    [LabelText("玩家位置")]
    [ShowInInspector]
    [ReadOnly]
    [ShowIf("HasPlayer")]
    private Vector3 PlayerPosition => currentPlayer != null ? currentPlayer.transform.position : Vector3.zero;
    
    [FoldoutGroup("实时状态/玩家信息/角色状态")]
    [LabelText("玩家生命值")]
    [ShowInInspector]
    [ReadOnly]
    [ShowIf("HasPlayer")]
    [ProgressBar(0, 100, ColorGetter = "GetPlayerHealthColor")]
    private float PlayerHealth
    {
        get
        {
            if (currentPlayer != null)
            {
                var character = currentPlayer.GetComponent<Character>();
                return character != null ? character.currentHealth : 0f;
            }
            return 0f;
        }
    }
    
    [FoldoutGroup("实时状态/玩家信息/角色状态")]
    [LabelText("玩家魔法值")]
    [ShowInInspector]
    [ReadOnly]
    [ShowIf("HasPlayer")]
    [ProgressBar(0, 100, ColorGetter = "GetPlayerManaColor")]
    private float PlayerMana
    {
        get
        {
            if (currentPlayer != null)
            {
                var character = currentPlayer.GetComponent<Character>();
                return character != null ? character.currentMana : 0f;
            }
            return 0f;
        }
    }
    #endregion
    
    #region Odin Inspector 辅助方法
    private bool CanSpawnEnemy => isSceneInitialized && currentPlayer != null && !isGamePaused;
    private bool HasActiveEnemies => activeEnemies.Count > 0 || enemies.Count > 0;
    
    private Color GetSceneTimeColor()
    {
        float time = SceneRunTime;
        if (time < 60f) return Color.green;
        if (time < 180f) return Color.yellow;
        return Color.red;
    }
    
    private Color GetFPSColor()
    {
        if (fps >= 60f) return Color.green;
        if (fps >= 30f) return Color.yellow;
        return Color.red;
    }
    
    private Color GetTimeScaleColor()
    {
        if (Mathf.Approximately(Time.timeScale, 1f)) return Color.green;
        if (Time.timeScale > 0f) return Color.yellow;
        return Color.red;
    }
    
    private Color GetEnemyCountColor()
    {
        int count = ActiveEnemyCount;
        if (count == 0) return Color.gray;
        if (count <= 3) return Color.green;
        if (count <= 8) return Color.yellow;
        return Color.red;
    }
    
    private Color GetPlayerHealthColor()
    {
        float healthPercent = 0f;
        if (currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null && character.maxHealth > 0)
            {
                healthPercent = character.currentHealth / character.maxHealth;
            }
        }
        
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    private Color GetPlayerManaColor()
    {
        float manaPercent = 0f;
        if (currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null && character.maxMana > 0)
            {
                manaPercent = character.currentMana / character.maxMana;
            }
        }
        
        if (manaPercent > 0.6f) return Color.cyan;
        if (manaPercent > 0.3f) return Color.blue;
        return Color.magenta;
    }
    
    private Color GetDebugTimerColor()
    {
        return Color.Lerp(Color.green, Color.yellow, debugUpdateTimer);
    }
    
    private IEnumerator ReinitializeSceneCoroutine()
    {
        CleanupTestScene();
        yield return new WaitForSeconds(0.5f);
        isSceneInitialized = false;
        yield return StartCoroutine(InitializeTestSceneAsync());
    }
    // 添加单例引用
public static TestSceneController Instance { get; private set; }
    #endregion
    
    #region Unity生命周期
    private void Awake()
    {
          Instance = this;
        // 验证配置
        ValidateConfigurations();
        
        // 初始化事件系统
        eventBus = new TestSceneEventBus();
        
        // 记录场景开始时间
        sceneStartTime = Time.time;
    }
    
    private void Start()
    {
        StartCoroutine(InitializeTestSceneAsync());
          if (InputManager.Instance != null)
    {
        InputManager.Instance.OnPausePressed += TogglePause;
    }
    }
    
    private void Update()
    {
        if (!isSceneInitialized) return;
        
        // 更新FPS计算
        UpdateFPSCalculation();
        
        
        // 更新敌人系统
        enemySystem?.UpdateEnemySystem(Time.deltaTime);
        
        // 更新UI系统
        uiManager?.UpdateUI();
        
        // 更新调试信息
        UpdateDebugInfo();
        
        // 检查游戏结束条件
     //   CheckGameEndConditions();
    }
    
    private void LateUpdate()
    {
        // 更新摄像机跟随
        if (cameraFollow != null && currentPlayer != null)
        {
            cameraFollow.LateUpdateFollow();
        }
    }
    
    private void OnDestroy()
    {
        CleanupTestScene();
            if (InputManager.Instance != null)
    {
        InputManager.Instance.OnPausePressed -= TogglePause;
    }
    }
    public void OnPlayerDied()
{
    Debug.Log("[TestSceneController] 玩家死亡，游戏结束");
      // 停止游戏
    isGamePaused = true;
    Time.timeScale = 0f;
    // 显示游戏结束界面
    if (UIManager.Instance != null)
    {
     //   UIManager.Instance.ShowGameOverScreen();
    }
    
    // 可以添加其他游戏结束逻辑
}
    /// <summary>
    /// 清理测试场景
    /// </summary>
    private void CleanupTestScene()
    {
        Debug.Log("[TestSceneController] 开始清理测试场景...");
        
        // 清理敌人系统
        enemySystem?.ClearAllEnemies();
        
        // 清理UI管理器
        if (uiManager != null)
        {
            uiManager.SetHUDVisible(false);
        }
        
        
        // 停止所有协程
        StopAllCoroutines();
        
        Debug.Log("[TestSceneController] 测试场景清理完成");
    }
    

    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 验证配置文件
    /// </summary>
    private void ValidateConfigurations()
    {
        if (unifiedConfig == null)
        {
            Debug.LogError("[TestSceneController] UnifiedSceneConfig 未配置！");
            return;
        }
        
        Debug.Log("[TestSceneController] 统一配置验证通过");
    }
    
    /// <summary>
    /// 异步初始化测试场景
    /// </summary>
    private IEnumerator InitializeTestSceneAsync()
    {
        if (isSceneInitialized) yield break;
        
        Debug.Log("[TestSceneController] 开始异步初始化测试场景");
        AudioManager.Instance.PlayMusic("menu_music",0.6f,true);
        // 获取选择的角色类型
        selectedCharacterType = PlayerPrefs.GetString("SelectedCharacter", "warrior");
        
        // 步骤1: 初始化地图
        yield return StartCoroutine(InitializeMapAsync());
        
        
        // 步骤4: 创建玩家
        yield return StartCoroutine(CreatePlayerAsync());
        
        // 步骤5: 初始化敌人系统
        yield return StartCoroutine(InitializeEnemySystemAsync());
        
        // 步骤6: 初始化UI系统
        yield return StartCoroutine(InitializeUISystemAsync());
        
        // 步骤7: 设置摄像机
        yield return StartCoroutine(SetupCameraAsync());
        
        // 步骤8: 设置游戏状态
        SetupGameState();
        
        isSceneInitialized = true;
        
        Debug.Log("[TestSceneController] 测试场景异步初始化完成");
        
        // 触发场景初始化完成事件
        eventBus?.TriggerEvent("SceneInitialized", new { sceneName = unifiedConfig.sceneName });
    }
    
    /// <summary>
    /// 初始化地图
    /// </summary>
    private IEnumerator InitializeMapAsync()
    {
        Debug.Log("[TestSceneController] 初始化地图...");
        
        if (unifiedConfig.mapPrefab != null)
        {
            currentMap = Instantiate(unifiedConfig.mapPrefab, mapContainer != null ? mapContainer.transform : transform);
            currentMap.name = "TestScene_Map";
        }
        
        // 设置背景色
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = unifiedConfig.backgroundColor;
        }
        
        yield return null;
        Debug.Log("[TestSceneController] 地图初始化完成");
    }
    

    /// <summary>
    /// 异步创建玩家
    /// </summary>
    private IEnumerator CreatePlayerAsync()
    {
        Debug.Log("[TestSceneController] 创建玩家...");
        
      //  CreatePlayer();
        
        yield return null;
        Debug.Log("[TestSceneController] 玩家创建完成");
    }
    
    /// <summary>
    /// 异步初始化敌人系统
    /// </summary>
    private IEnumerator InitializeEnemySystemAsync()
    {
        Debug.Log("[TestSceneController] 初始化敌人系统...");
        
        if (enemySystem == null)
        {
            // 使用UnifiedSceneConfig中的敌人系统配置
            var enemyConfig = unifiedConfig.enemySystemConfig;
            if (enemyConfig == null)
            {
                Debug.LogWarning("[TestSceneController] UnifiedSceneConfig中未配置敌人系统，使用默认配置");
                enemyConfig = CreateTempEnemySystemConfig();
            }
            enemySystem = new TestSceneEnemySystem(enemyConfig, eventBus);
        }
        
        enemySystem.SetPlayerTransform(currentPlayer?.transform);
        
        // 设置敌人波次配置
        if (unifiedConfig.enemyWaves != null)
        {
            enemySystem.SetEnemyWaves(unifiedConfig.enemyWaves);
        }
        
        // 创建敌人
        CreateEnemies();
        
        yield return null;
        Debug.Log("[TestSceneController] 敌人系统初始化完成");
    }
    
    /// <summary>
    /// 异步初始化UI系统
    /// </summary>
    private IEnumerator InitializeUISystemAsync()
    {
        Debug.Log("[TestSceneController] 初始化UI系统...");
        
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("TestSceneUIManager");
            uiManagerObj.transform.SetParent(transform);
            uiManager = uiManagerObj.AddComponent<TestSceneUIManager>();
        }
        
        // 创建临时的HUD配置
        var tempHudConfig = CreateTempHudConfig();
        uiManager.Initialize(tempHudConfig, eventBus);
        
        if (currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null)
            {
                uiManager.SetPlayerCharacter(character);
            }
        }
        
        InitializeUI();
        
        yield return null;
        Debug.Log("[TestSceneController] UI系统初始化完成");
    }
    
    /// <summary>
    /// 异步设置摄像机
    /// </summary>
    private IEnumerator SetupCameraAsync()
    {
        Debug.Log("[TestSceneController] 设置摄像机...");
        
        SetupCamera();
        
        yield return null;
        Debug.Log("[TestSceneController] 摄像机设置完成");
    }
    
    /// <summary>
    /// 设置游戏状态
    /// </summary>
    private void SetupGameState()
    {
        isGamePaused = false;
        isPlayerDead = false;
        Time.timeScale = 1f;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Playing);
        }
    }
    

    /// <summary>
    /// 更新FPS计算
    /// </summary>
    private void UpdateFPSCalculation()
    {
        frameCount++;
        debugUpdateTimer += Time.deltaTime;
        
        if (debugUpdateTimer >= 1f)
        {
            fps = frameCount / debugUpdateTimer;
            frameCount = 0;
            debugUpdateTimer = 0f;
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (!debugMode) return;
        
        // 调试信息更新逻辑
    }
    
    /// <summary>
    /// 检查游戏结束条件
    /// </summary>
    private void CheckGameEndConditions()
    {
        // 检查玩家是否死亡
        if (currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null && character.currentHealth <= 0 && !isPlayerDead)
            {
            }
        }
        
        // 检查是否所有敌人都被消灭
        if (enemies.Count == 0 && enemyControllers.Count > 0)
        {
            OnAllEnemiesDefeated();
        }
    }
    

    #endregion
    

  
    /// <summary>
    /// 创建玩家
    /// </summary>
    void CreatePlayer()
    {
        // 确定生成位置
        Vector3 spawnPosition = unifiedConfig.GetPlayerSpawnPosition();
        
        // 根据选择的角色类型创建玩家
        GameObject playerPrefab = GetPlayerPrefab(selectedCharacterType);
        if (playerPrefab != null)
        {
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            currentPlayer.name = $"Player_{selectedCharacterType}";
            
            // 获取玩家控制器
            playerController = currentPlayer.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = currentPlayer.AddComponent<PlayerController>();
            }
              playerCharacter = currentPlayer.GetComponent<Character>();
        
        // 让角色自己初始化属性
        if (playerCharacter != null)
        {
            playerCharacter.InitializeWithConfig(selectedCharacterType);
        }
            // 配置玩家属性 在角色脚本配置
        //    ConfigurePlayer();
            
            if (debugMode)
            {
                Debug.Log($"[TestSceneController] 创建玩家: {selectedCharacterType} at {spawnPosition}");
            }
        }
        else
        {
            Debug.LogError($"[TestSceneController] 找不到角色预制体: {selectedCharacterType}");
        }
    }
    
    /// <summary>
    /// 获取玩家预制体
    /// </summary>
    GameObject GetPlayerPrefab(string characterType)
    {
        return unifiedConfig.GetCharacterPrefab(characterType) ?? unifiedConfig.GetCharacterPrefab(unifiedConfig.defaultCharacterType);
    }
    
    /// <summary>
    /// 配置玩家属性
    /// </summary>
    // void ConfigurePlayer()
    // {
    //     if (playerController == null) return;
        
    //     // 从配置文件获取角色属性
    //     var characterConfig = ConfigManager.Instance?.GetCharacterConfig(selectedCharacterType);
    //     if (characterConfig != null)
    //     {
    //         // 设置角色属性（这里需要根据实际的PlayerController接口调整）
    //         var character = playerController.GetComponent<Character>();
    //         if (character != null)
    //         {
    //             character.maxHealth = characterConfig.health;
    //             character.maxMana = characterConfig.mana;
    //             character.currentHealth = characterConfig.health;
    //             character.currentMana = characterConfig.mana;
    //         }
            
    //         // 设置移动速度等属性
    //         var rigidbody = currentPlayer.GetComponent<Rigidbody2D>();
    //         if (rigidbody != null)
    //         {
    //             // 可以在这里设置物理属性
    //         }
    //     }
    // }
    
    /// <summary>
    /// 创建敌人
    /// </summary>
    void CreateEnemies()
    {
        // 使用统一配置中的敌人生成配置
        if (unifiedConfig.enemySpawns != null && unifiedConfig.enemySpawns.Count > 0)
        {
            foreach (var enemySpawn in unifiedConfig.enemySpawns)
            {
                if (enemySpawn.autoSpawn)
                {
                    StartCoroutine(SpawnEnemyWithDelay(enemySpawn));
                }
            }
        }
        else
        {
            // 如果没有配置敌人生成，创建一个默认的敌人
            CreateDefaultEnemy();
        }
        
        if (debugMode)
        {
            Debug.Log($"[TestSceneController] 创建了 {enemies.Count} 个敌人");
        }
    }
    
    /// <summary>
    /// 创建默认敌人
    /// </summary>
    void CreateDefaultEnemy()
    {
        Vector3 enemyPosition = Vector3.zero;
        if (currentPlayer != null)
        {
            enemyPosition = currentPlayer.transform.position + Vector3.right * 5f;
        }
        
        var defaultEnemyType = unifiedConfig.enemyPrefabs.Count > 0 ? unifiedConfig.enemyPrefabs[0].enemyType : "wild_boar";
        CreateEnemyAtPosition(defaultEnemyType, enemyPosition);
    }
    
    /// <summary>
    /// 在指定位置创建敌人
    /// </summary>
  void CreateEnemyAtPosition(string enemyType, Vector3 position)
{
    GameObject enemyPrefab = GetEnemyPrefab(enemyType);
    if (enemyPrefab != null)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.name = $"Enemy_{enemyType}_{enemies.Count}";
        
        // 只添加到列表，不配置属性
        enemies.Add(enemy);
        
        // 设置事件监听
        Enemy enemyController = enemy.GetComponent<Enemy>();
        if (enemyController != null)
        {
            enemyControllers.Add(enemyController);
         //   SetupEnemyEvents(enemyController);
        }
    }
}
    /// <summary>
    /// 获取敌人预制体
    /// </summary>
    GameObject GetEnemyPrefab(string enemyType)
    {
        return unifiedConfig.GetEnemyPrefab(enemyType);
    }
    




    /// <summary>
    /// 所有敌人被消灭
    /// </summary>
    void OnAllEnemiesDefeated()
    {
        Debug.Log("[TestSceneController] 所有敌人被消灭！");
        
        // 显示胜利消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("胜利！所有敌人已被消灭！");
        }
        
        // 可以在这里添加胜利逻辑，比如显示胜利界面或返回主菜单
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        // UI初始化通过UIManager处理
        if (uiManager != null)
        {
            // uiManager.ShowHUD();
            // uiManager.HidePauseMenu();
        }
        
        // 注册UI到UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanel("Gameplay");
            UIManager.Instance.SetCurrentCharacter(currentPlayer?.GetComponent<Character>());
        }
    }
    
    /// <summary>
    /// 设置摄像机
    /// </summary>
    void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null && currentPlayer != null)
        {
            // 设置摄像机跟随
            cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
            
            cameraFollow.target = currentPlayer.transform;
        }
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            // 暂停菜单通过UI管理器处理
            if (GameManager.Instance != null) GameManager.Instance.ChangeGameState(GameState.Paused);
        }
        else
        {
            Time.timeScale = 1f;
            // 暂停菜单通过UI管理器处理
            if (GameManager.Instance != null) GameManager.Instance.ChangeGameState(GameState.Playing);
        }
    }
    
    /// <summary>
    /// 更新敌人系统
    /// </summary>
    void UpdateEnemySystem()
    {
        if (isGamePaused) return;
        
        // 更新所有敌人的AI
        foreach (var enemy in enemyControllers)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                // 设置玩家为目标
                if (currentPlayer != null)
                {
                    enemy.SetPlayer(currentPlayer.transform);
                }
            }
        }
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    void UpdateUI()
    {
        if (UIManager.Instance != null && currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null)
            {
                UIManager.Instance.UpdateCharacterUI(character);
            }
        }
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[TestSceneController] 返回主菜单");
        
        // 停止背景音乐
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        
        // 恢复时间缩放
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene("MainMenuScene");
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    
    void OnGUI()
    {
        if (!debugMode || !showDebugUI) return;
        
        // 调试信息显示
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"玩家: {selectedCharacterType}");
        GUILayout.Label($"敌人数量: {enemies.Count}");
        GUILayout.Label($"游戏状态: {(isGamePaused ? "暂停" : "运行中")}");
        
        if (currentPlayer != null)
        {
            GUILayout.Label($"玩家位置: {currentPlayer.transform.position}");
        }
        
        if (GUILayout.Button("重新开始"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (GUILayout.Button("返回主菜单"))
        {
            ReturnToMainMenu();
        }
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 延迟生成敌人
    /// </summary>
    private IEnumerator SpawnEnemyWithDelay(EnemySpawnConfig enemySpawn)
    {
        yield return new WaitForSeconds(enemySpawn.spawnDelay);
        
        for (int i = 0; i < enemySpawn.spawnCount; i++)
        {
            Vector3 spawnPosition = enemySpawn.spawnPosition;
            if (enemySpawn.randomizePosition)
            {
                spawnPosition += new Vector3(
                    Random.Range(-enemySpawn.spawnRadius, enemySpawn.spawnRadius),
                    0,
                    Random.Range(-enemySpawn.spawnRadius, enemySpawn.spawnRadius)
                );
            }
            
            CreateEnemyAtPosition(enemySpawn.enemyType, spawnPosition);
            
            if (i < enemySpawn.spawnCount - 1)
            {
                yield return new WaitForSeconds(0.5f); // 每个敌人之间的间隔
            }
        }
    }
    
    /// <summary>
    /// 创建临时的敌人系统配置
    /// </summary>
    private EnemySystemConfig CreateTempEnemySystemConfig()
    {
        var config = ScriptableObject.CreateInstance<EnemySystemConfig>();
        
        // 设置基础配置
        config.maxEnemyCount = 10;
        config.enemyUpdateInterval = 0.1f;
        config.aiUpdateInterval = 0.2f;
        
        // 创建野猪配置
        config.wildBoarConfig = new WildBoarConfig
        {
            health = 50,
            moveSpeed = 2f,
            attackDamage = 15,
            attackRange = 1.5f,
            detectionRange = 5f,
            chargeSpeed = 8f,
            chargeDistance = 6f,
            chargeCooldown = 3f,
            chargePreparationTime = 0.5f,
            patrolSpeed = 1f,
            patrolWaitTime = 2f,
            patrolRadius = 3f
        };
        
        // 设置碰撞层
        config.collisionLayers = -1;
        config.platformLayers = -1;
        config.playerLayers = -1;
        
        return config;
    }
    
    /// <summary>
    /// 创建临时的HUD配置
    /// </summary>
    private HUDConfig CreateTempHudConfig()
    {
        // 这里需要根据实际的HUDConfig结构来创建
        // 暂时返回null，需要根据实际情况实现
        return null;
    }
    
    /// <summary>
    /// 创建临时的场景配置
    /// </summary>
    private UnifiedSceneConfig CreateTempSceneConfig()
    {
        // 这里需要根据实际的SceneConfig结构来创建
        // 暂时返回null，需要根据实际情况实现
        return null;
    }
    
    // OnDestroy方法已在第133行定义，此处移除重复定义
}