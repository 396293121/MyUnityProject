using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏事件数据基类
/// </summary>
public abstract class GameEventData
{
    public float timestamp;
    
    protected GameEventData()
    {
        timestamp = Time.time;
    }
}

/// <summary>
/// 玩家事件数据
/// </summary>
public class PlayerEventData : GameEventData
{
    public float health;
    public float maxHealth;
    public Vector3 position;
    
    public PlayerEventData(float health, float maxHealth, Vector3 position) : base()
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.position = position;
    }
}

/// <summary>
/// 伤害事件数据
/// </summary>
public class DamageEventData : GameEventData
{
    public float damage;
    public Vector3 position;
    public GameObject attacker;
    public GameObject target;
    
    public DamageEventData(float damage, Vector3 position, GameObject attacker, GameObject target) : base()
    {
        this.damage = damage;
        this.position = position;
        this.attacker = attacker;
        this.target = target;
    }
}

/// <summary>
/// 技能事件数据
/// </summary>
public class SkillEventData : GameEventData
{
    public string skillName;
    public Vector3 castPosition;
    public GameObject caster;
    public float manaCost;
    
    public SkillEventData(string skillName, Vector3 castPosition, GameObject caster, float manaCost) : base()
    {
        this.skillName = skillName;
        this.castPosition = castPosition;
        this.caster = caster;
        this.manaCost = manaCost;
    }
}

/// <summary>
/// 增强的游戏事件系统 - 用于解耦组件间的依赖关系
/// 优化目标：提高代码的可维护性和扩展性
/// </summary>
public class GameEventSystem : MonoBehaviour
{
    // 单例模式
    public static GameEventSystem Instance { get; private set; }
    
    [TabGroup("事件统计")]
    [ShowInInspector, ReadOnly]
    [LabelText("事件触发次数")]
    private Dictionary<string, int> eventCounts = new Dictionary<string, int>();
    
    [TabGroup("事件统计")]
    [ShowInInspector, ReadOnly]
    [LabelText("最近事件")]
    private List<string> recentEvents = new List<string>();
    
    [TabGroup("调试设置")]
    [LabelText("启用事件日志")]
    public bool enableEventLogging = true;
    
    [TabGroup("调试设置")]
    [LabelText("最大日志数量")]
    [Range(10, 100)]
    public int maxLogCount = 50;
    
    #region 玩家相关事件
    
    [TabGroup("玩家事件")]
    [LabelText("玩家受伤事件")]
    public static UnityEvent<DamageEventData> OnPlayerTakeDamage = new UnityEvent<DamageEventData>();
    
    [TabGroup("玩家事件")]
    [LabelText("玩家治疗事件")]
    public static UnityEvent<PlayerEventData> OnPlayerHeal = new UnityEvent<PlayerEventData>();
    
    [TabGroup("玩家事件")]
    [LabelText("玩家死亡事件")]
    public static UnityEvent<PlayerEventData> OnPlayerDeath = new UnityEvent<PlayerEventData>();
    
    [TabGroup("玩家事件")]
    [LabelText("玩家重生事件")]
    public static UnityEvent<PlayerEventData> OnPlayerRespawn = new UnityEvent<PlayerEventData>();
    
    [TabGroup("玩家事件")]
    [LabelText("玩家攻击事件")]
    public static UnityEvent<DamageEventData> OnPlayerAttack = new UnityEvent<DamageEventData>();
    
    #endregion
    
    #region 技能相关事件
    
    [TabGroup("技能事件")]
    [LabelText("技能释放事件")]
    public static UnityEvent<SkillEventData> OnSkillCast = new UnityEvent<SkillEventData>();
    
    [TabGroup("技能事件")]
    [LabelText("技能结束事件")]
    public static UnityEvent<SkillEventData> OnSkillEnd = new UnityEvent<SkillEventData>();
    
