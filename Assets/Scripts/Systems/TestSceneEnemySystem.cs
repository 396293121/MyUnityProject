using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 测试场景敌人系统
/// 基于Phaser项目中的敌人管理逻辑
/// </summary>
public class TestSceneEnemySystem : MonoBehaviour
{
    #region 私有字段
    private TestSceneEventBus eventBus;
    private Transform playerTransform;
    private LayerMask platformLayers;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<Enemy> enemyControllers = new List<Enemy>();
    private Dictionary<string, EnemyWaveConfig> waveConfigs = new Dictionary<string, EnemyWaveConfig>();
    
    private float lastUpdateTime = 0f;
    private float lastAIUpdateTime = 0f;
    private bool isSystemActive = true;
    #endregion
    
    #region 公共属性
    public int ActiveEnemyCount => activeEnemies.Count(e => e != null && e.activeInHierarchy);
    public bool HasActiveEnemies => ActiveEnemyCount > 0;
    public List<Enemy> GetActiveEnemyControllers() => enemyControllers.Where(e => e != null && e.gameObject.activeInHierarchy).ToList();
    public EnemySystemConfig config { get; private set; }
    #endregion
    
    #region 构造函数
    public TestSceneEnemySystem(EnemySystemConfig systemConfig, TestSceneEventBus eventSystem)
    {
        config = systemConfig;
        eventBus = eventSystem;
        
        // 初始化波次配置
        InitializeWaveConfigs();
        
        Debug.Log("[TestSceneEnemySystem] 敌人系统初始化完成");
    }
    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 初始化波次配置
    /// </summary>
    private void InitializeWaveConfigs()
    {
        // 波次配置现在从UnifiedSceneConfig中获取
        // 这个方法将在SetEnemyWaves中被调用
    }
    
    /// <summary>
    /// 设置敌人波次配置
    /// </summary>
    public void SetEnemyWaves(List<EnemyWaveConfig> enemyWaves)
    {
        waveConfigs.Clear();
        
        if (enemyWaves != null)
        {
            foreach (var wave in enemyWaves)
            {
                if (!string.IsNullOrEmpty(wave.waveName))
                {
                    waveConfigs[wave.waveName] = wave;
                }
            }
        }
        
        Debug.Log($"[TestSceneEnemySystem] 已加载 {waveConfigs.Count} 个敌人波次配置");
    }
    
    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayerReference(Transform player)
    {
        playerTransform = player;
    }
    
    /// <summary>
    /// 设置玩家Transform
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    /// <summary>
    /// 设置平台碰撞层
    /// </summary>
    public void SetPlatformLayers(LayerMask layers)
    {
        platformLayers = layers;
    }
    #endregion
    
    #region 敌人生成方法
    /// <summary>
    /// 生成敌人波次
    /// </summary>
    public void SpawnWave(string waveName)
    {
        if (waveConfigs.TryGetValue(waveName, out EnemyWaveConfig waveConfig))
        {
            SpawnWave(waveConfig);
        }
        else
        {
            Debug.LogWarning($"[TestSceneEnemySystem] 未找到波次配置: {waveName}");
        }
    }
    
    /// <summary>
    /// 生成敌人波次
    /// </summary>
    public void SpawnWave(EnemyWaveConfig waveConfig)
    {
        if (waveConfig == null || waveConfig.enemies == null) return;
        
        Debug.Log($"[TestSceneEnemySystem] 生成敌人波次: {waveConfig.waveName}");
        
        foreach (var enemySpawn in waveConfig.enemies)
        {
            SpawnEnemy(enemySpawn);
        }
        
        // 触发波次生成事件
        eventBus?.TriggerEvent("EnemyWaveSpawned", new { waveName = waveConfig.waveName, enemyCount = waveConfig.enemies.Count });
    }
    
