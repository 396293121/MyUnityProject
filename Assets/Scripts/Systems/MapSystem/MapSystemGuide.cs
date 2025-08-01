using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 地图系统使用指南 - 提供完整的地图系统使用说明和最佳实践
/// 包含系统概述、使用流程、最佳实践和常见问题解决方案
/// </summary>
public class MapSystemGuide : MonoBehaviour
{
    #region 系统概述
    [TitleGroup("地图系统概述")]
    [FoldoutGroup("地图系统概述/架构说明", expanded: true)]
    [InfoBox("地图系统采用主区域-分区域-场景粒度的三级层次结构，支持流畅的场景切换、状态保持和高性能渲染。")]
    [HideInInspector]
    public bool architectureInfo;

    [FoldoutGroup("地图系统概述/核心组件")]
    [InfoBox("核心组件说明：\n" +
             "• MapManager - 地图系统核心控制器\n" +
             "• MapSystemConfig - 地图系统配置文件\n" +
             "• MapStateManager - 地图状态管理器\n" +
             "• MapLayerManager - 地图层级管理器\n" +
             "• AreaTransitionTrigger - 区域切换触发器\n" +
             "• MapTransitionEffect - 地图切换特效管理器")]
    [HideInInspector]
    public bool coreComponentsInfo;

    [FoldoutGroup("地图系统概述/层级结构")]
    [InfoBox("标准层级结构：\n" +
             "• BackgroundLayer (渲染顺序: -10) - 背景元素\n" +
             "• TileLayer (渲染顺序: 0) - 地形和墙壁\n" +
             "• DecorationLayer (渲染顺序: 5) - 装饰性元素\n" +
             "• InteractableLayer (渲染顺序: 10) - NPC和可交互对象")]
    [HideInInspector]
    public bool layerStructureInfo;
    #endregion

    #region 快速开始
    [TitleGroup("快速开始")]
    [FoldoutGroup("快速开始/配置地图系统", expanded: true)]
    [InfoBox("步骤2：配置地图系统\n" +
             "1. 创建MapSystemConfig资源文件\n" +
             "2. 配置主区域、分区域和场景区域\n" +
             "3. 设置mapPrefab引用\n" +
             "4. 配置切换效果和性能参数")]
    [Button("创建地图系统配置", ButtonSizes.Large)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void CreateMapSystemConfig()
    {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanelInProject(
            "创建地图系统配置",
            "MapSystemConfig",
            "asset",
            "选择保存位置");

        if (!string.IsNullOrEmpty(path))
        {
            MapSystemConfig config = ScriptableObject.CreateInstance<MapSystemConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log($"[MapSystemGuide] 地图系统配置已创建: {path}");
        }
#endif
    }

    [FoldoutGroup("快速开始/集成到场景")]
    [InfoBox("步骤3：集成到场景\n" +
             "1. 在MapSystemConfig中配置地图预制体引用\n" +
             "2. 确保SceneController已包含地图系统初始化代码\n" +
             "3. 运行场景测试地图系统功能")]
    [Button("验证场景集成", ButtonSizes.Large)]
    [GUIColor(1f, 1f, 0.7f)]
    public void ValidateSceneIntegration()
    {
        Debug.Log("[MapSystemGuide] 开始验证场景集成...");

        int issueCount = 0;

        // 检查SceneController
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null)
        {
            Debug.LogError("[MapSystemGuide] 场景中未找到SceneController");
            issueCount++;
        }

        // 检查MapManager
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogWarning("[MapSystemGuide] 场景中未找到MapManager，将在运行时自动创建");
        }

        // 检查UnifiedSceneConfig
        if (sceneController != null)
        {
            // 通过反射获取unifiedConfig字段
            var field = typeof(SceneController).GetField("unifiedConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                UnifiedSceneConfig config = field.GetValue(sceneController) as UnifiedSceneConfig;
                if (config == null)
                {
                    Debug.LogError("[MapSystemGuide] SceneController的unifiedConfig未设置");
                    issueCount++;
                }
                // 注意：mapPrefab已迁移到MapSystemConfig，不再检查UnifiedSceneConfig中的mapPrefab
            }
        }

