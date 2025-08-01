using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;

/// <summary>
/// 增强版测试场景控制器
/// 基于Phaser项目TestScene.js完全重构
/// 使用ScriptableObject配置驱动，实现高可维护性和可配置性
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class SceneController : MonoBehaviour
{
    #region 配置引用
    [TabGroup("配置", "统一配置")]
    [FoldoutGroup("配置/统一配置/ScriptableObject配置", expanded: true)]
    [LabelText("统一场景配置")]
    [Required("必须指定统一场景配置文件")]
    [AssetsOnly]
    [SerializeField] private UnifiedSceneConfig unifiedConfig;
    [LabelText("玩家层")]
    [Tooltip("玩家生成的游戏层")]
    [Required("必须指定玩家层")]
    [SerializeField] private GameObject PlayerLayer;
    [LabelText("敌人层")]
    [Tooltip("敌人生成的游戏层")]
    [Required("必须指定敌人层")]
    [SerializeField] private GameObject EnemyLayer;
    [LabelText("NPC层")]
    [Tooltip("npc生成的游戏层")]
    [Required("必须指定npc层")]
    [SerializeField] private GameObject NPCLayer;
        [LabelText("地图层")]
    [Tooltip("地图生成的游戏层")]
    [Required("必须指定地图层")]
    [SerializeField] private GameObject MapLayer;
    [TabGroup("配置", "场景引用")]
    [FoldoutGroup("配置/场景引用/核心组件", expanded: true)]
    [LabelText("主摄像机")]
    [Required("必须指定主摄像机")]
    [SerializeField] private Camera mainCamera;

    [TabGroup("配置", "调试设置")]
    [FoldoutGroup("配置/调试设置/调试选项", expanded: true)]
    [LabelText("调试模式")]
    [InfoBox("启用后会在控制台输出详细的调试信息")]
    [SerializeField] private bool debugMode = false;

    [FoldoutGroup("配置/调试设置/调试选项", expanded: true)]
    [LabelText("显示调试UI")]
    [InfoBox("在游戏中显示调试信息界面")]
    [SerializeField] private bool showDebugUI = false;
    #region 性能优化配置
    [FoldoutGroup("性能优化", expanded: false)]
    [LabelText("FPS更新间隔")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [SerializeField] private float fpsUpdateInterval = 1f;

    [FoldoutGroup("性能优化")]
    [LabelText("调试信息更新间隔")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [SerializeField] private float debugUpdateInterval = 0.5f;

    // 性能优化计时器
    private float lastFpsUpdateTime;
    private float lastDebugUpdateTime;
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

    public List<Enemy> EnemyControllers => enemyControllers;
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("活跃NPC列表")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    private List<GameObject> activeNPCs = new List<GameObject>();
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("所有NPC列表")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    private List<NPCController> npcs = new List<NPCController>();
    public List<NPCController> NPCControllers => npcs;
    [FoldoutGroup("状态/游戏对象/实例引用")]
    [LabelText("当前地图")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject currentMap;

    [TabGroup("状态", "系统管理器")]
    [FoldoutGroup("状态/系统管理器/地图系统", expanded: true)]
    [LabelText("地图管理器")]
    [ReadOnly]
    [ShowInInspector]
    private MapManager mapManager;

    [FoldoutGroup("状态/系统管理器/地图系统")]
    [LabelText("地图状态管理器")]
    [ReadOnly]
    [ShowInInspector]
    private MapStateManager mapStateManager;

    [FoldoutGroup("状态/系统管理器/地图系统")]
    [LabelText("当前区域ID")]
    [ReadOnly]
    [ShowInInspector]
    private string currentAreaId;





    [TabGroup("状态", "组件引用")]
    [FoldoutGroup("状态/组件引用/控制器组件", expanded: true)]
    [LabelText("玩家控制器")]
    [ReadOnly]
    [ShowInInspector]
    private PlayerController playerController;
    public PlayerController PlayerController => playerController;
    [FoldoutGroup("状态/组件引用/控制器组件")]
    [LabelText("玩家角色组件")]
    [ReadOnly]
    [ShowInInspector]
    private Character character;

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
    [TabGroup("实时状态", "场景状态")]
    [FoldoutGroup("实时状态/场景状态/基本信息")]
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



    #endregion



    // 添加单例引用
    public static SceneController Instance { get; private set; }
    #endregion

    #region 地图切换事件处理
    /// <summary>
    /// 初始化事件监听器
    /// </summary>
    private void InitializeEventListeners()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete += OnMapTransitionComplete;
            Debug.Log("[SceneController] 已订阅地图切换完成事件");
        }
        else
        {
            Debug.LogWarning("[SceneController] MapManager实例未找到，无法订阅地图切换事件");
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
            Debug.Log("[SceneController] 已取消订阅地图切换完成事件");
        }
    }

    /// <summary>
    /// 地图切换完成事件处理
    /// </summary>
    /// <param name="targetAreaId">目标区域ID</param>
    private void OnMapTransitionComplete(string targetAreaId)
    {
        Debug.Log($"[SceneController] 地图切换完成，目标区域: {targetAreaId}");
        
        // 只有在场景已经初始化完成后才响应地图切换事件
        // 避免在初始化期间重复生成敌人/NPC
        if (isSceneInitialized)
        {
            // 重新加载场景内容
            StartCoroutine(ReloadSceneContentAfterTransition(targetAreaId));
        }
        else
        {
            Debug.Log("[SceneController] 场景正在初始化中，跳过地图切换事件处理");
        }
    }

    /// <summary>
    /// 场景内容重新加载完成事件
    /// </summary>






    // 修改事件定义，添加场景加载状态参数
    public static System.Action<string> OnSceneContentReloaded; // 保持原有
    public static System.Action<Enemy> OnEnemySpawned; // 修改为携带敌人实例

    // 修改场景重载协程
    private IEnumerator ReloadSceneContentAfterTransition(string targetAreaId)
    {
        Debug.Log("[SceneController] 开始重新加载场景内容...");

        // 清理现有的敌人和NPC（但不清理地图，因为MapManager已经处理了）
        CleanupExistingContentExceptMap();

        // 等待一帧确保清理完成
        yield return null;

        // 引用MapManager创建的新地图实例
        yield return StartCoroutine(InitializeMapAsync());
    QuestManager.Instance.SetBatchMode(true);

        // 重新生成NPC
        yield return StartCoroutine(SpawnNPCsAsync());

        // 重新生成敌人
        yield return StartCoroutine(SpawnEnemiesAsync());
        
        // 重新设置相机
        SetupCamera();

        // 等待一帧确保所有内容完全加载
        yield return null;

        Debug.Log("[SceneController] 场景内容重新加载完成");
        
        // 关闭批量模式并触发场景加载完成事件
    QuestManager.Instance.SetBatchMode(false);
        OnSceneContentReloaded?.Invoke(targetAreaId);
    }

    /// <summary>
    /// 清理现有的场景内容（除了地图）
    /// </summary>
    private void CleanupExistingContentExceptMap()
    {
        Debug.Log("[SceneController] 清理现有场景内容（保留地图）...");

        // 清理敌人
        foreach (var enemy in enemies.ToList())
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        enemies.Clear();
        enemyControllers.Clear();

        // 清理NPC
        foreach (var npc in npcs.ToList())
        {
            if (npc != null && npc.gameObject != null)
            {
                Destroy(npc.gameObject);
            }
        }
        npcs.Clear();
        activeNPCs.Clear();

        // 不清理地图，因为MapManager已经处理了地图的切换
        // currentMap将在InitializeMapAsync中重新引用

        Debug.Log("[SceneController] 现有场景内容清理完成（地图由MapManager管理）");
    }

    /// <summary>
    /// 设置玩家位置到指定传送门位置
    /// </summary>
    /// <param name="portalName">传送门名称</param>
    public void SetPlayerToPortalPosition(string portalName)
    {
        Debug.Log($"[SceneController] 尝试设置玩家位置到传送门: {portalName}");
        if (currentPlayer != null && mapManager != null && mapManager.CurrentSceneArea != null)
        {
            // 查找指定传送门的位置
            var portalConfig = mapManager.CurrentSceneArea.areaTransitionTriggers.Find(p => p.triggerName == portalName);
            if (portalConfig != null)
            {
                Vector3 portalPosition = portalConfig.triggerPosition;
                currentPlayer.transform.position = portalPosition;
                Debug.Log($"[SceneController] 玩家位置已设置到传送门: {portalName} 位置: {portalPosition}");
            }
            else
            {
                Debug.LogWarning($"[SceneController] 未找到传送门: {portalName}");
            }
        }
    }
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        Instance = this;
        // 验证配置
        ValidateConfigurations();


        // 记录场景开始时间
        sceneStartTime = Time.time;

    }

    [FoldoutGroup("性能优化")]
    [LabelText("内存管理")]
    [SerializeField] private bool enableAutoGC = true;
    [SerializeField] private float gcInterval = 30f;

    [FoldoutGroup("性能优化")]
    [LabelText("状态持久化")]
    [InfoBox("是否启用场景切换时的状态保存")]
    [SerializeField] private bool enableStatePersistence = true;

    private void Start()
    {
        
        // 初始化事件系统
        InitializeEventListeners();
        StartCoroutine(InitializeTestSceneAsync());
            if (enableAutoGC)
    {
        StartCoroutine(AutoGarbageCollection());
    }
    }

    private void Update()
    {
        if (!isSceneInitialized) return;

        float currentTime = Time.time;

        // 更新FPS计算 - 降低频率
        if (currentTime - lastFpsUpdateTime >= fpsUpdateInterval)
        {
            UpdateFPSCalculation();




            // 检查游戏结束条件
            //   CheckGameEndConditions();
        }
        // 更新调试信息
        if (currentTime - lastDebugUpdateTime >= debugUpdateInterval)
        {
            UpdateDebugInfo();
            lastDebugUpdateTime = currentTime;
        }
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
        // 清理事件监听
        CleanupEventListeners();
        
        CleanupTestScene();
    }
    public void OnPlayerDied()
    {
        Debug.Log("[TestSceneController] 玩家死亡，游戏结束");
        // 停止游戏
        // isGamePaused = true;
        //  Time.timeScale = 0f;

        // 可以添加其他游戏结束逻辑
    }
    /// <summary>
    /// 清理测试场景
    /// </summary>
    private void CleanupTestScene()
    {
        Debug.Log("[TestSceneController] 开始清理测试场景...");

        // 保存当前状态到地图状态管理器
        if (mapStateManager != null && enableStatePersistence)
        {
            mapStateManager.SaveCurrentState();
        }

        // 清理敌人列表
        foreach (var enemy in enemies.ToList())
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        enemies.Clear();
        enemyControllers.Clear();

        // 清理NPC列表
        foreach (var npc in npcs.ToList())
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        npcs.Clear();
        activeNPCs.Clear();

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
        // BGMManager.Instance.PlayMusic("menu_music", 0.6f, true);
        
        // 获取选择的角色类型
        selectedCharacterType = PlayerPrefs.GetString("SelectedCharacter", "warrior");

        // 步骤0: 初始化地图系统
        yield return StartCoroutine(InitializeMapSystemAsync());

        // 步骤1: 引用MapManager创建的地图实例（MapManager在初始化时已经创建了地图）
        yield return StartCoroutine(InitializeMapAsync());

        // 步骤2: 生成NPC
        yield return StartCoroutine(SpawnNPCsAsync());

        // 步骤3: 创建玩家
        yield return StartCoroutine(CreatePlayerAsync());

        // 步骤4: 生成敌人
        yield return StartCoroutine(SpawnEnemiesAsync());

        // 步骤5: 设置游戏状态
        SetupGameState();

        isSceneInitialized = true;

        Debug.Log("[TestSceneController] 测试场景异步初始化完成");
    }

    /// <summary>
    /// 初始化地图系统
    /// </summary>
    private IEnumerator InitializeMapSystemAsync()
    {
        Debug.Log("[TestSceneController] 初始化地图系统...");

        // 获取或创建MapManager
    
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
            if (mapManager == null)
            {
                GameObject mapManagerObj = new GameObject("MapManager");
                mapManager = mapManagerObj.AddComponent<MapManager>();
            }
        }

        // 获取或创建MapStateManager
        if (mapStateManager == null)
        {
            mapStateManager = FindObjectOfType<MapStateManager>();
            if (mapStateManager == null)
            {
                mapStateManager = mapManager.gameObject.AddComponent<MapStateManager>();
            }
        }

        // 设置当前区域ID（如果需要）
        if (!string.IsNullOrEmpty(currentAreaId))
        {
            mapStateManager.SetCurrentAreaId(currentAreaId);
        }

        // 直接使用MapSystemConfig初始化（如果已配置）
        mapManager.InitializeMapSystem();
        yield return null;
        Debug.Log("[TestSceneController] 地图系统初始化完成");
    }

    /// <summary>
    /// 初始化地图（引用MapManager创建的地图实例）
    /// </summary>
    private IEnumerator InitializeMapAsync()
    {
        Debug.Log("[TestSceneController] 初始化地图引用...");

        // 等待MapManager创建地图实例
        float timeout = 5f;
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            if (mapManager != null && mapManager.CurrentMapInstance != null)
            {
                // 引用MapManager创建的地图实例，而不是重新创建
                currentMap = mapManager.CurrentMapInstance;
                Debug.Log($"[TestSceneController] 成功引用MapManager的地图实例: {currentMap.name}");
                break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (currentMap == null)
        {
            Debug.LogWarning("[TestSceneController] 未能获取MapManager的地图实例，可能需要检查MapManager初始化");
        }
        yield return null;
        Debug.Log("[TestSceneController] 地图引用初始化完成");
    }


    /// <summary>
    /// 异步创建玩家
    /// </summary>
    private IEnumerator CreatePlayerAsync()
    {
        Debug.Log("[TestSceneController] 创建玩家...");

        CreatePlayer();

        // 等待1帧确保组件初始化完成
        yield return null;

        // 初始化HUD组件
        if (ModernHUDComponent.Instance != null && currentPlayer != null)
        {
            ModernHUDComponent.Instance.InitializeHUDComponents(
                playerController,
               character,
                currentPlayer.GetComponent<SkillComponent>()
            );
        }
        SetupCamera();
        Debug.Log("[TestSceneController] 玩家创建完成");
    }
    /// <summary>
    /// 异步生成敌人
    /// </summary>
    private IEnumerator SpawnEnemiesAsync()
    {
        Debug.Log("[TestSceneController] 生成敌人...");

        // 直接创建敌人，不再使用TestSceneEnemySystem
      CreateEnemies();

        yield return null;
        Debug.Log("[TestSceneController] 敌人生成完成");
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


    }

    /// <summary>
    /// 检查游戏结束条件
    /// </summary>
    private void CheckGameEndConditions()
    {
        // 检查玩家是否死亡
        if (currentPlayer != null)
        {
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
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity,PlayerLayer.transform);
            currentPlayer.name = $"Player_{selectedCharacterType}";

            // 获取玩家控制器
            playerController = currentPlayer.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = currentPlayer.AddComponent<PlayerController>();
            }
            character = currentPlayer.GetComponent<Character>();
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
        // 优先从MapManager的配置获取角色预制体
        if (mapManager != null && mapManager.MapSystemConfig != null)
        {
            GameObject prefab = mapManager.MapSystemConfig.GetCharacterPrefab(characterType);
            if (prefab != null) return prefab;
            
            // 如果指定类型不存在，尝试获取默认角色类型
            if (!string.IsNullOrEmpty(mapManager.MapSystemConfig.defaultCharacterType))
            {
                prefab = mapManager.MapSystemConfig.GetCharacterPrefab(mapManager.MapSystemConfig.defaultCharacterType);
                if (prefab != null) return prefab;
            }
        }
        
        // 如果MapManager不可用，记录警告
        Debug.LogWarning($"[TestSceneController] MapManager或MapSystemConfig不可用，无法获取角色预制体: {characterType}");
        return null;
    }
    /// <summary>
    /// 创建敌人
    /// </summary>
    void CreateEnemies()
    {
        // 从MapManager获取当前场景区域的敌人生成配置
        if (mapManager != null && mapManager.CurrentSceneArea != null)
        {
            var currentSceneArea = mapManager.CurrentSceneArea;
            if (currentSceneArea.enemySpawns != null && currentSceneArea.enemySpawns.Count > 0)
            {
                foreach (var enemySpawn in currentSceneArea.enemySpawns)
                {
                    if (enemySpawn.autoSpawn)
                    {
                        StartCoroutine(SpawnEnemyWithDelay(enemySpawn));
                    }
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"[TestSceneController] 创建了 {enemies.Count} 个敌人");
        }
    }
    /// <summary>
        /// 在指定位置创建敌人
        /// </summary>
        void CreateEnemyAtPosition(string enemyType, Vector3 position, float patrolRadius)
    {
        GameObject enemyPrefab = GetEnemyPrefab(enemyType);
        if (enemyPrefab != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity,EnemyLayer.transform);
            enemy.name = $"Enemy_{enemyType}_{enemies.Count}";
            enemies.Add(enemy);
            // 设置事件监听
            Enemy enemyController = enemy.GetComponent<Enemy>();

            //通知敌人已生成
                    OnEnemySpawned?.Invoke(enemyController); // 传递生成的敌人实例

            //设置敌人巡逻半径
            enemyController.patrolRange = patrolRadius;
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
        // 优先从MapManager的配置获取
        if (mapManager != null && mapManager.MapSystemConfig != null)
        {
            GameObject prefab = mapManager.MapSystemConfig.GetEnemyPrefab(enemyType);
            if (prefab != null) return prefab;
        }

        // 回退到UnifiedConfig
        return null;
    }





    /// <summary>
    /// 所有敌人被消灭
    /// </summary>
    void OnAllEnemiesDefeated()
    {
        Debug.Log("[TestSceneController] 所有敌人被消灭！");

        // 显示胜利消息

        // 可以在这里添加胜利逻辑，比如显示胜利界面或返回主菜单
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
            cameraFollow.initCamera(playerController.transform);
            DynamicCameraBounds d= mainCamera.GetComponent<DynamicCameraBounds>();
            //设置地图边界
            if (d != null)
            {
                Tilemap[] tilemaps = currentMap.GetComponentsInChildren<Tilemap>();
                d.DetectAndSetBounds(System.Array.FindAll(tilemaps, t => t.tag == "Ground"));
            }
        //    cameraFollow.target = currentPlayer.transform;
        }
    }

    /// <summary>
    /// 处理输入
    /// </summary>




    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[TestSceneController] 返回主菜单");

        // 停止背景音乐
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopMusic();
        }

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 加载主菜单场景
        SceneManager.LoadScene("MainMenuScene");
    }

    void OnGUI()
    {
        if (!debugMode || !showDebugUI) return;

        // 调试信息显示
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"玩家: {selectedCharacterType}");
        GUILayout.Label($"敌人数量: {enemies.Count}");
        GUILayout.Label($"游戏状态: {(isGamePaused ? "暂停" : "运行中")}");
        GUILayout.Label($"事件驱动更新率: {SkillComponent.EventDrivenRate:P}");
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

            CreateEnemyAtPosition(enemySpawn.enemyType, spawnPosition, enemySpawn.patrolRadius);

            if (i < enemySpawn.spawnCount - 1)
            {
                yield return new WaitForSeconds(0.5f); // 每个敌人之间的间隔
            }
        }
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
    