    /// <summary>
    /// 生成单个敌人
    /// </summary>
    public GameObject SpawnEnemy(EnemySpawnConfig spawnConfig)
    {
        if (spawnConfig?.enemyPrefab == null) return null;
        
        // 检查敌人数量限制
        if (ActiveEnemyCount >= config.maxEnemyCount)
        {
            Debug.LogWarning("[TestSceneEnemySystem] 已达到最大敌人数量限制");
            return null;
        }
        
        // 实例化敌人
        GameObject enemyObj = Object.Instantiate(spawnConfig.enemyPrefab, spawnConfig.spawnPosition, Quaternion.identity);
        enemyObj.name = $"Enemy_{spawnConfig.enemyType}_{activeEnemies.Count}";
        
        // 获取敌人控制器
        Enemy enemyController = enemyObj.GetComponent<Enemy>();
        if (enemyController == null)
        {
            // 根据敌人类型添加对应的控制器
            enemyController = AddEnemyController(enemyObj, spawnConfig.enemyType);
        }
        
        if (enemyController != null)
        {
            // 配置敌人属性
            ConfigureEnemy(enemyController, spawnConfig);
            
            // 设置巡逻点
            SetupPatrolPoints(enemyController, spawnConfig);
            
            // 注册事件
            RegisterEnemyEvents(enemyController);
            
            // 添加到管理列表
            activeEnemies.Add(enemyObj);
            enemyControllers.Add(enemyController);
            
            Debug.Log($"[TestSceneEnemySystem] 生成敌人: {spawnConfig.enemyType} at {spawnConfig.spawnPosition}");
        }
        
        return enemyObj;
    }
    
    /// <summary>
    /// 添加敌人控制器
    /// </summary>
    private Enemy AddEnemyController(GameObject enemyObj, string enemyType)
    {
        switch (enemyType.ToLower())
        {
            case "wild_boar":
                return enemyObj.AddComponent<WildBoar>();
            default:
                return enemyObj.AddComponent<Enemy>();
        }
    }
    
    /// <summary>
    /// 配置敌人属性
    /// 使用EnemySystemConfig统一配置
    /// </summary>
    private void ConfigureEnemy(Enemy enemyController, EnemySpawnConfig spawnConfig)
    {
        // 使用EnemySystemConfig配置敌人属性
        if (config != null)
        {
            // 根据敌人类型获取对应配置
            if (spawnConfig.enemyType.ToLower() == "wild_boar" && config.wildBoarConfig != null)
            {
                var wildBoarConfig = config.wildBoarConfig;
                enemyController.maxHealth = wildBoarConfig.health;
                enemyController.currentHealth = wildBoarConfig.health;
                enemyController.attackDamage = wildBoarConfig.attackDamage;
                enemyController.moveSpeed = wildBoarConfig.moveSpeed;
                enemyController.attackRange = wildBoarConfig.attackRange;
                enemyController.detectionRange = wildBoarConfig.detectionRange;
            }
            else
            {
                // 使用默认配置或其他敌人类型配置
                Debug.LogWarning($"[TestSceneEnemySystem] 未找到敌人类型 {spawnConfig.enemyType} 的配置，使用默认值");
            }
        }
        else
        {
            Debug.LogWarning("[TestSceneEnemySystem] EnemySystemConfig为空，无法配置敌人属性");
        }
        
        // 设置玩家目标
        if (playerTransform != null)
        {
            enemyController.SetTarget(playerTransform);
        }
    }
    
    /// <summary>
    /// 设置巡逻点
    /// </summary>
    private void SetupPatrolPoints(Enemy enemyController, EnemySpawnConfig spawnConfig)
    {
        if (spawnConfig.autoGeneratePatrolPoints)
        {
            // 自动生成巡逻点
            var patrolPoints = new List<Vector3>
            {
                spawnConfig.spawnPosition + Vector3.left * spawnConfig.patrolRadius,
                spawnConfig.spawnPosition + Vector3.right * spawnConfig.patrolRadius
            };
            
            // 设置巡逻点（需要Enemy类支持）
            if (enemyController is WildBoar wildBoar)
            {
                // wildBoar.SetPatrolPoints(patrolPoints);
            }
        }
        else if (spawnConfig.patrolPoints?.Count > 0)
        {
            // 使用配置的巡逻点
            if (enemyController is WildBoar wildBoar)
            {
                // wildBoar.SetPatrolPoints(spawnConfig.patrolPoints);
            }
        }
    }
    #endregion
    
