using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 地图管理器 - 地图系统的核心控制器
/// 负责管理主区域-分区域-场景粒度的层级结构
/// 支持流畅的场景切换和状态保持
/// </summary>
[System.Serializable]
public class MapManager : MonoBehaviour
{
    #region 单例模式
    public static MapManager Instance{ get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMapSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region 配置和状态
    [TitleGroup("地图系统配置")]
    [FoldoutGroup("地图系统配置/核心配置", expanded: true)]
    [LabelText("地图系统配置")]
    [Required("必须指定地图系统配置")]
    [AssetsOnly]
    [SerializeField] private MapSystemConfig mapSystemConfig;
    public MapSystemConfig MapSystemConfig=>mapSystemConfig;
    [FoldoutGroup("地图系统配置/当前状态", expanded: true)]
    [LabelText("当前主区域")]
    [ReadOnly]
    [ShowInInspector]
    private MainArea currentMainArea;

    [FoldoutGroup("地图系统配置/当前状态")]
    [LabelText("当前分区域")]
    [ReadOnly]
    [ShowInInspector]
    private SubArea currentSubArea;

    [FoldoutGroup("地图系统配置/当前状态")]
    [LabelText("当前场景区域")]
    [ReadOnly]
    [ShowInInspector]
    private SceneArea currentSceneArea;
    public SceneArea CurrentSceneArea=>currentSceneArea;
    [FoldoutGroup("地图系统配置/当前状态")]
    [LabelText("当前地图实例")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject currentMapInstance;
    
    /// <summary>
    /// 当前地图实例（公开属性）
    /// </summary>
    public GameObject CurrentMapInstance => currentMapInstance;

    [FoldoutGroup("地图系统配置/运行时状态", expanded: false)]
    [LabelText("地图系统已初始化")]
    [ReadOnly]
    [ShowInInspector]
    private bool isMapSystemInitialized = false;

    [FoldoutGroup("地图系统配置/运行时状态")]
    [LabelText("正在切换场景")]
    [ReadOnly]
    [ShowInInspector]
    private bool isTransitioning = false;
    
    /// <summary>
    /// 是否正在切换场景
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    [FoldoutGroup("地图系统配置/运行时状态")]
    [LabelText("已加载的地图缓存")]
    [ReadOnly]
    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "区域ID", ValueLabel = "地图实例")]
    private Dictionary<string, GameObject> loadedMaps = new Dictionary<string, GameObject>();
    #endregion

    #region 事件系统
    [TitleGroup("事件系统")]
    [FoldoutGroup("事件系统/地图事件", expanded: false)]
    [LabelText("地图切换开始事件")]
    [ShowInInspector]
    public System.Action<string, string> OnMapTransitionStart; // 从哪个区域，到哪个区域

    [FoldoutGroup("事件系统/地图事件")]
    [LabelText("地图切换完成事件")]
    [ShowInInspector]
    public System.Action<string> OnMapTransitionComplete; // 切换到的区域

    [FoldoutGroup("事件系统/地图事件")]
    [LabelText("地图加载事件")]
    [ShowInInspector]
    public System.Action<string> OnMapLoaded; // 加载的区域

    [FoldoutGroup("事件系统/地图事件")]
    [LabelText("地图卸载事件")]
    [ShowInInspector]
    public System.Action<string> OnMapUnloaded; // 卸载的区域
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化地图系统
    /// </summary>
    public void InitializeMapSystem()
    {
        if (isMapSystemInitialized) return;

        if (mapSystemConfig == null)
        {
            Debug.LogError("[MapManager] MapSystemConfig 为空，无法初始化地图系统");
            return;
        }
   // 新增默认区域加载校验
    if (mapSystemConfig.mainAreas.Count == 0)
    {
        Debug.LogError("[MapManager] MapSystemConfig 中未配置任何主区域");
        return;
    }
        Debug.Log($"[MapManager] 初始化地图系统: {mapSystemConfig.systemName}");
        
        // 初始化缓存
        loadedMaps = new Dictionary<string, GameObject>();
        
        // 加载默认初始区域
        LoadDefaultInitialArea();
        
        isMapSystemInitialized = true;
        Debug.Log("[MapManager] 地图系统初始化完成");
    }

