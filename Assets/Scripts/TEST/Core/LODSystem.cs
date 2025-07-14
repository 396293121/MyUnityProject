using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// LOD级别枚举
/// </summary>
public enum LODLevel
{
    High,    // 高精度 - 近距离
    Medium,  // 中等精度 - 中距离
    Low,     // 低精度 - 远距离
    Culled   // 剔除 - 超远距离
}

/// <summary>
/// LOD配置数据
/// </summary>
[System.Serializable]
public class LODConfig
{
    [LabelText("距离阈值")]
    [Tooltip("切换到此LOD级别的距离阈值")]
    public float distance;
    
    [LabelText("更新频率")]
    [Range(0.1f, 1f)]
    [Tooltip("AI更新频率（1为每帧更新，0.5为每两帧更新一次）")]
    public float updateFrequency = 1f;
    
    [LabelText("动画帧率")]
    [Range(5, 60)]
    [Tooltip("动画播放帧率")]
    public int animationFrameRate = 30;
    
    [LabelText("启用物理")]
    [Tooltip("是否启用物理计算")]
    public bool enablePhysics = true;
    
    [LabelText("启用碰撞检测")]
    [Tooltip("是否启用碰撞检测")]
    public bool enableCollision = true;
    
    [LabelText("启用音效")]
    [Tooltip("是否播放音效")]
    public bool enableAudio = true;
}

/// <summary>
/// LOD组件接口
/// </summary>
public interface ILODComponent
{
    void SetLODLevel(LODLevel level, LODConfig config);
    LODLevel GetCurrentLODLevel();
}

/// <summary>
/// LOD系统管理器
/// </summary>
public class LODSystem : MonoBehaviour
{
    #region 单例模式
    
