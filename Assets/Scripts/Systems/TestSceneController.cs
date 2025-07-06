using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 增强版测试场景控制器
/// 基于Phaser项目TestScene.js完全重构
/// 使用ScriptableObject配置驱动，实现高可维护性和可配置性
/// </summary>
public class TestSceneController : MonoBehaviour
{
    #region 配置引用
    [Header("ScriptableObject 配置")]
    [SerializeField] private TestSceneConfig sceneConfig;
    [SerializeField] private EnemySystemConfig enemySystemConfig;
    [SerializeField] private HUDConfig hudConfig;
    [SerializeField] private InputSystemConfig inputConfig;
    
    [Header("场景引用")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mapContainer;
    
    [Header("预制体引用")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject wildBoarPrefab;
    
    [Header("UI引用")]
    [SerializeField] private GameObject gameHudUI;
    [SerializeField] private GameObject pauseMenuUI;
    
    [Header("调试设置")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool showDebugUI = false;
    [SerializeField] private bool enableAudioEffects = true;
    #endregion
    
    #region 私有字段
    // 游戏对象引用
    private GameObject currentPlayer;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();
    private List<Enemy> enemyControllers = new List<Enemy>();
    private GameObject currentMap;
    
    // 系统管理器
    private TestSceneEnemySystem enemySystem;
    private TestSceneUIManager uiManager;
    private TestSceneInputManager inputManager;
    private TestSceneAudioManager audioManager;
    
    // 组件引用
    private PlayerController playerController;
    private Character playerCharacter;
    private CameraFollow cameraFollow;
    
    // 场景状态
    private bool isSceneInitialized = false;
    private bool isGamePaused = false;
    private bool isPlayerDead = false;
    private float sceneStartTime;
    
    // 选择的角色类型
    private string selectedCharacterType;
    
    // 事件系统
    private TestSceneEventBus eventBus;
    
    // 调试信息
    private float debugUpdateTimer = 0f;
    private int frameCount = 0;
    private float fps = 0f;
    #endregion
    
    #region Unity生命周期
    private void Awake()
    {
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
    }
    
    private void Update()
    {
        if (!isSceneInitialized) return;
        
        // 更新FPS计算
        UpdateFPSCalculation();
        
        // 更新输入系统
        inputManager?.UpdateInput();
        
        // 更新敌人系统
        enemySystem?.UpdateEnemySystem(Time.deltaTime);
        
        // 更新UI系统
        uiManager?.UpdateUI();
        
        // 更新音频系统
        UpdateAudioSystem();
        
        // 更新调试信息
        UpdateDebugInfo();
        
        // 检查游戏结束条件
        CheckGameEndConditions();
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
    }
    
    /// <summary>
    /// 清理测试场景
    /// </summary>
    private void CleanupTestScene()
    {
        Debug.Log("[TestSceneController] 开始清理测试场景...");
        
        // 注销事件监听器
        UnregisterEventListeners();
        
        // 清理敌人系统
        enemySystem?.ClearAllEnemies();
        
        // 清理UI管理器
        if (uiManager != null)
        {
            uiManager.SetHUDVisible(false);
        }
        
        // 清理输入管理器
        inputManager?.SetInputLocked(true);
        
        // 清理音频管理器
        if (audioManager != null)
        {
            audioManager.StopMusic(0.5f);
            audioManager.StopAmbientSound(0.5f);
        }
        
        // 停止所有协程
        StopAllCoroutines();
        
        Debug.Log("[TestSceneController] 测试场景清理完成");
    }
    
    /// <summary>
    /// 注销事件监听器
    /// </summary>
    private void UnregisterEventListeners()
    {
        // 清理敌人事件监听
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                var enemyController = enemy.GetComponent<Enemy>();
                if (enemyController != null)
                {
                    enemyController.OnDeath -= OnEnemyDeath;
                    enemyController.OnAttack -= OnEnemyAttack;
                }
            }
        }
        
        // 清理事件总线
        eventBus?.ClearAllListeners();
    }
    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 验证配置文件
    /// </summary>
    private void ValidateConfigurations()
    {
        if (sceneConfig == null)
        {
            Debug.LogError("[TestSceneController] TestSceneConfig 未配置！");
            return;
        }
        
        if (enemySystemConfig == null)
        {
            Debug.LogError("[TestSceneController] EnemySystemConfig 未配置！");
            return;
        }
        
        if (hudConfig == null)
        {
            Debug.LogError("[TestSceneController] HUDConfig 未配置！");
            return;
        }
        
        if (inputConfig == null)
        {
            Debug.LogError("[TestSceneController] InputSystemConfig 未配置！");
            return;
        }
        
        Debug.Log("[TestSceneController] 所有配置验证通过");
    }
    
    /// <summary>
    /// 异步初始化测试场景
    /// </summary>
    private IEnumerator InitializeTestSceneAsync()
    {
        if (isSceneInitialized) yield break;
        
        Debug.Log("[TestSceneController] 开始异步初始化测试场景");
        
        // 获取选择的角色类型
        selectedCharacterType = PlayerPrefs.GetString("SelectedCharacter", "warrior");
        
        // 步骤1: 初始化地图
        yield return StartCoroutine(InitializeMapAsync());
        
        // 步骤2: 初始化音频系统
        yield return StartCoroutine(InitializeAudioSystemAsync());
        
        // 步骤3: 初始化输入系统
        yield return StartCoroutine(InitializeInputSystemAsync());
        
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
        
        // 步骤9: 注册事件监听
        RegisterEventListeners();
        
        isSceneInitialized = true;
        
        Debug.Log("[TestSceneController] 测试场景异步初始化完成");
        
        // 触发场景初始化完成事件
        eventBus?.TriggerEvent("SceneInitialized", new { sceneName = sceneConfig.sceneName });
    }
    
    /// <summary>
    /// 初始化地图
    /// </summary>
    private IEnumerator InitializeMapAsync()
    {
        Debug.Log("[TestSceneController] 初始化地图...");
        
        if (sceneConfig.mapPrefab != null)
        {
            currentMap = Instantiate(sceneConfig.mapPrefab, mapContainer != null ? mapContainer.transform : transform);
            currentMap.name = "TestScene_Map";
        }
        
        // 设置背景色
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = sceneConfig.backgroundColor;
        }
        
        yield return null;
        Debug.Log("[TestSceneController] 地图初始化完成");
    }
    