    /// <summary>
    /// 加载默认初始区域
    /// </summary>
    private void LoadDefaultInitialArea()
    {
        // 获取默认角色类型的初始场景区域
        string defaultCharacterType = mapSystemConfig.defaultCharacterType;
        var initialSceneArea = mapSystemConfig.GetCharacterInitialSceneArea(defaultCharacterType);
        
        if (initialSceneArea != null)
        {
            // 查找该场景区域所属的主区域和分区域
            foreach (var mainArea in mapSystemConfig.mainAreas)
            {
                foreach (var subArea in mainArea.subAreas)
                {
                    if (subArea.sceneAreas.Contains(initialSceneArea))
                    {
                        Debug.Log($"[MapManager] 加载默认初始区域: {mainArea.areaId}.{subArea.areaId}.{initialSceneArea.areaId}");
                        StartCoroutine(LoadInitialAreaCoroutine(mainArea.areaId, subArea.areaId, initialSceneArea.areaId));
                        return;
                    }
                }
            }
        }
        
        Debug.LogWarning("[MapManager] 未找到有效的初始区域配置，将使用第一个可用区域");
        // 备选方案：加载第一个可用区域
        if (mapSystemConfig.mainAreas.Count > 0 && 
            mapSystemConfig.mainAreas[0].subAreas.Count > 0 && 
            mapSystemConfig.mainAreas[0].subAreas[0].sceneAreas.Count > 0)
        {
            var firstMain = mapSystemConfig.mainAreas[0];
            var firstSub = firstMain.subAreas[0];
            var firstScene = firstSub.sceneAreas[0];
            StartCoroutine(LoadInitialAreaCoroutine(firstMain.areaId, firstSub.areaId, firstScene.areaId));
        }
    }

    /// <summary>
    /// 加载初始区域协程
    /// </summary>
    private IEnumerator LoadInitialAreaCoroutine(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        yield return StartCoroutine(LoadNewArea(mainAreaId, subAreaId, sceneAreaId));
        
        string areaId = $"{mainAreaId}.{subAreaId}.{sceneAreaId}";
        Debug.Log($"[MapManager] 初始区域加载完成: {areaId}");
        OnMapTransitionComplete?.Invoke(areaId);
    }
    /// <summary>
    /// 设置地图系统配置
    /// </summary>
    public void SetMapSystemConfig(MapSystemConfig config)
    {
        mapSystemConfig = config;
        Debug.Log($"[MapManager] 地图系统配置已设置: {config.name}");
    }
    #endregion

