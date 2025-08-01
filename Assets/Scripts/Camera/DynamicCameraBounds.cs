using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

/// <summary>
/// 动态相机边界管理器 - 自动检测地图边界并设置相机边界
/// 支持Tilemap、背景图片等多种边界检测方式
/// </summary>
public class DynamicCameraBounds : MonoBehaviour
{
    #region 边界检测设置

    [FoldoutGroup("边界检测设置")]
    [LabelText("使用Tilemap边界")]
    [InfoBox("勾选后，会检测Tilemap的边界来确定地图范围")]
    public bool useTilemapBounds = true;

    #endregion

    #region 边界偏移设置
    [FoldoutGroup("边界偏移设置", expanded: true)]
    [LabelText("左边界偏移")]
    [InfoBox("向左扩展边界的距离（单位：Unity单位）")]
    [Range(0f, 10f)]
    public float leftOffset = 0f;

    [FoldoutGroup("边界偏移设置")]
    [LabelText("右边界偏移")]
    [InfoBox("向右扩展边界的距离（单位：Unity单位）")]
    [Range(0f, 10f)]
    public float rightOffset = 0f;

    [FoldoutGroup("边界偏移设置")]
    [LabelText("上边界偏移")]
    [InfoBox("向上扩展边界的距离（单位：Unity单位）")]
    [Range(0f, 10f)]
    public float topOffset = 0f;

    [FoldoutGroup("边界偏移设置")]
    [LabelText("下边界偏移")]
    [InfoBox("向下扩展边界的距离（单位：Unity单位）")]
    [Range(0f, 10f)]
    public float bottomOffset = 0f;
    #endregion

    #region 相机设置
    [FoldoutGroup("相机设置", expanded: true)]
    [LabelText("目标相机")]
    [InfoBox("要设置边界的相机，如果为空则自动使用主相机")]
    [Required("必须指定目标相机")]
    public Camera targetCamera;

    [FoldoutGroup("相机设置")]
    [LabelText("相机跟随脚本")]
    [InfoBox("CameraFollow组件的引用，用于设置相机边界")]
    [Required("必须指定CameraFollow组件")]
    public CameraFollow cameraFollow;
    #endregion

    #region 检测对象设置
    [FoldoutGroup("检测对象设置", expanded: true)]
    [LabelText("Tilemap数组")]
    [InfoBox("要检测边界的Tilemap对象数组")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    public Tilemap[] tilemaps;
    #endregion

    #region 调试设置
    [FoldoutGroup("调试设置", expanded: false)]
    [LabelText("显示调试信息")]
    [InfoBox("勾选后，会在控制台输出详细的边界计算信息")]
    public bool showDebugInfo = true;
    #endregion

    #region 运行时数据
    [FoldoutGroup("运行时信息", expanded: false)]
    [LabelText("计算出的边界")]
    [InfoBox("当前计算出的边界信息")]
    [ShowInInspector, ReadOnly]
    private Bounds calculatedBounds;

    [FoldoutGroup("运行时信息")]
    [LabelText("边界是否已计算")]
    [ShowInInspector, ReadOnly]
    private bool boundsCalculated = false;
    #endregion

    #region Unity生命周期
    private void Start()
    {
        // 自动获取组件
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();


    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 检测并设置边界
    /// </summary>
    [FoldoutGroup("操作按钮", expanded: true)]
    [Button("检测并设置边界", ButtonSizes.Large)]
    [InfoBox("点击此按钮手动触发边界检测和设置")]
    public void DetectAndSetBounds(Tilemap[] tilemaps)
    {
        Debug.Log(tilemaps + "地图12345");
        this.tilemaps = tilemaps;
        CalculateBounds();
        ApplyBoundsToCamera();
    }



    /// <summary>
    /// 获取计算出的边界
    /// </summary>
    /// <returns>计算出的边界</returns>
    public Bounds GetCalculatedBounds()
    {
        return calculatedBounds;
    }

    /// <summary>
    /// 强制重新计算边界
    /// </summary>
    [FoldoutGroup("操作按钮")]
    [Button("重新计算边界", ButtonSizes.Medium)]
    [InfoBox("强制重新计算并应用边界设置")]
    public void RecalculateBounds()
    {
        DetectAndSetBounds(tilemaps);
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 计算地图边界
    /// </summary>
    private void CalculateBounds()
    {
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;
        bool foundAnyBounds = false;

        // 1. 检测Tilemap边界
        if (useTilemapBounds && tilemaps != null)
        {
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap != null && tilemap.cellBounds.size.x > 0)
                {
                    Bounds tilemapBounds = GetTilemapWorldBounds(tilemap);
                    UpdateMinMaxBounds(ref minBounds, ref maxBounds, tilemapBounds);
                    foundAnyBounds = true;
                }
            }

        }

        // 4. 如果没有找到任何边界，使用默认值
        if (!foundAnyBounds)
        {
            minBounds = new Vector3(-20f, -5f, 0f);
            maxBounds = new Vector3(20f, 10f, 0f);
            Debug.LogWarning("未检测到任何边界，使用默认边界值");
        }
        // 5. 应用偏移
        minBounds.x -= leftOffset;
        minBounds.y -= bottomOffset;
        maxBounds.x += rightOffset;
        maxBounds.y += topOffset;

        // 6. 创建最终边界
        Vector3 center = (minBounds + maxBounds) * 0.5f;
        Vector3 size = maxBounds - minBounds;
        calculatedBounds = new Bounds(center, size);
        boundsCalculated = true;
        if (showDebugInfo)
        {
            Debug.Log($"最终计算边界: 最小值={minBounds}, 最大值={maxBounds}");
            Debug.Log($"边界中心: {center}, 尺寸: {size}");
        }
    }

    /// <summary>
    /// 获取Tilemap的世界坐标边界
    /// </summary>
    /// <param name="tilemap">要计算边界的Tilemap</param>
    /// <returns>世界坐标系下的边界</returns>
    private Bounds GetTilemapWorldBounds(Tilemap tilemap)
    {
        //删除历史边界
        tilemap.CompressBounds();
        // 获取包含所有有效tile的单元格边界
        BoundsInt cellBounds = tilemap.cellBounds;

        // 转换为世界坐标时需要考虑tile锚点和单元格尺寸
        Vector3 cellSize = tilemap.cellSize;
        Vector3 origin = tilemap.CellToWorld(new Vector3Int(cellBounds.x, cellBounds.y, 0));

        // 计算实际世界坐标边界
        Vector3 min = origin + new Vector3(0, 0, 0);
        Vector3 max = origin + new Vector3(
            cellBounds.size.x * cellSize.x,
            cellBounds.size.y * cellSize.y,
            0
        );
    
        // 创建包含整个tilemap的边界
        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);


        return bounds;
    }

