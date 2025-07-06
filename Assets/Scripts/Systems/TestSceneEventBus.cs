using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 测试场景事件总线
/// 基于Phaser项目中的事件系统，提供解耦的事件通信机制
/// </summary>
public class TestSceneEventBus
{
    #region 事件定义
    /// <summary>
    /// 通用事件委托
    /// </summary>
    public delegate void GameEvent(object data);
    
    /// <summary>
    /// 玩家相关事件
    /// </summary>
    public event Action<float, float> OnPlayerHealthChanged;
    public event Action<float, float> OnPlayerManaChanged;
    public event Action<int> OnPlayerLevelUp;
    public event Action<int> OnPlayerExperienceGained;
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;
    public event Action<Vector3> OnPlayerMove;
    public event Action OnPlayerAttack;
    public event Action<string> OnPlayerSkillUsed;
    
    /// <summary>
    /// 敌人相关事件
    /// </summary>
    public event Action<Enemy> OnEnemySpawned;
    public event Action<Enemy> OnEnemyDeath;
    public event Action<Enemy> OnEnemyAttack;
    public event Action<string, int> OnEnemyWaveSpawned;
    public event Action OnAllEnemiesDefeated;
    
    /// <summary>
    /// UI相关事件
    /// </summary>
    public event Action<bool> OnGamePaused;
    public event Action OnMenuOpened;
    public event Action OnMenuClosed;
    public event Action<string> OnUINotification;
    public event Action<float> OnUIFadeIn;
    public event Action<float> OnUIFadeOut;
    
    /// <summary>
    /// 音频相关事件
    /// </summary>
    public event Action<string> OnSoundEffectPlay;
    public event Action<string> OnMusicPlay;
    public event Action OnMusicStop;
    public event Action<float> OnVolumeChanged;
    
    /// <summary>
    /// 游戏状态相关事件
    /// </summary>
    public event Action OnGameStart;
    public event Action OnGameEnd;
    public event Action OnGameWin;
    public event Action OnGameLose;
    public event Action OnSceneLoaded;
    public event Action OnSceneUnloaded;
    
    /// <summary>
    /// 输入相关事件
    /// </summary>
    public event Action<Vector2> OnMovementInput;
    public event Action OnAttackInput;
    public event Action<int> OnSkillInput;
    public event Action OnPauseInput;
    public event Action OnMenuInput;
    #endregion
    
    #region 私有字段
    private Dictionary<string, List<GameEvent>> eventListeners = new Dictionary<string, List<GameEvent>>();
    private Queue<EventData> eventQueue = new Queue<EventData>();
    private bool isProcessingEvents = false;
    private int maxEventsPerFrame = 10;
    
    /// <summary>
    /// 事件数据结构
    /// </summary>
    private struct EventData
    {
        public string eventName;
        public object data;
        public float timestamp;
    }
    #endregion
    
    #region 构造函数
    public TestSceneEventBus()
    {
        Debug.Log("[TestSceneEventBus] 事件总线初始化完成");
    }
    #endregion
    
    #region 事件注册和注销
    /// <summary>
    /// 注册事件监听器
    /// </summary>
    public void RegisterListener(string eventName, GameEvent listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;
        
        if (!eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName] = new List<GameEvent>();
        }
        
