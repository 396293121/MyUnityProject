using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

/// <summary>
/// 存档系统 - 负责游戏数据的保存和加载
/// 从原Phaser项目的SaveSystem.js迁移而来
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    [Header("存档设置")]
    public int maxSaveSlots = 5;
    public bool enableAutoSave = true;
    public float autoSaveInterval = 300f; // 5分钟自动存档
    
    // 存档路径
    private string saveDirectory;
    private string saveFilePrefix = "GameSave_";
    private string saveFileExtension = ".json";
    
    // 自动存档计时器
    private float autoSaveTimer = 0f;
    
    // 事件
    public System.Action<int> OnGameSaved;
    public System.Action<int> OnGameLoaded;
    public System.Action<int> OnSaveDeleted;
    public System.Action<string> OnSaveError;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // 自动存档
        if (enableAutoSave && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// 初始化存档系统
    /// </summary>
    private void InitializeSaveSystem()
    {
        // 设置存档目录
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        
        // 创建存档目录（如果不存在）
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[SaveSystem] 存档系统初始化完成，存档目录: {saveDirectory}");
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveSystem] 无效的存档槽位: {slotIndex}");
            OnSaveError?.Invoke($"无效的存档槽位: {slotIndex}");
            return false;
        }
        
        try
        {
            // 收集游戏数据
            GameSaveData saveData = CollectGameData();
            
            // 序列化为JSON
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            // 写入文件
            string filePath = GetSaveFilePath(slotIndex);
            File.WriteAllText(filePath, jsonData);
            
            // 触发保存事件
            OnGameSaved?.Invoke(slotIndex);
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[SaveSystem] 游戏已保存到槽位 {slotIndex}");
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 保存游戏失败: {e.Message}");
            OnSaveError?.Invoke($"保存失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public bool LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveSystem] 无效的存档槽位: {slotIndex}");
            OnSaveError?.Invoke($"无效的存档槽位: {slotIndex}");
            return false;
        }
        
        string filePath = GetSaveFilePath(slotIndex);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveSystem] 存档文件不存在: {filePath}");
            OnSaveError?.Invoke($"存档槽位 {slotIndex} 为空");
            return false;
        }
        
        try
        {
            // 读取文件
            string jsonData = File.ReadAllText(filePath);
            
            // 反序列化
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            // 应用游戏数据
            ApplyGameData(saveData);
            
            // 触发加载事件
            OnGameLoaded?.Invoke(slotIndex);
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[SaveSystem] 游戏已从槽位 {slotIndex} 加载");
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 加载游戏失败: {e.Message}");
            OnSaveError?.Invoke($"加载失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSave(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveSystem] 无效的存档槽位: {slotIndex}");
            return false;
        }
        
        string filePath = GetSaveFilePath(slotIndex);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveSystem] 存档文件不存在: {filePath}");
            return false;
        }
        
        try
        {
            File.Delete(filePath);
            OnSaveDeleted?.Invoke(slotIndex);
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[SaveSystem] 已删除槽位 {slotIndex} 的存档");
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 删除存档失败: {e.Message}");
            OnSaveError?.Invoke($"删除失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查槽位是否有存档
    /// </summary>
    public bool HasSave(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            return false;
        }
        
        string filePath = GetSaveFilePath(slotIndex);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 获取存档元数据
    /// </summary>
    public SaveMetadata GetSaveMetadata(int slotIndex)
    {
        if (!HasSave(slotIndex))
        {
            return null;
        }
        
        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            string jsonData = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            FileInfo fileInfo = new FileInfo(filePath);
            
            return new SaveMetadata
            {
                slotIndex = slotIndex,
                playerName = saveData.playerName,
                playerLevel = saveData.playerLevel,
                currentScene = saveData.currentScene,
                playTime = saveData.playTime,
                saveTime = fileInfo.LastWriteTime,
                fileSize = fileInfo.Length
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 获取存档元数据失败: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有存档的元数据
    /// </summary>
    public List<SaveMetadata> GetAllSaveMetadata()
    {
        List<SaveMetadata> metadataList = new List<SaveMetadata>();
        
        for (int i = 0; i < maxSaveSlots; i++)
        {
            SaveMetadata metadata = GetSaveMetadata(i);
            metadataList.Add(metadata); // null也会被添加，表示空槽位
        }
        
        return metadataList;
    }
    
    /// <summary>
    /// 自动存档
    /// </summary>
    public void AutoSave()
    {
        // 使用最后一个槽位作为自动存档槽位
        int autoSaveSlot = maxSaveSlots - 1;
        
        if (SaveGame(autoSaveSlot))
        {
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[SaveSystem] 自动存档完成，槽位: {autoSaveSlot}");
            }
        }
    }
    
    /// <summary>
    /// 收集游戏数据
    /// </summary>
    private GameSaveData CollectGameData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 基础信息
        saveData.saveVersion = "1.0";
        saveData.saveTime = DateTime.Now.ToBinary();
        saveData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // 玩家数据
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Character playerCharacter = player.GetComponent<Character>();
            if (playerCharacter != null)
            {
                saveData.playerName = "Player"; // 可以从角色组件获取
                saveData.playerLevel = playerCharacter.level;
                saveData.playerExperience = playerCharacter.experience;
                saveData.playerHealth = playerCharacter.currentHealth;
                saveData.playerMana = playerCharacter.currentMana;
                
                // 玩家位置
                saveData.playerPosition = new SerializableVector3(player.transform.position);
                
                // 玩家属性
                saveData.playerStats = new PlayerStats
                {
                    strength = playerCharacter.strength,
                    agility = playerCharacter.agility,
                    stamina = playerCharacter.stamina,
                    intelligence = playerCharacter.intelligence
                };
            }
        }
        
        // 游戏设置
        if (AudioManager.Instance != null)
        {
            saveData.gameSettings = new GameSettings
            {
                masterVolume = AudioManager.Instance.masterVolume,
                musicVolume = AudioManager.Instance.musicVolume,
                sfxVolume = AudioManager.Instance.sfxVolume,
                voiceVolume = AudioManager.Instance.voiceVolume
            };
        }
        
        // 游戏统计
        saveData.gameStats = new GameStats
        {
            playTime = Time.time, // 这里应该使用实际的游戏时间
            enemiesKilled = 0, // 需要从游戏管理器获取
            itemsCollected = 0, // 需要从游戏管理器获取
            levelsCompleted = 0 // 需要从游戏管理器获取
        };
        
        return saveData;
    }
    
    /// <summary>
    /// 应用游戏数据
    /// </summary>
    private void ApplyGameData(GameSaveData saveData)
    {
        // 加载场景（如果不同）
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != saveData.currentScene)
        {
            // 这里应该通过GameManager来加载场景
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(saveData.currentScene);
            }
        }
        
        // 应用玩家数据
        StartCoroutine(ApplyPlayerDataAfterSceneLoad(saveData));
        
        // 应用游戏设置
        if (AudioManager.Instance != null && saveData.gameSettings != null)
        {
            AudioManager.Instance.SetMasterVolume(saveData.gameSettings.masterVolume);
            AudioManager.Instance.SetMusicVolume(saveData.gameSettings.musicVolume);
            AudioManager.Instance.SetSfxVolume(saveData.gameSettings.sfxVolume);
            AudioManager.Instance.SetVoiceVolume(saveData.gameSettings.voiceVolume);
        }
    }
    
    /// <summary>
    /// 场景加载后应用玩家数据
    /// </summary>
    private System.Collections.IEnumerator ApplyPlayerDataAfterSceneLoad(GameSaveData saveData)
    {
        // 等待场景完全加载
        yield return new WaitForSeconds(0.5f);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Character playerCharacter = player.GetComponent<Character>();
            if (playerCharacter != null)
            {
                // 恢复玩家属性
                playerCharacter.level = saveData.playerLevel;
                playerCharacter.experience = saveData.playerExperience;
                playerCharacter.currentHealth = saveData.playerHealth;
                playerCharacter.currentMana = saveData.playerMana;
                
                if (saveData.playerStats != null)
                {
                    playerCharacter.strength = saveData.playerStats.strength;
                    playerCharacter.agility = saveData.playerStats.agility;
                    playerCharacter.stamina = saveData.playerStats.stamina;
                    playerCharacter.intelligence = saveData.playerStats.intelligence;
                    
                    // 重新计算衍生属性
                    playerCharacter.CalculateDerivedStats();
                }
            }
            
            // 恢复玩家位置
            if (saveData.playerPosition != null)
            {
                player.transform.position = saveData.playerPosition.ToVector3();
            }
        }
    }
    
    /// <summary>
    /// 获取存档文件路径
    /// </summary>
    private string GetSaveFilePath(int slotIndex)
    {
        string fileName = saveFilePrefix + slotIndex.ToString("D2") + saveFileExtension;
        return Path.Combine(saveDirectory, fileName);
    }
}

