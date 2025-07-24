using UnityEngine;
using Fungus;

/// <summary>
/// 自定义Fungus命令 - 给予玩家物品
/// 用于在对话中给予玩家物品
/// </summary>
[CommandInfo("Custom", "Give Item", "给予玩家指定物品")]
[AddComponentMenu("")]
public class GiveItemCommand : Command
{
    [Header("物品配置")]
    [Tooltip("物品名称")]
    [SerializeField] protected string itemName = "";
    
    [Tooltip("物品数量")]
    [SerializeField] protected int quantity = 1;
    
    [Tooltip("是否显示获得物品的消息")]
    [SerializeField] protected bool showMessage = true;
    
    [Tooltip("获得物品的消息文本")]
    [SerializeField] protected string messageText = "获得了 {0} x{1}";
    
    public override void OnEnter()
    {
        // 查找玩家
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 这里需要根据您的物品系统实现
            // 假设玩家有Inventory组件
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                // inventory.AddItem(itemName, quantity);
                Debug.Log($"[GiveItemCommand] 给予玩家 {quantity} 个 {itemName}");
                
                // 显示消息
                if (showMessage && UIManager.Instance != null)
                {
                    string message = string.Format(messageText, itemName, quantity);
                    UIManager.Instance.ShowMessage(message);
                }
            }
            else
            {
                Debug.LogWarning("[GiveItemCommand] 玩家没有Inventory组件");
            }
        }
        else
        {
            Debug.LogWarning("[GiveItemCommand] 找不到玩家");
        }
        
        Continue();
    }
    
    public override string GetSummary()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return "给予物品: 未设置";
        }
        return $"给予 {quantity} 个 {itemName}";
    }
    
    public override Color GetButtonColor()
    {
        return new Color32(255, 215, 0, 255); // 金色
    }
}

/// <summary>
/// 自定义Fungus命令 - 给予玩家经验值
/// 用于在对话中给予玩家经验值
/// </summary>
[CommandInfo("Custom", "Give Experience", "给予玩家经验值")]
[AddComponentMenu("")]
public class GiveExperienceCommand : Command
{
    [Header("经验值配置")]
    [Tooltip("经验值数量")]
    [SerializeField] protected int experienceAmount = 100;
    
    [Tooltip("是否显示获得经验的消息")]
    [SerializeField] protected bool showMessage = true;
    
    [Tooltip("获得经验的消息文本")]
    [SerializeField] protected string messageText = "获得了 {0} 点经验值";
    
    public override void OnEnter()
    {
        // 通过GameManager给予经验
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerExperience(experienceAmount);
            
            // 显示消息
            if (showMessage && UIManager.Instance != null)
            {
                string message = string.Format(messageText, experienceAmount);
                UIManager.Instance.ShowMessage(message);
            }
            
            Debug.Log($"[GiveExperienceCommand] 给予玩家 {experienceAmount} 点经验值");
        }
        else
        {
            Debug.LogWarning("[GiveExperienceCommand] 找不到GameManager");
        }
        
        Continue();
    }
    
    public override string GetSummary()
    {
        return $"给予 {experienceAmount} 点经验值";
    }
    
    public override Color GetButtonColor()
    {
        return new Color32(0, 255, 127, 255); // 春绿色
    }
}

/// <summary>
/// 自定义Fungus命令 - 检查玩家等级
/// 用于在对话中检查玩家等级并执行相应逻辑
/// </summary>
[CommandInfo("Custom", "Check Player Level", "检查玩家等级")]
[AddComponentMenu("")]
public class CheckPlayerLevelCommand : Command
{
    [Header("等级检查配置")]
    [Tooltip("要检查的等级")]
    [SerializeField] protected int requiredLevel = 1;
    
    [Tooltip("比较类型")]
    [SerializeField] protected CompareType compareType = CompareType.GreaterThanOrEqual;
    
    [Tooltip("检查成功时跳转的Block")]
    [SerializeField] protected Block successBlock;
    
