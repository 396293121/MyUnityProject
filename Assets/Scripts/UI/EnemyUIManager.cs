using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 敌人UI管理器 - 统一管理所有敌人UI的显示和更新
/// 解决敌人UI定位问题，提供对象池管理和性能优化
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class EnemyUIManager : MonoBehaviour
{
    public static EnemyUIManager Instance { get; private set; }

    [TitleGroup("UI配置", "敌人UI系统配置", TitleAlignments.Centered)]
    [FoldoutGroup("UI配置/Canvas设置", expanded: true)]
    [LabelText("World Space Canvas")]
    [Required]
    [InfoBox("用于显示敌人UI的世界空间Canvas")]
    public Canvas worldSpaceCanvas;

    [FoldoutGroup("UI配置/Canvas设置")]
    [LabelText("UI摄像机")]
    [InfoBox("专用于UI渲染的摄像机，如果为空则使用主摄像机")]
    public Camera uiCamera;

    [FoldoutGroup("UI配置/预制体设置", expanded: true)]
    [LabelText("敌人UI预制体")]
    [Required]
    [AssetsOnly]
    [InfoBox("敌人UI的预制体模板")]
    public GameObject enemyUIPrefab;

    [FoldoutGroup("UI配置/性能优化", expanded: false)]
    [LabelText("对象池初始大小")]
    [Range(5, 50)]
    [InfoBox("UI对象池的初始容量")]
    public int poolInitialSize = 10;

    [FoldoutGroup("UI配置/性能优化")]
    [LabelText("UI更新间隔")]
    [Range(0.01f, 0.2f)]
    [InfoBox("UI位置更新的时间间隔（秒）")]
    public float updateInterval = 0.05f;

    [FoldoutGroup("UI配置/性能优化")]
    [LabelText("屏幕边界扩展")]
    [Range(1f, 10f)]
    [InfoBox("屏幕边界的扩展范围，用于提前隐藏UI")]
    public float screenBoundaryExtension = 2f;

    [FoldoutGroup("UI配置/位置设置", expanded: false)]
    [LabelText("UI垂直偏移")]
    [Range(0f, 5f)]
    [InfoBox("UI相对于敌人位置的垂直偏移")]
    public float uiVerticalOffset = 2f;

    [TitleGroup("运行时状态", "实时监控信息", TitleAlignments.Centered)]
    [FoldoutGroup("运行时状态/对象池状态", expanded: false)]
    [LabelText("活跃UI数量")]
    [ReadOnly]
    [ShowInInspector]
    private int activeUICount = 0;

    [FoldoutGroup("运行时状态/对象池状态")]
    [LabelText("池中可用UI数量")]
    [ReadOnly]
    [ShowInInspector]
    private int availableUICount = 0;

    // 私有字段
    private Queue<GameObject> uiPool = new Queue<GameObject>();
    private Dictionary<Enemy, GameObject> activeEnemyUIs = new Dictionary<Enemy, GameObject>();
    private float lastUpdateTime = 0f;
    private Camera mainCamera;

    #region 单例和初始化

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void InitializeManager()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        // 如果没有指定UI摄像机，使用主摄像机
        if (uiCamera == null)
        {
            uiCamera = mainCamera;
        }

        // 设置World Space Canvas
        SetupWorldSpaceCanvas();

        // 初始化对象池
        InitializeUIPool();

        Debug.Log($"[EnemyUIManager] 初始化完成 - 对象池大小: {poolInitialSize}");
    }

    /// <summary>
    /// 设置World Space Canvas
    /// </summary>
    private void SetupWorldSpaceCanvas()
    {
        if (worldSpaceCanvas == null)
        {
            // 创建World Space Canvas
            GameObject canvasGO = new GameObject("EnemyUI_WorldCanvas");
            canvasGO.transform.SetParent(transform);
            
            worldSpaceCanvas = canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // 配置Canvas设置
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        worldSpaceCanvas.worldCamera = uiCamera;
        worldSpaceCanvas.sortingOrder = 100; // 确保在其他UI之上

        // 设置Canvas大小和位置
        RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100);
        canvasRect.localScale = Vector3.one * 0.01f; // 缩小以适应世界空间
    }

    /// <summary>
    /// 初始化UI对象池
    /// </summary>
    private void InitializeUIPool()
    {
        if (enemyUIPrefab == null)
        {
            Debug.LogError("[EnemyUIManager] 敌人UI预制体未设置！");
            return;
        }

        for (int i = 0; i < poolInitialSize; i++)
        {
            GameObject uiInstance = CreateUIInstance();
            uiInstance.SetActive(false);
            uiPool.Enqueue(uiInstance);
        }

        availableUICount = poolInitialSize;
    }

    /// <summary>
    /// 创建UI实例
    /// </summary>
    private GameObject CreateUIInstance()
    {
        GameObject uiInstance = Instantiate(enemyUIPrefab, worldSpaceCanvas.transform);
        
        // 确保UI实例有正确的组件
        if (uiInstance.GetComponent<EnemyUI>() == null)
        {
            Debug.LogWarning("[EnemyUIManager] UI预制体缺少EnemyUI组件！");
        }

        return uiInstance;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 为敌人创建UI
    /// </summary>
    public void CreateEnemyUI(Enemy enemy)
    {
        if (enemy == null || activeEnemyUIs.ContainsKey(enemy))
            return;

        GameObject uiInstance = GetUIFromPool();
        if (uiInstance == null)
            return;
  // 配置UI父对象为敌人transform，而非Canvas
        uiInstance.transform.SetParent(enemy.transform);
        // 设置局部位置偏移（垂直偏移）
        uiInstance.transform.localPosition = new Vector3(0, uiVerticalOffset, 0);
        uiInstance.transform.localScale = Vector3.one; // 确保不继承敌人缩放

        // 配置UI
        EnemyUI enemyUI = uiInstance.GetComponent<EnemyUI>();
        if (enemyUI != null)
        {
            enemyUI.enemyName.text = enemy.EnemyId ?? "Unknown Enemy";
             enemyUI.uiCamera = uiCamera; // 传递UI摄像机引用

        }

        // 设置初始位置
        UpdateUIPosition(enemy, uiInstance);

        // 激活UI
        uiInstance.SetActive(true);
        activeEnemyUIs[enemy] = uiInstance;
        enemy.OnDeath += OnEnemyDeath;
        activeUICount++;
        availableUICount--;
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        RemoveEnemyUI(enemy);
        enemy.OnDeath -= OnEnemyDeath;
    }


    /// <summary>
    /// 更新UI旋转（独立方法）
    /// </summary>
    private void UpdateUIRotation(GameObject uiInstance)
    {
        if (uiCamera != null && uiInstance != null)
        {
            // 保持UI始终面向摄像机
            uiInstance.transform.LookAt(uiCamera.transform);
            uiInstance.transform.Rotate(0, 180, 0); // 翻转以正确显示
        }
    }
    /// <summary>
    /// 移除敌人UI
    /// </summary>
    public void RemoveEnemyUI(Enemy enemy)
    {
        if (enemy == null || !activeEnemyUIs.ContainsKey(enemy))
            return;

        GameObject uiInstance = activeEnemyUIs[enemy];
        activeEnemyUIs.Remove(enemy);

        // 返回到对象池
        ReturnUIToPool(uiInstance);

        activeUICount--;
        availableUICount++;
    }


    #endregion

    #region 私有方法

    /// <summary>
    /// 从对象池获取UI
    /// </summary>
    private GameObject GetUIFromPool()
    {
        if (uiPool.Count > 0)
        {
            return uiPool.Dequeue();
        }

        // 池中没有可用对象，创建新的
        return CreateUIInstance();
    }

    /// <summary>
    /// 将UI返回到对象池
    /// </summary>
    private void ReturnUIToPool(GameObject uiInstance)
    {
        if (uiInstance != null)
        {
            uiInstance.SetActive(false);
            uiPool.Enqueue(uiInstance);
        }
    }

    /// <summary>
    /// 更新UI位置
    /// </summary>
    private void UpdateUIPosition(Enemy enemy, GameObject uiInstance)
    {
        if (enemy == null || uiInstance == null)
            return;


        // 确保UI始终面向摄像机
        if (uiCamera != null)
        {
            uiInstance.transform.LookAt(uiCamera.transform);
            uiInstance.transform.Rotate(0, 180, 0); // 翻转以正确显示
        }
    }

    /// <summary>
    /// 更新UI可见性
    /// </summary>
    private void UpdateUIVisibility(Enemy enemy, GameObject uiInstance)
    {
        if (mainCamera == null || enemy == null || uiInstance == null)
            return;

        // 检查敌人是否在屏幕范围内
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(enemy.transform.position);
        
        bool isVisible = screenPoint.x >= -screenBoundaryExtension && 
                        screenPoint.x <= 1 + screenBoundaryExtension &&
                        screenPoint.y >= -screenBoundaryExtension && 
                        screenPoint.y <= 1 + screenBoundaryExtension &&
                        screenPoint.z > 0;

        uiInstance.SetActive(isVisible);
    }

    #endregion

    #region Unity生命周期


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region 调试方法

    [Button("清理所有UI")]
    [FoldoutGroup("运行时状态/调试工具")]
    private void ClearAllUIs()
    {
        var enemies = new List<Enemy>(activeEnemyUIs.Keys);
        foreach (Enemy enemy in enemies)
        {
            RemoveEnemyUI(enemy);
        }
    }

    [Button("重新初始化对象池")]
    [FoldoutGroup("运行时状态/调试工具")]
    private void ReinitializePool()
    {
        ClearAllUIs();
        
        // 清空现有池
        while (uiPool.Count > 0)
        {
            GameObject ui = uiPool.Dequeue();
            if (ui != null)
                DestroyImmediate(ui);
        }

        // 重新初始化
        InitializeUIPool();
    }

    #endregion
}