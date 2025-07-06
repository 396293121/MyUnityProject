using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 测试场景配置 ScriptableObject
/// 基于Phaser项目TestScene.js的功能需求设计
/// </summary>
[CreateAssetMenu(fileName = "TestSceneConfig", menuName = "Game/Test Scene Config")]
public class TestSceneConfig : ScriptableObject
{
    [Header("场景基础设置")]
    [Tooltip("场景名称")]
    public string sceneName = "TestScene";
    
    [Tooltip("场景描述")]
    public string sceneDescription = "测试场景 - 包含战士角色移动攻击和野猪怪物交互";
    
    [Header("地图配置")]
    [Tooltip("地图预制体")]
    public GameObject mapPrefab;
    
    [Tooltip("地图大小")]
    public Vector2 mapSize = new Vector2(1920, 1080);
    
    [Tooltip("地图背景色")]
    public Color backgroundColor = new Color(0.2f, 0.3f, 0.4f, 1f);
    
    [Header("玩家配置")]
    [Tooltip("玩家生成点")]
    public Vector3 playerSpawnPoint = Vector3.zero;
    
    [Tooltip("支持的角色类型")]
    public List<CharacterTypeConfig> supportedCharacters = new List<CharacterTypeConfig>();
    
    [Header("敌人配置")]
    [Tooltip("敌人生成配置")]
    public List<EnemySpawnConfig> enemySpawns = new List<EnemySpawnConfig>();
    
    [Tooltip("敌人波次配置")]
    public List<EnemyWaveConfig> enemyWaves = new List<EnemyWaveConfig>();
    
    [Header("音频配置")]
    [Tooltip("音频系统配置")]
    public GlobalAudioConfig audioConfig;
    
    [Tooltip("背景音乐")]
    public AudioClip backgroundMusic;
    
    [Tooltip("音效配置")]
    public List<SoundEffectConfig> soundEffects = new List<SoundEffectConfig>();
    
    [Header("UI配置")]
    [Tooltip("HUD UI预制体")]
    public GameObject hudUIPrefab;
    
    [Tooltip("暂停菜单预制体")]
    public GameObject pauseMenuPrefab;
    
    [Tooltip("游戏结束UI预制体")]
    public GameObject gameOverUIPrefab;
    
    [Header("摄像机配置")]
    [Tooltip("摄像机跟随设置")]
    public CameraFollowConfig cameraConfig;
    
    [Header("调试设置")]
    [Tooltip("启用调试模式")]
    public bool enableDebugMode = false;
    
    [Tooltip("显示调试UI")]
    public bool showDebugUI = false;
    
    [Tooltip("调试信息显示位置")]
    public Vector2 debugUIPosition = new Vector2(10, 10);
}

/// <summary>
/// 角色类型配置
/// </summary>
[System.Serializable]
public class CharacterTypeConfig
{
    [Tooltip("角色类型名称")]
    public string characterType;
    
    [Tooltip("角色预制体")]
    public GameObject characterPrefab;
    
    [Tooltip("角色配置")]
    public CharacterConfig characterConfig;
    
    [Tooltip("角色动画控制器")]
    public RuntimeAnimatorController animatorController;
}

// EnemySpawnConfig已在LevelConfig.cs中定义，此处移除重复定义

/// <summary>
/// 敌人波次配置
/// </summary>
[System.Serializable]
public class EnemyWaveConfig
{
    [Tooltip("波次名称")]
    public string waveName;
    
    [Tooltip("波次延迟时间")]
    public float waveDelay = 0f;
    
    [Tooltip("波次中的敌人")]
    public List<EnemySpawnConfig> enemies = new List<EnemySpawnConfig>();
    
    [Tooltip("是否为默认波次")]
    public bool isDefaultWave = false;
}

/// <summary>
/// 音效配置
/// </summary>
[System.Serializable]
public class SoundEffectConfig
{
    [Tooltip("音效名称")]
    public string soundName;
    
    [Tooltip("音效文件")]
    public AudioClip audioClip;
    
    [Tooltip("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("音调")]
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    
    [Tooltip("是否循环播放")]
    public bool loop = false;
    
    [Tooltip("是否为3D音效")]
    public bool is3D = false;
    
    [Tooltip("最小距离")]
    public float minDistance = 1f;
    
    [Tooltip("最大距离")]
    public float maxDistance = 500f;
    
    [Tooltip("冷却时间")]
    public float cooldownTime = 0f;
}

/// <summary>
/// 摄像机跟随配置
/// </summary>
[System.Serializable]
public class CameraFollowConfig
{
    [Tooltip("跟随速度")]
    public float followSpeed = 5f;
    
    [Tooltip("偏移量")]
    public Vector3 offset = Vector3.zero;
    
    [Tooltip("边界限制")]
    public bool useBounds = false;
    
    [Tooltip("边界范围")]
    public Bounds cameraBounds;
    
    [Tooltip("平滑跟随")]
    public bool smoothFollow = true;
}