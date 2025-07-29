using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 敌人击杀统计管理器
/// 负责统计各种敌人的击杀数量，为任务系统提供数据支持
/// </summary>
public class EnemyKillTracker : MonoBehaviour
{
    public static EnemyKillTracker Instance { get; private set; }
    
    [Header("调试信息")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 击杀统计字典
    private Dictionary<string, int> killCounts = new Dictionary<string, int>();
    
    // 击杀事件
    public event Action<string, int> OnEnemyKilled;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeKillTracker();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 延迟注册，确保所有敌人都已初始化
        Invoke("RegisterExistingEnemies", 0.5f);
    }
    
    private void InitializeKillTracker()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[EnemyKillTracker] 击杀统计管理器初始化完成");
        }
    }
    
    /// <summary>
    /// 注册场景中已存在的敌人
    /// </summary>
    private void RegisterExistingEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyKillTracker] 注册了 {enemies.Length} 个敌人的死亡事件");
        }
    }
    
    /// <summary>
    /// 注册单个敌人的死亡事件
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.OnDeath += OnEnemyDeath;
        }
    }
    
    /// <summary>
    /// 注销敌人的死亡事件
    /// </summary>
    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.OnDeath -= OnEnemyDeath;
        }
    }
    
    /// <summary>
    /// 敌人死亡事件处理
    /// </summary>
    private void OnEnemyDeath(Enemy enemy)
    {
        if (enemy == null) return;
        
        string enemyType = enemy.GetType().Name;
        
        // 更新击杀计数
        if (!killCounts.ContainsKey(enemyType))
        {
            killCounts[enemyType] = 0;
        }
        
        killCounts[enemyType]++;
        
        // 触发击杀事件
        OnEnemyKilled?.Invoke(enemyType, killCounts[enemyType]);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyKillTracker] 击杀 {GetEnemyDisplayName(enemyType)}，总计: {killCounts[enemyType]}");
        }
    }
    
    /// <summary>
    /// 获取指定敌人类型的击杀数量
    /// </summary>
    public int GetKillCount(string enemyType)
    {
        return killCounts.ContainsKey(enemyType) ? killCounts[enemyType] : 0;
    }
    
    /// <summary>
    /// 获取所有击杀统计
    /// </summary>
    public Dictionary<string, int> GetAllKillCounts()
    {
        return new Dictionary<string, int>(killCounts);
    }
    
    /// <summary>
    /// 重置指定敌人类型的击杀计数
    /// </summary>
    public void ResetKillCount(string enemyType)
    {
        if (killCounts.ContainsKey(enemyType))
        {
            killCounts[enemyType] = 0;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyKillTracker] 重置 {enemyType} 的击杀计数");
            }
        }
    }
    
    /// <summary>
    /// 重置所有击杀计数
    /// </summary>
    public void ResetAllKillCounts()
    {
        killCounts.Clear();
        
        if (enableDebugLogs)
        {
            Debug.Log("[EnemyKillTracker] 重置所有击杀计数");
        }
    }
    
    /// <summary>
    /// 手动添加击杀计数（用于测试或特殊情况）
    /// </summary>
    public void AddKillCount(string enemyType, int count = 1)
    {
        if (!killCounts.ContainsKey(enemyType))
        {
            killCounts[enemyType] = 0;
        }
        
        killCounts[enemyType] += count;
        
        // 触发击杀事件
        OnEnemyKilled?.Invoke(enemyType, killCounts[enemyType]);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyKillTracker] 手动添加 {enemyType} 击杀计数 +{count}，总计: {killCounts[enemyType]}");
        }
    }
    
    /// <summary>
    /// 获取敌人类型的显示名称
    /// </summary>
    private string GetEnemyDisplayName(string enemyType)
    {
        switch (enemyType)
        {
            case "WildBoar": return "野猪";
            case "Goblin": return "哥布林";
            case "Skeleton": return "骷髅";
            case "Orc": return "兽人";
            default: return enemyType;
        }
    }
    
    /// <summary>
    /// 保存击杀数据到PlayerPrefs（简单持久化）
    /// </summary>
    public void SaveKillData()
    {
        foreach (var kvp in killCounts)
        {
            PlayerPrefs.SetInt($"KillCount_{kvp.Key}", kvp.Value);
        }
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
        {
            Debug.Log("[EnemyKillTracker] 击杀数据已保存");
        }
    }
    
    /// <summary>
    /// 从PlayerPrefs加载击杀数据
    /// </summary>
    public void LoadKillData()
    {
        // 这里可以根据需要加载已知的敌人类型
        string[] knownEnemyTypes = { "WildBoar", "Goblin", "Skeleton", "Orc" };
        
        foreach (string enemyType in knownEnemyTypes)
        {
            int count = PlayerPrefs.GetInt($"KillCount_{enemyType}", 0);
            if (count > 0)
            {
                killCounts[enemyType] = count;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("[EnemyKillTracker] 击杀数据已加载");
        }
    }
    
    void OnDestroy()
    {
        // 清理事件订阅
        OnEnemyKilled = null;
    }
}