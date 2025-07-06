using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏管理器 - 负责游戏的整体状态管理
/// 从原Phaser项目的GameManager.js迁移而来
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("游戏状态")]
    public GameState currentState = GameState.MainMenu;
    
    [Header("调试设置")]
    public bool debugMode = false;
    
    // 单例模式
    public static GameManager Instance { get; private set; }
    
    // 当前游戏状态属性（用于SaveSystem兼容）
    public GameState CurrentState => currentState;
    
    // 事件系统
    public System.Action<GameState> OnGameStateChanged;
    
    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeGame()
    {
        // 初始化游戏设置
        Application.targetFrameRate = 60;
        
        if (debugMode)
        {
            Debug.Log("[GameManager] 游戏初始化完成");
        }
    }
    
    /// <summary>
    /// 改变游戏状态
    /// </summary>
    public void ChangeGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        if (debugMode)
        {
            Debug.Log($"[GameManager] 游戏状态改变: {previousState} -> {newState}");
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (debugMode)
        {
            Debug.Log($"[GameManager] 加载场景: {sceneName}");
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        ChangeGameState(GameState.Paused);
    }
    
    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ChangeGameState(GameState.Playing);
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        if (debugMode)
        {
            Debug.Log("[GameManager] 退出游戏");
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 获取玩家等级
    /// </summary>
    public int GetPlayerLevel()
    {
        var player = FindObjectOfType<PlayerController>();
        return player != null ? player.GetComponent<Character>().level : 1;
    }
    
    /// <summary>
    /// 增加玩家经验
    /// </summary>
    public void AddPlayerExperience(int amount)
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var character = player.GetComponent<Character>();
            if (character != null)
            {
                character.AddExperience(amount);
            }
        }
    }
    
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    public void OnPlayerDeath()
    {
        ChangeGameState(GameState.GameOver);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameOver();
        }
    }
    
    /// <summary>
    /// 增加玩家金币
    /// </summary>
    public void AddPlayerGold(int amount)
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                // 假设金币存储在inventory中，这里需要根据实际实现调整
                if (debugMode)
                {
                    Debug.Log($"[GameManager] 增加金币: {amount}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取玩家金币
    /// </summary>
    public int GetPlayerGold()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                // 假设金币存储在inventory中，这里需要根据实际实现调整
                return 0; // 临时返回0，需要根据实际金币系统实现
            }
        }
        return 0;
    }
    
    /// <summary>
    /// 获取玩家名称
    /// </summary>
    public string GetPlayerName()
    {
        var player = FindObjectOfType<PlayerController>();
        return player != null ? player.name : "Player";
    }
    
    /// <summary>
    /// 获取玩家职业
    /// </summary>
    public string GetPlayerClass()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var character = player.GetComponent<Character>();
            if (character != null)
            {
                // 根据角色类型返回职业名称
                if (character is Warrior) return "Warrior";
                if (character is Archer) return "Archer";
                if (character is Mage) return "Mage";
            }
        }
        return "Unknown";
    }
    
    /// <summary>
    /// 获取玩家技能点
    /// </summary>
    public int GetPlayerSkillPoints()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var character = player.GetComponent<Character>();
            if (character != null)
            {
                // 假设技能点基于等级计算
                return character.level - 1;
            }
        }
        return 0;
    }
    
    /// <summary>
    /// 增加玩家技能点
    /// </summary>
    public void AddPlayerSkillPoints(int amount)
    {
        // 技能点通常通过升级获得，这里可以记录额外的技能点
        if (debugMode)
        {
            Debug.Log($"[GameManager] 增加技能点: {amount}");
        }
    }
    
    /// <summary>
    /// 检查是否有存档数据
    /// </summary>
    public bool HasSaveData()
    {
        // 检查PlayerPrefs中是否有存档数据
        return PlayerPrefs.HasKey("GameSave_HasData");
    }
    
    /// <summary>
    /// 加载游戏存档
    /// </summary>
    public void LoadGame()
    {
        if (HasSaveData())
        {
            // 从PlayerPrefs加载游戏数据
            string savedScene = PlayerPrefs.GetString("GameSave_Scene", "GameScene");
            int playerLevel = PlayerPrefs.GetInt("GameSave_PlayerLevel", 1);
            
            if (debugMode)
            {
                Debug.Log($"[GameManager] 加载存档: 场景={savedScene}, 等级={playerLevel}");
            }
            
            // 加载保存的场景
            LoadScene(savedScene);
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning("[GameManager] 没有找到存档数据");
            }
        }
    }
    
    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void SaveGame()
    {
        // 保存基本游戏状态
        PlayerPrefs.SetString("GameSave_HasData", "true");
        PlayerPrefs.SetString("GameSave_Scene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("GameSave_PlayerLevel", GetPlayerLevel());
        PlayerPrefs.SetString("GameSave_PlayerName", GetPlayerName());
        PlayerPrefs.SetString("GameSave_PlayerClass", GetPlayerClass());
        PlayerPrefs.SetInt("GameSave_PlayerGold", GetPlayerGold());
        PlayerPrefs.SetInt("GameSave_PlayerMana", GetPlayerMana());
        PlayerPrefs.SetInt("GameSave_PlayerSkillPoints", GetPlayerSkillPoints());
        
        // 保存时间戳
        PlayerPrefs.SetString("GameSave_Timestamp", System.DateTime.Now.ToBinary().ToString());
        
        PlayerPrefs.Save();
        
        if (debugMode)
        {
            Debug.Log("[GameManager] 游戏已保存");
        }
    }
    
    /// <summary>
    /// 获取玩家魔法值
    /// </summary>
    public int GetPlayerMana()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var character = player.GetComponent<Character>();
            if (character != null)
            {
                return character.currentMana;
            }
        }
        return 0;
    }
    
    /// <summary>
    /// 增加玩家魔法值
    /// </summary>
    public void AddPlayerMana(int amount)
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var character = player.GetComponent<Character>();
            if (character != null)
            {
                character.RestoreMana(amount);
            }
        }
    }
}

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    MainMenu,
    CharacterSelect,
    Playing,
    Paused,
    GameOver,
    Victory
}