    private static LODSystem _instance;
    public static LODSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LODSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("LODSystem");
                    _instance = go.AddComponent<LODSystem>();
                }
            }
            return _instance;
        }
    }
    
    #endregion
    
    #region 配置设置
    
    [TabGroup("配置", "LOD设置")]
    [BoxGroup("配置/LOD设置/基础配置")]
    [LabelText("启用LOD系统")]
    [Tooltip("是否启用LOD系统优化")]
    public bool enableLOD = true;
    
    [BoxGroup("配置/LOD设置/基础配置")]
    [LabelText("更新间隔")]
    [Range(0.1f, 2f)]
    [Tooltip("LOD系统更新间隔（秒）")]
    public float updateInterval = 0.2f;
    
    [BoxGroup("配置/LOD设置/基础配置")]
    [LabelText("玩家引用")]
    [Tooltip("玩家Transform引用，用于计算距离")]
    public Transform playerTransform;
    
    [TabGroup("配置", "LOD级别")]
    [BoxGroup("配置/LOD级别/高精度LOD")]
    [LabelText("高精度LOD")]
    [Tooltip("近距离高精度LOD配置")]
    public LODConfig highLOD = new LODConfig
    {
        distance = 10f,
        updateFrequency = 1f,
        animationFrameRate = 60,
        enablePhysics = true,
        enableCollision = true,
        enableAudio = true
    };
    
    [BoxGroup("配置/LOD级别/中等精度LOD")]
    [LabelText("中等精度LOD")]
    [Tooltip("中距离中等精度LOD配置")]
    public LODConfig mediumLOD = new LODConfig
    {
        distance = 25f,
        updateFrequency = 0.5f,
        animationFrameRate = 30,
        enablePhysics = true,
        enableCollision = true,
        enableAudio = false
    };
    
    [BoxGroup("配置/LOD级别/低精度LOD")]
    [LabelText("低精度LOD")]
    [Tooltip("远距离低精度LOD配置")]
    public LODConfig lowLOD = new LODConfig
    {
        distance = 50f,
        updateFrequency = 0.2f,
        animationFrameRate = 15,
        enablePhysics = false,
        enableCollision = false,
        enableAudio = false
    };
    
    [BoxGroup("配置/LOD级别/剔除LOD")]
    [LabelText("剔除LOD")]
    [Tooltip("超远距离剔除LOD配置")]
    public LODConfig culledLOD = new LODConfig
    {
        distance = float.MaxValue,
        updateFrequency = 0f,
        animationFrameRate = 5,
        enablePhysics = false,
        enableCollision = false,
        enableAudio = false
    };
    
    #endregion
    
    #region 运行时数据
    
    [TabGroup("运行状态", "注册对象")]
    [FoldoutGroup("运行状态/注册对象/LOD对象列表", expanded: true)]
    [LabelText("注册的LOD对象")]
    [ShowInInspector, ReadOnly]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
    private List<ILODComponent> registeredObjects = new List<ILODComponent>();
    
    [FoldoutGroup("运行状态/注册对象/统计信息", expanded: true)]
    [LabelText("高精度对象数量")]
    [ShowInInspector, ReadOnly]
    private int highLODCount = 0;
    
    [FoldoutGroup("运行状态/注册对象/统计信息")]
    [LabelText("中等精度对象数量")]
    [ShowInInspector, ReadOnly]
    private int mediumLODCount = 0;
    
    [FoldoutGroup("运行状态/注册对象/统计信息")]
    [LabelText("低精度对象数量")]
    [ShowInInspector, ReadOnly]
    private int lowLODCount = 0;
    
    [FoldoutGroup("运行状态/注册对象/统计信息")]
    [LabelText("剔除对象数量")]
    [ShowInInspector, ReadOnly]
    private int culledCount = 0;
    
    #endregion
    
    #region Unity生命周期
    
    private void Awake()
    {
        // 确保单例
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 查找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
    
    private void Start()
    {
        // 启动LOD更新协程
        if (enableLOD)
        {
            StartCoroutine(UpdateLODCoroutine());
        }
    }
    
    #endregion
    
    #region LOD管理
    
    /// <summary>
    /// 注册LOD对象
    /// </summary>
    /// <param name="lodComponent">要注册的LOD组件</param>
    public void RegisterLODObject(ILODComponent lodComponent)
    {
        if (lodComponent != null && !registeredObjects.Contains(lodComponent))
        {
            registeredObjects.Add(lodComponent);
            Debug.Log($"LOD对象已注册，当前总数: {registeredObjects.Count}");
        }
    }
    
    /// <summary>
    /// 取消注册LOD对象
    /// </summary>
    /// <param name="lodComponent">要取消注册的LOD组件</param>
    public void UnregisterLODObject(ILODComponent lodComponent)
    {
        if (lodComponent != null && registeredObjects.Contains(lodComponent))
        {
            registeredObjects.Remove(lodComponent);
            Debug.Log($"LOD对象已取消注册，当前总数: {registeredObjects.Count}");
        }
    }
    
    /// <summary>
    /// LOD更新协程
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator UpdateLODCoroutine()
    {
        while (enableLOD)
        {
            UpdateAllLODObjects();
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    /// <summary>
    /// 更新所有LOD对象
    /// </summary>
    private void UpdateAllLODObjects()
    {
        if (playerTransform == null)
        {
            return;
        }
        
        // 重置计数器
        highLODCount = mediumLODCount = lowLODCount = culledCount = 0;
        
        // 更新每个注册的LOD对象
        for (int i = registeredObjects.Count - 1; i >= 0; i--)
        {
            if (registeredObjects[i] == null)
            {
                // 移除已销毁的对象
                registeredObjects.RemoveAt(i);
                continue;
            }
            
            UpdateSingleLODObject(registeredObjects[i]);
        }
    }
    
    /// <summary>
    /// 更新单个LOD对象
    /// </summary>
    /// <param name="lodComponent">要更新的LOD组件</param>
    private void UpdateSingleLODObject(ILODComponent lodComponent)
    {
        // 获取对象的Transform组件
        MonoBehaviour mb = lodComponent as MonoBehaviour;
        if (mb == null) return;
        
        // 计算到玩家的距离
        float distance = Vector3.Distance(mb.transform.position, playerTransform.position);
        
        // 确定LOD级别
        LODLevel newLevel = DetermineLODLevel(distance);
        LODConfig config = GetLODConfig(newLevel);
        
        // 应用LOD设置
        lodComponent.SetLODLevel(newLevel, config);
        
        // 更新统计
        UpdateLODStatistics(newLevel);
    }
    
    /// <summary>
    /// 根据距离确定LOD级别
    /// </summary>
    /// <param name="distance">到玩家的距离</param>
    /// <returns>LOD级别</returns>
    private LODLevel DetermineLODLevel(float distance)
    {
        if (distance <= highLOD.distance)
        {
            return LODLevel.High;
        }
        else if (distance <= mediumLOD.distance)
        {
            return LODLevel.Medium;
        }
        else if (distance <= lowLOD.distance)
        {
            return LODLevel.Low;
        }
        else
        {
            return LODLevel.Culled;
        }
    }
    
    /// <summary>
    /// 获取LOD配置
    /// </summary>
    /// <param name="level">LOD级别</param>
    /// <returns>LOD配置</returns>
    private LODConfig GetLODConfig(LODLevel level)
    {
        switch (level)
        {
            case LODLevel.High:
                return highLOD;
            case LODLevel.Medium:
                return mediumLOD;
            case LODLevel.Low:
                return lowLOD;
            case LODLevel.Culled:
                return culledLOD;
            default:
                return highLOD;
        }
    }
    
    /// <summary>
    /// 更新LOD统计信息
    /// </summary>
    /// <param name="level">LOD级别</param>
    private void UpdateLODStatistics(LODLevel level)
    {
        switch (level)
        {
            case LODLevel.High:
                highLODCount++;
                break;
            case LODLevel.Medium:
                mediumLODCount++;
                break;
            case LODLevel.Low:
                lowLODCount++;
                break;
            case LODLevel.Culled:
                culledCount++;
                break;
        }
    }
    
    #endregion
    
    #region 调试功能
    
    [TabGroup("调试", "LOD调试")]
    [BoxGroup("调试/LOD调试/调试按钮")]
    [Button("强制更新所有LOD", ButtonSizes.Medium)]
    [Tooltip("立即更新所有注册的LOD对象")]
    private void ForceUpdateAllLOD()
    {
        UpdateAllLODObjects();
        Debug.Log("已强制更新所有LOD对象");
    }
    
    [BoxGroup("调试/LOD调试/调试按钮")]
    [Button("清理无效对象", ButtonSizes.Medium)]
    [Tooltip("清理已销毁的LOD对象引用")]
    private void CleanupInvalidObjects()
    {
        int originalCount = registeredObjects.Count;
        registeredObjects.RemoveAll(obj => obj == null);
        int removedCount = originalCount - registeredObjects.Count;
        Debug.Log($"已清理 {removedCount} 个无效LOD对象引用");
    }
    
    [BoxGroup("调试/LOD调试/调试信息")]
    [Button("打印LOD统计", ButtonSizes.Medium)]
    [Tooltip("在控制台打印当前LOD统计信息")]
    private void PrintLODStatistics()
    {
        Debug.Log($"LOD统计信息:\n" +
                 $"高精度: {highLODCount}\n" +
                 $"中等精度: {mediumLODCount}\n" +
                 $"低精度: {lowLODCount}\n" +
                 $"剔除: {culledCount}\n" +
                 $"总计: {registeredObjects.Count}");
    }
    
    /// <summary>
    /// 在Scene视图中绘制LOD范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        
        Vector3 playerPos = playerTransform.position;
        
        // 绘制高精度范围
        Gizmos.color = Color.green;
     //   Gizmos.DrawWireCircle(playerPos, highLOD.distance);
        
        // 绘制中等精度范围
        Gizmos.color = Color.yellow;
       // Gizmos.DrawWireCircle(playerPos, mediumLOD.distance);
        
        // 绘制低精度范围
        Gizmos.color = Color.red;
       // Gizmos.DrawWireCircle(playerPos, lowLOD.distance);
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 设置LOD系统启用状态
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetLODEnabled(bool enabled)
    {
        enableLOD = enabled;
        
        if (enabled)
        {
            StartCoroutine(UpdateLODCoroutine());
        }
        else
        {
            StopAllCoroutines();
            // 将所有对象设置为高精度
            foreach (var obj in registeredObjects)
            {
                if (obj != null)
                {
                    obj.SetLODLevel(LODLevel.High, highLOD);
                }
            }
        }
    }
    
    /// <summary>
    /// 获取当前注册对象数量
    /// </summary>
    /// <returns>注册对象数量</returns>
    public int GetRegisteredObjectCount()
    {
        return registeredObjects.Count;
    }
    
    /// <summary>
    /// 获取指定LOD级别的对象数量
    /// </summary>
    /// <param name="level">LOD级别</param>
    /// <returns>对象数量</returns>
    public int GetLODLevelCount(LODLevel level)
    {
        switch (level)
        {
            case LODLevel.High:
                return highLODCount;
            case LODLevel.Medium:
                return mediumLODCount;
            case LODLevel.Low:
                return lowLODCount;
            case LODLevel.Culled:
                return culledCount;
            default:
                return 0;
        }
    }
    
    #endregion
}