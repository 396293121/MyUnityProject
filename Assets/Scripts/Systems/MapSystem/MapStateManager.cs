using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 地图状态管理器 - 负责保存和恢复地图状态数据
/// 支持场景切换时的状态持久化，包括NPC状态、敌人状态、物品状态等
/// </summary>
public class MapStateManager : MonoBehaviour
{
    #region 单例模式
    private static MapStateManager _instance;
    public static MapStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MapStateManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MapStateManager");
                    _instance = go.AddComponent<MapStateManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 配置
    [TitleGroup("状态管理配置")]
    [FoldoutGroup("状态管理配置/基础设置", expanded: true)]
    [LabelText("启用状态持久化")]
    [InfoBox("是否启用场景切换时的状态保存")]
    public bool enableStatePersistence = true;

    [FoldoutGroup("状态管理配置/基础设置")]
    [LabelText("最大状态历史")]
    [InfoBox("保留的最大状态历史数量")]
    [Range(1, 10)]
    public int maxStateHistory = 5;

    [FoldoutGroup("状态管理配置/保存设置", expanded: false)]
    [LabelText("保存NPC状态")]
    public bool saveNPCStates = true;

    [FoldoutGroup("状态管理配置/保存设置")]
    [LabelText("保存敌人状态")]
    public bool saveEnemyStates = true;

    [FoldoutGroup("状态管理配置/保存设置")]
    [LabelText("保存物品状态")]
    public bool saveItemStates = true;

    [FoldoutGroup("状态管理配置/保存设置")]
    [LabelText("保存玩家状态")]
    public bool savePlayerStates = true;
    #endregion

    #region 状态数据
    [TitleGroup("状态数据")]
    [FoldoutGroup("状态数据/当前状态", expanded: false)]
    [LabelText("区域状态字典")]
    [ReadOnly]
    [ShowInInspector]
    private Dictionary<string, AreaStateData> areaStates = new Dictionary<string, AreaStateData>();

    [FoldoutGroup("状态数据/当前状态")]
    [LabelText("全局状态")]
    [ReadOnly]
    [ShowInInspector]
    private GlobalStateData globalState = new GlobalStateData();

    [FoldoutGroup("状态数据/历史状态", expanded: false)]
    [LabelText("状态历史")]
    [ReadOnly]
    [ShowInInspector]
    private List<StateSnapshot> stateHistory = new List<StateSnapshot>();

    [FoldoutGroup("状态数据/运行时信息")]
    [LabelText("当前区域ID")]
    [ReadOnly]
    [ShowInInspector]
    private string currentAreaId;

    [FoldoutGroup("状态数据/运行时信息")]
    [LabelText("上次保存时间")]
    [ReadOnly]
    [ShowInInspector]
    private DateTime lastSaveTime;

