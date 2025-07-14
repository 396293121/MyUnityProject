using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人生成器 - 负责动态生成和管理敌人
/// 从原Phaser项目的敌人生成系统迁移而来
/// 支持Odin Inspector可视化编辑
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class EnemySpawner : MonoBehaviour
{
    [TabGroup("生成器配置", "基础设置")]
    [FoldoutGroup("生成器配置/基础设置/敌人配置", expanded: true)]
    [LabelText("敌人生成列表")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "enemyPrefab", DraggableItems = true)]
    public List<EnemySpawnData> enemySpawnList = new List<EnemySpawnData>();
    
    [FoldoutGroup("生成器配置/基础设置/敌人配置")]
    [LabelText("最大敌人数量")]
    [PropertyRange(1, 100)]
    [InfoBox("同时存在的最大敌人数量")]
    public int maxEnemies = 10;
    
    [FoldoutGroup("生成器配置/基础设置/敌人配置")]
    [LabelText("生成间隔")]
    [PropertyRange(0.1f, 30f)]
    [SuffixLabel("秒")]
    public float spawnInterval = 3f;
    
    [FoldoutGroup("生成器配置/基础设置/区域设置", expanded: true)]
    [LabelText("生成半径")]
    [PropertyRange(1f, 50f)]
    [SuffixLabel("米")]
    [ShowIf("@areaType == SpawnAreaType.Circle")]
    public float spawnRadius = 10f;
    
    [FoldoutGroup("生成器配置/基础设置/区域设置")]
    [LabelText("障碍物图层")]
    public LayerMask obstacleLayerMask = 1;
    
    [FoldoutGroup("生成器配置/基础设置/区域设置")]
    [LabelText("生成区域类型")]
    [EnumToggleButtons]
    public SpawnAreaType areaType = SpawnAreaType.Circle;
    
    [FoldoutGroup("生成器配置/基础设置/区域设置")]
    [LabelText("矩形区域大小")]
    [ShowIf("@areaType == SpawnAreaType.Rectangle")]
    public Vector2 areaSize = new Vector2(10f, 10f);
    
    [FoldoutGroup("生成器配置/基础设置/区域设置")]
    [LabelText("指定生成点")]
    [ShowIf("@areaType == SpawnAreaType.Points")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    public List<Transform> spawnPoints = new List<Transform>();
    
    [TabGroup("生成器配置", "玩家检测")]
    [FoldoutGroup("生成器配置/玩家检测/检测设置", expanded: true)]
    [LabelText("玩家检测范围")]
    [PropertyRange(5f, 100f)]
    [SuffixLabel("米")]
    public float playerDetectionRange = 15f;
    
    [FoldoutGroup("生成器配置/玩家检测/检测设置")]
    [LabelText("只在玩家附近生成")]
    [InfoBox("启用后只有玩家在检测范围内时才会生成敌人")]
    public bool spawnOnlyWhenPlayerNear = true;
    
    [TabGroup("生成器配置", "波次系统")]
    [FoldoutGroup("生成器配置/波次系统/波次设置", expanded: true)]
    [LabelText("使用波次系统")]
    [InfoBox("启用波次系统后将按照预设波次生成敌人")]
    public bool useWaveSystem = false;
    
    [FoldoutGroup("生成器配置/波次系统/波次设置")]
    [LabelText("波次列表")]
    [ShowIf("useWaveSystem")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "waveName", DraggableItems = true)]
    public List<WaveData> waves = new List<WaveData>();
    
    [FoldoutGroup("生成器配置/波次系统/波次设置")]
    [LabelText("波次间隔时间")]
    [ShowIf("useWaveSystem")]
    [PropertyRange(1f, 60f)]
    [SuffixLabel("秒")]
    public float timeBetweenWaves = 10f;
    
    [TabGroup("生成器配置", "调试设置")]
    [FoldoutGroup("生成器配置/调试设置/可视化", expanded: true)]
    [LabelText("显示调试线框")]
    public bool showDebugGizmos = true;
    
    [FoldoutGroup("生成器配置/调试设置/可视化")]
    [LabelText("线框颜色")]
    [ShowIf("showDebugGizmos")]
    public Color gizmoColor = Color.red;
    
    [TabGroup("生成器配置", "运行状态")]
    [FoldoutGroup("生成器配置/运行状态/敌人管理", expanded: true)]
    [LabelText("已生成敌人列表")]
    [ReadOnly]
    [ListDrawerSettings(ShowIndexLabels = true)]
    [ShowInInspector]
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    [FoldoutGroup("生成器配置/运行状态/敌人管理")]
    [LabelText("玩家Transform")]
    [ReadOnly]
    [ShowInInspector]
    private Transform playerTransform;
    
    [FoldoutGroup("生成器配置/运行状态/生成控制", expanded: true)]
    [LabelText("生成协程")]
    [ReadOnly]
    [ShowInInspector]
    private Coroutine spawnCoroutine;
    
    [FoldoutGroup("生成器配置/运行状态/生成控制")]
    [LabelText("正在生成")]
    [ReadOnly]
    [ShowInInspector]
    private bool isSpawning = false;
    
    [FoldoutGroup("生成器配置/运行状态/波次状态", expanded: true)]
    [LabelText("当前波次")]
    [ReadOnly]
    [ShowInInspector]
    private int currentWave = 0;
    
    [FoldoutGroup("生成器配置/运行状态/波次状态")]
    [LabelText("波次进行中")]
    [ReadOnly]
    [ShowInInspector]
    private bool waveInProgress = false;
    
    [FoldoutGroup("生成器配置/运行状态/波次状态")]
    [LabelText("本波已生成敌人数")]
    [ReadOnly]
    [ShowInInspector]
    private int enemiesSpawnedInWave = 0;
    
    [TabGroup("生成器配置", "事件系统")]
    [FoldoutGroup("生成器配置/事件系统/敌人事件", expanded: true)]
    [LabelText("敌人生成事件")]
    [InfoBox("敌人生成时触发")]
    public System.Action<GameObject> OnEnemySpawned;
    
    [FoldoutGroup("生成器配置/事件系统/敌人事件")]
    [LabelText("敌人销毁事件")]
    [InfoBox("敌人被销毁时触发")]
    public System.Action<GameObject> OnEnemyDestroyed;
    
    [FoldoutGroup("生成器配置/事件系统/波次事件", expanded: true)]
    [LabelText("波次开始事件")]
    [InfoBox("新波次开始时触发")]
    public System.Action<int> OnWaveStarted;
    
    [FoldoutGroup("生成器配置/事件系统/波次事件")]
    [LabelText("波次完成事件")]
    [InfoBox("波次完成时触发")]
    public System.Action<int> OnWaveCompleted;
    
    private void Start()
    {
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // 开始生成
        StartSpawning();
    }
    
    private void Update()
    {
        // 清理已销毁的敌人引用
        CleanupDestroyedEnemies();
        
        // 检查波次完成
        if (useWaveSystem && waveInProgress)
        {
            CheckWaveCompletion();
        }
    }
    
    /// <summary>
    /// 开始生成敌人
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning) return;
        
        isSpawning = true;
        
        if (useWaveSystem)
        {
            StartNextWave();
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnEnemiesCoroutine());
        }
    }
    
    /// <summary>
    /// 停止生成敌人
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// 生成敌人协程
    /// </summary>
    private IEnumerator SpawnEnemiesCoroutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // 检查是否需要生成敌人
            if (ShouldSpawnEnemy())
            {
                SpawnRandomEnemy();
            }
        }
    }
    
    /// <summary>
    /// 检查是否应该生成敌人
    /// </summary>
    private bool ShouldSpawnEnemy()
    {
        // 检查敌人数量限制
        if (spawnedEnemies.Count >= maxEnemies)
        {
            return false;
        }
        
        // 检查玩家距离
        if (spawnOnlyWhenPlayerNear && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > playerDetectionRange)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 生成随机敌人
    /// </summary>
    public GameObject SpawnRandomEnemy()
    {
        if (enemySpawnList.Count == 0) return null;
        
        // 根据权重选择敌人类型
        EnemySpawnData selectedEnemy = SelectEnemyByWeight();
        if (selectedEnemy == null || selectedEnemy.enemyPrefab == null) return null;
        
        // 获取生成位置
        Vector3 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector3.zero) return null;
        
        // 生成敌人
        GameObject enemy = Instantiate(selectedEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
        
        // 添加到列表
        spawnedEnemies.Add(enemy);
        
        // 设置敌人属性
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            ApplyEnemyModifiers(enemyComponent, selectedEnemy);
            
            // 监听敌人死亡事件
            enemyComponent.OnDeath += (deadEnemy) => OnEnemyDied(enemy);
        }
        
        // 触发事件
        OnEnemySpawned?.Invoke(enemy);
        
        // 播放生成音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("enemy_spawn");
        }
        
        return enemy;
    }
    
    /// <summary>
    /// 根据权重选择敌人
    /// </summary>
    private EnemySpawnData SelectEnemyByWeight()
    {
        float totalWeight = enemySpawnList.Sum(e => e.spawnWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (EnemySpawnData enemyData in enemySpawnList)
        {
            currentWeight += enemyData.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemyData;
            }
        }
        
        return enemySpawnList[0]; // 默认返回第一个
    }
    
    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        int attempts = 0;
        int maxAttempts = 20;
        
        while (attempts < maxAttempts)
        {
            switch (areaType)
            {
                case SpawnAreaType.Circle:
                    spawnPos = GetRandomPositionInCircle();
                    break;
                case SpawnAreaType.Rectangle:
                    spawnPos = GetRandomPositionInRectangle();
                    break;
                case SpawnAreaType.Points:
                    spawnPos = GetRandomSpawnPoint();
                    break;
            }
            
            // 检查位置是否有效
            if (IsValidSpawnPosition(spawnPos))
            {
                return spawnPos;
            }
            
            attempts++;
        }
        
        return Vector3.zero; // 找不到有效位置
    }
    
    /// <summary>
    /// 在圆形区域内获取随机位置
    /// </summary>
    private Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);
    }
    
    /// <summary>
    /// 在矩形区域内获取随机位置
    /// </summary>
    private Vector3 GetRandomPositionInRectangle()
    {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float y = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        return transform.position + new Vector3(x, y, 0);
    }
    
    /// <summary>
    /// 从指定生成点获取随机位置
    /// </summary>
    private Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return transform.position;
        
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        return randomPoint.position;
    }
    
    /// <summary>
    /// 检查生成位置是否有效
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // 检查是否有障碍物
        Collider2D obstacle = Physics2D.OverlapCircle(position, 0.5f, obstacleLayerMask);
        if (obstacle != null) return false;
        
        // 检查是否离玩家太近
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(position, playerTransform.position);
            if (distanceToPlayer < 2f) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 应用敌人修饰符
    /// </summary>
    private void ApplyEnemyModifiers(Enemy enemy, EnemySpawnData spawnData)
    {
        // 应用等级修饰符
        if (spawnData.levelRange.x > 0)
        {
            int level = Random.Range(spawnData.levelRange.x, spawnData.levelRange.y + 1);
            enemy.SetLevel(level);
        }
        
        // 应用属性修饰符
        enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * spawnData.healthMultiplier);
        enemy.currentHealth = enemy.maxHealth;
        enemy.attackDamage = Mathf.RoundToInt(enemy.attackDamage * spawnData.damageMultiplier);
        enemy.moveSpeed *= spawnData.speedMultiplier;
    }
    
    /// <summary>
    /// 敌人死亡处理
    /// </summary>
    private void OnEnemyDied(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
        }
        
        OnEnemyDestroyed?.Invoke(enemy);
        
        // 波次系统中的敌人死亡处理
        if (useWaveSystem && waveInProgress)
        {
            CheckWaveCompletion();
        }
    }
    
    /// <summary>
    /// 清理已销毁的敌人引用
    /// </summary>
    private void CleanupDestroyedEnemies()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
    }
    
    /// <summary>
    /// 开始下一波
    /// </summary>
    private void StartNextWave()
    {
        if (currentWave >= waves.Count) return;
        
        WaveData wave = waves[currentWave];
        waveInProgress = true;
        enemiesSpawnedInWave = 0;
        
        OnWaveStarted?.Invoke(currentWave + 1);
        
        // 显示波次信息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"第 {currentWave + 1} 波开始！");
        }
        
        StartCoroutine(SpawnWaveEnemies(wave));
    }
    
    /// <summary>
    /// 生成波次敌人
    /// </summary>
    private IEnumerator SpawnWaveEnemies(WaveData wave)
    {
        while (enemiesSpawnedInWave < wave.enemyCount && waveInProgress)
        {
            if (spawnedEnemies.Count < maxEnemies)
            {
                SpawnRandomEnemy();
                enemiesSpawnedInWave++;
            }
            
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
    
    /// <summary>
    /// 检查波次完成
    /// </summary>
    private void CheckWaveCompletion()
    {
        if (!waveInProgress) return;
        
        // 检查是否所有敌人都被击败
        if (spawnedEnemies.Count == 0 && enemiesSpawnedInWave >= waves[currentWave].enemyCount)
        {
            CompleteWave();
        }
    }
    
    /// <summary>
    /// 完成波次
    /// </summary>
    private void CompleteWave()
    {
        waveInProgress = false;
        OnWaveCompleted?.Invoke(currentWave + 1);
        
        // 显示波次完成信息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"第 {currentWave + 1} 波完成！");
        }
        
        currentWave++;
        
        // 检查是否还有下一波
        if (currentWave < waves.Count)
        {
            StartCoroutine(WaitForNextWave());
        }
        else
        {
            // 所有波次完成
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("所有波次完成！");
            }
        }
    }
    
    /// <summary>
    /// 等待下一波
    /// </summary>
    private IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        
        if (isSpawning)
        {
            StartNextWave();
        }
    }
    
    [FoldoutGroup("生成器配置/控制面板", expanded: false)]
    [HorizontalGroup("生成器配置/控制面板/控制设置")]
    [VerticalGroup("生成器配置/控制面板/控制设置/生成控制")]
    [Button("开始生成", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    [ShowIf("@!isSpawning")]
    private void StartSpawningButton() => StartSpawning();
    
    [VerticalGroup("生成器配置/控制面板/控制设置/生成控制")]
    [Button("停止生成", ButtonSizes.Medium)]
    [GUIColor(1f, 0.6f, 0.4f)]
    [ShowIf("isSpawning")]
    private void StopSpawningButton() => StopSpawning();
    
    [VerticalGroup("生成器配置/控制面板/控制设置/敌人管理")]
    [Button("清除所有敌人", ButtonSizes.Medium)]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void ClearAllEnemiesButton() => ClearAllEnemies();
    
    [VerticalGroup("生成器配置/控制面板/控制设置/敌人管理")]
    [Button("重置生成器", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.4f, 1f)]
    private void ResetSpawnerButton() => ResetSpawner();
    
    [VerticalGroup("生成器配置/控制面板/控制设置/敌人管理")]
    [Button("生成一个敌人", ButtonSizes.Medium)]
    [GUIColor(0.4f, 1f, 0.4f)]
    private void SpawnOneEnemyButton() => SpawnRandomEnemy();
    
    [VerticalGroup("生成器配置/控制面板/控制设置/信息显示")]
    [ShowInInspector, ReadOnly, LabelText("当前敌人数量")]
    [PropertyOrder(100)]
    public int CurrentEnemyCount => spawnedEnemies.Count;
    
    [VerticalGroup("生成器配置/控制面板/控制设置/信息显示")]
    [ShowInInspector, ReadOnly, LabelText("当前波次进度")]
    [PropertyOrder(101)]
    [ShowIf("useWaveSystem")]
    public string WaveProgress => useWaveSystem ? $"{currentWave + 1}/{waves.Count}" : "未使用波次系统";
    
    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies.ToList())
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }
    
    /// <summary>
    /// 重置生成器
    /// </summary>
    public void ResetSpawner()
    {
        StopSpawning();
        ClearAllEnemies();
        currentWave = 0;
        waveInProgress = false;
        enemiesSpawnedInWave = 0;
    }
    
    /// <summary>
    /// 获取当前敌人数量
    /// </summary>
    public int GetCurrentEnemyCount()
    {
        return spawnedEnemies.Count;
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        switch (areaType)
        {
            case SpawnAreaType.Circle:
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
                break;
            case SpawnAreaType.Rectangle:
                Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0));
                break;
            case SpawnAreaType.Points:
                foreach (Transform point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                    }
                }
                break;
        }
        
        // 绘制玩家检测范围
        if (spawnOnlyWhenPlayerNear)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        }
    }
}