    /// <summary>
    /// 异步初始化输入系统
    /// </summary>
    private IEnumerator InitializeInputSystemAsync()
    {
        Debug.Log("[TestSceneController] 初始化输入系统...");
        
        if (inputManager == null)
        {
            GameObject inputManagerObj = new GameObject("TestSceneInputManager");
            inputManagerObj.transform.SetParent(transform);
            inputManager = inputManagerObj.AddComponent<TestSceneInputManager>();
        }
        
        inputManager.Initialize(inputConfig, eventBus);
        
        yield return null;
        Debug.Log("[TestSceneController] 输入系统初始化完成");
    }
    
    /// <summary>
    /// 异步创建玩家
    /// </summary>
    private IEnumerator CreatePlayerAsync()
    {
        Debug.Log("[TestSceneController] 创建玩家...");
        
        CreatePlayer();
        
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
            enemySystem = new TestSceneEnemySystem(enemySystemConfig, eventBus);
        }
        
        enemySystem.SetPlayerTransform(currentPlayer?.transform);
        
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
        
        uiManager.Initialize(hudConfig, eventBus);
        
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
    /// 注册事件监听器
    /// </summary>
    private void RegisterEventListeners()
    {
        // 注册玩家事件
        if (currentPlayer != null)
        {
            var character = currentPlayer.GetComponent<Character>();
            if (character != null)
            {
                character.OnHealthChanged += (health) => eventBus?.TriggerPlayerHealthChanged(health, character.maxHealth);
                character.OnManaChanged += (mana) => eventBus?.TriggerPlayerManaChanged(mana, character.maxMana);
            }
        }
        
        // 注册敌人事件
        foreach (var enemyController in enemyControllers)
        {
            if (enemyController != null)
            {
                enemyController.OnDeath += OnEnemyDeath;
                enemyController.OnAttack += OnEnemyAttack;
            }
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
                isPlayerDead = true;
                OnPlayerDeath();
            }
        }
        