    #region 地图切换核心方法
    /// <summary>
    /// 切换到指定区域
    /// </summary>
    /// <param name="mainAreaId">主区域ID</param>
    /// <param name="subAreaId">分区域ID</param>
    /// <param name="sceneAreaId">场景区域ID</param>
    /// <param name="transitionType">切换类型</param>
    public void TransitionToArea(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MapManager] 正在切换场景中，请稍后再试");
            return;
        }

        StartCoroutine(TransitionToAreaCoroutine(mainAreaId, subAreaId, sceneAreaId));
    }

    /// <summary>
    /// 切换区域协程
    /// </summary>
    private IEnumerator TransitionToAreaCoroutine(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        isTransitioning = true;
        string fromAreaId = GetCurrentAreaId();
        string toAreaId = $"{mainAreaId}.{subAreaId}.{sceneAreaId}";

        Debug.Log($"[MapManager] 开始切换场景: {fromAreaId} -> {toAreaId}");
        OnMapTransitionStart?.Invoke(fromAreaId, toAreaId);


        // 2. 播放切换效果
        yield return StartCoroutine(PlayTransitionEffect(true));
        MapStateManager.Instance.SaveCurrentState();
        // 3. 卸载当前地图
        if (currentMapInstance != null)
        {
            yield return StartCoroutine(UnloadCurrentMap());
        }

        // 4. 加载新地图
        yield return StartCoroutine(LoadNewArea(mainAreaId, subAreaId, sceneAreaId));
        MapStateManager.Instance.RestoreAreaState(mainAreaId);
        // 6. 结束切换效果
        yield return StartCoroutine(PlayTransitionEffect(false));

        isTransitioning = false;
        Debug.Log($"[MapManager] 场景切换完成: {toAreaId}");
        OnMapTransitionComplete?.Invoke(toAreaId);
    }

    /// <summary>
    /// 加载新区域
    /// </summary>
    private IEnumerator LoadNewArea(string mainAreaId, string subAreaId, string sceneAreaId)
    {
        // 查找区域配置
        MainArea mainArea = mapSystemConfig.mainAreas.Find(m => m.areaId == mainAreaId);
        if (mainArea == null)
        {
            Debug.LogError($"[MapManager] 未找到主区域: {mainAreaId}");
            yield break;
        }

        SubArea subArea = mainArea.subAreas.Find(s => s.areaId == subAreaId);
        if (subArea == null)
        {
            Debug.LogError($"[MapManager] 未找到分区域: {subAreaId}");
            yield break;
        }

        SceneArea sceneArea = subArea.sceneAreas.Find(s => s.areaId == sceneAreaId);
        if (sceneArea == null)
        {
            Debug.LogError($"[MapManager] 未找到场景区域: {sceneAreaId}");
            yield break;
        }

        // 更新当前区域引用
        currentMainArea = mainArea;
        currentSubArea = subArea;
        currentSceneArea = sceneArea;

        // 实例化地图预制体
        if (sceneArea.mapPrefab != null)
        {
            currentMapInstance = Instantiate(sceneArea.mapPrefab);
            currentMapInstance.name = $"Map_{mainAreaId}_{subAreaId}_{sceneAreaId}";
            
            // 设置地图位置
            currentMapInstance.transform.position = sceneArea.mapSpawnPosition;
            Debug.Log($"[MapManager] 地图加载完成: {sceneArea.areaName}");
            OnMapLoaded?.Invoke(GetCurrentAreaId());
        }

        yield return null;
    }

    /// <summary>
    /// 卸载当前地图
    /// </summary>
    private IEnumerator UnloadCurrentMap()
    {
        if (currentMapInstance != null)
        {
            string currentAreaId = GetCurrentAreaId();
            
            // 可选：将地图实例缓存起来而不是直接销毁
            if (mapSystemConfig.enableMapCaching && !loadedMaps.ContainsKey(currentAreaId))
            {
                currentMapInstance.SetActive(false);
                loadedMaps[currentAreaId] = currentMapInstance;
                Debug.Log($"[MapManager] 地图已缓存: {currentAreaId}");
            }
            else
            {
                Destroy(currentMapInstance);
                Debug.Log($"[MapManager] 地图已卸载: {currentAreaId}");
            }
            
            OnMapUnloaded?.Invoke(currentAreaId);
            currentMapInstance = null;
        }
        
        yield return null;
    }
    #endregion

    #region 状态管理
    #endregion

    #region 切换效果
    /// <summary>
    /// 播放切换效果
    /// </summary>
    private IEnumerator PlayTransitionEffect(bool isEntering)
    {       yield return StartCoroutine(PlayFadeEffect(isEntering));
    }

    /// <summary>
    /// 淡入淡出效果
    /// </summary>
    private IEnumerator PlayFadeEffect(bool isEntering)
    {
        // 这里可以调用UI系统的淡入淡出效果
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = isEntering ? elapsed / duration : 1f - (elapsed / duration);
            // 设置屏幕遮罩透明度
            yield return null;
        }
    }

    /// <summary>
    /// 滑动效果
    /// </summary>
    private IEnumerator PlaySlideEffect(bool isEntering)
    {
        // 实现滑动切换效果
        yield return new WaitForSeconds(0.3f);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 获取当前区域ID
    /// </summary>
    public string GetCurrentAreaId()
    {
        if (currentMainArea == null || currentSubArea == null || currentSceneArea == null)
            return "none";
        
        return $"{currentMainArea.areaId}.{currentSubArea.areaId}.{currentSceneArea.areaId}";
    }
    #endregion

    #region 调试和工具方法
    [TitleGroup("调试工具")]
    [FoldoutGroup("调试工具/调试操作", expanded: false)]
    [Button("输出当前地图信息", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void LogCurrentMapInfo()
    {
        Debug.Log($"[MapManager] 当前区域: {GetCurrentAreaId()}");
        Debug.Log($"[MapManager] 主区域: {currentMainArea?.areaName ?? "无"}");
        Debug.Log($"[MapManager] 分区域: {currentSubArea?.areaName ?? "无"}");
        Debug.Log($"[MapManager] 场景区域: {currentSceneArea?.areaName ?? "无"}");
        Debug.Log($"[MapManager] 地图实例: {(currentMapInstance != null ? currentMapInstance.name : "无")}");
        Debug.Log($"[MapManager] 缓存地图数量: {loadedMaps.Count}");
    }
    [FoldoutGroup("调试工具/调试操作")]
    [Button("清除所有缓存", ButtonSizes.Medium)]
    [GUIColor(1f, 0.7f, 0.7f)]
    public void ClearAllCache()
    {
        foreach (var map in loadedMaps.Values)
        {
            if (map != null) Destroy(map);
        }
        loadedMaps.Clear();
        Debug.Log("[MapManager] 所有缓存已清除");
    }
    #endregion
}