    #region 更新方法
    /// <summary>
    /// 更新敌人系统
    /// </summary>
    public void UpdateEnemySystem(float deltaTime)
    {
        if (!isSystemActive) return;
        
        // 只处理系统级别的管理任务，敌人个体更新由各自的Update方法处理
        
        // 更新敌人AI目标（较低频率）
        if (Time.time - lastAIUpdateTime >= config.aiUpdateInterval)
        {
            UpdateEnemyTargets();
            lastAIUpdateTime = Time.time;
        }
        
        // 清理死亡的敌人
        CleanupDeadEnemies();
    }
    
    /// <summary>
    /// 更新敌人目标（系统级别管理）
    /// </summary>
    private void UpdateEnemyTargets()
    {
        // 只负责设置玩家目标，具体AI逻辑由各敌人自己的Update方法处理
        if (playerTransform == null) return;
        
        foreach (var enemy in enemyControllers.ToList())
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.SetTarget(playerTransform);
            }
        }
    }
    
    /// <summary>
    /// 清理死亡的敌人
    /// </summary>
    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
            }
        }
        
        for (int i = enemyControllers.Count - 1; i >= 0; i--)
        {
            if (enemyControllers[i] == null || !enemyControllers[i].gameObject.activeInHierarchy)
            {
                enemyControllers.RemoveAt(i);
            }
        }
    }
    #endregion
    
    #region 事件处理
    /// <summary>
    /// 注册敌人事件
    /// </summary>
    private void RegisterEnemyEvents(Enemy enemy)
    {
        if (enemy == null) return;
        
        enemy.OnDeath += OnEnemyDeath;
        enemy.OnAttack += OnEnemyAttack;
        
        // 如果是野猪，注册特殊事件
        if (enemy is WildBoar wildBoar)
        {
            // wildBoar.OnChargeStart += () => OnWildBoarChargeStart(wildBoar);
            // wildBoar.OnChargeEnd += () => OnWildBoarChargeEnd(wildBoar);
        }
    }
    
    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    private void OnEnemyDeath(Enemy enemy)
    {
        Debug.Log($"[TestSceneEnemySystem] 敌人死亡: {enemy.name}");
        
        // 触发敌人死亡事件
        eventBus?.TriggerEvent("EnemyDeath", new 
        { 
            enemyType = enemy.GetType().Name,
            position = enemy.transform.position,
            enemy = enemy
        });
        
        // 检查是否所有敌人都被消灭
        if (ActiveEnemyCount <= 1) // 当前敌人还未从列表中移除
        {
            eventBus?.TriggerEvent("AllEnemiesDefeated", new { });
        }
    }
    
    /// <summary>
    /// 敌人攻击事件处理
    /// </summary>
    private void OnEnemyAttack(Enemy enemy)
    {
        Debug.Log($"[TestSceneEnemySystem] 敌人攻击: {enemy.name}");
        
        // 触发敌人攻击事件
        eventBus?.TriggerEvent("EnemyAttack", new 
        { 
            enemyType = enemy.GetType().Name,
            position = enemy.transform.position,
            enemy = enemy
        });
    }
    #endregion
    
    #region 公共方法
    /// <summary>
    /// 激活/停用敌人系统
    /// </summary>
    public void SetSystemActive(bool active)
    {
        isSystemActive = active;
    }
    
    /// <summary>
    /// 清理所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies.ToList())
        {
            if (enemy != null)
            {
                Object.Destroy(enemy);
            }
        }
        
        activeEnemies.Clear();
        enemyControllers.Clear();
        
        Debug.Log("[TestSceneEnemySystem] 清理所有敌人完成");
    }
    
    /// <summary>
    /// 获取最近的敌人
    /// </summary>
    public Enemy GetNearestEnemy(Vector3 position, float maxDistance = float.MaxValue)
    {
        Enemy nearestEnemy = null;
        float nearestDistance = maxDistance;
        
        foreach (var enemy in enemyControllers)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }
        
        return nearestEnemy;
    }
    
    /// <summary>
    /// 获取指定范围内的敌人
    /// </summary>
    public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
    {
        var enemiesInRange = new List<Enemy>();
        
        foreach (var enemy in enemyControllers)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance <= range)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        return enemiesInRange;
    }
    #endregion
}