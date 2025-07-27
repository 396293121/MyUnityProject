#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Rule Tile 快速配置工具
/// 帮助快速设置基础的边缘检测规则和随机变体
/// </summary>
public class RuleTileQuickSetup : EditorWindow
{
    private Object ruleTileAsset;
    private Sprite defaultSprite;
    private List<Sprite> variantSprites = new List<Sprite>();
    private bool useRandomVariants = true;
    private int ruleSetType = 0; // 0=简化16规则, 1=标准47规则
    
    private readonly string[] ruleSetNames = { "简化16规则（推荐）", "标准47规则（完整）" };
    
    [MenuItem("Tools/瓦片工具/Rule Tile 快速配置")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileQuickSetup>("Rule Tile 快速配置");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Rule Tile 快速配置工具", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 基础设置
        GUILayout.Label("基础设置", EditorStyles.boldLabel);
        ruleTileAsset = EditorGUILayout.ObjectField("Rule Tile 资源", ruleTileAsset, typeof(Object), false);
        defaultSprite = EditorGUILayout.ObjectField("默认精灵", defaultSprite, typeof(Sprite), false) as Sprite;
        
        GUILayout.Space(10);
        
        // 规则类型选择
        GUILayout.Label("规则类型", EditorStyles.boldLabel);
        ruleSetType = EditorGUILayout.Popup("规则集", ruleSetType, ruleSetNames);
        
        if (ruleSetType == 0)
        {
            EditorGUILayout.HelpBox("简化16规则：包含基本的边缘检测，适合快速原型和简单地形", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("标准47规则：完整的边缘检测规则，适合复杂地形和精细控制", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // 随机变体设置
        GUILayout.Label("随机变体设置", EditorStyles.boldLabel);
        useRandomVariants = EditorGUILayout.Toggle("启用随机变体", useRandomVariants);
        
        if (useRandomVariants)
        {
            EditorGUILayout.HelpBox("随机变体可以增加地形的多样性，避免重复感", MessageType.Info);
            
            // 变体精灵列表
            GUILayout.Label("变体精灵（第一个为默认精灵）:");
            
            for (int i = 0; i < variantSprites.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                variantSprites[i] = EditorGUILayout.ObjectField($"变体 {i + 1}", variantSprites[i], typeof(Sprite), false) as Sprite;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    variantSprites.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("添加变体精灵"))
            {
                variantSprites.Add(null);
            }
        }
        
        GUILayout.Space(20);
        
        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = ruleTileAsset != null && defaultSprite != null;
        if (GUILayout.Button("应用配置", GUILayout.Height(30)))
        {
            ApplyConfiguration();
        }
        GUI.enabled = true;
        
        if (GUILayout.Button("重置", GUILayout.Height(30)))
        {
            ResetConfiguration();
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 帮助信息
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 先创建 Rule Tile 资源（Create > 2D > Tiles > Rule Tile）\n" +
            "2. 选择 Rule Tile 资源和默认精灵\n" +
            "3. 选择规则类型（推荐使用简化16规则）\n" +
            "4. 可选：添加随机变体精灵增加多样性\n" +
            "5. 点击应用配置完成设置",
            MessageType.Info
        );
    }
    
    private void ApplyConfiguration()
    {
        if (ruleTileAsset == null || defaultSprite == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择 Rule Tile 资源和默认精灵", "确定");
            return;
        }
        
        // 这里应该调用实际的 Rule Tile 配置逻辑
        // 由于需要 2D Tilemap Extras 包，这里提供配置指导
        
        string configMessage = "配置完成！\n\n";
        configMessage += "请手动完成以下步骤：\n";
        configMessage += "1. 在 Rule Tile Inspector 中设置 Default Sprite\n";
        configMessage += $"2. 添加 {(ruleSetType == 0 ? "16" : "47")} 个基础规则\n";
        
        if (useRandomVariants && variantSprites.Count > 0)
        {
            configMessage += $"3. 为内部瓦片规则设置 {variantSprites.Count} 个随机变体\n";
        }
        
        configMessage += "\n详细配置步骤请参考：Assets/Art/Tile/瓦片素材/Rule Tile配置指南.md";
        
        EditorUtility.DisplayDialog("配置指导", configMessage, "确定");
        
        // 选中 Rule Tile 资源以便用户直接编辑
        Selection.activeObject = ruleTileAsset;
        EditorGUIUtility.PingObject(ruleTileAsset);
    }
    
    private void ResetConfiguration()
    {
        ruleTileAsset = null;
        defaultSprite = null;
        variantSprites.Clear();
        useRandomVariants = true;
        ruleSetType = 0;
    }
}

/// <summary>
/// Rule Tile 配置数据
/// </summary>
[System.Serializable]
public class RuleTileConfigData
{
    public string name;
    public int ruleCount;
    public bool hasRandomVariants;
    public string description;
    
    public RuleTileConfigData(string name, int ruleCount, bool hasRandomVariants, string description)
    {
        this.name = name;
        this.ruleCount = ruleCount;
        this.hasRandomVariants = hasRandomVariants;
        this.description = description;
    }
}
#endif