private IEnumerator AutoGarbageCollection()
{
    while (true)
    {
        yield return new WaitForSeconds(gcInterval);
        if (Time.frameCount % 1800 == 0) // 每30秒在低负载时执行
        {
            System.GC.Collect();
        }
    }
}

    /// <summary>
    /// 异步生成NPC
    /// </summary>
    private IEnumerator SpawnNPCsAsync()
    {
        Debug.Log("[TestSceneController] 生成NPC...");

        CreateNPCs();

        yield return null;
        Debug.Log("[TestSceneController] NPC生成完成");
    }

    /// <summary>
    /// 创建NPC
    /// </summary>
    void CreateNPCs()
    {
        // 从MapManager获取当前场景区域的NPC生成配置
        if (mapManager != null && mapManager.CurrentSceneArea != null)
        {
            var currentSceneArea = mapManager.CurrentSceneArea;
            if (currentSceneArea.npcSpawns != null && currentSceneArea.npcSpawns.Count > 0)
            {
                foreach (var npcSpawn in currentSceneArea.npcSpawns)
                {
                    if (npcSpawn.autoSpawn)
                    {
                        CreateNPCAtPosition(npcSpawn);
                    }
                }
            }
        }
        if (debugMode)
        {
            Debug.Log($"[TestSceneController] 创建了 {activeNPCs.Count} 个NPC");
        }
    }

    /// <summary>
    /// 在指定位置创建NPC
    /// </summary>
    void CreateNPCAtPosition(NPCSpawnConfig npcSpawn)
    {
        GameObject npcPrefab = GetNPCPrefab(npcSpawn.npcType);
        if (npcPrefab != null)
        {
            GameObject npc = Instantiate(npcPrefab, npcSpawn.spawnPosition, 
                Quaternion.Euler(0, npcSpawn.facingAngle, 0), NPCLayer.transform);
            npc.name = $"NPC_{npcSpawn.npcType}_{activeNPCs.Count}";
            
            // 添加到NPC列表
            activeNPCs.Add(npc);
            // 获取NPC控制器并配置
            NPCController npcController = npc.GetComponent<NPCController>();
            
            npcs.Add(npcController);

            if (npcController != null)
            {
                // 如果配置中指定了特定的NPC配置文件，则使用它
                if (npcSpawn.npcConfig != null)
                {
                    npcController.npcConfig = npcSpawn.npcConfig;
                }

                if (debugMode)
                {
                    Debug.Log($"[TestSceneController] 创建NPC: {npcSpawn.npcType} at {npcSpawn.spawnPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"[TestSceneController] NPC预制体 {npcSpawn.npcType} 缺少NPCController组件");
            }
        }
        else
        {
            Debug.LogError($"[TestSceneController] 找不到NPC预制体: {npcSpawn.npcType}");
        }
    }

    /// <summary>
    /// 获取NPC预制体
    /// </summary>
    GameObject GetNPCPrefab(string npcType)
    {
        // 优先从MapManager的配置获取
        if (mapManager != null && mapManager.MapSystemConfig != null)
        {
            GameObject prefab = mapManager.MapSystemConfig.GetNPCPrefab(npcType);
            if (prefab != null) return prefab;
        }
        
        // 回退到UnifiedConfig
        return null;
    }
    
}
