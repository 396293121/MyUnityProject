#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// UI配置编辑器 - 提供在Unity编辑器中编辑UI配置的功能
/// </summary>
public class UIConfigEditor : EditorWindow
{
    private GameConfig gameConfig;
    private Vector2 scrollPosition;
    private bool showMainMenuConfig = true;
    private bool showCharacterSelectConfig = true;
    private bool showUIAnimationSettings = true;
    
    [MenuItem("Game/UI Config Editor")]
    public static void ShowWindow()
    {
        GetWindow<UIConfigEditor>("UI Config Editor");
    }
    
    private void OnEnable()
    {
        LoadGameConfig();
    }
    
    private void LoadGameConfig()
    {
        gameConfig = Resources.Load<GameConfig>("GameConfig");
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig not found in Resources folder!");
        }
    }
    
    private void OnGUI()
    {
        if (gameConfig == null)
        {
            EditorGUILayout.HelpBox("GameConfig not found in Resources folder!", MessageType.Error);
            if (GUILayout.Button("Reload GameConfig"))
            {
                LoadGameConfig();
            }
            return;
        }
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("UI Config Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // UI动画设置
        showUIAnimationSettings = EditorGUILayout.Foldout(showUIAnimationSettings, "UI动画设置", true);
        if (showUIAnimationSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            
            gameConfig.uiConfig.panelFadeTime = EditorGUILayout.FloatField("面板淡入时间", gameConfig.uiConfig.panelFadeTime);
            gameConfig.uiConfig.buttonScaleTime = EditorGUILayout.FloatField("按钮缩放时间", gameConfig.uiConfig.buttonScaleTime);
            gameConfig.uiConfig.messageDisplayTime = EditorGUILayout.FloatField("消息显示时间", gameConfig.uiConfig.messageDisplayTime);
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 主菜单配置
        showMainMenuConfig = EditorGUILayout.Foldout(showMainMenuConfig, "主菜单配置", true);
        if (showMainMenuConfig)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            
            // 这里可以添加主菜单特定的配置项
            // 例如：背景音乐、按钮音效、淡入时间等
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 角色选择配置
        showCharacterSelectConfig = EditorGUILayout.Foldout(showCharacterSelectConfig, "角色选择配置", true);
        if (showCharacterSelectConfig)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            
            // 这里可以添加角色选择界面特定的配置项
            // 例如：角色旋转速度、选择效果等
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        // 保存按钮
        if (GUILayout.Button("保存配置"))
        {
            SaveConfig();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void SaveConfig()
    {
        if (gameConfig != null)
        {
            EditorUtility.SetDirty(gameConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UI配置已保存");
        }
    }
}
#endif