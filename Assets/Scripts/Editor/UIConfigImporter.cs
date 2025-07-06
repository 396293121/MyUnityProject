#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// UI配置导入器 - 提供在Unity编辑器中从JSON文件导入UI配置的功能
/// </summary>
public class UIConfigImporter : EditorWindow
{
    private string importPath = "Assets/Resources/Configs/ui_config.json";
    private bool importMainMenuConfig = true;
    private bool importCharacterSelectConfig = true;
    private UIConfigDataWrapper configWrapper;
    
    [MenuItem("Game/UI Config Importer")]
    public static void ShowWindow()
    {
        GetWindow<UIConfigImporter>("UI Config Importer");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("UI配置导入器", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 导入路径
        EditorGUILayout.BeginHorizontal();
        importPath = EditorGUILayout.TextField("导入文件", importPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择导入文件", Path.GetDirectoryName(importPath), "json");
            if (!string.IsNullOrEmpty(path))
            {
                // 将绝对路径转换为相对于项目的路径
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                importPath = path;
                LoadConfigFile();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("加载配置文件"))
        {
            LoadConfigFile();
        }
        
        EditorGUILayout.Space();
        
        // 导入选项
        EditorGUILayout.LabelField("导入选项", EditorStyles.boldLabel);
        importMainMenuConfig = EditorGUILayout.Toggle("导入主菜单配置", importMainMenuConfig);
        importCharacterSelectConfig = EditorGUILayout.Toggle("导入角色选择配置", importCharacterSelectConfig);
        
        EditorGUILayout.Space();
        
        // 显示配置预览
        if (configWrapper != null)
        {
            EditorGUILayout.LabelField("配置预览", EditorStyles.boldLabel);
            
            if (configWrapper.mainMenuConfig != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("主菜单配置", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"淡入时间: {configWrapper.mainMenuConfig.fadeInTime}");
                EditorGUILayout.LabelField($"按钮悬停缩放: {configWrapper.mainMenuConfig.buttonHoverScale}");
                EditorGUILayout.LabelField($"按钮点击缩放: {configWrapper.mainMenuConfig.buttonClickScale}");
                EditorGUILayout.LabelField($"播放背景音乐: {configWrapper.mainMenuConfig.playBackgroundMusic}");
                EditorGUILayout.LabelField($"播放按钮音效: {configWrapper.mainMenuConfig.playButtonSounds}");
                EditorGUILayout.LabelField($"下一个场景: {configWrapper.mainMenuConfig.nextScene}");
                EditorGUILayout.EndVertical();
            }
            
            if (configWrapper.characterSelectConfig != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("角色选择配置", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"淡入时间: {configWrapper.characterSelectConfig.fadeInTime}");
                EditorGUILayout.LabelField($"按钮悬停缩放: {configWrapper.characterSelectConfig.buttonHoverScale}");
                EditorGUILayout.LabelField($"按钮点击缩放: {configWrapper.characterSelectConfig.buttonClickScale}");
                EditorGUILayout.LabelField($"播放背景音乐: {configWrapper.characterSelectConfig.playBackgroundMusic}");
                EditorGUILayout.LabelField($"播放按钮音效: {configWrapper.characterSelectConfig.playButtonSounds}");
                EditorGUILayout.LabelField($"下一个场景: {configWrapper.characterSelectConfig.nextScene}");
                EditorGUILayout.EndVertical();
            }
        }
        
        EditorGUILayout.Space();
        
        // 导入按钮
        GUI.enabled = configWrapper != null;
        if (GUILayout.Button("导入配置"))
        {
            ImportConfigs();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
    }
    
    private void LoadConfigFile()
    {
        if (!File.Exists(importPath))
        {
            Debug.LogError($"配置文件不存在: {importPath}");
            EditorUtility.DisplayDialog("加载失败", $"配置文件不存在: {importPath}", "确定");
            configWrapper = null;
            return;
        }
        
        try
        {
            // 读取JSON文件
            string json = File.ReadAllText(importPath);
            
            // 反序列化
            configWrapper = JsonUtility.FromJson<UIConfigDataWrapper>(json);
            
            if (configWrapper == null)
            {
                Debug.LogError("配置文件格式错误");
                EditorUtility.DisplayDialog("加载失败", "配置文件格式错误", "确定");
                return;
            }
            
            Debug.Log($"配置文件已加载: {importPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载配置文件失败: {e.Message}");
            EditorUtility.DisplayDialog("加载失败", $"加载配置文件失败: {e.Message}", "确定");
            configWrapper = null;
        }
    }
    
    private void ImportConfigs()
    {
        if (configWrapper == null)
        {
            Debug.LogError("没有加载配置文件");
            EditorUtility.DisplayDialog("导入失败", "没有加载配置文件", "确定");
            return;
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
                EditorUtility.DisplayDialog("导入失败", "无法找到ConfigManager实例或预制体", "确定");
                return;
            }
        }
        
        // 导入主菜单配置
        if (importMainMenuConfig && configWrapper.mainMenuConfig != null)
        {
            configManager.UpdateUIConfig("MainMenuUI", configWrapper.mainMenuConfig);
            Debug.Log("主菜单配置已导入");
        }
        
        // 导入角色选择配置
        if (importCharacterSelectConfig && configWrapper.characterSelectConfig != null)
        {
            configManager.UpdateUIConfig("CharacterSelectUI", configWrapper.characterSelectConfig);
            Debug.Log("角色选择配置已导入");
        }
        
        EditorUtility.SetDirty(configManager);
        AssetDatabase.SaveAssets();
        
        Debug.Log("UI配置已导入");
        EditorUtility.DisplayDialog("导入成功", "UI配置已导入", "确定");
    }
}
#endif