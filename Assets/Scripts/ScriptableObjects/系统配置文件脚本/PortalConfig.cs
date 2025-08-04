using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 传送门配置文件 - 持久化存储传送门数据
/// </summary>
[CreateAssetMenu(fileName = "PortalConfig", menuName = "Game/Portal Config")]
public class PortalConfig : ScriptableObject
{
    [TitleGroup("传送门基本信息")]
    [LabelText("传送门ID")]
    [Required("传送门ID不能为空")]
    public string portalId;
    
    [LabelText("显示名称")]
    [Required("显示名称不能为空")]
    public string displayName;
    
    [LabelText("描述")]
    [TextArea(2, 4)]
    public string description;
    
    [TitleGroup("传送门区域设置")]
    [FoldoutGroup("传送门区域设置/区域配置", expanded: true)]
    [LabelText("主区域ID")]
    [InfoBox("传送门所在的主区域")]
    [ValueDropdown("GetAvailableMainAreaIds")]
    [OnValueChanged("OnMainAreaChanged")]
    public string mainAreaId;
    
    [FoldoutGroup("传送门区域设置/区域配置")]
    [LabelText("分区域ID")]
    [InfoBox("传送门所在的分区域")]
    [ValueDropdown("GetAvailableSubAreaIds")]
    [OnValueChanged("OnSubAreaChanged")]
    public string subAreaId;
    
    [FoldoutGroup("传送门区域设置/区域配置")]
    [LabelText("场景区域ID")]
    [InfoBox("传送门所在的场景区域")]
    [ValueDropdown("GetAvailableSceneAreaIds")]
    public string sceneAreaId;
    
    [TitleGroup("传送门位置")]
    [FoldoutGroup("传送门位置/位置设置", expanded: true)]
    [LabelText("传送门对象")]
    [InfoBox("拖拽传送门GameObject到此处，将自动读取位置信息")]
    [OnValueChanged("OnPortalObjectChanged")]
    public GameObject portalObject;
    
    [FoldoutGroup("传送门位置/位置设置")]
    [LabelText("传送门位置")]
    [InfoBox("传送门在场景中的位置坐标，可通过拖拽传送门对象自动设置")]
    public Vector3 position;
    
