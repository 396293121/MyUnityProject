using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 地图数据验证器 - 验证mapPrefab和地图系统配置的完整性
/// 确保地图系统的数据结构符合规范，提供自动修复功能
/// </summary>
public class MapDataValidator : MonoBehaviour
{
    #region 验证配置
    [TitleGroup("验证配置")]
    [FoldoutGroup("验证配置/基础设置", expanded: true)]
    [LabelText("自动验证")]
    [InfoBox("启用后会在场景加载时自动验证地图数据")]
    public bool autoValidateOnLoad = true;

    [FoldoutGroup("验证配置/基础设置")]
    [LabelText("详细日志")]
    [InfoBox("输出详细的验证日志信息")]
    public bool verboseLogging = false;

    [FoldoutGroup("验证配置/基础设置")]
    [LabelText("自动修复")]
    [InfoBox("尝试自动修复发现的问题")]
    public bool autoFix = true;
    #endregion

    #region 验证规则
    [TitleGroup("验证规则")]
    [FoldoutGroup("验证规则/层级验证", expanded: true)]
    [LabelText("验证层级命名")]
    public bool validateLayerNaming = true;

    [FoldoutGroup("验证规则/层级验证")]
    [LabelText("验证渲染顺序")]
    public bool validateRenderOrder = true;

    [FoldoutGroup("验证规则/组件验证", expanded: false)]
    [LabelText("验证必需组件")]
    public bool validateRequiredComponents = true;

    [FoldoutGroup("验证规则/组件验证")]
    [LabelText("验证碰撞体设置")]
    public bool validateColliderSettings = true;

    [FoldoutGroup("验证规则/性能验证", expanded: false)]
    [LabelText("验证性能设置")]
    public bool validatePerformanceSettings = true;

    [FoldoutGroup("验证规则/性能验证")]
    [LabelText("验证静态标志")]
    public bool validateStaticFlags = true;
    #endregion

    #region 标准配置
    [TitleGroup("标准配置")]
    [FoldoutGroup("标准配置/层级标准", expanded: false)]
    [LabelText("标准层级名称")]
    [InfoBox("定义标准的层级命名规范")]
    public string[] standardLayerNames = new string[]
    {
        "BackgroundLayer",
        "TileLayer", 
        "DecorationLayer",
        "InteractableLayer"
    };

    [FoldoutGroup("标准配置/层级标准")]
    [LabelText("标准渲染顺序")]
    [InfoBox("定义各层级的标准渲染顺序")]
    public int[] standardSortingOrders = new int[]
    {
        -10,  // BackgroundLayer
        0,    // TileLayer
        5,    // DecorationLayer
        10    // InteractableLayer
    };

    [FoldoutGroup("标准配置/组件标准", expanded: false)]
    [LabelText("必需组件类型")]
    [InfoBox("地图预制体必须包含的组件类型")]
    public string[] requiredComponentTypes = new string[]
    {
        "MapLayerManager"
    };
    #endregion

    #region 验证结果
    [TitleGroup("验证结果")]
    [FoldoutGroup("验证结果/统计信息", expanded: false)]
    [LabelText("验证状态")]
    [ReadOnly]
    [ShowInInspector]
    private ValidationStatus currentStatus = ValidationStatus.NotValidated;

    [FoldoutGroup("验证结果/统计信息")]
    [LabelText("错误数量")]
    [ReadOnly]
    [ShowInInspector]
    private int errorCount = 0;

    [FoldoutGroup("验证结果/统计信息")]
    [LabelText("警告数量")]
    [ReadOnly]
    [ShowInInspector]
    private int warningCount = 0;

    [FoldoutGroup("验证结果/统计信息")]
    [LabelText("修复数量")]
    [ReadOnly]
    [ShowInInspector]
    private int fixedCount = 0;

    [FoldoutGroup("验证结果/详细结果", expanded: false)]
    [LabelText("验证报告")]
    [ReadOnly]
    [ShowInInspector]
    [TextArea(5, 10)]
    private string validationReport = "";
    #endregion

    #region 枚举定义
    public enum ValidationStatus
    {
        NotValidated,
        Validating,
        Passed,
        Failed,
        PartiallyFixed
    }

    public enum ValidationLevel
    {
        Error,
        Warning,
        Info
    }
    #endregion

    #region 验证结果类
    [System.Serializable]
    public class ValidationResult
    {
        public ValidationLevel level;
        public string category;
        public string message;
        public GameObject target;
        public bool canAutoFix;
        public bool wasFixed;

        public ValidationResult(ValidationLevel level, string category, string message, GameObject target = null, bool canAutoFix = false)
        {
            this.level = level;
            this.category = category;
            this.message = message;
            this.target = target;
            this.canAutoFix = canAutoFix;
            this.wasFixed = false;
        }
    }
    #endregion

