using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 地图系统配置 - 定义地图层级结构和配置
/// 支持主区域-分区域-场景粒度的层级结构
/// 整合敌人、NPC等游戏对象配置
/// </summary>
[CreateAssetMenu(fileName = "MapSystemConfig", menuName = "Game/Map System Config")]
public class MapSystemConfig : ScriptableObject
{
    #region 基础配置
    [TitleGroup("地图系统基础配置")]
    [FoldoutGroup("地图系统基础配置/系统设置", expanded: true)]
    [LabelText("系统名称")]
    [Required("必须指定系统名称")]
    public string systemName = "游戏地图系统";

    [FoldoutGroup("地图系统基础配置/系统设置")]
    [LabelText("系统版本")]
    public string systemVersion = "1.0.0";

    [FoldoutGroup("地图系统基础配置/系统设置")]
    [LabelText("系统描述")]
    [TextArea(2, 4)]
    public string systemDescription = "基于层级结构的地图管理系统";
    #endregion

    #region 游戏对象预制体配置
    [TitleGroup("游戏对象配置")]
    [FoldoutGroup("游戏对象配置/角色系统", expanded: true)]
    [LabelText("支持的角色类型")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "characterType")]
    public List<CharacterPrefabConfig> characterPrefabs = new List<CharacterPrefabConfig>();

    [FoldoutGroup("游戏对象配置/角色系统")]
    [LabelText("默认角色类型")]
    [ValueDropdown("GetCharacterTypes")]
    public string defaultCharacterType = "warrior";

    [FoldoutGroup("游戏对象配置/敌人系统", expanded: false)]
    [LabelText("敌人预制体配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "enemyType")]
    public List<EnemyPrefabConfig> enemyPrefabs = new List<EnemyPrefabConfig>();

    [FoldoutGroup("游戏对象配置/NPC系统", expanded: false)]
    [LabelText("NPC预制体配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "npcType")]
    public List<NPCPrefabConfig> npcPrefabs = new List<NPCPrefabConfig>();
    #endregion

    #region 性能配置
    [FoldoutGroup("地图系统基础配置/性能设置", expanded: true)]
    [LabelText("启用地图缓存")]
    [InfoBox("启用后，切换过的地图会保留在内存中，提高切换速度但占用更多内存")]
    public bool enableMapCaching = true;

    [FoldoutGroup("地图系统基础配置/性能设置")]
    [LabelText("最大缓存地图数量")]
    [ShowIf("enableMapCaching")]
    [Range(1, 10)]
    public int maxCachedMaps = 3;

    [FoldoutGroup("地图系统基础配置/性能设置")]
    [LabelText("启用状态保持")]
    [InfoBox("启用后，会保存和恢复区域中的游戏对象状态")]
    public bool enableStatePersistence = true;

    [FoldoutGroup("地图系统基础配置/性能设置")]
    [LabelText("自动清理间隔")]
    [SuffixLabel("秒")]
    [Range(30f, 300f)]
    public float autoCleanupInterval = 120f;
    #endregion

    #region 切换效果配置
    [FoldoutGroup("地图系统基础配置/切换效果", expanded: false)]
    [LabelText("切换持续时间")]
    [SuffixLabel("秒")]
    [Range(0.1f, 3f)]
    public float transitionDuration = 0.5f;

    [FoldoutGroup("地图系统基础配置/切换效果")]
    [LabelText("切换遮罩颜色")]
    public Color transitionMaskColor = Color.black;
    #endregion

    #region 初始场景配置
    [TitleGroup("初始场景配置")]
    [FoldoutGroup("初始场景配置/角色初始位置", expanded: true)]
    [LabelText("角色初始场景配置")]
    [InfoBox("配置不同角色类型的初始场景区域，避免游戏首次加载时失败")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "characterType")]
    public List<CharacterInitialAreaConfig> characterInitialAreas = new List<CharacterInitialAreaConfig>();

    [FoldoutGroup("初始场景配置/默认设置")]
    [LabelText("默认初始主区域ID")]
    [ValueDropdown("GetMainAreaOptions")]
    public string defaultInitialMainAreaId = "";

    [FoldoutGroup("初始场景配置/默认设置")]
    [LabelText("默认初始分区域ID")]
    [ValueDropdown("GetSubAreaOptionsForDefault")]
    [ShowIf("@!string.IsNullOrEmpty(defaultInitialMainAreaId)")]
    public string defaultInitialSubAreaId = "";

    [FoldoutGroup("初始场景配置/默认设置")]
    [LabelText("默认初始场景区域ID")]
    [ValueDropdown("GetSceneAreaOptionsForDefault")]
    [ShowIf("@!string.IsNullOrEmpty(defaultInitialSubAreaId)")]
    public string defaultInitialSceneAreaId = "";
    #endregion

    #region 地图层级结构
    [TitleGroup("地图层级结构")]
    [FoldoutGroup("地图层级结构/主区域配置", expanded: true)]
    [LabelText("主区域列表")]
    [InfoBox("定义游戏中的主要区域，如：新手村、森林、城镇等")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "areaName")]
    public List<MainArea> mainAreas = new List<MainArea>();
    #endregion

    #region 可视化位置调整工具
    [TitleGroup("可视化位置调整工具")]
    [FoldoutGroup("可视化位置调整工具/位置调整", expanded: true)]
    [InfoBox("在编辑模式下，可以在Scene面板中调整游戏对象位置，然后点击对应按钮将位置同步到配置文件")]
    [Button("同步所有位置", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.8f, 1f)]
    public void SyncAllPositions()
    {
#if UNITY_EDITOR
        SyncPositionFromScene();
        SyncNPCPositionsFromScene();
        SyncEnemyPositionsFromScene();
        SyncMapPositionFromScene();
        Debug.Log("[MapSystemConfig] 所有位置同步完成！");
#endif
    }

    [FoldoutGroup("可视化位置调整工具/生成预览", expanded: true)]
    [LabelText("选择主区域")]
    [ValueDropdown("GetMainAreaOptions")]
    [OnValueChanged("OnMainAreaChanged")]
    public string selectedMainAreaId = "";

    [FoldoutGroup("可视化位置调整工具/生成预览")]
    [LabelText("选择分区域")]
    [ValueDropdown("GetSubAreaOptions")]
    [OnValueChanged("OnSubAreaChanged")]
    [ShowIf("@!string.IsNullOrEmpty(selectedMainAreaId)")]
    public string selectedSubAreaId = "";

    [FoldoutGroup("可视化位置调整工具/生成预览")]
    [LabelText("选择场景区域")]
    [ValueDropdown("GetSceneAreaOptions")]
    [OnValueChanged("OnSceneAreaChanged")]
    [ShowIf("@!string.IsNullOrEmpty(selectedSubAreaId)")]
    public string selectedSceneAreaId = "";

    [FoldoutGroup("可视化位置调整工具/生成预览")]
    [Button("生成位置预览", ButtonSizes.Medium)]
    [GUIColor(1f, 1f, 0.7f)]
    [ShowIf("@!string.IsNullOrEmpty(selectedSceneAreaId)")]
    public void GeneratePositionPreviews()
    {
#if UNITY_EDITOR
        GenerateScenePreview();
#endif
    }

    [FoldoutGroup("可视化位置调整工具/生成预览")]
    [Button("清除位置预览", ButtonSizes.Medium)]
    [GUIColor(1f, 0.7f, 0.7f)]
    public void ClearPositionPreviews()
    {
#if UNITY_EDITOR
        ClearScenePreview();
#endif
    }
    #endregion
    #region 辅助方法
    /// <summary>
    /// 获取角色类型列表（用于下拉菜单）
    /// </summary>
    private IEnumerable<string> GetCharacterTypes()
    {
        foreach (var character in characterPrefabs)
        {
            if (!string.IsNullOrEmpty(character.characterType))
                yield return character.characterType;
        }
    }
    /// <summary>
    /// 根据角色类型获取预制体
    /// </summary>
    public GameObject GetCharacterPrefab(string characterType)
    {
        var config = characterPrefabs.Find(c => c.characterType == characterType);
        return config?.prefab;
    }

    /// <summary>
    /// 根据敌人类型获取预制体
    /// </summary>
    public GameObject GetEnemyPrefab(string enemyType)
    {
        var config = enemyPrefabs.Find(e => e.enemyType == enemyType);
        return config?.prefab;
    }

    /// <summary>
    /// 根据NPC类型获取预制体
    /// </summary>
    public GameObject GetNPCPrefab(string npcType)
    {
        var config = npcPrefabs.Find(n => n.npcType == npcType);
        return config?.prefab;
    }

    /// <summary>
    /// 获取角色的初始场景区域配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>初始场景区域配置，如果未找到则返回null</returns>
    public CharacterInitialAreaConfig GetCharacterInitialAreaConfig(string characterType)
    {
        return characterInitialAreas.Find(config => config.characterType == characterType);
    }

    /// <summary>
    /// 获取角色的初始场景区域
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>初始场景区域，如果未找到则返回默认场景区域</returns>
    public SceneArea GetCharacterInitialSceneArea(string characterType)
    {
        // 首先尝试获取角色特定的初始区域配置
        var characterConfig = GetCharacterInitialAreaConfig(characterType);
        if (characterConfig != null && 
            !string.IsNullOrEmpty(characterConfig.initialMainAreaId) &&
            !string.IsNullOrEmpty(characterConfig.initialSubAreaId) &&
            !string.IsNullOrEmpty(characterConfig.initialSceneAreaId))
        {
            var sceneArea = FindSceneArea(characterConfig.initialMainAreaId, 
                                        characterConfig.initialSubAreaId, 
                                        characterConfig.initialSceneAreaId);
            if (sceneArea != null)
            {
                return sceneArea;
            }
        }

        // 如果没有找到角色特定配置，使用默认配置
        if (!string.IsNullOrEmpty(defaultInitialMainAreaId) &&
            !string.IsNullOrEmpty(defaultInitialSubAreaId) &&
            !string.IsNullOrEmpty(defaultInitialSceneAreaId))
        {
            var defaultSceneArea = FindSceneArea(defaultInitialMainAreaId, 
                                               defaultInitialSubAreaId, 
                                               defaultInitialSceneAreaId);
            if (defaultSceneArea != null)
            {
                return defaultSceneArea;
            }
        }

        // 最后的备选方案：返回第一个可用的场景区域
        foreach (var mainArea in mainAreas)
        {
            foreach (var subArea in mainArea.subAreas)
            {
                if (subArea.sceneAreas.Count > 0)
                {
                    return subArea.sceneAreas[0];
                }
            }
        }

        return null;
    }


    /// <summary>
    /// 获取角色的初始生成朝向
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>初始生成朝向（角度）</returns>
    public float GetCharacterInitialSpawnRotation(string characterType)
    {
        var characterConfig = GetCharacterInitialAreaConfig(characterType);
        return characterConfig?.spawnRotation ?? 0f;
    }

    /// <summary>
    /// 根据ID查找主区域
    /// </summary>
    public MainArea FindMainArea(string areaId)
    {
        return mainAreas.Find(area => area.areaId == areaId);
    }

    /// <summary>
    /// 根据ID查找分区域
    /// </summary>
    public SubArea FindSubArea(string mainAreaId, string subAreaId)
    {
        MainArea mainArea = FindMainArea(mainAreaId);
        return mainArea?.subAreas.Find(area => area.areaId == subAreaId);
    }

    /// <summary>
    /// 根据ID查找场景区域
    /// </summary>
    public SceneArea FindSceneArea(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        SubArea subArea = FindSubArea(mainAreaId, subAreaId);
        return subArea?.sceneAreas.Find(area => area.areaId == sceneAreaId);
    }

    /// <summary>
    /// 获取所有场景区域
    /// </summary>
    public List<SceneArea> GetAllSceneAreas()
    {
        List<SceneArea> allScenes = new List<SceneArea>();
        foreach (MainArea mainArea in mainAreas)
        {
            foreach (SubArea subArea in mainArea.subAreas)
            {
                allScenes.AddRange(subArea.sceneAreas);
            }
        }
        return allScenes;
    }

    /// <summary>
    /// 验证配置完整性
    /// </summary>
    [Button("验证地图配置", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void ValidateMapConfiguration()
    {
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();

        if (string.IsNullOrEmpty(systemName))
            errors.Add("系统名称不能为空");

        if (mainAreas.Count == 0)
            errors.Add("至少需要配置一个主区域");

        // 验证主区域
        HashSet<string> mainAreaIds = new HashSet<string>();
        foreach (MainArea mainArea in mainAreas)
        {
            if (string.IsNullOrEmpty(mainArea.areaId))
                errors.Add($"主区域ID不能为空");
            else if (mainAreaIds.Contains(mainArea.areaId))
                errors.Add($"主区域ID重复: {mainArea.areaId}");
            else
                mainAreaIds.Add(mainArea.areaId);

            if (string.IsNullOrEmpty(mainArea.areaName))
                warnings.Add($"主区域 '{mainArea.areaId}' 缺少名称");

            if (mainArea.subAreas.Count == 0)
                warnings.Add($"主区域 '{mainArea.areaId}' 没有分区域");

            // 验证分区域
            HashSet<string> subAreaIds = new HashSet<string>();
            foreach (SubArea subArea in mainArea.subAreas)
            {
                if (string.IsNullOrEmpty(subArea.areaId))
                    errors.Add($"分区域ID不能为空 (主区域: {mainArea.areaId})");
                else if (subAreaIds.Contains(subArea.areaId))
                    errors.Add($"分区域ID重复: {subArea.areaId} (主区域: {mainArea.areaId})");
                else
                    subAreaIds.Add(subArea.areaId);

                if (subArea.sceneAreas.Count == 0)
                    warnings.Add($"分区域 '{subArea.areaId}' 没有场景区域");

                // 验证场景区域
                HashSet<string> sceneAreaIds = new HashSet<string>();
                foreach (SceneArea sceneArea in subArea.sceneAreas)
                {
                    if (string.IsNullOrEmpty(sceneArea.areaId))
                        errors.Add($"场景区域ID不能为空 (分区域: {subArea.areaId})");
                    else if (sceneAreaIds.Contains(sceneArea.areaId))
                        errors.Add($"场景区域ID重复: {sceneArea.areaId} (分区域: {subArea.areaId})");
                    else
                        sceneAreaIds.Add(sceneArea.areaId);

                    if (sceneArea.mapPrefab == null)
                        warnings.Add($"场景区域 '{sceneArea.areaId}' 没有地图预制体");
                }
            }
        }

        // 输出验证结果
        if (errors.Count > 0)
        {
            Debug.LogError($"[MapSystemConfig] 配置验证失败:\n{string.Join("\n", errors)}");
        }
        
        if (warnings.Count > 0)
        {
            Debug.LogWarning($"[MapSystemConfig] 配置警告:\n{string.Join("\n", warnings)}");
        }

        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log($"[MapSystemConfig] 配置验证通过！共有 {GetAllSceneAreas().Count} 个场景区域");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 从场景中同步玩家信息
    /// </summary>
    private void SyncPositionFromScene()
    {
        GameObject player = Array.Find(GameObject.FindGameObjectsWithTag("EditorOnly"), (player) => player.name.StartsWith("PlayerPreview"));
        if (player != null)
        {
            // 查找当前选中的场景区域
            SceneArea currentSceneArea = GetCurrentEditingSceneArea();
            if (currentSceneArea != null)
            {
                currentSceneArea.playerSpawnPosition = player.transform.position;
                EditorUtility.SetDirty(this);
                Debug.Log($"[MapSystemConfig] 玩家位置已同步: {currentSceneArea.playerSpawnPosition}");
            }
            else
            {
                Debug.LogWarning("[MapSystemConfig] 未找到当前编辑的场景区域");
            }
        }
        else
        {
            Debug.LogWarning("[MapSystemConfig] 场景中未找到标签为 'PlayerPreview' 的对象");
        }
        
    }

    /// <summary>
    /// 从场景中同步NPC位置
    /// </summary>
    private void SyncNPCPositionsFromScene()
    {
        int syncedCount = 0;
        SceneArea currentSceneArea = GetCurrentEditingSceneArea();
        if (currentSceneArea == null)
        {
            Debug.LogWarning("[MapSystemConfig] 未找到当前编辑的场景区域");
            return;
        }

        GameObject[] npcs = Array.FindAll(GameObject.FindGameObjectsWithTag("EditorOnly"), (npc) => npc.name.StartsWith("npc_"));
        foreach (GameObject npc in npcs)
        {
            if (npc != null)
            {
                // 查找对应的NPC生成配置
                int npcIndex = int.Parse(npc.name.Substring(4));
                if (npcIndex < currentSceneArea.npcSpawns.Count)
                {
                    currentSceneArea.npcSpawns[npcIndex].spawnPosition = npc.transform.position;
                    syncedCount++;
                }
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[MapSystemConfig] 已同步 {syncedCount} 个NPC位置");
    }

    /// <summary>
    /// 从场景中同步敌人位置
    /// </summary>
    private void SyncEnemyPositionsFromScene()
    {
        int syncedCount = 0;
        SceneArea currentSceneArea = GetCurrentEditingSceneArea();
        if (currentSceneArea == null)
        {
            Debug.LogWarning("[MapSystemConfig] 未找到当前编辑的场景区域");
            return;
        }

        GameObject[] enemies = Array.FindAll(GameObject.FindGameObjectsWithTag("EditorOnly"), (enemy) => enemy.name.StartsWith("enemy_"));
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                // 查找对应的敌人生成配置
                int enemyIndex = int.Parse(enemy.name.Substring(6));
                if (enemyIndex < currentSceneArea.enemySpawns.Count)
                {
                    currentSceneArea.enemySpawns[enemyIndex].spawnPosition = enemy.transform.position;
                    syncedCount++;
                }
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[MapSystemConfig] 已同步 {syncedCount} 个敌人位置");
    }

    /// <summary>
    /// 从场景中同步地图位置
    /// </summary>
    private void SyncMapPositionFromScene()
    {
        GameObject mapObject = Array.Find(GameObject.FindGameObjectsWithTag("EditorOnly"), (map) => map.name.StartsWith("MapPreview"));
        if (mapObject == null)
        {
            mapObject = GameObject.Find("Map");
        }

        if (mapObject != null)
        {
            SceneArea currentSceneArea = GetCurrentEditingSceneArea();
            if (currentSceneArea != null)
            {
                if (currentSceneArea.mapPrefab == null)
                {
                    Debug.LogWarning("[MapSystemConfig] 地图预制体未设置，无法同步位置");
                    return;
                }
                currentSceneArea.mapSpawnPosition = mapObject.transform.position;
                Debug.Log($"[MapSystemConfig] 地图位置: {mapObject.transform.position}");
                EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogWarning("[MapSystemConfig] 未找到当前编辑的场景区域");
            }
        }
        else
        {
            Debug.LogWarning("[MapSystemConfig] 场景中未找到地图对象");
        }
    }

    /// <summary>
    /// 生成场景预览
    /// </summary>
    private void GenerateScenePreview()
    {
        ClearScenePreview(); // 先清除旧的预览

        SceneArea currentSceneArea = GetCurrentEditingSceneArea();
        if (currentSceneArea == null)
        {
            Debug.LogWarning("[MapSystemConfig] 未找到当前编辑的场景区域");
            return;
        }

        // 生成玩家位置预览
        if (currentSceneArea.playerSpawnPosition != Vector3.zero)
        {
            CreatePositionPreview(currentSceneArea.playerSpawnPosition, "PlayerPreview", Color.green, GetCharacterPrefab(defaultCharacterType));
        }

        // 生成NPC位置预览
        for (var i = 0; i < currentSceneArea.npcSpawns.Count; i++)
        {
            CreatePositionPreview(currentSceneArea.npcSpawns[i].spawnPosition, $"npc_{i}", Color.blue, GetNPCPrefab(currentSceneArea.npcSpawns[i].npcType));
        }

        // 生成敌人位置预览
        for (var i = 0; i < currentSceneArea.enemySpawns.Count; i++)
        {
            CreatePositionPreview(currentSceneArea.enemySpawns[i].spawnPosition, $"enemy_{i}", Color.red, GetEnemyPrefab(currentSceneArea.enemySpawns[i].enemyType));
        }

        // 生成地图预览
        if (currentSceneArea.mapPrefab != null)
        {
            CreatePositionPreview(currentSceneArea.mapSpawnPosition, "MapPreview", Color.yellow, currentSceneArea.mapPrefab);
        }

        Debug.Log("[MapSystemConfig] 位置预览生成完成");
    }

    /// <summary>
    /// 创建位置预览对象
    /// </summary>
    private void CreatePositionPreview(Vector3 position, string name, Color color, GameObject preview)
    {
        if (preview == null)
        {
            // 如果没有预制体，创建一个简单的球体作为预览
            GameObject previewInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            previewInstance.name = name;
            previewInstance.transform.position = position;
            previewInstance.transform.localScale = Vector3.one * 0.5f;
            
            // 设置颜色
            Renderer renderer = previewInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                renderer.material = mat;
            }
            
            // 移除碰撞体
            Collider collider = previewInstance.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            previewInstance.tag = "EditorOnly";
        }
        else
        {
            GameObject previewInstance = Instantiate(preview, position, Quaternion.identity);
            previewInstance.name = name;
            previewInstance.transform.position = position;

            // 移除碰撞体
            Collider collider = previewInstance.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }

            // 添加标签以便清理
            previewInstance.tag = "EditorOnly";
        }
    }

    /// <summary>
    /// 清除场景预览
    /// </summary>
    private void ClearScenePreview()
    {
        GameObject[] previews = GameObject.FindGameObjectsWithTag("EditorOnly");
        foreach (GameObject preview in previews)
        {
            DestroyImmediate(preview);
        }
        Debug.Log("[MapSystemConfig] 位置预览已清除");
    }

    /// <summary>
    /// 获取当前正在编辑的场景区域
    /// 根据选择的下拉框值返回对应的场景区域
    /// </summary>
    private SceneArea GetCurrentEditingSceneArea()
    {
        if (!string.IsNullOrEmpty(selectedMainAreaId) && 
            !string.IsNullOrEmpty(selectedSubAreaId) && 
            !string.IsNullOrEmpty(selectedSceneAreaId))
        {
            return FindSceneArea(selectedMainAreaId, selectedSubAreaId, selectedSceneAreaId);
        }

        // 如果没有选择，返回第一个场景区域作为默认值
        foreach (MainArea mainArea in mainAreas)
        {
            foreach (SubArea subArea in mainArea.subAreas)
            {
                if (subArea.sceneAreas.Count > 0)
                {
                    return subArea.sceneAreas[0];
                }
            }
        }
        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 获取主区域选项
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetMainAreaOptions()
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择主区域", ""));
        
        foreach (var mainArea in mainAreas)
        {
            if (!string.IsNullOrEmpty(mainArea.areaId))
            {
                string displayName = string.IsNullOrEmpty(mainArea.areaName) ? mainArea.areaId : $"{mainArea.areaName} ({mainArea.areaId})";
                options.Add(new ValueDropdownItem<string>(displayName, mainArea.areaId));
            }
        }
        return options;
    }

    /// <summary>
    /// 获取分区域选项
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetSubAreaOptions()
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择分区域", ""));
        
        if (!string.IsNullOrEmpty(selectedMainAreaId))
        {
            var mainArea = FindMainArea(selectedMainAreaId);
            if (mainArea != null)
            {
                foreach (var subArea in mainArea.subAreas)
                {
                    if (!string.IsNullOrEmpty(subArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(subArea.areaName) ? subArea.areaId : $"{subArea.areaName} ({subArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, subArea.areaId));
                    }
                }
            }
        }
        return options;
    }

    /// <summary>
    /// 获取场景区域选项
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetSceneAreaOptions()
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择场景区域", ""));
        
        if (!string.IsNullOrEmpty(selectedMainAreaId) && !string.IsNullOrEmpty(selectedSubAreaId))
        {
            var subArea = FindSubArea(selectedMainAreaId, selectedSubAreaId);
            if (subArea != null)
            {
                foreach (var sceneArea in subArea.sceneAreas)
                {
                    if (!string.IsNullOrEmpty(sceneArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(sceneArea.areaName) ? sceneArea.areaId : $"{sceneArea.areaName} ({sceneArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, sceneArea.areaId));
                    }
                }
            }
        }
        return options;
    }

    /// <summary>
    /// 主区域选择变更回调
    /// </summary>
    private void OnMainAreaChanged()
    {
        selectedSubAreaId = "";
        selectedSceneAreaId = "";
    }

    /// <summary>
    /// 分区域选择变更回调
    /// </summary>
    private void OnSubAreaChanged()
    {
        selectedSceneAreaId = "";
    }

    /// <summary>
    /// 场景区域选择变更回调
    /// </summary>
    private void OnSceneAreaChanged()
    {
        // 可以在这里添加场景区域变更时的额外逻辑
    }

    /// <summary>
    /// 获取默认设置的分区域选项
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetSubAreaOptionsForDefault()
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择分区域", ""));
        
        if (!string.IsNullOrEmpty(defaultInitialMainAreaId))
        {
            var mainArea = FindMainArea(defaultInitialMainAreaId);
            if (mainArea != null)
            {
                foreach (var subArea in mainArea.subAreas)
                {
                    if (!string.IsNullOrEmpty(subArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(subArea.areaName) ? subArea.areaId : $"{subArea.areaName} ({subArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, subArea.areaId));
                    }
                }
            }
        }
        return options;
    }

    /// <summary>
    /// 获取默认设置的场景区域选项
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetSceneAreaOptionsForDefault()
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择场景区域", ""));
        
        if (!string.IsNullOrEmpty(defaultInitialMainAreaId) && !string.IsNullOrEmpty(defaultInitialSubAreaId))
        {
            var subArea = FindSubArea(defaultInitialMainAreaId, defaultInitialSubAreaId);
            if (subArea != null)
            {
                foreach (var sceneArea in subArea.sceneAreas)
                {
                    if (!string.IsNullOrEmpty(sceneArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(sceneArea.areaName) ? sceneArea.areaId : $"{sceneArea.areaName} ({sceneArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, sceneArea.areaId));
                    }
                }
            }
        }
        return options;
    }

    /// <summary>
    /// 获取角色配置的分区域选项
    /// </summary>
    public IEnumerable<ValueDropdownItem<string>> GetSubAreaOptionsForCharacter(string mainAreaId)
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择分区域", ""));
        
        if (!string.IsNullOrEmpty(mainAreaId))
        {
            var mainArea = FindMainArea(mainAreaId);
            if (mainArea != null)
            {
                foreach (var subArea in mainArea.subAreas)
                {
                    if (!string.IsNullOrEmpty(subArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(subArea.areaName) ? subArea.areaId : $"{subArea.areaName} ({subArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, subArea.areaId));
                    }
                }
            }
        }
        return options;
    }

    /// <summary>
    /// 获取角色配置的场景区域选项
    /// </summary>
    public IEnumerable<ValueDropdownItem<string>> GetSceneAreaOptionsForCharacter(string mainAreaId, string subAreaId)
    {
        var options = new List<ValueDropdownItem<string>>();
        options.Add(new ValueDropdownItem<string>("请选择场景区域", ""));
        
        if (!string.IsNullOrEmpty(mainAreaId) && !string.IsNullOrEmpty(subAreaId))
        {
            var subArea = FindSubArea(mainAreaId, subAreaId);
            if (subArea != null)
            {
                foreach (var sceneArea in subArea.sceneAreas)
                {
                    if (!string.IsNullOrEmpty(sceneArea.areaId))
                    {
                        string displayName = string.IsNullOrEmpty(sceneArea.areaName) ? sceneArea.areaId : $"{sceneArea.areaName} ({sceneArea.areaId})";
                        options.Add(new ValueDropdownItem<string>(displayName, sceneArea.areaId));
                    }
                }
            }
        }
        return options;
    }
#endif
#endif
    #endregion
}

#region 游戏对象配置类定义

/// <summary>
/// 角色预制体配置
/// </summary>
[System.Serializable]
public class CharacterPrefabConfig
{
    [LabelText("角色类型")]
    public string characterType;

    [LabelText("角色预制体")]
    [AssetsOnly]
    public GameObject prefab;
}

/// <summary>
/// 敌人预制体配置
/// </summary>
[System.Serializable]
public class EnemyPrefabConfig
{
    [LabelText("敌人类型")]
    public string enemyType;

    [LabelText("敌人预制体")]
    [AssetsOnly]
    public GameObject prefab;
}

/// <summary>
/// NPC预制体配置
/// </summary>
[System.Serializable]
public class NPCPrefabConfig
{
    [LabelText("NPC类型")]
    public string npcType;

    [LabelText("NPC预制体")]
    [AssetsOnly]
    public GameObject prefab;
}

/// <summary>
/// 敌人生成配置
/// </summary>
[System.Serializable]
public class EnemySpawnConfig
{
    [LabelText("敌人类型")]
    [ValueDropdown("GetAvailableEnemyTypes")]
    [OnValueChanged("OnEnemyTypeChanged")]
    public string enemyType;

    [LabelText("敌人显示名称")]
    [ReadOnly]
    [ShowInInspector]
    public string enemyDisplayName;

    [LabelText("生成位置")]
    public Vector3 spawnPosition;

    [LabelText("生成延迟")]
    [SuffixLabel("秒")]
    public float spawnDelay = 0f;

    [LabelText("生成数量")]
    [Range(0, 10)]
    public int spawnCount = 1;

    [LabelText("随机化位置")]
    public bool randomizePosition = false;

    [LabelText("生成半径")]
    [ShowIf("randomizePosition")]
    [SuffixLabel("米")]
    public float spawnRadius = 2f;

    [LabelText("是否自动生成")]
    public bool autoSpawn = true;

    [LabelText("自动生成巡逻点")]
    public bool autoGeneratePatrolPoints = true;

    [LabelText("巡逻半径")]
    [ShowIf("autoGeneratePatrolPoints")]
    [SuffixLabel("米")]
    public float patrolRadius = 5f;

    [LabelText("巡逻点列表")]
    [HideIf("autoGeneratePatrolPoints")]
    public List<Vector3> patrolPoints = new List<Vector3>();

    /// <summary>
    /// 获取可用的敌人类型列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableEnemyTypes()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        // 获取MapSystemConfig实例
        var mapConfig = GetMapSystemConfig();
        if (mapConfig == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到MapSystemConfig", ""));
            return items;
        }

        foreach (var enemyPrefab in mapConfig.enemyPrefabs)
        {
            if (!string.IsNullOrEmpty(enemyPrefab.enemyType))
            {
                items.Add(new ValueDropdownItem<string>($"({enemyPrefab.enemyType})", enemyPrefab.enemyType));
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("没有可用的敌人类型", ""));
        }

        return items;
    }

    /// <summary>
    /// 敌人类型改变时的回调
    /// </summary>
    private void OnEnemyTypeChanged()
    {
        UpdateEnemyDisplayName();
    }

    /// <summary>
    /// 更新敌人显示名称
    /// </summary>
    private void UpdateEnemyDisplayName()
    {
        var mapConfig = GetMapSystemConfig();
        if (mapConfig != null && !string.IsNullOrEmpty(enemyType))
        {
            var enemyPrefab = mapConfig.enemyPrefabs.Find(e => e.enemyType == enemyType);
            if (enemyPrefab != null)
            {
     enemyDisplayName = enemyType;            }
        }
        else
        {
            enemyDisplayName = enemyType;
        }
    }

    /// <summary>
    /// 获取MapSystemConfig实例
    /// </summary>
    private MapSystemConfig GetMapSystemConfig()
    {
#if UNITY_EDITOR
        // 在编辑器模式下，尝试查找MapSystemConfig资源
        var configs = UnityEditor.AssetDatabase.FindAssets("t:MapSystemConfig");
        if (configs.Length > 0)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(configs[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<MapSystemConfig>(path);
        }
#endif
        return null;
    }
}

/// <summary>
/// NPC生成配置
/// </summary>
[System.Serializable]
public class NPCSpawnConfig
{
    [LabelText("NPC类型")]
    [ValueDropdown("GetAvailableNPCTypes")]
    [OnValueChanged("OnNPCTypeChanged")]
    public string npcType;

    [LabelText("NPC显示名称")]
    [ReadOnly]
    [ShowInInspector]
    public string npcDisplayName;

    [LabelText("生成位置")]
    public Vector3 spawnPosition;

    [LabelText("是否自动生成")]
    public bool autoSpawn = true;

    [LabelText("朝向角度")]
    [Range(0, 360)]
    public float facingAngle = 0f;

    [LabelText("NPC配置文件")]
    [InfoBox("可选：指定特定的NPC配置文件，如果为空则使用默认配置")]
    [AssetsOnly]
    public NPCConfig npcConfig;

    /// <summary>
    /// 获取可用的NPC类型列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableNPCTypes()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        // 获取MapSystemConfig实例
        var mapConfig = GetMapSystemConfig();
        if (mapConfig == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到MapSystemConfig", ""));
            return items;
        }

        foreach (var npcPrefab in mapConfig.npcPrefabs)
        {
            if (!string.IsNullOrEmpty(npcPrefab.npcType))
            {
                items.Add(new ValueDropdownItem<string>($"({npcPrefab.npcType})", npcPrefab.npcType));
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("没有可用的NPC类型", ""));
        }

        return items;
    }

    /// <summary>
    /// NPC类型改变时的回调
    /// </summary>
    private void OnNPCTypeChanged()
    {
        UpdateNPCDisplayName();
    }

    /// <summary>
    /// 更新NPC显示名称
    /// </summary>
    private void UpdateNPCDisplayName()
    {
        var mapConfig = GetMapSystemConfig();
        if (mapConfig != null && !string.IsNullOrEmpty(npcType))
        {
            var npcPrefab = mapConfig.npcPrefabs.Find(n => n.npcType == npcType);
            if (npcPrefab != null)
            {
                npcDisplayName = npcType;
            }
        }
        else
        {
            npcDisplayName = npcType;
        }
    }

    /// <summary>
    /// 获取MapSystemConfig实例
    /// </summary>
    private MapSystemConfig GetMapSystemConfig()
    {
#if UNITY_EDITOR
        // 在编辑器模式下，尝试查找MapSystemConfig资源
        var configs = UnityEditor.AssetDatabase.FindAssets("t:MapSystemConfig");
        if (configs.Length > 0)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(configs[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<MapSystemConfig>(path);
        }
#endif
        return null;
    }
}

#endregion

#region 地图层级数据结构

/// <summary>
/// 主区域 - 地图系统的顶级区域
/// </summary>
[System.Serializable]
public class MainArea
{
    [FoldoutGroup("基本信息", expanded: true)]
    [LabelText("区域ID")]
    [Required("必须指定区域ID")]
    public string areaId;

    [FoldoutGroup("基本信息")]
    [LabelText("区域名称")]
    [Required("必须指定区域名称")]
    public string areaName;

    [FoldoutGroup("基本信息")]
    [LabelText("区域描述")]
    [TextArea(2, 3)]
    public string areaDescription;

    [FoldoutGroup("基本信息")]
    [LabelText("区域类型")]
    public AreaType areaType = AreaType.Normal;

    [FoldoutGroup("视觉设置", expanded: false)]
    [LabelText("区域图标")]
    [AssetsOnly]
    public Sprite areaIcon;

    [FoldoutGroup("视觉设置")]
    [LabelText("区域颜色")]
    public Color areaColor = Color.white;

    [FoldoutGroup("子区域", expanded: true)]
    [LabelText("分区域列表")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "areaName")]
    public List<SubArea> subAreas = new List<SubArea>();

    [FoldoutGroup("配置", expanded: false)]
    [LabelText("区域配置")]
    [InfoBox("可选：指定特定的区域配置文件")]
    [AssetsOnly]
    public UnifiedSceneConfig areaConfig;
}

/// <summary>
/// 分区域 - 主区域下的子区域
/// </summary>
[System.Serializable]
public class SubArea
{
    [FoldoutGroup("基本信息", expanded: true)]
    [LabelText("区域ID")]
    [Required("必须指定区域ID")]
    public string areaId;

    [FoldoutGroup("基本信息")]
    [LabelText("区域名称")]
    [Required("必须指定区域名称")]
    public string areaName;

    [FoldoutGroup("基本信息")]
    [LabelText("区域描述")]
    [TextArea(2, 3)]
    public string areaDescription;

    [FoldoutGroup("基本信息")]
    [LabelText("区域类型")]
    public AreaType areaType = AreaType.Normal;

    [FoldoutGroup("视觉设置", expanded: false)]
    [LabelText("区域图标")]
    [AssetsOnly]
    public Sprite areaIcon;

    [FoldoutGroup("视觉设置")]
    [LabelText("区域颜色")]
    public Color areaColor = Color.white;

    [FoldoutGroup("场景区域", expanded: true)]
    [LabelText("场景区域列表")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "areaName")]
    public List<SceneArea> sceneAreas = new List<SceneArea>();
}

/// <summary>
/// 场景区域 - 实际的游戏场景，对应mapPrefab
/// </summary>
[System.Serializable]
public class SceneArea
{
    [FoldoutGroup("基本信息", expanded: true)]
    [LabelText("区域ID")]
    [Required("必须指定区域ID")]
    public string areaId;

    [FoldoutGroup("基本信息")]
    [LabelText("区域名称")]
    [Required("必须指定区域名称")]
    public string areaName;

    [FoldoutGroup("基本信息")]
    [LabelText("区域描述")]
    [TextArea(2, 3)]
    public string areaDescription;

    [FoldoutGroup("基本信息")]
    [LabelText("区域类型")]
    public AreaType areaType = AreaType.Normal;

    [FoldoutGroup("基本信息")]
    [LabelText("背景音乐")]
    public AudioClip backgroundMusic;
     [FoldoutGroup("基本信息")]
    [LabelText("是否是初始区域")]
    public bool isInitArea = false;

    [FoldoutGroup("地图设置", expanded: true)]
    [LabelText("地图预制体")]
    [Required("必须指定地图预制体")]
    [AssetsOnly]
    [InfoBox("包含TILE地图层、装饰层、背景层、可交互层的完整地图预制体")]
    public GameObject mapPrefab;

    [FoldoutGroup("地图设置")]
    [LabelText("地图生成位置")]
    public Vector3 mapSpawnPosition = Vector3.zero;
    [FoldoutGroup("玩家设置", expanded: false)]
    [LabelText("玩家生成点")]
    [InfoBox("玩家进入此区域时的生成位置")]
    public Vector3 playerSpawnPosition = Vector3.zero;

    [FoldoutGroup("敌人配置", expanded: false)]
    [LabelText("敌人生成配置")]
    [InfoBox("此场景区域中的敌人生成设置")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<EnemySpawnConfig> enemySpawns = new List<EnemySpawnConfig>();

    [FoldoutGroup("NPC配置", expanded: false)]
    [LabelText("NPC生成配置")]
    [InfoBox("此场景区域中的NPC生成设置")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<NPCSpawnConfig> npcSpawns = new List<NPCSpawnConfig>();

    [FoldoutGroup("配置", expanded: false)]
    [LabelText("场景配置")]
    [InfoBox("可选：指定特定的场景配置文件，会覆盖默认配置")]
    [AssetsOnly]
    public UnifiedSceneConfig sceneConfig;

    [FoldoutGroup("配置")]
    [LabelText("启用状态保持")]
    [InfoBox("是否保存和恢复此区域的游戏状态")]
    public bool enableStatePersistence = true;

    [FoldoutGroup("配置")]
    [LabelText("预加载")]
    [InfoBox("是否在游戏开始时预加载此区域")]
    public bool preloadOnStart = false;

}
#endregion



#region 初始场景配置数据结构

/// <summary>
/// 角色初始区域配置
/// </summary>
[System.Serializable]
public class CharacterInitialAreaConfig
{
    [FoldoutGroup("角色信息", expanded: true)]
    [LabelText("角色类型")]
    [Required("必须指定角色类型")]
    [ValueDropdown("@UnityEngine.Resources.FindObjectsOfTypeAll<MapSystemConfig>().FirstOrDefault()?.GetCharacterTypes()")]
    public string characterType;

    [FoldoutGroup("角色信息")]
    [LabelText("角色显示名称")]
    public string characterDisplayName;

    [FoldoutGroup("初始位置", expanded: true)]
    [LabelText("初始主区域ID")]
    [ValueDropdown("@UnityEngine.Resources.FindObjectsOfTypeAll<MapSystemConfig>().FirstOrDefault()?.GetMainAreaOptions()")]
    public string initialMainAreaId;

    [FoldoutGroup("初始位置")]
    [LabelText("初始分区域ID")]
    [ValueDropdown("@UnityEngine.Resources.FindObjectsOfTypeAll<MapSystemConfig>().FirstOrDefault()?.GetSubAreaOptionsForCharacter(initialMainAreaId)")]
    [ShowIf("@!string.IsNullOrEmpty(initialMainAreaId)")]
    public string initialSubAreaId;

    [FoldoutGroup("初始位置")]
    [LabelText("初始场景区域ID")]
    [ValueDropdown("@UnityEngine.Resources.FindObjectsOfTypeAll<MapSystemConfig>().FirstOrDefault()?.GetSceneAreaOptionsForCharacter(initialMainAreaId, initialSubAreaId)")]
    [ShowIf("@!string.IsNullOrEmpty(initialSubAreaId)")]
    public string initialSceneAreaId;

    [FoldoutGroup("生成设置", expanded: false)]
    [LabelText("自定义生成位置")]
    [InfoBox("如果不为零向量，将覆盖场景区域的默认玩家生成点")]
    public Vector3 customSpawnPosition = Vector3.zero;

    [FoldoutGroup("生成设置")]
    [LabelText("生成朝向")]
    [Range(0f, 360f)]
    public float spawnRotation = 0f;
}

#endregion

#region 枚举定义

/// <summary>
/// 区域类型
/// </summary>
public enum AreaType
{
    Normal,     // 普通区域
    Town,       // 城镇
    Dungeon,    // 地牢
    Boss,       // Boss区域
    Safe,       // 安全区域
    PvP,        // PvP区域
    Special     // 特殊区域
}

#endregion

#region 状态数据结构

/// <summary>
/// 区域状态数据
/// </summary>
[System.Serializable]
public class AreaState
{
    public string areaId;
    public Vector3 playerPosition;
    public List<EnemyStateData> enemyStates;
    public List<NPCStateData> npcStates;
    public List<ItemStateData> itemStates;
    public List<QuestStateData> questStates;
    public System.DateTime saveTime;
}


/// <summary>
/// 任务状态数据
/// </summary>
[System.Serializable]
public class QuestStateData
{
    public string questId;
    public string questState;
    public int progress;
}

#endregion