/// <summary>
/// 生成区域类型
/// </summary>
public enum SpawnAreaType
{
    [LabelText("圆形区域")] Circle,
    [LabelText("矩形区域")] Rectangle,
    [LabelText("指定点位")] Points
}

/// <summary>
/// 敌人生成数据
/// </summary>
[System.Serializable]
[InlineProperty]
public class EnemySpawnData
{
    [HorizontalGroup("敌人数据")]
    [VerticalGroup("敌人数据/敌人配置")]
    [LabelText("敌人预制体")]
    [Required("必须指定敌人预制体")]
    [AssetsOnly]
    public GameObject enemyPrefab;
    
    [VerticalGroup("敌人数据/敌人配置")]
    [LabelText("生成权重")]
    [PropertyRange(0.1f, 10f)]
    [InfoBox("权重越高，生成概率越大")]
    public float spawnWeight = 1f;
    
    [VerticalGroup("敌人数据/等级设置")]
    [LabelText("等级范围")]
    [MinMaxSlider(1, 100, true)]
    public Vector2Int levelRange = new Vector2Int(1, 1);
    
    [VerticalGroup("敌人数据/属性修饰符")]
    [LabelText("生命值倍数")]
    [PropertyRange(0.1f, 5f)]
    public float healthMultiplier = 1f;
    