    [FoldoutGroup("传送门位置/位置设置")]
    [Button("从传送门对象读取位置", ButtonSizes.Medium)]
    [ShowIf("@portalObject != null")]
    [GUIColor(0.7f, 0.9f, 1f)]
    private void ReadPositionFromObject()
    {
        if (portalObject != null)
        {
            position = portalObject.transform.position;
            Debug.Log($"[PortalConfig] 已从对象 {portalObject.name} 读取位置: {position}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
    
    [TitleGroup("传送门链接")]
    [LabelText("目标传送门ID")]
    [InfoBox("传送到的目标传送门ID，留空表示无链接")]
    [ValueDropdown("GetAvailableTargetPortals")]
    public string targetPortalId;
        private GameIdDictionary idDictionary
    {
        get
        {
            return GameIdDictionaryManager.GetIdDictionary();
        }
    }
    /// <summary>
    /// 当传送门对象改变时自动读取位置
    /// </summary>
    private void OnPortalObjectChanged()
    {
        if (portalObject != null)
        {
            ReadPositionFromObject();
        }
    }
    
    /// <summary>
    /// 主区域改变时的回调
    /// </summary>
    private void OnMainAreaChanged()
    {
        // 清空分区域和场景区域选择
        subAreaId = "";
        sceneAreaId = "";
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// 分区域改变时的回调
    /// </summary>
    private void OnSubAreaChanged()
    {
        // 清空场景区域选择
        sceneAreaId = "";
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    
    /// <summary>
    /// 获取可用的目标传送门列表（用于下拉框）
    /// </summary>
    /// <returns>传送门ID和显示名称的列表</returns>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableTargetPortals()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        // 添加空选项
        items.Add(new ValueDropdownItem<string>("无链接", ""));
        
#if UNITY_EDITOR
        // 在编辑器模式下获取所有传送门配置
        if (idDictionary != null)
        {
            var allPortalIds = idDictionary.GetAllPortalIds();
            foreach (var portalId in allPortalIds)
            {
                // 排除自己
                if (portalId != this.portalId)
                {
                    var displayName = idDictionary.GetPortalDisplayName(portalId);
                    var itemText = string.IsNullOrEmpty(displayName) ? portalId : $"{displayName} ({portalId})";
                    items.Add(new ValueDropdownItem<string>(itemText, portalId));
                }
            }
        }
        else
        {
            // 如果找不到 GameIdDictionary，尝试从资源中加载所有 PortalConfig
            var portalConfigs = Resources.FindObjectsOfTypeAll<PortalConfig>();
            foreach (var config in portalConfigs)
            {
                if (config != this && !string.IsNullOrEmpty(config.portalId))
                {
                    var itemText = string.IsNullOrEmpty(config.displayName) ? 
                        config.portalId : $"{config.displayName} ({config.portalId})";
                    items.Add(new ValueDropdownItem<string>(itemText, config.portalId));
                }
            }
        }
#endif
        
        return items.OrderBy(x => x.Text);
    }
    
    /// <summary>
    /// 获取可用的主区域ID列表
    /// </summary>
    /// <returns>主区域ID和显示名称的列表</returns>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableMainAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        // 添加空选项
        items.Add(new ValueDropdownItem<string>("请选择主区域", ""));
        
#if UNITY_EDITOR
        if (idDictionary != null)
        {
            var mainAreaIds = idDictionary.GetAllMainAreaIds();
            foreach (var areaId in mainAreaIds)
            {
                var displayName = idDictionary.GetMainAreaDisplayName(areaId);
                var itemText = string.IsNullOrEmpty(displayName) ? areaId : $"{displayName} ({areaId})";
                items.Add(new ValueDropdownItem<string>(itemText, areaId));
            }
        }
#endif
        
        return items.OrderBy(x => x.Text);
    }
    
    /// <summary>
    /// 获取可用的分区域ID列表
    /// </summary>
    /// <returns>分区域ID和显示名称的列表</returns>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableSubAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        if (string.IsNullOrEmpty(mainAreaId))
        {
            items.Add(new ValueDropdownItem<string>("请先选择主区域", ""));
            return items;
        }
        
        // 添加空选项
        items.Add(new ValueDropdownItem<string>("请选择分区域", ""));
        
#if UNITY_EDITOR
        if (idDictionary != null)
        {
            var subAreaIds = idDictionary.GetAllSubAreaIds(mainAreaId);
            foreach (var areaId in subAreaIds)
            {
                var displayName = idDictionary.GetSubAreaDisplayName(mainAreaId, areaId);
                var itemText = string.IsNullOrEmpty(displayName) ? areaId : $"{displayName} ({areaId})";
                items.Add(new ValueDropdownItem<string>(itemText, areaId));
            }
        }
#endif
        
        return items.OrderBy(x => x.Text);
    }
    
    /// <summary>
    /// 获取可用的场景区域ID列表
    /// </summary>
    /// <returns>场景区域ID和显示名称的列表</returns>
    private IEnumerable<ValueDropdownItem<string>> GetAvailableSceneAreaIds()
    {
        var items = new List<ValueDropdownItem<string>>();
        
        if (string.IsNullOrEmpty(mainAreaId) || string.IsNullOrEmpty(subAreaId))
        {
            items.Add(new ValueDropdownItem<string>("请先选择主区域和分区域", ""));
            return items;
        }
        
        // 添加空选项
        items.Add(new ValueDropdownItem<string>("请选择场景区域", ""));
        
#if UNITY_EDITOR
        if (idDictionary != null)
        {
            var sceneAreaIds = idDictionary.GetAllSceneAreaIds(mainAreaId, subAreaId);
            foreach (var areaId in sceneAreaIds)
            {
                var displayName = idDictionary.GetSceneAreaDisplayName(mainAreaId, subAreaId, areaId);
                var itemText = string.IsNullOrEmpty(displayName) ? areaId : $"{displayName} ({areaId})";
                items.Add(new ValueDropdownItem<string>(itemText, areaId));
            }
        }
#endif
        
        return items.OrderBy(x => x.Text);
    }
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(portalId) && 
               !string.IsNullOrEmpty(displayName) &&
               !string.IsNullOrEmpty(mainAreaId) &&
               !string.IsNullOrEmpty(subAreaId) &&
               !string.IsNullOrEmpty(sceneAreaId);
    }
    
    /// <summary>
    /// 获取完整的区域ID
    /// </summary>
    /// <returns>格式为 mainAreaId.subAreaId.sceneAreaId 的完整区域ID</returns>
    public string GetFullAreaId()
    {
        if (string.IsNullOrEmpty(mainAreaId) || string.IsNullOrEmpty(subAreaId) || string.IsNullOrEmpty(sceneAreaId))
        {
            return "";
        }
        return $"{mainAreaId}.{subAreaId}.{sceneAreaId}";
    }
    
    /// <summary>
    /// 获取传送门的完整信息字符串
    /// </summary>
    /// <returns>传送门信息</returns>
    public override string ToString()
    {
        var areaInfo = GetFullAreaId();
        var areaText = string.IsNullOrEmpty(areaInfo) ? "未设置区域" : areaInfo;
        return $"Portal[{portalId}]: {displayName} ({areaText}) -> {targetPortalId}";
    }
}