    [TabGroup("技能事件")]
    [LabelText("技能冷却完成事件")]
    public static UnityEvent<string> OnSkillCooldownComplete = new UnityEvent<string>();
    
    #endregion
    
    #region 敌人相关事件
    
    [TabGroup("敌人事件")]
    [LabelText("敌人受伤事件")]
    public static UnityEvent<DamageEventData> OnEnemyTakeDamage = new UnityEvent<DamageEventData>();
    
    [TabGroup("敌人事件")]
    [LabelText("敌人死亡事件")]
    public static UnityEvent<GameObject> OnEnemyDeath = new UnityEvent<GameObject>();
    
    [TabGroup("敌人事件")]
    [LabelText("敌人发现玩家事件")]
    public static UnityEvent<GameObject> OnEnemyDetectPlayer = new UnityEvent<GameObject>();
    
    #endregion
    
    #region 游戏状态事件
    
    [TabGroup("游戏状态")]
    [LabelText("游戏暂停事件")]
    public static UnityEvent<bool> OnGamePause = new UnityEvent<bool>();
    
    [TabGroup("游戏状态")]
    [LabelText("关卡完成事件")]
    public static UnityEvent<string> OnLevelComplete = new UnityEvent<string>();
    
    [TabGroup("游戏状态")]
    [LabelText("游戏结束事件")]
    public static UnityEvent<bool> OnGameOver = new UnityEvent<bool>();
    
    #endregion
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化事件系统
    /// </summary>
    private void InitializeEventSystem()
    {
        // 注册事件监听器用于统计
        RegisterEventListeners();
        
        Debug.Log("游戏事件系统初始化完成");
    }
    
    /// <summary>
    /// 注册事件监听器
    /// </summary>
    private void RegisterEventListeners()
    {
        OnPlayerTakeDamage.AddListener(data => LogEvent("玩家受伤", $"伤害: {data.damage}"));
        OnPlayerHeal.AddListener(data => LogEvent("玩家治疗", $"生命值: {data.health}/{data.maxHealth}"));
        OnPlayerDeath.AddListener(data => LogEvent("玩家死亡", $"位置: {data.position}"));
        OnPlayerRespawn.AddListener(data => LogEvent("玩家重生", $"位置: {data.position}"));
        OnPlayerAttack.AddListener(data => LogEvent("玩家攻击", $"伤害: {data.damage}"));
        
        OnSkillCast.AddListener(data => LogEvent("技能释放", $"技能: {data.skillName}, 法力消耗: {data.manaCost}"));
        OnSkillEnd.AddListener(data => LogEvent("技能结束", $"技能: {data.skillName}"));
        OnSkillCooldownComplete.AddListener(skillName => LogEvent("技能冷却完成", $"技能: {skillName}"));
        
        OnEnemyTakeDamage.AddListener(data => LogEvent("敌人受伤", $"伤害: {data.damage}"));
        OnEnemyDeath.AddListener(enemy => LogEvent("敌人死亡", $"敌人: {enemy.name}"));
        OnEnemyDetectPlayer.AddListener(enemy => LogEvent("敌人发现玩家", $"敌人: {enemy.name}"));
        
        OnGamePause.AddListener(paused => LogEvent("游戏暂停", $"暂停状态: {paused}"));
        OnLevelComplete.AddListener(level => LogEvent("关卡完成", $"关卡: {level}"));
        OnGameOver.AddListener(won => LogEvent("游戏结束", $"胜利: {won}"));
    }
    
    /// <summary>
    /// 记录事件日志
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="details">事件详情</param>
    private void LogEvent(string eventName, string details = "")
    {
        // 更新事件计数
        if (eventCounts.ContainsKey(eventName))
        {
            eventCounts[eventName]++;
        }
        else
        {
            eventCounts[eventName] = 1;
        }
        
        // 添加到最近事件列表
        string logEntry = $"[{Time.time:F2}] {eventName}";
        if (!string.IsNullOrEmpty(details))
        {
            logEntry += $" - {details}";
        }
        
        recentEvents.Add(logEntry);
        
        // 限制日志数量
        if (recentEvents.Count > maxLogCount)
        {
            recentEvents.RemoveAt(0);
        }
        
        // 输出到控制台
        if (enableEventLogging)
        {
            Debug.Log($"[事件系统] {logEntry}");
        }
    }
    
