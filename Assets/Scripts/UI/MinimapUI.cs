using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 小地图UI组件 - 显示玩家位置、敌人、物品等信息
/// 支持缩放、标记、区域显示等功能
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class MinimapUI : MonoBehaviour
{
    [TitleGroup("小地图UI", "显示游戏世界的小地图", TitleAlignments.Centered)]
    
    [TabGroup("UI元素")]
    [BoxGroup("UI元素/基础组件")]
    [LabelText("地图背景")]
    [Required]
    public Image mapBackground;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("地图遮罩")]
    [Required]
    public RawImage mapMask;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("玩家图标")]
    [Required]
    public Image playerIcon;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("边框图像")]
    public Image borderImage;
    
    [BoxGroup("UI元素/控制")]
    [LabelText("缩放按钮+")]
    public Button zoomInButton;
    
    [BoxGroup("UI元素/控制")]
    [LabelText("缩放按钮-")]
    public Button zoomOutButton;
    
    [BoxGroup("UI元素/控制")]
    [LabelText("缩放文本")]
    public TextMeshProUGUI zoomText;
    
    [BoxGroup("UI元素/标记")]
    [LabelText("敌人标记预制体")]
    public GameObject enemyMarkerPrefab;
    
    [BoxGroup("UI元素/标记")]
    [LabelText("物品标记预制体")]
    public GameObject itemMarkerPrefab;
    
    [BoxGroup("UI元素/标记")]
    [LabelText("NPC标记预制体")]
    public GameObject npcMarkerPrefab;
    
    [BoxGroup("UI元素/标记")]
    [LabelText("标记容器")]
    public Transform markerContainer;
    
    [TabGroup("配置")]
    [BoxGroup("配置/地图设置")]
    [LabelText("地图尺寸")]
    public Vector2 mapSize = new Vector2(200f, 200f);
    
    [BoxGroup("配置/地图设置")]
    [LabelText("世界尺寸")]
    public Vector2 worldSize = new Vector2(1000f, 1000f);
    
    [BoxGroup("配置/缩放设置")]
    [LabelText("最小缩放")]
    [PropertyRange(0.5f, 1f)]
    public float minZoom = 0.5f;
    
    [BoxGroup("配置/缩放设置")]
    [LabelText("最大缩放")]
    [PropertyRange(1f, 3f)]
    public float maxZoom = 2f;
    
    [BoxGroup("配置/缩放设置")]
    [LabelText("缩放步长")]
    [PropertyRange(0.1f, 0.5f)]
    public float zoomStep = 0.25f;
    
    [BoxGroup("配置/更新设置")]
    [LabelText("更新间隔")]
    [PropertyRange(0.1f, 1f)]
    public float updateInterval = 0.2f;
    
    [TabGroup("状态")]
    [FoldoutGroup("状态/运行时数据", expanded: true)]
    [LabelText("当前缩放")]
    [ReadOnly]
    [ShowInInspector]
    private float currentZoom = 1f;
    
    [FoldoutGroup("状态/运行时数据")]
    [LabelText("玩家控制器")]
    [ReadOnly]
    [ShowInInspector]
    private PlayerController playerController;
    
    [FoldoutGroup("状态/运行时数据")]
    [LabelText("地图中心")]
    [ReadOnly]
    [ShowInInspector]
    private Vector2 mapCenter;
    
    [FoldoutGroup("状态/运行时数据")]
    [LabelText("标记列表")]
    [ReadOnly]
    [ShowInInspector]
    private System.Collections.Generic.List<MinimapMarker> markers = new System.Collections.Generic.List<MinimapMarker>();
    
    private RectTransform rectTransform;
    private Camera minimapCamera;
    private float lastUpdateTime;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // 设置按钮事件
        if (zoomInButton != null)
        {
            zoomInButton.onClick.AddListener(ZoomIn);
        }
        if (zoomOutButton != null)
        {
            zoomOutButton.onClick.AddListener(ZoomOut);
        }
        
        // 初始化地图中心
        mapCenter = worldSize * 0.5f;
    }
    
    private void Start()
    {
        // 查找玩家控制器
        playerController = FindObjectOfType<PlayerController>();
        
        // 创建小地图相机
        CreateMinimapCamera();
        
        // 初始化显示
        UpdateZoomDisplay();
    }
    
    private void Update()
    {
        // 定时更新
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMinimap();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 创建小地图相机
    /// </summary>
    private void CreateMinimapCamera()
    {
        GameObject cameraObj = new GameObject("MinimapCamera");
        minimapCamera = cameraObj.AddComponent<Camera>();
        
        // 设置相机参数
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = worldSize.y * 0.5f / currentZoom;
        minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.black;
        minimapCamera.depth = -10;
        
        // 设置渲染目标
        RenderTexture renderTexture = new RenderTexture((int)mapSize.x, (int)mapSize.y, 16);
        minimapCamera.targetTexture = renderTexture;
        
        if (mapMask != null)
        {
            mapMask.texture = renderTexture;
        }
        
        // 设置相机位置
        cameraObj.transform.position = new Vector3(mapCenter.x, 100f, mapCenter.y);
        cameraObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
    
    /// <summary>
    /// 更新小地图
    /// </summary>
    private void UpdateMinimap()
    {
        if (playerController == null) return;
        
        // 更新玩家位置
        UpdatePlayerPosition();
        
        // 更新相机位置
        UpdateCameraPosition();
        
        // 更新标记
        UpdateMarkers();
    }
    
    /// <summary>
    /// 更新玩家位置
    /// </summary>
    private void UpdatePlayerPosition()
    {
        if (playerIcon == null || playerController == null) return;
        
        Vector3 playerWorldPos = playerController.transform.position;
        Vector2 playerMapPos = WorldToMapPosition(new Vector2(playerWorldPos.x, playerWorldPos.z));
        
        // 设置玩家图标位置
        RectTransform playerRect = playerIcon.GetComponent<RectTransform>();
        if (playerRect != null)
        {
            playerRect.anchoredPosition = playerMapPos;
        }
        
        // 设置玩家图标旋转
        float playerRotation = playerController.transform.eulerAngles.y;
        playerIcon.transform.rotation = Quaternion.Euler(0f, 0f, -playerRotation);
    }
    
    /// <summary>
    /// 更新相机位置
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (minimapCamera == null || playerController == null) return;
        
        Vector3 playerPos = playerController.transform.position;
        Vector3 cameraPos = new Vector3(playerPos.x, 100f, playerPos.z);
        minimapCamera.transform.position = cameraPos;
        
        // 更新正交大小
        minimapCamera.orthographicSize = worldSize.y * 0.5f / currentZoom;
    }
    
    /// <summary>
    /// 更新标记
    /// </summary>
    private void UpdateMarkers()
    {
        // 清除过期标记
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            if (markers[i] == null || markers[i].target == null)
            {
                if (markers[i] != null && markers[i].markerObject != null)
                {
                    Destroy(markers[i].markerObject);
                }
                markers.RemoveAt(i);
            }
        }
        
        // 更新现有标记位置
        foreach (var marker in markers)
        {
            if (marker.target != null && marker.markerObject != null)
            {
                Vector3 targetWorldPos = marker.target.position;
                Vector2 targetMapPos = WorldToMapPosition(new Vector2(targetWorldPos.x, targetWorldPos.z));
                
                RectTransform markerRect = marker.markerObject.GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchoredPosition = targetMapPos;
                }
                
                // 检查是否在地图范围内
                bool inRange = IsInMapRange(targetMapPos);
                marker.markerObject.SetActive(inRange);
            }
        }
    }
    
    /// <summary>
    /// 世界坐标转地图坐标
    /// </summary>
    private Vector2 WorldToMapPosition(Vector2 worldPos)
    {
        Vector2 normalizedPos = new Vector2(
            (worldPos.x / worldSize.x) - 0.5f,
            (worldPos.y / worldSize.y) - 0.5f
        );
        
        return new Vector2(
            normalizedPos.x * mapSize.x * currentZoom,
            normalizedPos.y * mapSize.y * currentZoom
        );
    }
    
    /// <summary>
    /// 检查是否在地图范围内
    /// </summary>
    private bool IsInMapRange(Vector2 mapPos)
    {
        float halfWidth = mapSize.x * 0.5f;
        float halfHeight = mapSize.y * 0.5f;
        
        return mapPos.x >= -halfWidth && mapPos.x <= halfWidth &&
               mapPos.y >= -halfHeight && mapPos.y <= halfHeight;
    }
    
    /// <summary>
    /// 放大
    /// </summary>
    public void ZoomIn()
    {
        currentZoom = Mathf.Clamp(currentZoom + zoomStep, minZoom, maxZoom);
        UpdateZoomDisplay();
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ui_click", 0.5f);
        }
    }
    
    /// <summary>
    /// 缩小
    /// </summary>
    public void ZoomOut()
    {
        currentZoom = Mathf.Clamp(currentZoom - zoomStep, minZoom, maxZoom);
        UpdateZoomDisplay();
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ui_click", 0.5f);
        }
    }
    
    /// <summary>
    /// 更新缩放显示
    /// </summary>
    private void UpdateZoomDisplay()
    {
        if (zoomText != null)
        {
            zoomText.text = $"{(currentZoom * 100):F0}%";
        }
        
        // 更新按钮状态
        if (zoomInButton != null)
        {
            zoomInButton.interactable = currentZoom < maxZoom;
        }
        if (zoomOutButton != null)
        {
            zoomOutButton.interactable = currentZoom > minZoom;
        }
    }
    
    /// <summary>
    /// 添加标记
    /// </summary>
    public void AddMarker(Transform target, MinimapMarkerType type)
    {
        GameObject markerPrefab = null;
        
        switch (type)
        {
            case MinimapMarkerType.Enemy:
                markerPrefab = enemyMarkerPrefab;
                break;
            case MinimapMarkerType.Item:
                markerPrefab = itemMarkerPrefab;
                break;
            case MinimapMarkerType.NPC:
                markerPrefab = npcMarkerPrefab;
                break;
        }
        
        if (markerPrefab != null && markerContainer != null)
        {
            GameObject markerObj = Instantiate(markerPrefab, markerContainer);
            MinimapMarker marker = new MinimapMarker
            {
                target = target,
                markerObject = markerObj,
                type = type
            };
            
            markers.Add(marker);
            
            Debug.Log($"[MinimapUI] 添加标记: {type}");
        }
    }
    
    /// <summary>
    /// 移除标记
    /// </summary>
    public void RemoveMarker(Transform target)
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            if (markers[i].target == target)
            {
                if (markers[i].markerObject != null)
                {
                    Destroy(markers[i].markerObject);
                }
                markers.RemoveAt(i);
                break;
            }
        }
    }
    
    /// <summary>
    /// 清除所有标记
    /// </summary>
    public void ClearAllMarkers()
    {
        foreach (var marker in markers)
        {
            if (marker.markerObject != null)
            {
                Destroy(marker.markerObject);
            }
        }
        markers.Clear();
    }
}

/// <summary>
/// 小地图标记类型
/// </summary>
public enum MinimapMarkerType
{
    Enemy,      // 敌人
    Item,       // 物品
    NPC,        // NPC
    Objective   // 目标点
}

/// <summary>
/// 小地图标记数据
/// </summary>
[System.Serializable]
public class MinimapMarker
{
    public Transform target;
    public GameObject markerObject;
    public MinimapMarkerType type;
}