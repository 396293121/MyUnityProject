using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// UI配置保存加载 - 负责将UI配置保存到JSON文件和从JSON文件加载配置
/// </summary>
public class UIConfigSaveLoad : MonoBehaviour
{
    [Header("配置文件设置")]
    public string configFileName = "ui_config.json";
    public bool loadOnAwake = true;
    public bool autoSaveOnQuit = true;
    
    private string ConfigFilePath => Path.Combine(Application.persistentDataPath, configFileName);
    
    private void Awake()
    {
        if (loadOnAwake)
        {
            LoadConfigFromFile();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit)
        {
            SaveConfigToFile();
        }
    }
    
    /// <summary>
    /// 将UI配置保存到JSON文件
    /// </summary>
    public void SaveConfigToFile()
    {
        if (ConfigManager.Instance == null)
        {
            Debug.LogError("[UIConfigSaveLoad] ConfigManager实例不存在");
            return;
        }
        
        try
        {
            UIConfigDataWrapper wrapper = new UIConfigDataWrapper();
            
            // 获取所有UI配置
            wrapper.mainMenuConfig = ConfigManager.Instance.GetUIConfig("MainMenuUI");
            wrapper.characterSelectConfig = ConfigManager.Instance.GetUIConfig("CharacterSelectUI");
            
            // 序列化为JSON
            string json = JsonUtility.ToJson(wrapper, true);
            
            // 写入文件
            File.WriteAllText(ConfigFilePath, json);
            
            Debug.Log($"[UIConfigSaveLoad] UI配置已保存到: {ConfigFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UIConfigSaveLoad] 保存UI配置失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 从JSON文件加载UI配置
    /// </summary>
    public void LoadConfigFromFile()
    {
        if (ConfigManager.Instance == null)
        {
            Debug.LogError("[UIConfigSaveLoad] ConfigManager实例不存在");
            return;
        }
        
        if (!File.Exists(ConfigFilePath))
        {
            Debug.Log($"[UIConfigSaveLoad] 配置文件不存在: {ConfigFilePath}，将使用默认配置");
            return;
        }
        
        try
        {
            // 读取JSON文件
            string json = File.ReadAllText(ConfigFilePath);
            
            // 反序列化
            UIConfigDataWrapper wrapper = JsonUtility.FromJson<UIConfigDataWrapper>(json);
            
            // 更新ConfigManager中的配置
            if (wrapper.mainMenuConfig != null)
            {
                ConfigManager.Instance.UpdateUIConfig("MainMenuUI", wrapper.mainMenuConfig);
            }
            
            if (wrapper.characterSelectConfig != null)
            {
                ConfigManager.Instance.UpdateUIConfig("CharacterSelectUI", wrapper.characterSelectConfig);
            }
            
            Debug.Log($"[UIConfigSaveLoad] UI配置已从文件加载: {ConfigFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UIConfigSaveLoad] 加载UI配置失败: {e.Message}");
        }
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