    #region 便捷方法
    
    /// <summary>
    /// 触发玩家受伤事件
    /// </summary>
    public static void TriggerPlayerTakeDamage(float damage, Vector3 position, GameObject attacker, GameObject player)
    {
        var eventData = new DamageEventData(damage, position, attacker, player);
        OnPlayerTakeDamage?.Invoke(eventData);
    }
    
    /// <summary>
    /// 触发玩家治疗事件
    /// </summary>
    public static void TriggerPlayerHeal(float health, float maxHealth, Vector3 position)
    {
        var eventData = new PlayerEventData(health, maxHealth, position);
        OnPlayerHeal?.Invoke(eventData);
    }
    
    /// <summary>
    /// 触发技能释放事件
    /// </summary>
    public static void TriggerSkillCast(string skillName, Vector3 position, GameObject caster, float manaCost)
    {
        var eventData = new SkillEventData(skillName, position, caster, manaCost);
        OnSkillCast?.Invoke(eventData);
    }
    
    /// <summary>
    /// 触发敌人受伤事件
    /// </summary>
    public static void TriggerEnemyTakeDamage(float damage, Vector3 position, GameObject attacker, GameObject enemy)
    {
        var eventData = new DamageEventData(damage, position, attacker, enemy);
        OnEnemyTakeDamage?.Invoke(eventData);
    }
    
    #endregion
    
    #region 调试功能
    
    [TabGroup("调试")]
    [Button("清空事件统计")]
    private void ClearEventStats()
    {
        eventCounts.Clear();
        recentEvents.Clear();
        Debug.Log("事件统计已清空");
    }
    
    [TabGroup("调试")]
    [Button("打印事件统计")]
    private void PrintEventStats()
    {
        Debug.Log("=== 事件统计 ===");
        foreach (var kvp in eventCounts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} 次");
        }
    }
    
    [TabGroup("调试")]
    [Button("测试事件")]
    private void TestEvent()
    {
        TriggerPlayerTakeDamage(10f, Vector3.zero, null, null);
        Debug.Log("测试事件已触发");
    }
    
    /// <summary>
    /// 获取事件触发次数
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>触发次数</returns>
    public int GetEventCount(string eventName)
    {
        return eventCounts.ContainsKey(eventName) ? eventCounts[eventName] : 0;
    }
    
    /// <summary>
    /// 获取最近的事件日志
    /// </summary>
    /// <param name="count">获取数量</param>
    /// <returns>事件日志列表</returns>
    public List<string> GetRecentEvents(int count = 10)
    {
        int startIndex = Mathf.Max(0, recentEvents.Count - count);
        return recentEvents.GetRange(startIndex, recentEvents.Count - startIndex);
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 清理事件监听器
        OnPlayerTakeDamage?.RemoveAllListeners();
        OnPlayerHeal?.RemoveAllListeners();
        OnPlayerDeath?.RemoveAllListeners();
        OnPlayerRespawn?.RemoveAllListeners();
        OnPlayerAttack?.RemoveAllListeners();
        
        OnSkillCast?.RemoveAllListeners();
        OnSkillEnd?.RemoveAllListeners();
        OnSkillCooldownComplete?.RemoveAllListeners();
        
        OnEnemyTakeDamage?.RemoveAllListeners();
        OnEnemyDeath?.RemoveAllListeners();
        OnEnemyDetectPlayer?.RemoveAllListeners();
        
        OnGamePause?.RemoveAllListeners();
        OnLevelComplete?.RemoveAllListeners();
        OnGameOver?.RemoveAllListeners();
    }
}