    [FoldoutGroup("状态数据/运行时信息")]
    [LabelText("自动保存计时器")]
    [ReadOnly]
    [ShowInInspector]
    private float autoSaveTimer;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeStateManager();
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && enableStatePersistence)
        {
            SaveCurrentState();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
 
    }

    private void OnDestroy()
    {
        if (enableStatePersistence)
        {
            SaveCurrentState();
        }
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化单例
    /// </summary>
    private void InitializeSingleton()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化状态管理器
    /// </summary>
    private void InitializeStateManager()
    {
        Debug.Log("[MapStateManager] 状态管理器初始化");
        
        // 初始化全局状态
        globalState = new GlobalStateData();
        
        // 重置计时器
        autoSaveTimer = 0f;
        lastSaveTime = DateTime.Now;

        // 加载保存的状态
        LoadSavedStates();
    }
    #endregion

    #region 状态保存和加载
    /// <summary>
    /// 保存当前状态
    /// </summary>
    public void SaveCurrentState()
    {
        if (!enableStatePersistence)
            return;


        try
        {
            // 收集当前区域状态
            if (!string.IsNullOrEmpty(currentAreaId))
            {
                AreaStateData areaState = CollectCurrentAreaState();
                areaStates[currentAreaId] = areaState;
            }

            // 收集全局状态
            globalState = CollectGlobalState();

            // 创建状态快照
            StateSnapshot snapshot = new StateSnapshot
            {
                timestamp = DateTime.Now,
                areaStates = new Dictionary<string, AreaStateData>(areaStates),
                globalState = globalState.Clone()
            };

            // 添加到历史记录
            stateHistory.Add(snapshot);

            // 限制历史记录数量
            if (stateHistory.Count > maxStateHistory)
            {
                stateHistory.RemoveAt(0);
            }

            // 保存到持久化存储
            SaveToPersistentStorage();

            lastSaveTime = DateTime.Now;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapStateManager] 保存状态时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 加载保存的状态
    /// </summary>
    public void LoadSavedStates()
    {
        if (!enableStatePersistence)
            return;

        Debug.Log("[MapStateManager] 加载保存的状态");

        try
        {
            LoadFromPersistentStorage();
            Debug.Log($"[MapStateManager] 状态加载完成，历史记录数量: {stateHistory.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapStateManager] 加载状态时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 恢复区域状态
    /// </summary>
    public void RestoreAreaState(string areaId)
    {
        if (!enableStatePersistence || !areaStates.ContainsKey(areaId))
            return;

        Debug.Log($"[MapStateManager] 恢复区域状态: {areaId}");

        try
        {
            AreaStateData areaState = areaStates[areaId];
            ApplyAreaState(areaState);
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapStateManager] 恢复区域状态时发生错误: {e.Message}");
        }
    }
    #endregion

    #region 状态收集
    /// <summary>
    /// 收集当前区域状态
    /// </summary>
    private AreaStateData CollectCurrentAreaState()
    {
        AreaStateData areaState = new AreaStateData
        {
            areaId = currentAreaId,
            timestamp = DateTime.Now
        };

        // 收集NPC状态
        if (saveNPCStates)
        {
            areaState.npcStates = CollectNPCStates();
        }

        // 收集敌人状态
        if (saveEnemyStates)
        {
            areaState.enemyStates = CollectEnemyStates();
        }

        // 收集物品状态
        if (saveItemStates)
        {
            areaState.itemStates = CollectItemStates();
        }


        return areaState;
    }

    /// <summary>
    /// 收集全局状态
    /// </summary>
    private GlobalStateData CollectGlobalState()
    {
        GlobalStateData state = new GlobalStateData
        {
            timestamp = DateTime.Now
        };

        // 收集玩家状态
        if (savePlayerStates)
        {
            state.playerState = CollectPlayerState();
        }

        // 收集游戏进度
        state.gameProgress = CollectGameProgress();

        return state;
    }

    /// <summary>
    /// 收集NPC状态
    /// </summary>
    private List<NPCStateData> CollectNPCStates()
    {
        List<NPCStateData> npcStates = new List<NPCStateData>();

        // 查找场景中的所有NPC
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        foreach (NPCController npc in npcs)
        {
            NPCStateData npcState = new NPCStateData
            {
              //  npcId = npc.npcId,
                position = npc.transform.position,
                rotation = npc.transform.rotation,
                isActive = npc.gameObject.activeInHierarchy,
              //  dialogueState = npc.GetDialogueState(),
             //   customData = npc.GetCustomStateData()
            };
            npcStates.Add(npcState);
        }

        return npcStates;
    }

    /// <summary>
    /// 收集敌人状态
    /// </summary>
    private List<EnemyStateData> CollectEnemyStates()
    {
        List<EnemyStateData> enemyStates = new List<EnemyStateData>();

        // 查找场景中的所有敌人
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            EnemyStateData enemyState = new EnemyStateData
            {
                enemyId = enemy.EnemyId,
                position = enemy.transform.position,
                rotation = enemy.transform.rotation,
                isActive = enemy.gameObject.activeInHierarchy,
                 health = enemy.currentHealth,
                // isDefeated = enemy.IsDefeated(),
                // customData = enemy.GetCustomStateData()
            };
            enemyStates.Add(enemyState);
        }

        return enemyStates;
    }

    /// <summary>
    /// 收集物品状态
    /// </summary>
    private List<ItemStateData> CollectItemStates()
    {
        List<ItemStateData> itemStates = new List<ItemStateData>();

        // 查找场景中的所有物品
        Item[] items = FindObjectsOfType<Item>();
        foreach (Item item in items)
        {
            ItemStateData itemState = new ItemStateData
            {
                // itemId = item.itemId,
                // position = item.transform.position,
                // rotation = item.transform.rotation,
                // isCollected = item.IsCollected(),
                // customData = item.GetCustomStateData()
            };
            itemStates.Add(itemState);
        }

        return itemStates;
    }

    /// <summary>
    /// 收集玩家状态
    /// </summary>
    private PlayerStateData CollectPlayerState()
    {
        PlayerController player = SceneController.Instance.PlayerController;
        if (player == null)
            return new PlayerStateData();

        return new PlayerStateData
        {
            position = player.transform.position,
            rotation = player.transform.rotation,
             health = player.playerCharacter.currentHealth,
            // level = player.GetLevel(),
            // experience = player.GetExperience(),
            // inventory = player.GetInventoryData(),
            // customData = player.GetCustomStateData()
        };
    }

    /// <summary>
    /// 收集游戏进度
    /// </summary>
    private GameProgressData CollectGameProgress()
    {
        // 这里可以收集任务进度、解锁内容等
        return new GameProgressData
        {
            completedQuests = new List<string>(),
            unlockedAreas = new List<string>(),
            gameFlags = new Dictionary<string, bool>(),
            customData = new Dictionary<string, object>()
        };
    }
    #endregion

    #region 状态应用
    /// <summary>
    /// 应用区域状态
    /// </summary>
    private void ApplyAreaState(AreaStateData areaState)
    {
        Debug.Log($"[MapStateManager] 应用区域状态: {areaState.areaId}");

        // 应用NPC状态
        if (saveNPCStates && areaState.npcStates != null)
        {
            ApplyNPCStates(areaState.npcStates);
        }

        // 应用敌人状态
        if (saveEnemyStates && areaState.enemyStates != null)
        {
            ApplyEnemyStates(areaState.enemyStates);
        }

        // 应用物品状态
        if (saveItemStates && areaState.itemStates != null)
        {
            ApplyItemStates(areaState.itemStates);
        }

    }

    /// <summary>
    /// 应用NPC状态
    /// </summary>
    private void ApplyNPCStates(List<NPCStateData> npcStates)
    {
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        
        foreach (NPCStateData npcState in npcStates)
        {
            NPCController npc = Array.Find(npcs, n => n.npcConfig.npcId == npcState.npcId);
            if (npc != null)
            {
                npc.transform.position = npcState.position;
                npc.transform.rotation = npcState.rotation;
                npc.gameObject.SetActive(npcState.isActive);
                // npc.SetDialogueState(npcState.dialogueState);
                // npc.SetCustomStateData(npcState.customData);
            }
        }
    }

    /// <summary>
    /// 应用敌人状态
    /// </summary>
    private void ApplyEnemyStates(List<EnemyStateData> enemyStates)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        
        foreach (EnemyStateData enemyState in enemyStates)
        {
            Enemy enemy = Array.Find(enemies, e => e.EnemyId== enemyState.enemyId);
            if (enemy != null)
            {
                enemy.transform.position = enemyState.position;
                enemy.transform.rotation = enemyState.rotation;
                enemy.gameObject.SetActive(enemyState.isActive);
                // enemy.SetCurrentHealth(enemyState.health);
                // enemy.SetDefeated(enemyState.isDefeated);
                // enemy.SetCustomStateData(enemyState.customData);
            }
        }
    }

    /// <summary>
    /// 应用物品状态
    /// </summary>
    private void ApplyItemStates(List<ItemStateData> itemStates)
    {
        Item[] items = FindObjectsOfType<Item>();
        
        foreach (ItemStateData itemState in itemStates)
        {
            Item item = Array.Find(items, i => i.id == itemState.itemId);
            if (item != null)
            {
                // item.transform.position = itemState.position;
                // item.transform.rotation = itemState.rotation;
                // item.SetCollected(itemState.isCollected);
                // item.SetCustomStateData(itemState.customData);
            }
        }
    }

    #endregion

    #region 持久化存储
    /// <summary>
    /// 保存到持久化存储
    /// </summary>
    private void SaveToPersistentStorage()
    {
        // 这里可以实现具体的持久化存储逻辑
        // 例如：保存到文件、数据库或云存储
        string json = JsonUtility.ToJson(new SerializableStateData
        {
            areaStates = areaStates,
            globalState = globalState,
            stateHistory = stateHistory
        });
        
        string filePath = Application.persistentDataPath + "/mapstates.json";
        System.IO.File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从持久化存储加载
    /// </summary>
    private void LoadFromPersistentStorage()
    {
        string filePath = Application.persistentDataPath + "/mapstates.json";
        
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            SerializableStateData data = JsonUtility.FromJson<SerializableStateData>(json);
            
            if (data != null)
            {
                areaStates = data.areaStates ?? new Dictionary<string, AreaStateData>();
                globalState = data.globalState ?? new GlobalStateData();
                stateHistory = data.stateHistory ?? new List<StateSnapshot>();
            }
            
            Debug.Log($"[MapStateManager] 状态已从文件加载: {filePath}");
        }
        else
        {
            Debug.Log("[MapStateManager] 未找到保存的状态文件，使用默认状态");
        }
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 设置当前区域ID
    /// </summary>
    public void SetCurrentAreaId(string areaId)
    {
        if (currentAreaId != areaId)
        {
            // 保存旧区域状态
            if (!string.IsNullOrEmpty(currentAreaId))
            {
                AreaStateData oldAreaState = CollectCurrentAreaState();
                areaStates[currentAreaId] = oldAreaState;
            }
            
            currentAreaId = areaId;
            Debug.Log($"[MapStateManager] 当前区域ID设置为: {areaId}");
        }
    }

    /// <summary>
    /// 获取区域状态
    /// </summary>
    public AreaStateData GetAreaState(string areaId)
    {
        return areaStates.ContainsKey(areaId) ? areaStates[areaId] : null;
    }

    /// <summary>
    /// 清除区域状态
    /// </summary>
    public void ClearAreaState(string areaId)
    {
        if (areaStates.ContainsKey(areaId))
        {
            areaStates.Remove(areaId);
            Debug.Log($"[MapStateManager] 已清除区域状态: {areaId}");
        }
    }

    /// <summary>
    /// 清除所有状态
    /// </summary>
    public void ClearAllStates()
    {
        areaStates.Clear();
        globalState = new GlobalStateData();
        stateHistory.Clear();
        Debug.Log("[MapStateManager] 已清除所有状态");
    }
    #endregion

    #region 调试工具
    [TitleGroup("调试工具")]
    [FoldoutGroup("调试工具/调试操作", expanded: false)]
    [Button("立即保存状态", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void ForceSave()
    {
        SaveCurrentState();
    }

    [FoldoutGroup("调试工具/调试操作")]
    [Button("重新加载状态", ButtonSizes.Medium)]
    [GUIColor(1f, 0.9f, 0.7f)]
    public void ForceReload()
    {
        LoadSavedStates();
    }

    [FoldoutGroup("调试工具/调试操作")]
    [Button("清除所有状态", ButtonSizes.Medium)]
    [GUIColor(1f, 0.7f, 0.7f)]
    public void ForceClearAll()
    {
        ClearAllStates();
    }

    [FoldoutGroup("调试工具/调试操作")]
    [Button("输出状态信息", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void LogStateInfo()
    {
        Debug.Log($"[MapStateManager] 状态信息:");
        Debug.Log($"  当前区域: {currentAreaId}");
        Debug.Log($"  区域状态数量: {areaStates.Count}");
        Debug.Log($"  历史记录数量: {stateHistory.Count}");
        Debug.Log($"  上次保存时间: {lastSaveTime}");
    }
    #endregion
}

#region 数据结构
/// <summary>
/// 可序列化的状态数据
/// </summary>
[Serializable]
public class SerializableStateData
{
    public Dictionary<string, AreaStateData> areaStates;
    public GlobalStateData globalState;
    public List<StateSnapshot> stateHistory;
}

/// <summary>
/// 状态快照
/// </summary>
[Serializable]
public class StateSnapshot
{
    public DateTime timestamp;
    public Dictionary<string, AreaStateData> areaStates;
    public GlobalStateData globalState;
}

/// <summary>
/// 区域状态数据
/// </summary>
[Serializable]
public class AreaStateData
{
    public string areaId;
    public DateTime timestamp;
    public List<NPCStateData> npcStates;
    public List<EnemyStateData> enemyStates;
    public List<ItemStateData> itemStates;
}

/// <summary>
/// 全局状态数据
/// </summary>
[Serializable]
public class GlobalStateData
{
    public DateTime timestamp;
    public PlayerStateData playerState;
    public GameProgressData gameProgress;

    public GlobalStateData Clone()
    {
        return JsonUtility.FromJson<GlobalStateData>(JsonUtility.ToJson(this));
    }
}

/// <summary>
/// NPC状态数据
/// </summary>
[Serializable]
public class NPCStateData
{
    public string npcId;
    public Vector3 position;
    public Quaternion rotation;
    public bool isActive;
    public string dialogueState;
    public Dictionary<string, object> customData;
}

/// <summary>
/// 敌人状态数据
/// </summary>
[Serializable]
public class EnemyStateData
{
    public string enemyId;
    public Vector3 position;
    public Quaternion rotation;
    public bool isActive;
    public float health;
    public bool isDefeated;
    public Dictionary<string, object> customData;
}

/// <summary>
/// 物品状态数据
/// </summary>
[Serializable]
public class ItemStateData
{
    public string itemId;
    public Vector3 position;
    public Quaternion rotation;
    public bool isCollected;
    public Dictionary<string, object> customData;
}

/// <summary>
/// 玩家状态数据
/// </summary>
[Serializable]
public class PlayerStateData
{
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public int level;
    public float experience;
    public Dictionary<string, int> inventory;
    public Dictionary<string, object> customData;
}

/// <summary>
/// 游戏进度数据
/// </summary>
[Serializable]
public class GameProgressData
{
    public List<string> completedQuests;
    public List<string> unlockedAreas;
    public Dictionary<string, bool> gameFlags;
    public Dictionary<string, object> customData;
}
#endregion