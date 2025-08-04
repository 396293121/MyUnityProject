using UnityEngine;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人击杀统计管理器
/// 负责统计各种敌人的击杀数量，为任务系统提供数据支持
/// 支持基于敌人ID的灵活统计和任务系统集成
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class EnemyKillTracker : MonoBehaviour
{
    public static EnemyKillTracker Instance { get; private set; }
    
    [BoxGroup("配置设置")]
    [LabelText("启用调试日志")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [BoxGroup("配置设置")]
    [LabelText("使用敌人ID统计")]
    [InfoBox("启用后将使用敌人配置中的enemyId进行统计，否则使用类名")]
    [SerializeField] private bool useEnemyId = true;
    
    // 击杀统计字典 - 基于敌人ID或类名
    private Dictionary<string, int> killCounts = new Dictionary<string, int>();
    
    // 击杀事件 - 参数：敌人标识符、击杀数量、敌人实例
    public event Action<string, int, Enemy> OnEnemyKilled;
    
    // ID字典引用
    private GameIdDictionary idDictionary
    {
        get { return GameIdDictionaryManager.GetIdDictionary(); }
    }
    
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
      //  Invoke("RegisterExistingEnemies", 0.5f);
    }
    
    private void InitializeKillTracker()
    {
        if (enableDebugLogs)
        {
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
        
        string enemyIdentifier = GetEnemyIdentifier(enemy);
        
        // 更新击杀计数
        if (!killCounts.ContainsKey(enemyIdentifier))
        {
            killCounts[enemyIdentifier] = 0;
        }
        
        killCounts[enemyIdentifier]++;
        
        // 触发击杀事件，传递敌人实例以便任务系统获取更多信息
        OnEnemyKilled?.Invoke(enemyIdentifier, killCounts[enemyIdentifier], enemy);
        
        if (enableDebugLogs)
        {
            string displayName = GetEnemyDisplayName(enemyIdentifier);
            Debug.Log($"[EnemyKillTracker] 击杀 {displayName} (ID: {enemyIdentifier})，总计: {killCounts[enemyIdentifier]}");
        }
    }
    
    /// <summary>
    /// 获取敌人标识符（ID或类名）
    /// </summary>
    private string GetEnemyIdentifier(Enemy enemy)
    {
        if (useEnemyId && enemy.enemyConfig != null && !string.IsNullOrEmpty(enemy.enemyConfig.enemyId))
        {
            return enemy.enemyConfig.enemyId;
        }
        
        // 回退到使用类名
        return enemy.GetType().Name;
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
    public void AddKillCount(string enemyIdentifier, int count = 1)
    {
        if (!killCounts.ContainsKey(enemyIdentifier))
        {
            killCounts[enemyIdentifier] = 0;
        }
        
        killCounts[enemyIdentifier] += count;
        
        // 触发击杀事件（手动添加时敌人实例为null）
        OnEnemyKilled?.Invoke(enemyIdentifier, killCounts[enemyIdentifier], null);
        
        if (enableDebugLogs)
        {
            string displayName = GetEnemyDisplayName(enemyIdentifier);
            Debug.Log($"[EnemyKillTracker] 手动添加 {displayName} (ID: {enemyIdentifier}) 击杀计数 +{count}，总计: {killCounts[enemyIdentifier]}");
        }
    }
    
    /// <summary>
    /// 检查是否满足击杀任务条件
    /// </summary>
    public bool CheckKillObjective(string enemyId, int requiredCount)
    {
        int currentCount = GetKillCount(enemyId);
        return currentCount >= requiredCount;
    }
    
    /// <summary>
    /// 获取击杀任务进度
    /// </summary>
    public float GetKillProgress(string enemyId, int requiredCount)
    {
        int currentCount = GetKillCount(enemyId);
        return requiredCount > 0 ? (float)currentCount / requiredCount : 0f;
    }
    
    /// <summary>
    /// 获取所有可用于击杀任务的敌人ID
    /// </summary>
    public List<string> GetAvailableKillTargets()
    {
        if (idDictionary != null)
        {
            return idDictionary.GetKillTargetEnemyIds();
        }
        
        // 回退到已击杀的敌人列表
        return new List<string>(killCounts.Keys);
    }
    
    /// <summary>
    /// 获取所有可用于BOSS任务的敌人ID
    /// </summary>
    public List<string> GetAvailableBossTargets()
    {
        if (idDictionary != null)
        {
            return idDictionary.GetBossTargetEnemyIds();
        }
        
        return new List<string>();
    }
    
    /// <summary>
    /// 检查敌人是否可以作为击杀目标
    /// </summary>
    public bool CanBeKillTarget(string enemyId)
    {
        if (idDictionary != null)
        {
            return idDictionary.GetKillTargetEnemyIds().Contains(enemyId);
        }
        
        return true; // 默认所有敌人都可以作为击杀目标
    }
    
    /// <summary>
    /// 检查敌人是否可以作为BOSS目标
    /// </summary>
    public bool CanBeBossTarget(string enemyId)
    {
        if (idDictionary != null)
        {
            return idDictionary.GetBossTargetEnemyIds().Contains(enemyId);
        }
        
        return false; // 默认不是BOSS目标
    }
    
    #region Inspector调试面板
    
    [BoxGroup("统计信息")]
    [LabelText("当前击杀统计")]
    [ShowInInspector]
    [ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "敌人ID/类名", ValueLabel = "击杀数量")]
    private Dictionary<string, int> KillCountsDisplay => killCounts;
    
    [BoxGroup("统计信息")]
    [LabelText("总击杀数")]
    [ShowInInspector]
    [ReadOnly]
    private int TotalKills
    {
        get
        {
            int total = 0;
            foreach (var count in killCounts.Values)
            {
                total += count;
            }
            return total;
        }
    }
    
    [BoxGroup("统计信息")]
    [LabelText("已击杀敌人种类")]
    [ShowInInspector]
    [ReadOnly]
    private int EnemyTypesKilled => killCounts.Count;
    
    [BoxGroup("调试工具")]
    [Button("刷新ID字典")]
    [InfoBox("重新加载敌人ID字典")]
    private void RefreshIdDictionary()
    {
        if (GameIdDictionaryManager.IsIdDictionaryLoaded())
        {
            GameIdDictionaryManager.Instance.ReloadIdDictionary();
            Debug.Log("[EnemyKillTracker] ID字典已刷新");
        }
        else
        {
            Debug.LogWarning("[EnemyKillTracker] ID字典未加载");
        }
    }
    
    [BoxGroup("调试工具")]
    [Button("显示可用击杀目标")]
    private void ShowAvailableKillTargets()
    {
        var targets = GetAvailableKillTargets();
        Debug.Log($"[EnemyKillTracker] 可用击杀目标 ({targets.Count}): {string.Join(", ", targets)}");
    }
    
    [BoxGroup("调试工具")]
    [Button("显示可用BOSS目标")]
    private void ShowAvailableBossTargets()
    {
        var targets = GetAvailableBossTargets();
        Debug.Log($"[EnemyKillTracker] 可用BOSS目标 ({targets.Count}): {string.Join(", ", targets)}");
    }
    
    #endregion
    
    /// <summary>
    /// 获取敌人标识符的显示名称
    /// </summary>
    private string GetEnemyDisplayName(string enemyIdentifier)
    {
        // 优先从ID字典获取显示名称
        if (idDictionary != null)
        {
            string displayName = idDictionary.GetDisplayName(enemyIdentifier, ObjectiveType.KillEnemy);
            if (!string.IsNullOrEmpty(displayName) && displayName != enemyIdentifier)
            {
                return displayName;
            }
        }
        return null;
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
    
    void OnDestroy()
    {
        // 清理事件订阅
        OnEnemyKilled = null;
    }
}