/// <summary>
/// 游戏存档数据结构
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public string saveVersion;
    public long saveTime;
    public string currentScene;
    
    // 玩家数据
    public string playerName;
    public int playerLevel;
    public int playerExperience;
    public int playerHealth;
    public int playerMana;
    public SerializableVector3 playerPosition;
    public PlayerStats playerStats;
    
    // 游戏设置
    public GameSettings gameSettings;
    
    // 游戏统计
    public GameStats gameStats;
    
    // 其他数据可以根据需要添加
    public float playTime;
}

/// <summary>
/// 玩家属性数据
/// </summary>
[System.Serializable]
public class PlayerStats
{
    public int strength;
    public int agility;
    public int stamina;
    public int intelligence;
}

/// <summary>
/// 游戏设置数据
/// </summary>
[System.Serializable]
public class GameSettings
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float voiceVolume;
}

/// <summary>
/// 游戏统计数据
/// </summary>
[System.Serializable]
public class GameStats
{
    public float playTime;
    public int enemiesKilled;
    public int itemsCollected;
    public int levelsCompleted;
}

/// <summary>
/// 可序列化的Vector3
/// </summary>
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;
    
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

/// <summary>
/// 存档元数据
/// </summary>
public class SaveMetadata
{
    public int slotIndex;
    public string playerName;
    public int playerLevel;
    public string currentScene;
    public float playTime;
    public DateTime saveTime;
    public long fileSize;
}