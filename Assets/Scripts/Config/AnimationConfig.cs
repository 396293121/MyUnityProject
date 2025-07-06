using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画配置文件
/// 定义所有角色的动画配置信息
/// 从原Phaser项目的AnimationConfig.js迁移而来
/// </summary>
[System.Serializable]
public class AnimationConfig
{
    [Header("标准尺寸配置")]
    public Vector2 standardSize;
    
    [Header("锚点配置")]
    public Vector2 anchorPoint;
    
    [Header("物理体配置")]
    public PhysicsBodyConfig physicsBody;
    
    [Header("增强动画系统配置")]
    public EnhancedAnimationConfig enhancedAnimation;
    
    [Header("动画配置")]
    public List<AnimationData> animations;
}

[System.Serializable]
public class PhysicsBodyConfig
{
    [Header("物理体尺寸")]
    public float width;
    public float height;
    
    [Header("物理体偏移")]
    public float offsetX;
    public float offsetY;
}

[System.Serializable]
public class EnhancedAnimationConfig
{
    [Header("跳跃动画配置")]
    public JumpAnimationConfig jump;
    
    [Header("攻击动画配置")]
    public AttackAnimationConfig attack;
    
    [Header("技能动画配置")]
    public SkillAnimationConfig heavySlash;
    public SkillAnimationConfig whirlwind;
    public SkillAnimationConfig battleCry;
}

[System.Serializable]
public class JumpAnimationConfig
{
    [Header("速度阈值")]
    public float risingThreshold = -50f;
    public float fallingThreshold = 50f;
    
    [Header("帧分布")]
    public int risingEndFrame = 10;
    public int fallingStartFrame = 11;
    public int fallingEndFrame = 14;
    
    [Header("帧率倍数")]
    public float risingFrameRateMultiplier = 1.0f;
    public float fallingFrameRateMultiplier = 0.8f;
    
    [Header("动画设置")]
    public bool holdLastFrame = true;
}

[System.Serializable]
public class AttackAnimationConfig
{
    [Header("关键帧")]
    public int keyFrameNumber = 7;
    
    [Header("攻击框配置")]
    public HitboxConfig hitbox;
}

[System.Serializable]
public class SkillAnimationConfig
{
    [Header("关键帧")]
    public int keyFrameNumber;
    
    [Header("攻击框配置")]
    public HitboxConfig hitbox;
}

[System.Serializable]
public class HitboxConfig
{
    [Header("攻击框尺寸")]
    public float width;
    public float height;
    
    [Header("攻击框偏移")]
    public float offsetX;
    public float offsetY;
}

[System.Serializable]
public class AnimationData
{
    [Header("动画基础信息")]
    public string key;
    public string name;
    
    [Header("动画参数")]
    public float frameRate = 10f;
    public bool loop = false;
    public int startFrame = 1;
    public int endFrame = 1;
    
    [Header("精灵图配置")]
    public string spritePrefix;
    public string spriteSuffix = ".png";
    public int zeroPadding = 6;
}

/// <summary>
/// 角色动画配置管理器
/// 提供统一的动画配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "CharacterAnimationConfig", menuName = "Game Config/Character Animation Config")]
public class CharacterAnimationConfigSO : ScriptableObject
{
    [Header("战士动画配置")]
    public AnimationConfig warriorConfig;
    
    [Header("法师动画配置")]
    public AnimationConfig mageConfig;
    
    [Header("射手动画配置")]
    public AnimationConfig archerConfig;
    
    /// <summary>
    /// 获取角色动画配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>动画配置</returns>
    public AnimationConfig GetCharacterConfig(string characterType)
    {
        switch (characterType.ToLower())
        {
            case "warrior":
                return warriorConfig;
            case "mage":
                return mageConfig;
            case "archer":
                return archerConfig;
            default:
                Debug.LogWarning($"未找到角色类型 {characterType} 的动画配置");
                return null;
        }
    }
    
    /// <summary>
    /// 获取角色物理体配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>物理体配置</returns>
    public PhysicsBodyConfig GetPhysicsBodyConfig(string characterType)
    {
        var config = GetCharacterConfig(characterType);
        return config?.physicsBody;
    }
    
    /// <summary>
    /// 获取角色增强动画配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>增强动画配置</returns>
    public EnhancedAnimationConfig GetEnhancedAnimationConfig(string characterType)
    {
        var config = GetCharacterConfig(characterType);
        return config?.enhancedAnimation;
    }
    
    /// <summary>
    /// 获取特定动画数据
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <param name="animationKey">动画键名</param>
    /// <returns>动画数据</returns>
    public AnimationData GetAnimationData(string characterType, string animationKey)
    {
        var config = GetCharacterConfig(characterType);
        if (config?.animations != null)
        {
            return config.animations.Find(anim => anim.key == animationKey);
        }
        return null;
    }
}