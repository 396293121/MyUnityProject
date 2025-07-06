using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 敌人生成器 - 负责动态生成和管理敌人
/// 从原Phaser项目的敌人生成系统迁移而来
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    public List<EnemySpawnData> enemySpawnList = new List<EnemySpawnData>();
    public int maxEnemies = 10;              // 最大敌人数量
    public float spawnInterval = 3f;         // 生成间隔
    public float spawnRadius = 10f;          // 生成半径
    public LayerMask obstacleLayerMask = 1;  // 障碍物图层
    
    [Header("生成区域")]
    public SpawnAreaType areaType = SpawnAreaType.Circle;
    public Vector2 areaSize = new Vector2(10f, 10f); // 矩形区域大小
    public List<Transform> spawnPoints = new List<Transform>(); // 指定生成点
    
    [Header("玩家检测")]
    public float playerDetectionRange = 15f; // 玩家检测范围
    public bool spawnOnlyWhenPlayerNear = true; // 只在玩家附近生成
    
    [Header("波次设置")]
    public bool useWaveSystem = false;       // 使用波次系统
    public List<WaveData> waves = new List<WaveData>();
    public float timeBetweenWaves = 10f;     // 波次间隔
    
    [Header("调试设置")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.red;
    
    // 私有变量
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform playerTransform;
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    
    // 波次系统
    private int currentWave = 0;
    private bool waveInProgress = false;
    private int enemiesSpawnedInWave = 0;
    
    // 事件
    public System.Action<GameObject> OnEnemySpawned;
    public System.Action<GameObject> OnEnemyDestroyed;
    public System.Action<int> OnWaveStarted;
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
    Circle,
    Rectangle,
    Points
}

/// <summary>
/// 敌人生成数据
/// </summary>
[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public float spawnWeight = 1f;
    public Vector2Int levelRange = new Vector2Int(1, 1);
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float speedMultiplier = 1f;
}

/// <summary>
/// 波次数据
/// </summary>
[System.Serializable]
public class WaveData
{
    public string waveName;
    public int enemyCount;
    public float spawnInterval = 1f;
    public List<EnemySpawnData> waveEnemies = new List<EnemySpawnData>();
}