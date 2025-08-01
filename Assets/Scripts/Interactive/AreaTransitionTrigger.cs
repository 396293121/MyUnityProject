using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 区域切换触发器 - 处理玩家在不同区域间的切换
/// 可以放置在场景中作为切换点，或者动态创建
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AreaTransitionTrigger : MonoBehaviour, IInputListener
{
    #region 配置
    [TitleGroup("切换配置")]
    [FoldoutGroup("切换配置/目标区域", expanded: true)]
    [LabelText("目标主区域ID")]
    [Required("必须指定目标主区域ID")]
    [ValueDropdown("GetAvailableMainAreaIds")]
    [OnValueChanged("OnMainAreaIdChanged")]
    public string targetMainAreaId;

    [FoldoutGroup("切换配置/目标区域")]
    [LabelText("目标分区域ID")]
    [Required("必须指定目标分区域ID")]
    [ValueDropdown("GetAvailableSubAreaIds")]
    [OnValueChanged("OnSubAreaIdChanged")]
    public string targetSubAreaId;

    [FoldoutGroup("切换配置/目标区域")]
    [LabelText("目标场景区域ID")]
    [Required("必须指定目标场景区域ID")]
    [ValueDropdown("GetAvailableSceneAreaIds")]
    public string targetSceneAreaId;

    [FoldoutGroup("切换配置/传送门选择", expanded: false)]
    [LabelText("目标传送门")]
    [ValueDropdown("GetAvailablePortals")]
    [InfoBox("选择目标场景中的传送门作为传送目标位置")]
    public string targetPortalName;
    [FoldoutGroup("切换配置/条件设置", expanded: false)]
    [LabelText("需要条件")]
    [InfoBox("可选：切换需要满足的条件，留空表示无条件")]
    [TextArea(2, 3)]
    public string requiredCondition;

    [FoldoutGroup("切换配置/条件设置")]
    [LabelText("条件不满足提示")]
    [ShowIf("@!string.IsNullOrEmpty(requiredCondition)")]
    public string conditionFailMessage = "条件不满足，无法进入该区域";
    /// <summary>
    /// 场景切换后设置玩家位置到传送门位置
    /// </summary>
    private void SetPlayerPositionAfterTransition(String targetAreaId)
    {

        // 通过SceneController设置玩家位置
        if (SceneController.Instance != null)
        {
            SceneController.Instance.SetPlayerToPortalPosition(targetPortalName);
        }
        else
        {
            Debug.LogWarning("[AreaTransitionTrigger] 未找到SceneController，无法设置玩家位置");
        }
    }
    public GameIdDictionary idDictionary
    {
        get
        {
            return GameIdDictionaryManager.GetIdDictionary();
        }
    }

    #region 视觉设置
    [TitleGroup("视觉设置")]
    [FoldoutGroup("视觉设置/显示设置", expanded: false)]
    [LabelText("显示提示UI")]
    [InfoBox("是否显示切换提示界面")]
    public bool showTransitionUI = true;

    [FoldoutGroup("视觉设置/显示设置")]
    [LabelText("提示文本")]
    [ShowIf("showTransitionUI")]
    public string transitionPrompt = "按 E 键进入";

    [FoldoutGroup("视觉设置/显示设置")]
    [LabelText("区域名称")]
    [ShowIf("showTransitionUI")]
    public string areaDisplayName;

    [FoldoutGroup("视觉设置/特效设置", expanded: false)]
    [LabelText("入口特效")]
    [AssetsOnly]
    public GameObject entranceEffect;

    [FoldoutGroup("视觉设置/特效设置")]
    [LabelText("传送特效")]
    [AssetsOnly]
    public GameObject teleportEffect;
    #endregion

    #region 音效设置
    [TitleGroup("音效设置")]
    [FoldoutGroup("音效设置/音效配置", expanded: false)]
    [LabelText("进入音效")]
    [AssetsOnly]
    public AudioClip enterSound;

    [FoldoutGroup("音效设置/音效配置")]
    [LabelText("传送音效")]
    [AssetsOnly]
    public AudioClip teleportSound;

    [FoldoutGroup("音效设置/音效配置")]
    [LabelText("条件不满足音效")]
    [AssetsOnly]
    public AudioClip conditionFailSound;
    #endregion

    #region 运行时状态
    [TitleGroup("运行时状态")]
    [FoldoutGroup("运行时状态/状态信息", expanded: false)]
    [LabelText("玩家在触发区域内")]
    [ReadOnly]
    [ShowInInspector]
    private bool playerInTrigger = false;

    [FoldoutGroup("运行时状态/状态信息")]
    [LabelText("正在切换")]
    [ReadOnly]
    [ShowInInspector]
    private bool isTransitioning = false;

    [FoldoutGroup("运行时状态/状态信息")]
    [LabelText("当前玩家")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject currentPlayer;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("碰撞体组件")]
    [ReadOnly]
    [ShowInInspector]
    private BoxCollider2D triggerCollider;

    [FoldoutGroup("运行时状态/组件引用")]
    [LabelText("音频源")]
    [ReadOnly]
    [ShowInInspector]
    private AudioSource audioSource;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
          InputManager.RegisterListener(this);
          MapManager.Instance.OnMapTransitionComplete += SetPlayerPositionAfterTransition;
        SetupTrigger();
    }
    void OnDestroy()
    {
        InputManager.UnregisterListener(this);
    }

    #endregion

    #region 初始化

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取或添加碰撞体
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        triggerCollider.isTrigger = true;

        // 获取或添加音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 设置触发器
    /// </summary>
    private void SetupTrigger()
    {
        // 设置默认区域名称
        if (string.IsNullOrEmpty(areaDisplayName))
        {
            areaDisplayName = $"{targetMainAreaId}.{targetSubAreaId}.{targetSceneAreaId}";
        }

        // 生成入口特效
        if (entranceEffect != null)
        {
            GameObject effect = Instantiate(entranceEffect, transform.position, transform.rotation);
            effect.transform.SetParent(transform);
        }

        Debug.Log($"[AreaTransitionTrigger] 切换触发器已设置: {gameObject.name} -> {areaDisplayName}");
    }
    #endregion

    #region 触发器事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;
            playerInTrigger = true;
            OnPlayerEnterTrigger();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            currentPlayer = null;
            OnPlayerExitTrigger();
        }
    }

    /// <summary>
    /// 玩家进入触发区域
    /// </summary>
    private void OnPlayerEnterTrigger()
    {
        Debug.Log($"[AreaTransitionTrigger] 玩家进入切换区域: {areaDisplayName}");

        // 播放进入音效
        if (enterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enterSound);
        }

        // 显示提示UI
        if (showTransitionUI)
        {
            ShowTransitionPrompt(true);
        }
    }

    /// <summary>
    /// 玩家离开触发区域
    /// </summary>
    private void OnPlayerExitTrigger()
    {
        Debug.Log($"[AreaTransitionTrigger] 玩家离开切换区域: {areaDisplayName}");

        // 隐藏提示UI
        if (showTransitionUI)
        {
            ShowTransitionPrompt(false);
        }
    }
    #endregion

    #region 切换逻辑
    /// <summary>
    /// 触发区域切换
    /// </summary>
    public void TriggerTransition()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[AreaTransitionTrigger] 正在切换中，请稍后再试");
            return;
        }

        // 检查切换条件
        if (!CheckTransitionCondition())
        {
            OnTransitionConditionFailed();
            return;
        }

        Debug.Log($"[AreaTransitionTrigger] 开始切换到区域: {areaDisplayName}");
        isTransitioning = true;

        // 播放传送音效
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // 播放传送特效
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, currentPlayer.transform.position, Quaternion.identity);
        }

        // 隐藏提示UI
        if (showTransitionUI)
        {
            ShowTransitionPrompt(false);
        }

        // 执行切换
        MapManager.Instance.TransitionToArea(targetMainAreaId, targetSubAreaId, targetSceneAreaId);
        Debug.Log($"[AreaTransitionTrigger] 目标传送门: {targetPortalName}");

        // 切换完成后重置状态
        StartCoroutine(ResetTransitionState());
    }

    /// <summary>
    /// 检查切换条件
    /// </summary>
    private bool CheckTransitionCondition()
    {
        if (string.IsNullOrEmpty(requiredCondition))
            return true;

        // 这里可以实现具体的条件检查逻辑
        // 例如：检查玩家等级、物品、任务状态等
        // 暂时返回true，实际项目中需要根据具体需求实现
        return true;
    }

    /// <summary>
    /// 切换条件不满足时的处理
    /// </summary>
    private void OnTransitionConditionFailed()
    {
        Debug.LogWarning($"[AreaTransitionTrigger] 切换条件不满足: {requiredCondition}");

        // 播放失败音效
        if (conditionFailSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(conditionFailSound);
        }

        // 显示失败提示
        ShowConditionFailMessage();
    }

    /// <summary>
    /// 重置切换状态
    /// </summary>
    private System.Collections.IEnumerator ResetTransitionState()
    {
        yield return new WaitForSeconds(1f);
        isTransitioning = false;
    }
    #endregion

    #region UI处理
    /// <summary>
    /// 显示切换提示
    /// </summary>
    private void ShowTransitionPrompt(bool show)
    {
        // 这里可以调用UI系统显示切换提示
        // 暂时使用Debug输出，实际项目中需要实现UI显示逻辑
        if (show)
        {
            Debug.Log($"[UI] 显示切换提示: {transitionPrompt} - {areaDisplayName}");
        }
        else
        {
            Debug.Log("[UI] 隐藏切换提示");
        }
    }

    /// <summary>
    /// 显示条件不满足消息
    /// </summary>
    private void ShowConditionFailMessage()
    {
        // 这里可以调用UI系统显示错误消息
        Debug.Log($"[UI] 显示错误消息: {conditionFailMessage}");
    }
    #endregion

    #region 调试和可视化
    [TitleGroup("调试工具")]
    [FoldoutGroup("调试工具/调试操作", expanded: false)]
    [Button("测试切换", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.9f, 1f)]
    public void TestTransition()
    {
        if (Application.isPlaying)
        {
            TriggerTransition();
        }
        else
        {
            Debug.Log($"[AreaTransitionTrigger] 测试切换: {areaDisplayName}");
            Debug.Log($"目标: {targetMainAreaId}.{targetSubAreaId}.{targetSceneAreaId}");
        }
    }

    [FoldoutGroup("调试工具/调试操作")]
    [Button("输出配置信息", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void LogConfigInfo()
    {
        Debug.Log($"[AreaTransitionTrigger] 配置信息:");
        Debug.Log($"  名称: {gameObject.name}");
        Debug.Log($"  显示名称: {areaDisplayName}");
        Debug.Log($"  目标区域: {targetMainAreaId}.{targetSubAreaId}.{targetSceneAreaId}");
        if ( !string.IsNullOrEmpty(targetPortalName))
        {
            Debug.Log($"  目标传送门: {targetPortalName}");
        }
        Debug.Log($"  需要条件: {(string.IsNullOrEmpty(requiredCondition) ? "无" : requiredCondition)}");
    }
    #endregion

    #region 下拉框数据获取方法
    /// <summary>
    /// 获取可用的主区域ID列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableMainAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();

        var gameIdDict = idDictionary;
        if (gameIdDict == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到GameIdDictionary", ""));
            return items;
        }

        var mainAreaIds = gameIdDict.GetAllMainAreaIds();
        foreach (var areaId in mainAreaIds)
        {
            var displayName = gameIdDict.GetMainAreaDisplayName(areaId);
            if (!string.IsNullOrEmpty(displayName))
            {
                items.Add(new ValueDropdownItem<string>($"{displayName} ({areaId})", areaId));
            }
            else
            {
                items.Add(new ValueDropdownItem<string>(areaId, areaId));
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("没有可用的主区域", ""));
        }

        return items;
    }

    /// <summary>
    /// 获取可用的传送门列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailablePortals()
    {
        var items = new List<ValueDropdownItem<string>>();

        if (string.IsNullOrEmpty(targetMainAreaId) || string.IsNullOrEmpty(targetSubAreaId) || string.IsNullOrEmpty(targetSceneAreaId))
        {
            items.Add(new ValueDropdownItem<string>("请先选择完整的目标区域", ""));
            return items;
        }

        // 获取MapSystemConfig
        var mapConfig = Resources.FindObjectsOfTypeAll<MapSystemConfig>().FirstOrDefault();
        if (mapConfig == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到MapSystemConfig", ""));
            return items;
        }

        // 查找目标场景区域
        SceneArea targetSceneArea = null;
        foreach (var mainArea in mapConfig.mainAreas)
        {
            if (mainArea.areaId == targetMainAreaId)
            {
                foreach (var subArea in mainArea.subAreas)
                {
                    if (subArea.areaId == targetSubAreaId)
                    {
                        targetSceneArea = subArea.sceneAreas.FirstOrDefault(sa => sa.areaId == targetSceneAreaId);
                        break;
                    }
                }
                break;
            }
        }

        if (targetSceneArea == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到目标场景区域", ""));
            return items;
        }

        // 获取该场景区域的传送门配置
        if (targetSceneArea.areaTransitionTriggers == null || targetSceneArea.areaTransitionTriggers.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("目标场景区域没有配置传送门", ""));
            return items;
        }

        foreach (var portal in targetSceneArea.areaTransitionTriggers)
        {
            if (!string.IsNullOrEmpty(portal.triggerName))
            {
                var displayText = portal.triggerName;
                if (!string.IsNullOrEmpty(portal.triggerName))
                {
                    displayText += $" ({portal.triggerName})";
                }
                items.Add(new ValueDropdownItem<string>(displayText, portal.triggerName));
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("目标场景区域的传送门配置无效", ""));
        }

        return items;
    }

    /// <summary>
    /// 获取可用的分区域ID列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableSubAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();

        if (string.IsNullOrEmpty(targetMainAreaId))
        {
            items.Add(new ValueDropdownItem<string>("请先选择主区域", ""));
            return items;
        }

        var gameIdDict = idDictionary;
        if (gameIdDict == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到GameIdDictionary", ""));
            return items;
        }

        var subAreaIds = gameIdDict.GetAllSubAreaIds(targetMainAreaId);
        if (subAreaIds == null || subAreaIds.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("主区域不存在或无分区域", ""));
            return items;
        }

        foreach (var areaId in subAreaIds)
        {
            var displayName = gameIdDict.GetSubAreaDisplayName(targetMainAreaId, areaId);
            if (!string.IsNullOrEmpty(displayName))
            {
                items.Add(new ValueDropdownItem<string>($"{displayName} ({areaId})", areaId));
            }
            else
            {
                items.Add(new ValueDropdownItem<string>(areaId, areaId));
            }
        }

        return items;
    }

    /// <summary>
    /// 获取可用的场景区域ID列表
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableSceneAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();

        if (string.IsNullOrEmpty(targetMainAreaId) || string.IsNullOrEmpty(targetSubAreaId))
        {
            items.Add(new ValueDropdownItem<string>("请先选择主区域和分区域", ""));
            return items;
        }

        var gameIdDict = idDictionary;
        if (gameIdDict == null)
        {
            items.Add(new ValueDropdownItem<string>("未找到GameIdDictionary", ""));
            return items;
        }

        var sceneAreaIds = gameIdDict.GetAllSceneAreaIds(targetMainAreaId, targetSubAreaId);
        if (sceneAreaIds == null || sceneAreaIds.Count == 0)
        {
            items.Add(new ValueDropdownItem<string>("分区域不存在或无场景区域", ""));
            return items;
        }

        foreach (var areaId in sceneAreaIds)
        {
            var displayName = gameIdDict.GetSceneAreaDisplayName(targetMainAreaId, targetSubAreaId, areaId);
            if (!string.IsNullOrEmpty(displayName))
            {
                items.Add(new ValueDropdownItem<string>($"{displayName} ({areaId})", areaId));
            }
            else
            {
                items.Add(new ValueDropdownItem<string>(areaId, areaId));
            }
        }

        return items;
    }

    /// <summary>


    /// <summary>
    /// 主区域ID改变时的回调
    /// </summary>
    private void OnMainAreaIdChanged()
    {
        // 清空下级选择
        targetSubAreaId = "";
        targetSceneAreaId = "";

        // 更新显示名称
        UpdateAreaDisplayName();
    }

    /// <summary>
    /// 分区域ID改变时的回调
    /// </summary>
    private void OnSubAreaIdChanged()
    {
        // 清空下级选择
        targetSceneAreaId = "";

        // 更新显示名称
        UpdateAreaDisplayName();
    }

    /// <summary>
    /// 更新区域显示名称
    /// </summary>
    private void UpdateAreaDisplayName()
    {
        if (string.IsNullOrEmpty(targetMainAreaId))
        {
            areaDisplayName = "未设置";
            return;
        }

        var gameIdDict = idDictionary;
        if (gameIdDict == null)
        {
            areaDisplayName = $"{targetMainAreaId}.{targetSubAreaId}.{targetSceneAreaId}";
            return;
        }

        string displayName = gameIdDict.GetMainAreaDisplayName(targetMainAreaId);
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = targetMainAreaId;
        }

        if (!string.IsNullOrEmpty(targetSubAreaId))
        {
            var subAreaDisplayName = gameIdDict.GetSubAreaDisplayName(targetMainAreaId, targetSubAreaId);
            if (!string.IsNullOrEmpty(subAreaDisplayName))
            {
                displayName += $" - {subAreaDisplayName}";
            }
            else
            {
                displayName += $" - {targetSubAreaId}";
            }

            if (!string.IsNullOrEmpty(targetSceneAreaId))
            {
                var sceneAreaDisplayName = gameIdDict.GetSceneAreaDisplayName(targetMainAreaId, targetSubAreaId, targetSceneAreaId);
                if (!string.IsNullOrEmpty(sceneAreaDisplayName))
                {
                    displayName += $" - {sceneAreaDisplayName}";
                }
                else
                {
                    displayName += $" - {targetSceneAreaId}";
                }
            }
        }

        areaDisplayName = displayName;
    }
    #endregion
        #region 输入监听实现
    // 只实现需要的攀爬输入
    public void OnClimbUpInput()
    {
        if (playerInTrigger)
        {
            TriggerTransition();
        }
    }
    // 其他输入方法显式实现为空
    void IInputListener.OnClimbDownInput() { }
    void IInputListener.OnMoveInput(Vector2 moveInput) { }          // 空实现
    void IInputListener.OnJumpInput() { }                          // 空实现
    void IInputListener.OnJumpReleased() { }                       // 空实现
    void IInputListener.OnAttackInput() { }                       // 空实现
    void IInputListener.OnSkillInput() { }                        // 空实现
    void IInputListener.OnInteractInput() { }                     // 空实现
    void IInputListener.OnPauseInput() { }                        // 空实现
    #endregion
    #endregion
}