    /// <summary>
    /// 更新最小最大边界
    /// </summary>
    /// <param name="minBounds">当前最小边界（引用传递）</param>
    /// <param name="maxBounds">当前最大边界（引用传递）</param>
    /// <param name="bounds">要比较的新边界</param>
    private void UpdateMinMaxBounds(ref Vector3 minBounds, ref Vector3 maxBounds, Bounds bounds)
    {

        minBounds = Vector3.Min(minBounds, bounds.min);
        maxBounds = Vector3.Max(maxBounds, bounds.max);
    }

    /// <summary>
    /// 将计算出的边界应用到相机
    /// </summary>
    private void ApplyBoundsToCamera()
    {
        if (!boundsCalculated)
        {
            Debug.LogError("边界尚未计算！");
            return;
        }

        if (cameraFollow != null)
        {
            // 考虑相机的视野大小
            float cameraHeight = targetCamera.orthographicSize;
            float cameraWidth = cameraHeight * targetCamera.aspect;

            // 调整边界以考虑相机视野
            float minX = calculatedBounds.min.x + cameraWidth;
            float maxX = calculatedBounds.max.x - cameraWidth;
            float minY = calculatedBounds.min.y + cameraHeight;
            float maxY = calculatedBounds.max.y - cameraHeight;

            // 确保边界有效
            if (minX >= maxX)
            {
                float center = (calculatedBounds.min.x + calculatedBounds.max.x) * 0.5f;
                minX = center - 1f;
                maxX = center + 1f;
            }

            if (minY >= maxY)
            {
                float center = (calculatedBounds.min.y + calculatedBounds.max.y) * 0.5f;
                minY = center - 1f;
                maxY = center + 1f;
            }

            // 设置相机边界
            cameraFollow.SetBounds(minX, maxX, minY, maxY);

            if (showDebugInfo)
            {
                Debug.Log($"应用相机边界: MinX={minX:F2}, MaxX={maxX:F2}, MinY={minY:F2}, MaxY={maxY:F2}");
                Debug.Log($"相机尺寸: 宽度={cameraWidth:F2}, 高度={cameraHeight:F2}");
            }
        }
        else
        {
            Debug.LogError("未找到CameraFollow组件！");
        }
    }
    #endregion

    #region 调试绘制
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!boundsCalculated) return;
        // 绘制检测到的Tilemap边界
        if (useTilemapBounds && tilemaps != null)
        {
           Gizmos.color = Color.green;
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap != null)
                {
                    Bounds tilemapBounds = GetTilemapWorldBounds(tilemap);
                    Gizmos.DrawWireCube(tilemapBounds.center, tilemapBounds.size);
                }
            }
        }
        #endregion
    }
}