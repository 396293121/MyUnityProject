using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI配置加载器 - 负责从GameConfig加载UI配置到ConfigManager
/// </summary>
public class UIConfigLoader : MonoBehaviour
{
    private void Awake()
    {
        LoadUIConfigs();
    }
    
    /// <summary>
    /// 从GameConfig加载UI配置到ConfigManager
    /// </summary>
    public void LoadUIConfigs()
    {
        if (GameConfig.Instance == null || ConfigManager.Instance == null)
        {
            Debug.LogError("[UIConfigLoader] GameConfig或ConfigManager实例不存在");
            return;
        }
        
        UIConfig uiConfig = GameConfig.Instance.uiConfig;
        if (uiConfig == null)
        {
            Debug.LogWarning("[UIConfigLoader] GameConfig中的UIConfig为空");
            return;
        }
        
        // 更新MainMenuUI配置
        UpdateMainMenuUIConfig(uiConfig);
        
        // 更新CharacterSelectUI配置
        UpdateCharacterSelectUIConfig(uiConfig);
        
        Debug.Log("[UIConfigLoader] UI配置已从GameConfig加载到ConfigManager");
    }
    
    /// <summary>
    /// 更新MainMenuUI配置
    /// </summary>
    private void UpdateMainMenuUIConfig(UIConfig uiConfig)
    {
        UIConfigData mainMenuConfig = ConfigManager.Instance.GetUIConfig("MainMenuUI");
        if (mainMenuConfig != null)
        {
            // 应用GameConfig中的UI动画设置
            mainMenuConfig.fadeInTime = uiConfig.panelFadeTime;
            mainMenuConfig.buttonHoverScale = 1.1f; // 可以根据需要从GameConfig中获取
            mainMenuConfig.buttonClickScale = 0.95f; // 可以根据需要从GameConfig中获取
            
            Debug.Log("[UIConfigLoader] MainMenuUI配置已更新");
        }
    }
    
    /// <summary>
    /// 更新CharacterSelectUI配置
    /// </summary>
    private void UpdateCharacterSelectUIConfig(UIConfig uiConfig)
    {
        UIConfigData characterSelectConfig = ConfigManager.Instance.GetUIConfig("CharacterSelectUI");
        if (characterSelectConfig != null)
        {
            // 应用GameConfig中的UI动画设置
            characterSelectConfig.fadeInTime = uiConfig.panelFadeTime;
            characterSelectConfig.buttonHoverScale = 1.05f; // 可以根据需要从GameConfig中获取
            characterSelectConfig.buttonClickScale = 0.98f; // 可以根据需要从GameConfig中获取
            
            Debug.Log("[UIConfigLoader] CharacterSelectUI配置已更新");
        }
    }
}