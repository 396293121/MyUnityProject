#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// UI配置导出器 - 提供在Unity编辑器中将UI配置导出为JSON文件的功能
/// </summary>
public class UIConfigExporter : EditorWindow
{
    private string exportPath = "Assets/Resources/Configs";
    private bool exportMainMenuConfig = true;
    private bool exportCharacterSelectConfig = true;
    
    [MenuItem("Game/UI Config Exporter")]
    public static void ShowWindow()
    {
        GetWindow<UIConfigExporter>("UI Config Exporter");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("UI配置导出器", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 导出路径
        EditorGUILayout.BeginHorizontal();
        exportPath = EditorGUILayout.TextField("导出路径", exportPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("选择导出路径", exportPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // 将绝对路径转换为相对于项目的路径
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                exportPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 导出选项
        EditorGUILayout.LabelField("导出选项", EditorStyles.boldLabel);
        exportMainMenuConfig = EditorGUILayout.Toggle("导出主菜单配置", exportMainMenuConfig);
        exportCharacterSelectConfig = EditorGUILayout.Toggle("导出角色选择配置", exportCharacterSelectConfig);
        
        EditorGUILayout.Space();
        
        // 导出按钮
        if (GUILayout.Button("导出配置"))
        {
            ExportConfigs();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void ExportConfigs()
    {
        // 确保导出目录存在
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }
        
        // 获取ConfigManager实例
        ConfigManager configManager = FindObjectOfType<ConfigManager>();
        if (configManager == null)
        {
            // 尝试从预制体加载
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Managers/ConfigManager");
            if (prefab != null)
            {
                configManager = prefab.GetComponent<ConfigManager>();
            }
            
            if (configManager == null)
            {
                Debug.LogError("无法找到ConfigManager实例或预制体");
                EditorUtility.DisplayDialog("导出失败", "无法找到ConfigManager实例或预制体", "确定");
                return;
            }
        }
        
        // 创建包装器
        UIConfigDataWrapper wrapper = new UIConfigDataWrapper();
        
        // 导出主菜单配置
        if (exportMainMenuConfig)
        {
            wrapper.mainMenuConfig = configManager.GetUIConfig("MainMenuUI");
            if (wrapper.mainMenuConfig == null)
            {
                Debug.LogWarning("MainMenuUI配置不存在");
            }
        }
        
        // 导出角色选择配置
        if (exportCharacterSelectConfig)
        {
            wrapper.characterSelectConfig = configManager.GetUIConfig("CharacterSelectUI");
            if (wrapper.characterSelectConfig == null)
            {
                Debug.LogWarning("CharacterSelectUI配置不存在");
            }
        }
        
        // 序列化为JSON
        string json = JsonUtility.ToJson(wrapper, true);
        
        // 写入文件
        string filePath = Path.Combine(exportPath, "ui_config.json");
        File.WriteAllText(filePath, json);
        
        Debug.Log($"UI配置已导出到: {filePath}");
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("导出成功", $"UI配置已导出到: {filePath}", "确定");
    }
}

/// <summary>
/// UI配置数据包装器 - 用于JSON序列化
/// </summary>
[System.Serializable]
public class UIConfigDataWrapper
{
    public UIConfigData mainMenuConfig;
    public UIConfigData characterSelectConfig;
}
#endif