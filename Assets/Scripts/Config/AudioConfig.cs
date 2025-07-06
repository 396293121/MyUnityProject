using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音效配置文件
/// 管理游戏中所有角色和敌人的音效
/// 从原Phaser项目的AudioConfig.js迁移而来
/// </summary>
[System.Serializable]
public class SoundConfig
{
    [Header("音效基础信息")]
    public string key;
    public AudioClip audioClip;
    
    [Header("音效参数")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    public bool loop = false;
    
    [Header("时间配置")]
    public TimingConfig timing;
    
    [Header("播放限制")]
    public float duration = 0f; // 强制停止时间（毫秒），0表示播放完整音效
    public float minInterval = 100f; // 防止重复播放的最小间隔（毫秒）
}

[System.Serializable]
public class TimingConfig
{
    [Header("触发时机")]
    public int frame = 1; // 在动画的第几帧触发
    public float delay = 0f; // 延迟播放时间（毫秒）
}

[System.Serializable]
public class CharacterSoundConfig
{
    [Header("攻击音效")]
    public List<SoundConfig> attackSounds;
    
    [Header("移动音效")]
    public List<SoundConfig> movementSounds;
    
    [Header("受伤音效")]
    public List<SoundConfig> damageSounds;
    
    /// <summary>
    /// 根据动作类型和音效类型获取音效配置
    /// </summary>
    /// <param name="actionType">动作类型（attack, movement, damage）</param>
    /// <param name="soundType">音效类型（swing, hit, jump, hurt, die等）</param>
    /// <returns>音效配置</returns>
    public SoundConfig GetSoundConfig(string actionType, string soundType)
    {
        List<SoundConfig> soundList = null;
        
        switch (actionType.ToLower())
        {
            case "attack":
                soundList = attackSounds;
                break;
            case "movement":
                soundList = movementSounds;
                break;
            case "damage":
                soundList = damageSounds;
                break;
        }
        
        return soundList?.Find(sound => sound.key.Contains(soundType));
    }
}

[System.Serializable]
public class GlobalAudioConfig
{
    [Header("全局音量设置")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
    
    [Header("并发限制")]
    public int maxConcurrentSounds = 3; // 同一音效最大并发数量
    public int maxTotalSounds = 32; // 总音效最大数量
    
    [Header("音频混合器")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup voiceMixerGroup;
}

/// <summary>
/// 音频配置管理器
/// 提供统一的音频配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game Config/Audio Config")]
public class AudioConfigSO : ScriptableObject
{
    [Header("全局音频配置")]
    public GlobalAudioConfig globalConfig;
    
    [Header("角色音效配置")]
    public CharacterSoundConfig warriorSounds;
    public CharacterSoundConfig mageSounds;
    public CharacterSoundConfig archerSounds;
    
    [Header("敌人音效配置")]
    public CharacterSoundConfig wildBoarSounds;
    
    [Header("背景音乐配置")]
    public List<BackgroundMusicConfig> backgroundMusic;
    
    [Header("UI音效配置")]
    public List<SoundConfig> uiSounds;
    
    /// <summary>
    /// 获取角色音效配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>角色音效配置</returns>
    public CharacterSoundConfig GetCharacterSoundConfig(string characterType)
    {
        switch (characterType.ToLower())
        {
            case "warrior":
                return warriorSounds;
            case "mage":
                return mageSounds;
            case "archer":
                return archerSounds;
            case "wild_boar":
                return wildBoarSounds;
            default:
                Debug.LogWarning($"未找到角色类型 {characterType} 的音效配置");
                return null;
        }
    }
    
    /// <summary>
    /// 获取角色音效
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <param name="actionType">动作类型</param>
    /// <param name="soundType">音效类型</param>
    /// <returns>音效配置</returns>
    public SoundConfig GetCharacterSound(string characterType, string actionType, string soundType)
    {
        var characterConfig = GetCharacterSoundConfig(characterType);
        return characterConfig?.GetSoundConfig(actionType, soundType);
    }
    
    /// <summary>
    /// 获取背景音乐配置
    /// </summary>
    /// <param name="musicKey">音乐键名</param>
    /// <returns>背景音乐配置</returns>
    public BackgroundMusicConfig GetBackgroundMusic(string musicKey)
    {
        return backgroundMusic?.Find(music => music.key == musicKey);
    }
    
    /// <summary>
    /// 获取UI音效配置
    /// </summary>
    /// <param name="soundKey">音效键名</param>
    /// <returns>UI音效配置</returns>
    public SoundConfig GetUISound(string soundKey)
    {
        return uiSounds?.Find(sound => sound.key == soundKey);
    }
}

[System.Serializable]
public class BackgroundMusicConfig
{
    [Header("音乐基础信息")]
    public string key;
    public string name;
    public AudioClip audioClip;
    
    [Header("音乐参数")]
    [Range(0f, 1f)]
    public float volume = 0.7f;
    public bool loop = true;
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;
    
    [Header("使用场景")]
    public List<string> scenes; // 适用的场景列表
}