    [VerticalGroup("敌人数据/属性修饰符")]
    [LabelText("伤害倍数")]
    [PropertyRange(0.1f, 5f)]
    public float damageMultiplier = 1f;
    
    [VerticalGroup("敌人数据/属性修饰符")]
    [LabelText("速度倍数")]
    [PropertyRange(0.1f, 3f)]
    public float speedMultiplier = 1f;
}

/// <summary>
/// 波次数据
/// </summary>
[System.Serializable]
[InlineProperty]
public class WaveData
{
    [HorizontalGroup("波次数据")]
    [VerticalGroup("波次数据/波次信息")]
    [LabelText("波次名称")]
    [InfoBox("用于标识和显示的波次名称")]
    public string waveName;
    
    [VerticalGroup("波次数据/波次信息")]
    [LabelText("敌人总数")]
    [PropertyRange(1, 100)]
    public int enemyCount;
    
    [VerticalGroup("波次数据/波次信息")]
    [LabelText("生成间隔")]
    [PropertyRange(0.1f, 10f)]
    [SuffixLabel("秒")]
    public float spawnInterval = 1f;
    
    [VerticalGroup("波次数据/敌人配置")]
    [LabelText("波次敌人列表")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    [InfoBox("如果为空，将使用全局敌人生成列表")]
    public List<EnemySpawnData> waveEnemies = new List<EnemySpawnData>();
}