    [Tooltip("检查失败时跳转的Block")]
    [SerializeField] protected Block failBlock;
    
    public override void OnEnter()
    {
        bool checkResult = false;
        
        // 获取玩家等级
        if (GameManager.Instance != null)
        {
            int playerLevel = GameManager.Instance.GetPlayerLevel();
            
            // 根据比较类型进行检查
            switch (compareType)
            {
                case CompareType.Equal:
                    checkResult = playerLevel == requiredLevel;
                    break;
                case CompareType.NotEqual:
                    checkResult = playerLevel != requiredLevel;
                    break;
                case CompareType.GreaterThan:
                    checkResult = playerLevel > requiredLevel;
                    break;
                case CompareType.GreaterThanOrEqual:
                    checkResult = playerLevel >= requiredLevel;
                    break;
                case CompareType.LessThan:
                    checkResult = playerLevel < requiredLevel;
                    break;
                case CompareType.LessThanOrEqual:
                    checkResult = playerLevel <= requiredLevel;
                    break;
            }
            
            Debug.Log($"[CheckPlayerLevelCommand] 玩家等级: {playerLevel}, 要求: {compareType} {requiredLevel}, 结果: {checkResult}");
        }
        
        // 根据检查结果跳转
        if (checkResult && successBlock != null)
        {
           // Continue(successBlock.CommandList[0]);
        }
        else if (!checkResult && failBlock != null)
        {
          //  Continue(failBlock.CommandList[0]);
        }
        else
        {
            Continue();
        }
    }
    
    public override string GetSummary()
    {
        return $"检查玩家等级 {compareType} {requiredLevel}";
    }
    
    public override Color GetButtonColor()
    {
        return new Color32(135, 206, 235, 255); // 天蓝色
    }
}

/// <summary>
/// 自定义Fungus命令 - 播放音效
/// 用于在对话中播放特定音效
/// </summary>
[CommandInfo("Custom", "Play Sound Effect", "播放音效")]
[AddComponentMenu("")]
public class PlaySoundEffectCommand : Command
{
    [Header("音效配置")]
    [Tooltip("要播放的音效")]
    [SerializeField] protected AudioClip soundEffect;
    
    [Tooltip("音量")]
    [Range(0f, 1f)]
    [SerializeField] protected float volume = 1f;
    
    [Tooltip("是否等待音效播放完成")]
    [SerializeField] protected bool waitForCompletion = false;
    
    public override void OnEnter()
    {
        if (soundEffect != null)
        {
            // 通过AudioManager播放音效（如果有的话）
            // 或者直接使用AudioSource播放
            var audioSource = Camera.main?.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // 创建临时AudioSource
                var tempGO = new GameObject("TempAudioSource");
                audioSource = tempGO.AddComponent<AudioSource>();
                
                if (waitForCompletion)
                {
                    // 等待播放完成后销毁
                    Destroy(tempGO, soundEffect.length);
                }
                else
                {
                    // 立即销毁（音效会继续播放）
                    Destroy(tempGO, 0.1f);
                }
            }
            
            audioSource.PlayOneShot(soundEffect, volume);
            
            Debug.Log($"[PlaySoundEffectCommand] 播放音效: {soundEffect.name}");
            
            if (waitForCompletion)
            {
                // 等待音效播放完成
                Invoke("Continue", soundEffect.length);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[PlaySoundEffectCommand] 音效为空");
        }
        
        Continue();
    }
    
    public override string GetSummary()
    {
        if (soundEffect == null)
        {
            return "播放音效: 未设置";
        }
        return $"播放音效: {soundEffect.name}";
    }
    
    public override Color GetButtonColor()
    {
        return new Color32(255, 165, 0, 255); // 橙色
    }
}

/// <summary>
/// 比较类型枚举
/// </summary>
public enum CompareType
{
    Equal,              // 等于
    NotEqual,           // 不等于
    GreaterThan,        // 大于
    GreaterThanOrEqual, // 大于等于
    LessThan,           // 小于
    LessThanOrEqual     // 小于等于
}