        if (!eventListeners[eventName].Contains(listener))
        {
            eventListeners[eventName].Add(listener);
            Debug.Log($"[TestSceneEventBus] 注册事件监听器: {eventName}");
        }
    }
    
    /// <summary>
    /// 注销事件监听器
    /// </summary>
    public void UnregisterListener(string eventName, GameEvent listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;
        
        if (eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName].Remove(listener);
            
            // 如果没有监听器了，移除事件键
            if (eventListeners[eventName].Count == 0)
            {
                eventListeners.Remove(eventName);
            }
            
            Debug.Log($"[TestSceneEventBus] 注销事件监听器: {eventName}");
        }
    }
    
    /// <summary>
    /// 清理所有事件监听器
    /// </summary>
    public void ClearAllListeners()
    {
        eventListeners.Clear();
        eventQueue.Clear();
        Debug.Log("[TestSceneEventBus] 清理所有事件监听器");
    }
    
    /// <summary>
    /// 清理指定事件的所有监听器
    /// </summary>
    public void ClearListeners(string eventName)
    {
        if (eventListeners.ContainsKey(eventName))
        {
            eventListeners.Remove(eventName);
            Debug.Log($"[TestSceneEventBus] 清理事件监听器: {eventName}");
        }
    }
    #endregion
    
    #region 事件触发
    /// <summary>
    /// 立即触发事件
    /// </summary>
    public void TriggerEvent(string eventName, object data = null)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        
        // 触发通用事件监听器
        if (eventListeners.ContainsKey(eventName))
        {
            var listeners = eventListeners[eventName];
            foreach (var listener in listeners.ToArray()) // 使用ToArray避免修改集合时的异常
            {
                try
                {
                    listener?.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[TestSceneEventBus] 事件处理异常 {eventName}: {e.Message}");
                }
            }
        }
        
        // 触发特定类型的事件
        TriggerSpecificEvent(eventName, data);
        
        Debug.Log($"[TestSceneEventBus] 触发事件: {eventName}");
    }
    
    /// <summary>
    /// 延迟触发事件（加入队列）
    /// </summary>
    public void TriggerEventDelayed(string eventName, object data = null)
    {
        eventQueue.Enqueue(new EventData
        {
            eventName = eventName,
            data = data,
            timestamp = Time.time
        });
    }
    
    /// <summary>
    /// 处理队列中的事件
    /// </summary>
    public void ProcessEventQueue()
    {
        if (isProcessingEvents) return;
        
        isProcessingEvents = true;
        int processedCount = 0;
        
        while (eventQueue.Count > 0 && processedCount < maxEventsPerFrame)
        {
            var eventData = eventQueue.Dequeue();
            TriggerEvent(eventData.eventName, eventData.data);
            processedCount++;
        }
        
        isProcessingEvents = false;
    }
    #endregion
    
    #region 特定事件触发方法
    /// <summary>
    /// 触发特定类型的事件
    /// </summary>
    private void TriggerSpecificEvent(string eventName, object data)
    {
        try
        {
            switch (eventName)
            {
                // 玩家事件
                case "PlayerHealthChanged":
                    if (data is ValueChangeData healthData)
                        OnPlayerHealthChanged?.Invoke(healthData.oldValue, healthData.newValue);
                    break;
                    
                case "PlayerManaChanged":
                    if (data is ValueChangeData manaData)
                        OnPlayerManaChanged?.Invoke(manaData.oldValue, manaData.newValue);
                    break;
                    
                case "PlayerLevelUp":
                    if (data is int level)
                        OnPlayerLevelUp?.Invoke(level);
                    break;
                    
                case "PlayerExperienceGained":
                    if (data is int exp)
                        OnPlayerExperienceGained?.Invoke(exp);
                    break;
                    
                case "PlayerDeath":
                    OnPlayerDeath?.Invoke();
                    break;
                    
                case "PlayerRespawn":
                    OnPlayerRespawn?.Invoke();
                    break;
                    
                case "PlayerMove":
                    if (data is Vector3 position)
                        OnPlayerMove?.Invoke(position);
                    break;
                    
                case "PlayerAttack":
                    OnPlayerAttack?.Invoke();
                    break;
                    
                case "PlayerSkillUsed":
                    if (data is string skillName)
                        OnPlayerSkillUsed?.Invoke(skillName);
                    break;
                
                // 敌人事件
                case "EnemySpawned":
                    if (data is Enemy enemy)
                        OnEnemySpawned?.Invoke(enemy);
                    break;
                    
                case "EnemyDeath":
                    if (data != null)
                    {
                        var enemyData = data.GetType().GetProperty("enemy")?.GetValue(data) as Enemy;
                        if (enemyData != null)
                            OnEnemyDeath?.Invoke(enemyData);
                    }
                    break;
                    
                case "EnemyAttack":
                    if (data != null)
                    {
                        var enemyData = data.GetType().GetProperty("enemy")?.GetValue(data) as Enemy;
                        if (enemyData != null)
                            OnEnemyAttack?.Invoke(enemyData);
                    }
                    break;
                    
                case "EnemyWaveSpawned":
                    if (data != null)
                    {
                        var waveNameProp = data.GetType().GetProperty("waveName");
                        var enemyCountProp = data.GetType().GetProperty("enemyCount");
                        if (waveNameProp != null && enemyCountProp != null)
                        {
                            string waveName = waveNameProp.GetValue(data) as string;
                            int enemyCount = (int)enemyCountProp.GetValue(data);
                            OnEnemyWaveSpawned?.Invoke(waveName, enemyCount);
                        }
                    }
                    break;
                    
                case "AllEnemiesDefeated":
                    OnAllEnemiesDefeated?.Invoke();
                    break;
                
                // UI事件
                case "GamePaused":
                    if (data is bool isPaused)
                        OnGamePaused?.Invoke(isPaused);
                    break;
                    
                case "MenuOpened":
                    OnMenuOpened?.Invoke();
                    break;
                    
                case "MenuClosed":
                    OnMenuClosed?.Invoke();
                    break;
                    
                case "UINotification":
                    if (data is string message)
                        OnUINotification?.Invoke(message);
                    break;
                
                // 音频事件
                case "SoundEffectPlay":
                    if (data is string soundName)
                        OnSoundEffectPlay?.Invoke(soundName);
                    break;
                    
                case "MusicPlay":
                    if (data is string musicName)
                        OnMusicPlay?.Invoke(musicName);
                    break;
                    
                case "MusicStop":
                    OnMusicStop?.Invoke();
                    break;
                
                // 游戏状态事件
                case "GameStart":
                    OnGameStart?.Invoke();
                    break;
                    
                case "GameEnd":
                    OnGameEnd?.Invoke();
                    break;
                    
                case "GameWin":
                    OnGameWin?.Invoke();
                    break;
                    
                case "GameLose":
                    OnGameLose?.Invoke();
                    break;
                    
                case "SceneLoaded":
                    OnSceneLoaded?.Invoke();
                    break;
                    
                case "SceneUnloaded":
                    OnSceneUnloaded?.Invoke();
                    break;
                
                // 输入事件
                case "MovementInput":
                    if (data is Vector2 movement)
                        OnMovementInput?.Invoke(movement);
                    break;
                    
                case "AttackInput":
                    OnAttackInput?.Invoke();
                    break;
                    
                case "SkillInput":
                    if (data is int skillIndex)
                        OnSkillInput?.Invoke(skillIndex);
                    break;
                    
                case "PauseInput":
                    OnPauseInput?.Invoke();
                    break;
                    
                case "MenuInput":
                    OnMenuInput?.Invoke();
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TestSceneEventBus] 特定事件处理异常 {eventName}: {e.Message}");
        }
    }
    #endregion
    
    #region 便捷方法
    /// <summary>
    /// 触发玩家生命值变化事件
    /// </summary>
    public void TriggerPlayerHealthChanged(float oldHealth, float newHealth)
    {
        TriggerEvent("PlayerHealthChanged", new ValueChangeData { oldValue = oldHealth, newValue = newHealth });
    }
    
    /// <summary>
    /// 触发玩家魔法值变化事件
    /// </summary>
    public void TriggerPlayerManaChanged(float oldMana, float newMana)
    {
        TriggerEvent("PlayerManaChanged", new ValueChangeData { oldValue = oldMana, newValue = newMana });
    }
    
    /// <summary>
    /// 触发游戏暂停事件
    /// </summary>
    public void TriggerGamePaused(bool isPaused)
    {
        TriggerEvent("GamePaused", isPaused);
    }
    
    /// <summary>
    /// 触发音效播放事件
    /// </summary>
    public void TriggerSoundEffect(string soundName)
    {
        TriggerEvent("SoundEffectPlay", soundName);
    }
    
    /// <summary>
    /// 触发UI通知事件
    /// </summary>
    public void TriggerUINotification(string message)
    {
        TriggerEvent("UINotification", message);
    }
    
    /// <summary>
    /// 触发敌人死亡事件
    /// </summary>
    public void TriggerEnemyDeath(Enemy enemy)
    {
        TriggerEvent("EnemyDeath", new { enemy = enemy });
    }
    
    /// <summary>
    /// 触发玩家死亡事件
    /// </summary>
    public void TriggerPlayerDeath()
    {
        TriggerEvent("PlayerDeath", null);
    }
    
    /// <summary>
    /// 触发敌人攻击事件
    /// </summary>
    public void TriggerEnemyAttack(Enemy enemy)
    {
        TriggerEvent("EnemyAttack", new { enemy = enemy });
    }
    #endregion
    
    #region 调试方法
    /// <summary>
    /// 获取事件统计信息
    /// </summary>
    public string GetEventStats()
    {
        var stats = $"事件总线统计:\n";
        stats += $"注册的事件类型数: {eventListeners.Count}\n";
        stats += $"队列中的事件数: {eventQueue.Count}\n";
        
        foreach (var kvp in eventListeners)
        {
            stats += $"  {kvp.Key}: {kvp.Value.Count} 个监听器\n";
        }
        
        return stats;
    }
    
    /// <summary>
    /// 检查事件是否有监听器
    /// </summary>
    public bool HasListeners(string eventName)
    {
        return eventListeners.ContainsKey(eventName) && eventListeners[eventName].Count > 0;
    }
    #endregion
}

/// <summary>
/// 数值变化数据结构
/// </summary>
public struct ValueChangeData
{
    public float oldValue;
    public float newValue;
}