    #region Unity生命周期
    private void Start()
    {
        if (autoValidateOnLoad)
        {
            ValidateMapData();
        }
    }
    #endregion

    #region 主要验证方法
    /// <summary>
    /// 验证地图数据
    /// </summary>
    [TitleGroup("验证操作")]
    [FoldoutGroup("验证操作/主要操作", expanded: true)]
    [Button("验证地图数据", ButtonSizes.Large)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void ValidateMapData()
    {
        currentStatus = ValidationStatus.Validating;
        List<ValidationResult> results = new List<ValidationResult>();

        if (verboseLogging)
        {
            Debug.Log("[MapDataValidator] 开始验证地图数据...");
        }

        // 重置计数器
        errorCount = 0;
        warningCount = 0;
        fixedCount = 0;

        if (validateLayerNaming)
        {
            ValidateLayerNaming(results);
        }

        if (validateRenderOrder)
        {
            ValidateRenderOrder(results);
        }

        if (validateRequiredComponents)
        {
            ValidateRequiredComponents(results);
        }

        if (validateColliderSettings)
        {
            ValidateColliderSettings(results);
        }

        if (validatePerformanceSettings)
        {
            ValidatePerformanceSettings(results);
        }

        if (validateStaticFlags)
        {
            ValidateStaticFlags(results);
        }

        // 处理验证结果
        ProcessValidationResults(results);

        // 更新状态
        if (errorCount == 0)
        {
            currentStatus = fixedCount > 0 ? ValidationStatus.PartiallyFixed : ValidationStatus.Passed;
        }
        else
        {
            currentStatus = ValidationStatus.Failed;
        }

        if (verboseLogging)
        {
            Debug.Log($"[MapDataValidator] 验证完成 - 状态: {currentStatus}, 错误: {errorCount}, 警告: {warningCount}, 修复: {fixedCount}");
        }
    }
    /// <summary>
    /// 验证层级命名
    /// </summary>
    private void ValidateLayerNaming(List<ValidationResult> results)
    {
        foreach (string standardName in standardLayerNames)
        {
            Transform layer = transform.Find(standardName);
            if (layer == null)
            {
                results.Add(new ValidationResult(ValidationLevel.Warning, "层级命名", $"未找到标准层级: {standardName}", gameObject));
            }
        }

        // 检查是否有非标准命名的层级
        foreach (Transform child in transform)
        {
            if (!standardLayerNames.Contains(child.name))
            {
                results.Add(new ValidationResult(ValidationLevel.Info, "层级命名", $"发现非标准层级命名: {child.name}", child.gameObject));
            }
        }
    }

    /// <summary>
    /// 验证渲染顺序
    /// </summary>
    private void ValidateRenderOrder(List<ValidationResult> results)
    {
        for (int i = 0; i < standardLayerNames.Length && i < standardSortingOrders.Length; i++)
        {
            Transform layer = transform.Find(standardLayerNames[i]);
            if (layer != null)
            {
                SpriteRenderer[] renderers = layer.GetComponentsInChildren<SpriteRenderer>();
                UnityEngine.Tilemaps.TilemapRenderer[] tilemapRenderers = layer.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>();

                // 检查SpriteRenderer的排序顺序
                foreach (var renderer in renderers)
                {
                    if (renderer.sortingOrder != standardSortingOrders[i])
                    {
                        results.Add(new ValidationResult(ValidationLevel.Warning, "渲染顺序", 
                            $"{standardLayerNames[i]}中的SpriteRenderer排序顺序不标准: 期望{standardSortingOrders[i]}, 实际{renderer.sortingOrder}", 
                            renderer.gameObject, true));
                    }
                }

                // 检查TilemapRenderer的排序顺序
                foreach (var renderer in tilemapRenderers)
                {
                    if (renderer.sortingOrder != standardSortingOrders[i])
                    {
                        results.Add(new ValidationResult(ValidationLevel.Warning, "渲染顺序", 
                            $"{standardLayerNames[i]}中的TilemapRenderer排序顺序不标准: 期望{standardSortingOrders[i]}, 实际{renderer.sortingOrder}", 
                            renderer.gameObject, true));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 验证必需组件
    /// </summary>
    private void ValidateRequiredComponents(List<ValidationResult> results)
    {
        foreach (string componentType in requiredComponentTypes)
        {
            Component component = GetComponent(componentType);
            if (component == null)
            {
                results.Add(new ValidationResult(ValidationLevel.Error, "必需组件", $"缺少必需组件: {componentType}", gameObject, true));
            }
        }
    }

    /// <summary>
    /// 验证碰撞体设置
    /// </summary>
    private void ValidateColliderSettings(List<ValidationResult> results)
    {
        // 检查TILE层的碰撞体
        Transform tileLayer = transform.Find("TileLayer");
        if (tileLayer != null)
        {
            UnityEngine.Tilemaps.TilemapCollider2D[] tilemapColliders = tileLayer.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapCollider2D>();
            if (tilemapColliders.Length == 0)
            {
                results.Add(new ValidationResult(ValidationLevel.Warning, "碰撞体设置", "TILE层缺少TilemapCollider2D", tileLayer.gameObject));
            }
        }

        // 检查可交互层的碰撞体
        Transform interactableLayer = transform.Find("InteractableLayer");
        if (interactableLayer != null)
        {
            Collider2D[] colliders = interactableLayer.GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                if (!collider.isTrigger)
                {
                    results.Add(new ValidationResult(ValidationLevel.Info, "碰撞体设置", 
                        $"可交互层的碰撞体建议设置为Trigger: {collider.name}", collider.gameObject, true));
                }
            }
        }
    }

    /// <summary>
    /// 验证性能设置
    /// </summary>
    private void ValidatePerformanceSettings(List<ValidationResult> results)
    {
        // 检查Tilemap压缩
        UnityEngine.Tilemaps.Tilemap[] tilemaps = GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>();
        foreach (var tilemap in tilemaps)
        {
            if (tilemap.cellBounds.size.magnitude > 100) // 大型地图
            {
                results.Add(new ValidationResult(ValidationLevel.Info, "性能设置", 
                    $"大型Tilemap建议进行边界压缩: {tilemap.name}", tilemap.gameObject, true));
            }
        }

        // 检查过多的子对象
        foreach (Transform child in transform)
        {
            if (child.childCount > 50)
            {
                results.Add(new ValidationResult(ValidationLevel.Warning, "性能设置", 
                    $"层级包含过多子对象({child.childCount})，可能影响性能: {child.name}", child.gameObject));
            }
        }
    }

    /// <summary>
    /// 验证静态标志
    /// </summary>
    private void ValidateStaticFlags(List<ValidationResult> results)
    {
#if UNITY_EDITOR
        // 检查背景层和TILE层的静态标志
        string[] staticLayers = { "BackgroundLayer", "TileLayer" };
        foreach (string layerName in staticLayers)
        {
            Transform layer = transform.Find(layerName);
            if (layer != null)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(layer.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) == 0)
                {
                    results.Add(new ValidationResult(ValidationLevel.Info, "静态标志", 
                        $"{layerName}建议设置BatchingStatic标志以优化性能", layer.gameObject, true));
                }
            }
        }
#endif
    }
    #endregion

    #region 结果处理
    /// <summary>
    /// 处理验证结果
    /// </summary>
    private void ProcessValidationResults(List<ValidationResult> results)
    {
        System.Text.StringBuilder reportBuilder = new System.Text.StringBuilder();
        reportBuilder.AppendLine("=== 地图数据验证报告 ===");
        reportBuilder.AppendLine($"验证时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        reportBuilder.AppendLine($"验证对象: {gameObject.name}");
        reportBuilder.AppendLine();

        // 按类别分组结果
        var groupedResults = results.GroupBy(r => r.category);

        foreach (var group in groupedResults)
        {
            reportBuilder.AppendLine($"[{group.Key}]");
            
            foreach (var result in group)
            {
                string levelIcon = GetLevelIcon(result.level);
                reportBuilder.AppendLine($"  {levelIcon} {result.message}");

                // 统计数量
                switch (result.level)
                {
                    case ValidationLevel.Error:
                        errorCount++;
                        break;
                    case ValidationLevel.Warning:
                        warningCount++;
                        break;
                }

                // 尝试自动修复
                if (autoFix && result.canAutoFix && !result.wasFixed)
                {
                    if (TryAutoFix(result))
                    {
                        result.wasFixed = true;
                        fixedCount++;
                        reportBuilder.AppendLine($"    ✓ 已自动修复");
                    }
                }

                // 输出到控制台
                if (verboseLogging)
                {
                    string logMessage = $"[MapDataValidator] [{group.Key}] {result.message}";
                    switch (result.level)
                    {
                        case ValidationLevel.Error:
                            Debug.LogError(logMessage, result.target);
                            break;
                        case ValidationLevel.Warning:
                            Debug.LogWarning(logMessage, result.target);
                            break;
                        case ValidationLevel.Info:
                            Debug.Log(logMessage, result.target);
                            break;
                    }
                }
            }
            reportBuilder.AppendLine();
        }

        // 添加统计信息
        reportBuilder.AppendLine("=== 统计信息 ===");
        reportBuilder.AppendLine($"错误: {errorCount}");
        reportBuilder.AppendLine($"警告: {warningCount}");
        reportBuilder.AppendLine($"修复: {fixedCount}");
        reportBuilder.AppendLine($"总计: {results.Count}");

        validationReport = reportBuilder.ToString();
    }

    /// <summary>
    /// 获取级别图标
    /// </summary>
    private string GetLevelIcon(ValidationLevel level)
    {
        switch (level)
        {
            case ValidationLevel.Error:
                return "❌";
            case ValidationLevel.Warning:
                return "⚠️";
            case ValidationLevel.Info:
                return "ℹ️";
            default:
                return "•";
        }
    }

    /// <summary>
    /// 尝试自动修复
    /// </summary>
    private bool TryAutoFix(ValidationResult result)
    {
        try
        {
            switch (result.category)
            {
                case "渲染顺序":
                    return FixRenderOrder(result);
                case "碰撞体设置":
                    return FixColliderSettings(result);
                case "性能设置":
                    return FixPerformanceSettings(result);
                case "静态标志":
                    return FixStaticFlags(result);
                default:
                    return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapDataValidator] 自动修复失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 修复渲染顺序
    /// </summary>
    private bool FixRenderOrder(ValidationResult result)
    {
        if (result.target != null)
        {
            SpriteRenderer spriteRenderer = result.target.GetComponent<SpriteRenderer>();
            UnityEngine.Tilemaps.TilemapRenderer tilemapRenderer = result.target.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();

            // 从消息中提取期望的排序顺序
            string message = result.message;
            int expectedOrder = 0;
            if (message.Contains("期望"))
            {
                string[] parts = message.Split(new string[] { "期望" }, System.StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    string orderPart = parts[1].Split(',')[0];
                    int.TryParse(orderPart, out expectedOrder);
                }
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = expectedOrder;
                return true;
            }

            if (tilemapRenderer != null)
            {
                tilemapRenderer.sortingOrder = expectedOrder;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 修复碰撞体设置
    /// </summary>
    private bool FixColliderSettings(ValidationResult result)
    {
        if (result.target != null && result.message.Contains("建议设置为Trigger"))
        {
            Collider2D collider = result.target.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 修复性能设置
    /// </summary>
    private bool FixPerformanceSettings(ValidationResult result)
    {
        if (result.target != null && result.message.Contains("边界压缩"))
        {
            UnityEngine.Tilemaps.Tilemap tilemap = result.target.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null)
            {
                tilemap.CompressBounds();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 修复静态标志
    /// </summary>
    private bool FixStaticFlags(ValidationResult result)
    {
#if UNITY_EDITOR
        if (result.target != null && result.message.Contains("BatchingStatic"))
        {
            StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(result.target);
            GameObjectUtility.SetStaticEditorFlags(result.target, currentFlags | StaticEditorFlags.BatchingStatic);
            return true;
        }
#endif
        return false;
    }
    #endregion

    #region 快速修复操作
    /// <summary>
    /// 快速修复所有问题
    /// </summary>
    [FoldoutGroup("验证操作/快速操作", expanded: false)]
    [Button("快速修复所有问题", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void QuickFixAll()
    {
        bool originalAutoFix = autoFix;
        autoFix = true;
        ValidateMapData();
        autoFix = originalAutoFix;
    }

    /// <summary>
    /// 重置验证状态
    /// </summary>
    [FoldoutGroup("验证操作/快速操作")]
    [Button("重置验证状态", ButtonSizes.Small)]
    [GUIColor(1f, 1f, 0.7f)]
    public void ResetValidationStatus()
    {
        currentStatus = ValidationStatus.NotValidated;
        errorCount = 0;
        warningCount = 0;
        fixedCount = 0;
        validationReport = "";
        
        Debug.Log("[MapDataValidator] 验证状态已重置");
    }
    #endregion

    #region 编辑器辅助
#if UNITY_EDITOR
    /// <summary>
    /// 添加地图数据验证器到选中对象
    /// </summary>
    [MenuItem("GameObject/地图系统/添加地图数据验证器", false, 11)]
    public static void AddMapDataValidator()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            MapDataValidator validator = selectedObject.GetComponent<MapDataValidator>();
            if (validator == null)
            {
                selectedObject.AddComponent<MapDataValidator>();
                Debug.Log($"[MapDataValidator] 已添加地图数据验证器到 {selectedObject.name}");
            }
            else
            {
                Debug.LogWarning($"[MapDataValidator] {selectedObject.name} 已经包含地图数据验证器");
            }
        }
        else
        {
            Debug.LogWarning("[MapDataValidator] 请先选中一个游戏对象");
        }
    }
#endif
    #endregion
}