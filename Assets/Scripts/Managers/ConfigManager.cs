using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 配置管理器 - 统一管理游戏配置数据
/// 从Phaser项目迁移的配置系统，避免硬编码
/// </summary>
public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    
    [Header("配置文件路径")]
    public string configPath = "Data/Configs/GameConfigData";
    
    [Header("调试设置")]
    public bool debugMode = false;
    
    // 配置数据
    private GameConfigData configData;
    
    // 缓存的配置
    private Dictionary<string, SceneConfigData> sceneConfigs;
    private Dictionary<string, CharacterConfigData> characterConfigs;
    // EnemySystemConfig 配置管理
    private Dictionary<string, AudioClipData> audioConfigs;
    private Dictionary<string, UIConfigData> uiConfigs;
    
    // 新的统一配置系统
    private EnemySystemConfig enemySystemConfig;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfigurations();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 加载所有配置
    /// </summary>
    void LoadConfigurations()
    {
        try
        {
            // 这里简化处理，实际项目中应该从ScriptableObject或JSON文件加载
            InitializeDefaultConfigs();
            
            if (debugMode)
            {
                Debug.Log("[ConfigManager] 配置加载完成");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConfigManager] 配置加载失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 初始化默认配置
    /// </summary>
    void InitializeDefaultConfigs()
    {
        // 初始化场景配置
        sceneConfigs = new Dictionary<string, SceneConfigData>
        {
            ["MainMenuScene"] = new SceneConfigData
            {
                sceneName = "MainMenuScene",
                displayName = "主菜单",
                backgroundMusic = "main_menu_music",
                preloadAssets = new List<string> { "ui/main_menu_bg", "ui/logo", "ui/buttons" }
            },
            ["CharacterSelectScene"] = new SceneConfigData
            {
                sceneName = "CharacterSelectScene",
                displayName = "角色选择",
                backgroundMusic = "character_select_music",
                preloadAssets = new List<string> { "ui/character_select_bg", "characters/portraits" }
            },
            ["TestScene"] = new SceneConfigData
            {
                sceneName = "TestScene",
                displayName = "测试场景",
                backgroundMusic = "game_music",
                preloadAssets = new List<string> { "maps/test_map", "characters/warrior_atlas", "enemies/wild_boar_atlas" }
            }
        };
        
        // 初始化角色配置
        characterConfigs = new Dictionary<string, CharacterConfigData>
        {
            ["warrior"] = new CharacterConfigData
            {
                characterType = "warrior",
                displayName = "战士",
                health = 120,
                mana = 30,
                strength = 15,
                agility = 8,
                vitality = 12,
                intelligence = 5,
                moveSpeed = 5.0f,
                jumpForce = 10.0f,
                attackRange = 2.0f,
                weaponType = "sword",
                attackType = "melee"
            },
            ["mage"] = new CharacterConfigData
            {
                characterType = "mage",
                displayName = "法师",
                health = 80,
                mana = 100,
                strength = 5,
                agility = 6,
                vitality = 8,
                intelligence = 15,
                moveSpeed = 4.0f,
                jumpForce = 8.0f,
                attackRange = 6.0f,
                weaponType = "staff",
                attackType = "ranged"
            },
            ["archer"] = new CharacterConfigData
            {
                characterType = "archer",
                displayName = "射手",
                health = 100,
                mana = 50,
                strength = 10,
                agility = 15,
                vitality = 10,
                intelligence = 8,
                moveSpeed = 6.0f,
                jumpForce = 12.0f,
                attackRange = 8.0f,
                weaponType = "bow",
                attackType = "ranged"
            }
        };
        
        // 旧的enemyConfigs初始化已移除，现在使用EnemySystemConfig
        
        // 初始化音频配置
        audioConfigs = new Dictionary<string, AudioClipData>
        {
            ["main_menu_music"] = new AudioClipData { key = "main_menu_music", volume = 0.6f, loop = true },
            ["character_select_music"] = new AudioClipData { key = "character_select_music", volume = 0.6f, loop = true },
            ["game_music"] = new AudioClipData { key = "game_music", volume = 0.5f, loop = true },
            ["button_click"] = new AudioClipData { key = "button_click", volume = 0.6f, loop = false },
            ["button_select"] = new AudioClipData { key = "button_select", volume = 0.5f, loop = false }
        };
        
        // 初始化UI配置
        uiConfigs = new Dictionary<string, UIConfigData>
        {
            ["MainMenuUI"] = new UIConfigData
            {
                uiName = "MainMenuUI",
                fadeInTime = 1.0f,
                buttonHoverScale = 1.1f,
                buttonClickScale = 0.95f,
                playBackgroundMusic = true,
                playButtonSounds = true,
                nextScene = "CharacterSelectScene",
                backgroundMusicKey = "menu_music",
                dialogTexts = new Dictionary<string, string>
                {
                    ["ExitConfirm"] = "确定要退出游戏吗？",
                    ["settings_title"] = "游戏设置"
                },
                buttonSoundMappings = new Dictionary<string, string>
                {
                    ["button_click"] = "button_click",
                    ["button_select"] = "button_select"
                }
            },
            ["CharacterSelectUI"] = new UIConfigData
            {
                uiName = "CharacterSelectUI",
                fadeInTime = 0.5f,
                buttonHoverScale = 1.05f,
                buttonClickScale = 0.98f,
                playBackgroundMusic = true,
                playButtonSounds = true,
                nextScene = "GameScene",
                backgroundMusicKey = "character_select_music",
                dialogTexts = new Dictionary<string, string>
                {
                    ["back_confirm"] = "确定要返回主菜单吗？",
                    ["character_locked"] = "该角色尚未解锁"
                },
                buttonSoundMappings = new Dictionary<string, string>
                {
                    ["button_click"] = "button_click",
                    ["button_select"] = "button_select",
                    ["character_select"] = "character_select"
                }
            }
        };
    }
    
    // 配置访问方法
    public SceneConfigData GetSceneConfig(string sceneName)
    {
        return sceneConfigs.ContainsKey(sceneName) ? sceneConfigs[sceneName] : null;
    }
    
    public CharacterConfigData GetCharacterConfig(string characterType)
    {
        return characterConfigs.ContainsKey(characterType) ? characterConfigs[characterType] : null;
    }
    
    // 使用 GetEnemySystemConfig 获取敌人系统配置
    
    /// <summary>
    /// 获取敌人系统配置（新的统一配置）
    /// </summary>
    public EnemySystemConfig GetEnemySystemConfig()
    {
        if (enemySystemConfig == null)
        {
            // 尝试从Resources加载
            enemySystemConfig = Resources.Load<EnemySystemConfig>("EnemySystemConfig");
            
            if (enemySystemConfig == null && debugMode)
            {
                Debug.LogWarning("[ConfigManager] 未找到EnemySystemConfig，请确保配置文件在Resources文件夹中");
            }
        }
        
        return enemySystemConfig;
    }
    
    /// <summary>
    /// 设置敌人系统配置
    /// </summary>
    public void SetEnemySystemConfig(EnemySystemConfig config)
    {
        enemySystemConfig = config;
        
        if (debugMode)
        {
            Debug.Log("[ConfigManager] 敌人系统配置已更新");
        }
    }
    
    public AudioClipData GetAudioConfig(string audioKey)
    {
        return audioConfigs.ContainsKey(audioKey) ? audioConfigs[audioKey] : null;
    }
    
    public List<string> GetAvailableCharacters()
    {
        return characterConfigs.Keys.ToList();
    }
    
    public UIConfigData GetUIConfig(string uiName)
    {
        return uiConfigs.ContainsKey(uiName) ? uiConfigs[uiName] : null;
    }
    
    /// <summary>
    /// 更新UI配置
    /// </summary>
    public void UpdateUIConfig(string uiName, UIConfigData newConfig)
    {
        if (string.IsNullOrEmpty(uiName) || newConfig == null)
        {
            Debug.LogWarning($"[ConfigManager] 无法更新UI配置: uiName={uiName}, newConfig={newConfig}");
            return;
        }
        
        if (uiConfigs.ContainsKey(uiName))
        {
            uiConfigs[uiName] = newConfig;
            Debug.Log($"[ConfigManager] UI配置已更新: {uiName}");
        }
        else
        {
            uiConfigs.Add(uiName, newConfig);
            Debug.Log($"[ConfigManager] 新增UI配置: {uiName}");
        }
    }
}

// 配置数据结构
[System.Serializable]
public class GameConfigData
{
    public string gameVersion = "1.0.0";
    public string gameName = "RPG Game";
    public bool debugMode = false;
}

[System.Serializable]
public class SceneConfigData
{
    public string sceneName;
    public string displayName;
    public string backgroundMusic;
    public List<string> preloadAssets = new List<string>();
}

[System.Serializable]
public class CharacterConfigData
{
    public string characterType;
    public string displayName;
    public int health;
    public int mana;
    public int strength;
    public int agility;
    public int vitality;
    public int intelligence;
    public float moveSpeed;
    public float jumpForce;
    public float attackRange;
    public string weaponType;
    public string attackType;
}



[System.Serializable]
public class AudioClipData
{
    public string key;
    public float volume = 1.0f;
    public bool loop = false;
    public float pitch = 1.0f;
}

[System.Serializable]
public class UIConfigData
{
    public string uiName;
    public float fadeInTime = 1.0f;
    public float buttonHoverScale = 1.1f;
    public float buttonClickScale = 0.95f;
    public bool playBackgroundMusic = true;
    public bool playButtonSounds = true;
    public string nextScene;
    public string backgroundMusicKey;
    public Dictionary<string, string> dialogTexts = new Dictionary<string, string>();
    public Dictionary<string, string> buttonSoundMappings = new Dictionary<string, string>();
}