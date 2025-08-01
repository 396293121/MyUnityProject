using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏ID字典 - 统一管理所有类型的ID
/// 为任务系统提供灵活的目标配置
/// </summary>
[CreateAssetMenu(fileName = "GameIdDictionary", menuName = "Game/Game ID Dictionary")]
public class GameIdDictionary : ScriptableObject
{
    [BoxGroup("敌人ID字典", Order = 1)]
    [LabelText("敌人配置列表")]
    [InfoBox("从敌人配置文件中自动收集ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<EnemyIdEntry> enemyIds = new List<EnemyIdEntry>();
    
    [BoxGroup("NPC ID字典", Order = 2)]
    [LabelText("NPC配置列表")]
    [InfoBox("从NPC配置文件中自动收集ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<NPCIdEntry> npcIds = new List<NPCIdEntry>();
    
    [BoxGroup("物品ID字典", Order = 3)]
    [LabelText("物品配置列表")]
    [InfoBox("从物品配置文件中自动收集ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<ItemIdEntry> itemIds = new List<ItemIdEntry>();
    
    [BoxGroup("区域ID字典", Order = 4)]
    [LabelText("主区域配置列表")]
    [InfoBox("从地图配置文件中自动收集主区域ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<MainAreaIdEntry> mainAreaIds = new List<MainAreaIdEntry>();
    
    [BoxGroup("区域ID字典")]
    [LabelText("分区域配置列表")]
    [InfoBox("从地图配置文件中自动收集分区域ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<SubAreaIdEntry> subAreaIds = new List<SubAreaIdEntry>();
    
    [BoxGroup("区域ID字典")]
    [LabelText("场景区域配置列表")]
    [InfoBox("从地图配置文件中自动收集场景区域ID")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    [ReadOnly]
    public List<SceneAreaIdEntry> sceneAreaIds = new List<SceneAreaIdEntry>();
    
    [BoxGroup("配置管理", Order = 0)]
    [LabelText("敌人配置文件夹")]
    [InfoBox("存放敌人配置文件的文件夹路径")]
    [FolderPath]
    public string enemyConfigFolder = "Assets/Scripts/ScriptableObjects/系统配置文件脚本";
    
    [BoxGroup("配置管理")]
    [LabelText("NPC配置文件夹")]
    [InfoBox("存放NPC配置文件的文件夹路径")]
    [FolderPath]
    public string npcConfigFolder = "Assets/Scripts/ScriptableObjects/NPC配置文件";
    
    [BoxGroup("配置管理")]
    [LabelText("物品配置文件夹")]
    [InfoBox("存放物品配置文件的文件夹路径")]
    [FolderPath]
    public string itemConfigFolder = "Assets/Scripts/ScriptableObjects/物品配置文件";
    
    [BoxGroup("配置管理")]
    [LabelText("地图配置文件夹")]
    [InfoBox("存放地图配置文件的文件夹路径")]
    [FolderPath]
    public string mapConfigFolder = "Assets/Scripts/ScriptableObjects/地图配置文件脚本";
    
    [BoxGroup("配置管理")]
    [Button("刷新所有ID字典", ButtonSizes.Large)]
    [InfoBox("点击此按钮从配置文件中重新收集所有ID")]
    public void RefreshAllDictionaries()
    {
        RefreshEnemyDictionary();
        RefreshNPCDictionary();
        RefreshItemDictionary();
        RefreshAreaDictionaries();
        
        Debug.Log($"[GameIdDictionary] 已刷新所有ID字典 - 敌人:{enemyIds.Count}, NPC:{npcIds.Count}, 物品:{itemIds.Count}, 主区域:{mainAreaIds.Count}, 分区域:{subAreaIds.Count}, 场景区域:{sceneAreaIds.Count}");
    }
    
    /// <summary>
    /// 刷新敌人ID字典
    /// </summary>
    [BoxGroup("配置管理")]
    [Button("刷新敌人ID")]
    public void RefreshEnemyDictionary()
    {
        enemyIds.Clear();
        
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EnemyConfig", new[] { enemyConfigFolder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EnemyConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            
            if (config != null && !string.IsNullOrEmpty(config.enemyId))
            {
                enemyIds.Add(new EnemyIdEntry
                {
                    id = config.enemyId,
                    displayName = config.displayName,
                    category = config.category,
                    rarity = config.rarity,
                    canBeKillTarget = config.canBeKillTarget,
                    canBeBossTarget = config.canBeBossTarget,
                    recommendedLevel = config.recommendedLevel,
                    config = config
                });
            }
        }
        
        // 按ID排序
        enemyIds = enemyIds.OrderBy(x => x.id).ToList();
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// 刷新NPC ID字典
    /// </summary>
    [BoxGroup("配置管理")]
    [Button("刷新NPC ID")]
    public void RefreshNPCDictionary()
    {
        npcIds.Clear();
        
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:NPCConfig", new[] { npcConfigFolder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            NPCConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<NPCConfig>(path);
            
            if (config != null && !string.IsNullOrEmpty(config.npcId))
            {
                npcIds.Add(new NPCIdEntry
                {
                    id = config.npcId,
                    displayName = config.npcName,
                    npcType = config.npcType,
                    canBeTalkTarget = config.canBeTalkTarget,
                    config = config
                });
            }
        }
        
        // 按ID排序
        npcIds = npcIds.OrderBy(x => x.id).ToList();
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// 刷新物品ID字典
    /// </summary>
    [BoxGroup("配置管理")]
    [Button("刷新物品ID")]
    public void RefreshItemDictionary()
    {
        itemIds.Clear();
        
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Item", new[] { itemConfigFolder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Item config = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(path);
            
            if (config != null && !string.IsNullOrEmpty(config.id))
            {
                itemIds.Add(new ItemIdEntry
                {
                    id = config.id,
                    displayName = config.itemName,
                    itemType = config.itemType,
                    rarity = config.rarity,
                    canBeCollectTarget = config.canBeCollectTarget,
                    canBeFindTarget = config.canBeFindTarget,
                    canBeUseTarget = config.canBeUseTarget,
                    config = config
                });
            }
        }
        
        // 按ID排序
        itemIds = itemIds.OrderBy(x => x.id).ToList();
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// 刷新区域ID字典
    /// </summary>
    [BoxGroup("配置管理")]
    [Button("刷新区域ID")]
    public void RefreshAreaDictionaries()
    {
        mainAreaIds.Clear();
        subAreaIds.Clear();
        sceneAreaIds.Clear();
        
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MapSystemConfig", new[] { mapConfigFolder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            MapSystemConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<MapSystemConfig>(path);
            
            if (config != null)
            {
                // 收集主区域ID
                foreach (var mainArea in config.mainAreas)
                {
                    if (!string.IsNullOrEmpty(mainArea.areaId))
                    {
                        mainAreaIds.Add(new MainAreaIdEntry
                        {
                            id = mainArea.areaId,
                            displayName = mainArea.areaName,
                            description = mainArea.areaDescription,
                            areaType = mainArea.areaType,
                            config = config
                        });
                        
                        // 收集分区域ID
                        foreach (var subArea in mainArea.subAreas)
                        {
                            if (!string.IsNullOrEmpty(subArea.areaId))
                            {
                                subAreaIds.Add(new SubAreaIdEntry
                                {
                                    id = subArea.areaId,
                                    displayName = subArea.areaName,
                                    description = subArea.areaDescription,
                                    mainAreaId = mainArea.areaId,
                                    mainAreaName = mainArea.areaName,
                                    config = config
                                });
                                
                                // 收集场景区域ID
                                foreach (var sceneArea in subArea.sceneAreas)
                                {
                                    if (!string.IsNullOrEmpty(sceneArea.areaId))
                                    {
                                        sceneAreaIds.Add(new SceneAreaIdEntry
                                        {
                                            id = sceneArea.areaId,
                                            displayName = sceneArea.areaName,
                                            description = sceneArea.areaDescription,
                                            mainAreaId = mainArea.areaId,
                                            mainAreaName = mainArea.areaName,
                                            subAreaId = subArea.areaId,
                                            subAreaName = subArea.areaName,
                                            config = config
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // 按ID排序
        mainAreaIds = mainAreaIds.OrderBy(x => x.id).ToList();
        subAreaIds = subAreaIds.OrderBy(x => x.id).ToList();
        sceneAreaIds = sceneAreaIds.OrderBy(x => x.id).ToList();
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    // ========== 查询方法 ==========
    
    /// <summary>
    /// 获取所有敌人ID
    /// </summary>
    public List<string> GetAllEnemyIds()
    {
        return enemyIds.Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取可用于击杀任务的敌人ID
    /// </summary>
    public List<string> GetKillTargetEnemyIds()
    {
        return enemyIds.Where(x => x.canBeKillTarget).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取可用于BOSS任务的敌人ID
    /// </summary>
    public List<string> GetBossTargetEnemyIds()
    {
        return enemyIds.Where(x => x.canBeBossTarget).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取所有NPC ID
    /// </summary>
    public List<string> GetAllNPCIds()
    {
        return npcIds.Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取可用于对话任务的NPC ID
    /// </summary>
    public List<string> GetTalkTargetNPCIds()
    {
        return npcIds.Where(x => x.canBeTalkTarget).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取所有物品ID
    /// </summary>
    public List<string> GetAllItemIds()
    {
        return itemIds.Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取可用于收集任务的物品ID
    /// </summary>
    public List<string> GetCollectTargetItemIds()
    {
        return itemIds.Where(x => x.canBeCollectTarget).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取可用于寻找任务的物品ID
    /// </summary>
    public List<string> GetFindTargetItemIds()
    {
        return itemIds.Where(x => x.canBeFindTarget).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取所有主区域ID
    /// </summary>
    public List<string> GetAllMainAreaIds()
    {
        return mainAreaIds.Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取所有分区域ID
    /// </summary>
    public List<string> GetAllSubAreaIds(string targetMainAreaId)
    {
        return subAreaIds.FindAll(x=>x.mainAreaId==targetMainAreaId).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 根据主区域ID获取分区域ID列表
    /// </summary>
    public List<string> GetSubAreaIdsByMainArea(string mainAreaId)
    {
        return subAreaIds.Where(x => x.mainAreaId == mainAreaId).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取所有场景区域ID
    /// </summary>
    public List<string> GetAllSceneAreaIds(string targetMainAreaId,string targetSubAreaId)
    {
        return sceneAreaIds.FindAll(x=>x.mainAreaId==targetMainAreaId&&x.subAreaId==targetSubAreaId).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 根据分区域ID获取场景区域ID列表
    /// </summary>
    public List<string> GetSceneAreaIdsBySubArea(string mainAreaId, string subAreaId)
    {
        return sceneAreaIds.Where(x => x.mainAreaId == mainAreaId && x.subAreaId == subAreaId).Select(x => x.id).ToList();
    }
    
    /// <summary>
    /// 获取主区域显示名称
    /// </summary>
    public string GetMainAreaDisplayName(string mainAreaId)
    {
        var area = mainAreaIds.FirstOrDefault(x => x.id == mainAreaId);
        return area?.displayName ?? mainAreaId;
    }
    
    /// <summary>
    /// 获取分区域显示名称
    /// </summary>
    public string GetSubAreaDisplayName(string targetMainAreaId,string subAreaId)
    {
        var area = subAreaIds.FindAll(x=>x.mainAreaId==targetMainAreaId). FirstOrDefault(x => x.id == subAreaId);
        return area?.displayName ?? subAreaId;
    }
    
    /// <summary>
    /// 获取场景区域显示名称
    /// </summary>
    public string GetSceneAreaDisplayName(string targetMainAreaId,string targetSubAreaId,string sceneAreaId)
    {
        var area = sceneAreaIds.FindAll(X=>X.mainAreaId==targetMainAreaId&&X.subAreaId==targetSubAreaId).FirstOrDefault(x => x.id == sceneAreaId);
        return area?.displayName ?? sceneAreaId;
    }
    
    /// <summary>
    /// 获取完整区域路径显示名称
    /// </summary>
    public string GetFullAreaDisplayName(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        var mainArea = mainAreaIds.FirstOrDefault(x => x.id == mainAreaId);
        var subArea = subAreaIds.FirstOrDefault(x => x.id == subAreaId && x.mainAreaId == mainAreaId);
        var sceneArea = sceneAreaIds.FirstOrDefault(x => x.id == sceneAreaId && x.mainAreaId == mainAreaId && x.subAreaId == subAreaId);
        
        string result = "";
        if (mainArea != null) result += mainArea.displayName;
        if (subArea != null) result += " - " + subArea.displayName;
        if (sceneArea != null) result += " - " + sceneArea.displayName;
        
        return !string.IsNullOrEmpty(result) ? result : $"{mainAreaId}.{subAreaId}.{sceneAreaId}";
    }
    
    /// <summary>
    /// 根据目标类型获取对应的ID列表
    /// </summary>
    public List<string> GetTargetIdsByObjectiveType(ObjectiveType objectiveType)
    {
        switch (objectiveType)
        {
            case ObjectiveType.KillEnemy:
                return GetKillTargetEnemyIds();
            case ObjectiveType.KillBoss:
                return GetBossTargetEnemyIds();
            case ObjectiveType.CollectItem:
                return GetCollectTargetItemIds();
            case ObjectiveType.FindItem:
                return GetFindTargetItemIds();
            case ObjectiveType.TalkToNPC:
                return GetTalkTargetNPCIds();
            case ObjectiveType.UseItem:
                return itemIds.Where(x => x.canBeUseTarget).Select(x => x.id).ToList();
            default:
                return new List<string>();
        }
    }
    
    /// <summary>
    /// 获取ID的显示名称
    /// </summary>
    public string GetDisplayName(string id, ObjectiveType objectiveType)
    {
        switch (objectiveType)
        {
            case ObjectiveType.KillEnemy:
            case ObjectiveType.KillBoss:
                var enemy = enemyIds.FirstOrDefault(x => x.id == id);
                return enemy?.displayName ?? id;
            case ObjectiveType.CollectItem:
            case ObjectiveType.FindItem:
            case ObjectiveType.UseItem:
                var item = itemIds.FirstOrDefault(x => x.id == id);
                return item?.displayName ?? id;
            case ObjectiveType.TalkToNPC:
                var npc = npcIds.FirstOrDefault(x => x.id == id);
                return npc?.displayName ?? id;
            default:
                return id;
        }
    }
}

/// <summary>
/// 敌人ID条目
/// </summary>
[System.Serializable]
public class EnemyIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("类型")]
    [ReadOnly]
    public EnemyCategory category;
    
    [HorizontalGroup("Details")]
    [LabelText("稀有度")]
    [ReadOnly]
    public EnemyRarity rarity;
    
    [HorizontalGroup("Quest")]
    [LabelText("击杀任务")]
    [ReadOnly]
    public bool canBeKillTarget;
    
    [HorizontalGroup("Quest")]
    [LabelText("BOSS任务")]
    [ReadOnly]
    public bool canBeBossTarget;
    
    [HorizontalGroup("Quest")]
    [LabelText("推荐等级")]
    [ReadOnly]
    public int recommendedLevel;
    
    [HideInInspector]
    public EnemyConfig config;
}

/// <summary>
/// NPC ID条目
/// </summary>
[System.Serializable]
public class NPCIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("类型")]
    [ReadOnly]
    public NPCType npcType;
    
    
    [HorizontalGroup("Quest")]
    [LabelText("对话任务")]
    [ReadOnly]
    public bool canBeTalkTarget;
    [HideInInspector]
    public NPCConfig config;
}

/// <summary>
/// 物品ID条目
/// </summary>
[System.Serializable]
public class ItemIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("类型")]
    [ReadOnly]
    public ItemType itemType;
    
    [HorizontalGroup("Details")]
    [LabelText("稀有度")]
    [ReadOnly]
    public ItemRarity rarity;
    
    [HorizontalGroup("Quest")]
    [LabelText("收集任务")]
    [ReadOnly]
    public bool canBeCollectTarget;
    
    [HorizontalGroup("Quest")]
    [LabelText("寻找任务")]
    [ReadOnly]
    public bool canBeFindTarget;
    
    [HorizontalGroup("Quest")]
    [LabelText("使用任务")]
    [ReadOnly]
    public bool canBeUseTarget;
    
    [HorizontalGroup("Quest")]
    [LabelText("推荐数量")]
    [ReadOnly]
    public int recommendedCollectAmount;
    
    [HideInInspector]
    public Item config;
}

/// <summary>
/// 主区域ID条目
/// </summary>
[System.Serializable]
public class MainAreaIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("描述")]
    [ReadOnly]
    public string description;
    
    [HorizontalGroup("Details")]
    [LabelText("区域类型")]
    [ReadOnly]
    public AreaType areaType;
    
    [HideInInspector]
    public MapSystemConfig config;
}

/// <summary>
/// 分区域ID条目
/// </summary>
[System.Serializable]
public class SubAreaIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("描述")]
    [ReadOnly]
    public string description;
    
    [HorizontalGroup("Parent")]
    [LabelText("主区域ID")]
    [ReadOnly]
    public string mainAreaId;
    
    [HorizontalGroup("Parent")]
    [LabelText("主区域名称")]
    [ReadOnly]
    public string mainAreaName;
    
    [HideInInspector]
    public MapSystemConfig config;
}

/// <summary>
/// 场景区域ID条目
/// </summary>
[System.Serializable]
public class SceneAreaIdEntry
{
    [HorizontalGroup("Info")]
    [LabelText("ID")]
    [ReadOnly]
    public string id;
    
    [HorizontalGroup("Info")]
    [LabelText("显示名称")]
    [ReadOnly]
    public string displayName;
    
    [HorizontalGroup("Details")]
    [LabelText("描述")]
    [ReadOnly]
    public string description;
    
    [HorizontalGroup("Parent")]
    [LabelText("主区域ID")]
    [ReadOnly]
    public string mainAreaId;
    
    [HorizontalGroup("Parent")]
    [LabelText("主区域名称")]
    [ReadOnly]
    public string mainAreaName;
    
    [HorizontalGroup("Parent")]
    [LabelText("分区域ID")]
    [ReadOnly]
    public string subAreaId;
    
    [HorizontalGroup("Parent")]
    [LabelText("分区域名称")]
    [ReadOnly]
    public string subAreaName;
    
    [HideInInspector]
    public MapSystemConfig config;
}