        if (issueCount == 0)
        {
            Debug.Log("[MapSystemGuide] 场景集成验证通过！");
        }
        else
        {
            Debug.Log($"[MapSystemGuide] 发现 {issueCount} 个问题，请检查上述错误信息");
        }
    }
    #endregion

    #region 使用流程
    [TitleGroup("使用流程")]
    [FoldoutGroup("使用流程/地图创建流程", expanded: false)]
    [InfoBox("完整的地图创建流程：\n" +
             "1. 使用MapPrefabTemplate创建地图预制体\n" +
             "2. 在各个层级中添加具体内容（瓦片、装饰、NPC等）\n" +
             "3. 使用MapDataValidator验证地图结构\n" +
             "4. 在MapSystemConfig中配置区域层次和地图预制体引用\n" +
             "5. 测试场景切换和状态保持功能")]
    [HideInInspector]
    public bool mapCreationFlow;

    [FoldoutGroup("使用流程/场景切换流程")]
    [InfoBox("场景切换的完整流程：\n" +
             "1. 玩家触发AreaTransitionTrigger\n" +
             "2. MapTransitionEffect播放切换特效\n" +
             "3. MapStateManager保存当前状态\n" +
             "4. MapManager卸载当前地图\n" +
             "5. MapManager加载新地图\n" +
             "6. MapStateManager恢复状态\n" +
             "7. MapTransitionEffect完成切换")]
    [HideInInspector]
    public bool sceneTransitionFlow;

    [FoldoutGroup("使用流程/状态管理流程")]
    [InfoBox("状态管理的工作流程：\n" +
             "1. 自动保存：根据配置定期保存状态\n" +
             "2. 切换保存：场景切换时保存当前状态\n" +
             "3. 状态恢复：加载新场景时恢复对应状态\n" +
             "4. 状态清理：根据配置清理过期状态")]
    [HideInInspector]
    public bool stateManagementFlow;
    #endregion

    #region 最佳实践
    [TitleGroup("最佳实践")]
    [FoldoutGroup("最佳实践/性能优化", expanded: false)]
    [InfoBox("性能优化建议：\n" +
             "• 启用地图缓存以减少重复加载\n" +
             "• 合理设置状态保存间隔\n" +
             "• 使用LOD系统优化远距离渲染\n" +
             "• 为静态对象设置BatchingStatic标志\n" +
             "• 定期压缩Tilemap边界\n" +
             "• 避免在单个层级中放置过多对象")]
    [Button("应用性能优化设置", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void ApplyPerformanceOptimizations()
    {
        Debug.Log("[MapSystemGuide] 开始应用性能优化设置...");

        // 查找所有地图预制体并优化
        GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("Map");
        foreach (GameObject mapObj in mapObjects)
        {
            OptimizeMapObject(mapObj);
        }

        Debug.Log("[MapSystemGuide] 性能优化设置应用完成");
    }

    private void OptimizeMapObject(GameObject mapObj)
    {
        // 压缩Tilemap边界
        UnityEngine.Tilemaps.Tilemap[] tilemaps = mapObj.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>();
        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
        }

        // 设置静态标志
#if UNITY_EDITOR
        Transform backgroundLayer = mapObj.transform.Find("BackgroundLayer");
        Transform tileLayer = mapObj.transform.Find("TileLayer");

        if (backgroundLayer != null)
        {
            GameObjectUtility.SetStaticEditorFlags(backgroundLayer.gameObject, StaticEditorFlags.BatchingStatic);
        }

        if (tileLayer != null)
        {
            GameObjectUtility.SetStaticEditorFlags(tileLayer.gameObject, StaticEditorFlags.BatchingStatic);
        }
#endif
    }

    [FoldoutGroup("最佳实践/内容组织")]
    [InfoBox("内容组织建议：\n" +
             "• 按功能将地图内容分组到对应层级\n" +
             "• 使用一致的命名规范\n" +
             "• 为可交互对象添加适当的标签\n" +
             "• 合理设置碰撞体和触发器\n" +
             "• 使用预制体变体管理相似对象\n" +
             "• 定期验证地图数据完整性")]
    [Button("验证内容组织", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void ValidateContentOrganization()
    {
        Debug.Log("[MapSystemGuide] 开始验证内容组织...");

        MapDataValidator[] validators = FindObjectsOfType<MapDataValidator>();
        if (validators.Length == 0)
        {
            Debug.LogWarning("[MapSystemGuide] 场景中未找到MapDataValidator，建议添加以验证地图数据");
        }
        else
        {
            foreach (MapDataValidator validator in validators)
            {
                validator.ValidateMapData();
            }
        }

        Debug.Log("[MapSystemGuide] 内容组织验证完成");
    }

    [FoldoutGroup("最佳实践/调试技巧")]
    [InfoBox("调试技巧：\n" +
             "• 启用MapManager的调试模式查看详细日志\n" +
             "• 使用MapDataValidator定期验证地图数据\n" +
             "• 在Scene视图中使用Gizmos显示区域边界\n" +
             "• 监控MapStateManager的状态保存情况\n" +
             "• 使用Profiler分析地图系统性能\n" +
             "• 测试不同场景切换路径")]
    [Button("启用调试模式", ButtonSizes.Medium)]
    [GUIColor(1f, 1f, 0.7f)]
    public void EnableDebugMode()
    {
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            // 通过反射启用调试模式
            var field = typeof(MapManager).GetField("debugMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(mapManager, true);
                Debug.Log("[MapSystemGuide] MapManager调试模式已启用");
            }
        }

        MapStateManager stateManager = FindObjectOfType<MapStateManager>();
        if (stateManager != null)
        {
            // 通过反射启用调试模式
            var field = typeof(MapStateManager).GetField("debugMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(stateManager, true);
                Debug.Log("[MapSystemGuide] MapStateManager调试模式已启用");
            }
        }
    }
    #endregion

    #region 常见问题
    [TitleGroup("常见问题")]
    [FoldoutGroup("常见问题/地图加载问题", expanded: false)]
    [InfoBox("地图加载问题解决方案：\n" +
             "• 检查mapPrefab引用是否正确设置\n" +
             "• 确认MapSystemConfig配置完整\n" +
             "• 验证区域ID命名是否一致\n" +
             "• 检查地图预制体是否包含必需组件\n" +
             "• 确认场景中MapManager正常工作")]
    [Button("诊断地图加载问题", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.7f)]
    public void DiagnoseMapLoadingIssues()
    {
        Debug.Log("[MapSystemGuide] 开始诊断地图加载问题...");

        // 检查MapManager
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogError("[诊断] MapManager未找到");
            return;
        }

        // 检查配置
        var configField = typeof(MapManager).GetField("mapSystemConfig", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configField != null)
        {
            MapSystemConfig config = configField.GetValue(mapManager) as MapSystemConfig;
            if (config == null)
            {
                Debug.LogError("[诊断] MapSystemConfig未设置");
            }
            else
            {
                Debug.Log($"[诊断] 找到MapSystemConfig: {config.name}");
                Debug.Log($"[诊断] 主区域数量: {config.mainAreas.Count}");
            }
        }

        // 检查当前地图实例
        var instanceField = typeof(MapManager).GetField("currentMapInstance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (instanceField != null)
        {
            GameObject currentMap = instanceField.GetValue(mapManager) as GameObject;
            if (currentMap != null)
            {
                Debug.Log($"[诊断] 当前地图实例: {currentMap.name}");
            }
            else
            {
                Debug.LogWarning("[诊断] 当前无地图实例");
            }
        }

        Debug.Log("[MapSystemGuide] 地图加载问题诊断完成");
    }

    [FoldoutGroup("常见问题/状态保存问题")]
    [InfoBox("状态保存问题解决方案：\n" +
             "• 确认enableStatePersistence已启用\n" +
             "• 检查状态保存路径是否可写\n" +
             "• 验证状态数据序列化是否正常\n" +
             "• 确认状态管理器初始化完成\n" +
             "• 检查状态历史是否超出限制")]
    [Button("诊断状态保存问题", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.7f)]
    public void DiagnoseStateSavingIssues()
    {
        Debug.Log("[MapSystemGuide] 开始诊断状态保存问题...");

        MapStateManager stateManager = FindObjectOfType<MapStateManager>();
        if (stateManager == null)
        {
            Debug.LogError("[诊断] MapStateManager未找到");
            return;
        }

        // 检查状态持久化设置
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController != null)
        {
            var field = typeof(SceneController).GetField("enableStatePersistence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                bool enabled = (bool)field.GetValue(sceneController);
                Debug.Log($"[诊断] 状态持久化启用状态: {enabled}");
            }
        }

        // 检查状态数据
        var statesField = typeof(MapStateManager).GetField("areaStates", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (statesField != null)
        {
            var states = statesField.GetValue(stateManager);
            if (states != null)
            {
                Debug.Log($"[诊断] 当前状态数据类型: {states.GetType().Name}");
            }
        }

        Debug.Log("[MapSystemGuide] 状态保存问题诊断完成");
    }

    [FoldoutGroup("常见问题/性能问题")]
    [InfoBox("性能问题解决方案：\n" +
             "• 减少同时加载的地图数量\n" +
             "• 启用地图缓存机制\n" +
             "• 优化Tilemap使用\n" +
             "• 合理设置LOD距离\n" +
             "• 避免频繁的状态保存\n" +
             "• 使用对象池管理动态对象")]
    [Button("性能分析报告", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.7f)]
    public void GeneratePerformanceReport()
    {
        Debug.Log("[MapSystemGuide] 生成性能分析报告...");

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== 地图系统性能分析报告 ===");
        report.AppendLine($"生成时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();


        // 统计Tilemap
        UnityEngine.Tilemaps.Tilemap[] tilemaps = FindObjectsOfType<UnityEngine.Tilemaps.Tilemap>();
        report.AppendLine($"Tilemap数量: {tilemaps.Length}");

        int totalTiles = 0;
        foreach (var tilemap in tilemaps)
        {
            totalTiles += tilemap.GetUsedTilesCount();
        }
        report.AppendLine($"总瓦片数量: {totalTiles}");

        // 统计触发器
        AreaTransitionTrigger[] triggers = FindObjectsOfType<AreaTransitionTrigger>();
        report.AppendLine($"区域切换触发器数量: {triggers.Length}");

        // 内存使用建议
        report.AppendLine();
        report.AppendLine("=== 性能建议 ===");
        if (totalTiles > 10000)
        {
            report.AppendLine("• 瓦片数量较多，建议优化Tilemap使用");
        }
        if (triggers.Length > 10)
        {
            report.AppendLine("• 触发器较多，建议合并相近的触发器");
        }

        Debug.Log(report.ToString());
    }
    #endregion

    #region 版本信息
    [TitleGroup("版本信息")]
    [FoldoutGroup("版本信息/系统版本", expanded: false)]
    [InfoBox("地图系统版本: 1.0.0\n" +
             "兼容Unity版本: 2021.3+\n" +
             "依赖插件: Odin Inspector\n" +
             "最后更新: 2024年")]
    [HideInInspector]
    public bool versionInfo;

    [FoldoutGroup("版本信息/更新日志")]
    [InfoBox("v1.0.0 更新内容：\n" +
             "• 完整的地图系统架构\n" +
             "• 三级层次结构支持\n" +
             "• 流畅的场景切换\n" +
             "• 状态保持功能\n" +
             "• 性能优化机制\n" +
             "• 完善的调试工具")]
    [HideInInspector]
    public bool changelogInfo;
    #endregion

    #region 编辑器辅助
#if UNITY_EDITOR
    /// <summary>
    /// 添加地图系统指南到场景
    /// </summary>
    [MenuItem("GameObject/地图系统/添加地图系统指南", false, 0)]
    public static void AddMapSystemGuide()
    {
        GameObject guideGO = new GameObject("MapSystemGuide");
        guideGO.AddComponent<MapSystemGuide>();
        
        Selection.activeGameObject = guideGO;
        Debug.Log("[MapSystemGuide] 地图系统指南已添加到场景");
    }
#endif
    #endregion
}