        // 检查是否所有敌人都被消灭
        if (enemies.Count == 0 && enemyControllers.Count > 0)
        {
            OnAllEnemiesDefeated();
        }
    }
    
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("[TestSceneController] 玩家死亡");
        
        eventBus?.TriggerPlayerDeath();
        
        // 可以在这里添加死亡处理逻辑
    }
    #endregion
    
    /// <summary>
    /// 更新音频系统
    /// </summary>
    private void UpdateAudioSystem()
    {
        if (audioManager == null) return;
        
        // 音频管理器会自动处理更新逻辑
        // 这里可以添加特定的音频状态检查或控制逻辑
        
        // 检查游戏暂停状态对音频的影响
        if (isGamePaused)
        {
            // 根据游戏状态调整音频
            audioManager.PauseMusic();
        }
        else
        {
            audioManager.ResumeMusic();
        }
    }
    
    /// <summary>
    /// 初始化音频系统
    /// </summary>
    private IEnumerator InitializeAudioSystemAsync()
    {
        Debug.Log("[TestSceneController] 初始化音频系统...");
        
        // 创建音频管理器
        if (audioManager == null)
        {
            GameObject audioManagerObj = new GameObject("TestSceneAudioManager");
            audioManagerObj.transform.SetParent(transform);
            audioManager = audioManagerObj.AddComponent<TestSceneAudioManager>();
        }
        
        // 初始化音频管理器
        try
        {
            audioManager.Initialize(sceneConfig, eventBus);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TestSceneController] 音频系统初始化失败: {e.Message}");
            yield break;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // 开始播放背景音乐和环境音
        if (sceneConfig.backgroundMusic != null)
        {
            audioManager.PlayMusic(sceneConfig.backgroundMusic.name);
        }
        
        Debug.Log("[TestSceneController] 音频系统初始化完成");
    }
    
    /// <summary>
    /// 创建玩家
    /// </summary>
    void CreatePlayer()
    {
        // 确定生成位置
        Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        
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
            
            // 配置玩家属性
            ConfigurePlayer();
            
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
        switch (characterType.ToLower())
        {
            case "warrior":
                return warriorPrefab;
            case "mage":
                return magePrefab;
            case "archer":
                return archerPrefab;
            default:
                return warriorPrefab; // 默认返回战士
        }
    }
    
    /// <summary>
    /// 配置玩家属性
    /// </summary>
    void ConfigurePlayer()
    {
        if (playerController == null) return;
        
        // 从配置文件获取角色属性
        var characterConfig = ConfigManager.Instance?.GetCharacterConfig(selectedCharacterType);
        if (characterConfig != null)
        {
            // 设置角色属性（这里需要根据实际的PlayerController接口调整）
            var character = playerController.GetComponent<Character>();
            if (character != null)
            {
                character.maxHealth = characterConfig.health;
                character.maxMana = characterConfig.mana;
                character.currentHealth = characterConfig.health;
                character.currentMana = characterConfig.mana;
            }
            
            // 设置移动速度等属性
            var rigidbody = currentPlayer.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                // 可以在这里设置物理属性
            }
        }
    }
    
    /// <summary>
    /// 创建敌人
    /// </summary>
    void CreateEnemies()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            // 如果没有指定生成点，创建一个默认的野猪
            CreateDefaultEnemy();
            return;
        }
        
        // 在每个生成点创建敌人
        foreach (var spawnPoint in enemySpawnPoints)
        {
            if (spawnPoint != null)
            {
                CreateEnemyAtPosition("wild_boar", spawnPoint.position);
            }
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
        
        CreateEnemyAtPosition("wild_boar", enemyPosition);
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
            
            // 获取敌人控制器
            Enemy enemyController = enemy.GetComponent<Enemy>();
            if (enemyController == null)
            {
                enemyController = enemy.AddComponent<WildBoar>();
            }
            
            // 配置敌人属性
            ConfigureEnemy(enemyController, enemyType);
            
            // 添加到列表
            enemies.Add(enemy);
            enemyControllers.Add(enemyController);
            
            // 设置敌人事件
            SetupEnemyEvents(enemyController);
            
            if (debugMode)
            {
                Debug.Log($"[TestSceneController] 创建敌人: {enemyType} at {position}");
            }
        }
    }
    
    /// <summary>
    /// 获取敌人预制体
    /// </summary>
    GameObject GetEnemyPrefab(string enemyType)
    {
        switch (enemyType.ToLower())
        {
            case "wild_boar":
                return wildBoarPrefab;
            default:
                return wildBoarPrefab;
        }
    }
    
    /// <summary>
    /// 配置敌人属性
    /// </summary>
    void ConfigureEnemy(Enemy enemyController, string enemyType)
    {
        if (enemyController == null) return;
        
        // 从配置文件获取敌人属性
        var enemyConfig = ConfigManager.Instance?.GetEnemyConfig(enemyType);
        if (enemyConfig != null)
        {
            enemyController.maxHealth = enemyConfig.stats.health;
            enemyController.currentHealth = enemyConfig.stats.health;
            enemyController.attackDamage = enemyConfig.stats.attack;
            enemyController.moveSpeed = enemyConfig.stats.speed;
            enemyController.attackRange = enemyConfig.behavior.attackRange;
            enemyController.detectionRange = enemyConfig.behavior.detectionRadius;
        }
    }
    
    /// <summary>
    /// 设置敌人事件
    /// </summary>
    void SetupEnemyEvents(Enemy enemyController)
    {
        if (enemyController == null) return;
        
        // 设置敌人事件
        enemyController.OnDeath += OnEnemyDeath;
        
        // 设置敌人攻击事件
        enemyController.OnAttack += OnEnemyAttack;
    }
    
    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    void OnEnemyDeath(Enemy deadEnemy)
    {
        Debug.Log("[TestSceneController] 敌人死亡");
        
        // 从列表中移除死亡的敌人
        if (deadEnemy != null)
        {
            var enemyGameObject = deadEnemy.gameObject;
            enemies.Remove(enemyGameObject);
            enemyControllers.Remove(deadEnemy);
            activeEnemies.Remove(enemyGameObject);
        }
        
        // 播放死亡音效
        if (enableAudioEffects && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("enemy_death", 0.7f);
        }
        
        // 触发事件
        eventBus?.TriggerEnemyDeath(deadEnemy);
        
        // 检查是否所有敌人都被消灭
        if (enemies.Count == 0)
        {
            OnAllEnemiesDefeated();
        }
    }
    
    /// <summary>
    /// 敌人攻击事件处理
    /// </summary>
    void OnEnemyAttack(Enemy attackingEnemy)
    {
        Debug.Log("[TestSceneController] 敌人攻击");
        
        // 播放攻击音效
        if (enableAudioEffects && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("enemy_attack", 0.6f);
        }
        
        // 触发事件
        eventBus?.TriggerEnemyAttack(attackingEnemy);
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
        // 激活游戏HUD
        if (gameHudUI != null)
        {
            gameHudUI.SetActive(true);
        }
        
        // 隐藏暂停菜单
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
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
    void HandleInput()
    {
        // 暂停游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // 调试功能
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showDebugUI = !showDebugUI;
            }
        }
    }
    
    /// <summary>
    /// 切换暂停状态
    /// </summary>
    void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.ChangeGameState(GameState.Paused);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
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
    
    // OnDestroy方法已在第133行定义，此处移除重复定义
}