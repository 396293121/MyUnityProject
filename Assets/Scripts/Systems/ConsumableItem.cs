using UnityEngine;
using System.Collections;

/// <summary>
/// 消耗品效果类型枚举
/// </summary>
public enum ConsumableEffectType
{
    InstantHeal,        // 瞬间治疗
    InstantMana,        // 瞬间回魔
    BuffStrength,       // 力量增益
    BuffAgility,        // 敏捷增益
    BuffStamina,        // 体力增益
    BuffIntelligence,   // 智力增益
    BuffSpeed,          // 速度增益
    BuffDefense,        // 防御增益
    Poison,             // 中毒
    Regeneration,       // 生命回复
    ManaRegeneration,   // 魔法回复
    Invisibility,       // 隐身
    Invincibility       // 无敌
}

/// <summary>
/// 消耗品类 - 继承自Item，实现各种消耗品效果
/// 从原Phaser项目的ConsumableItem.js迁移而来
/// </summary>
[System.Serializable]
public class ConsumableItem : Item
{
    [Header("消耗品属性")]
    public ConsumableEffectType effectType;  // 效果类型
    public float effectValue;                 // 效果数值
    public float duration;                    // 持续时间（秒）
    public bool isInstant = true;             // 是否为瞬间效果
    public float tickInterval = 1f;           // 持续效果的间隔时间
    
    [Header("视觉效果")]
    public GameObject useEffect;              // 使用时的特效
    public AudioClip useSound;                // 使用时的音效
    public Color effectColor = Color.white;   // 效果颜色
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ConsumableItem() : base()
    {
        itemType = ItemType.Consumable;
        isUsable = true;
        consumeOnUse = true;
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ConsumableItem(string id, string name, string desc, ConsumableEffectType effect, float value, float duration = 0f)
        : base(id, name, desc, ItemType.Consumable, ItemRarity.Common)
    {
        this.effectType = effect;
        this.effectValue = value;
        this.duration = duration;
        this.isInstant = duration <= 0f;
        this.isUsable = true;
        this.consumeOnUse = true;
    }
    
    /// <summary>
    /// 使用消耗品
    /// </summary>
    public override bool Use(Character user)
    {
        if (!base.Use(user))
        {
            return false;
        }
        
        // 播放使用音效
        if (useSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(useSound.name, 1f);
        }
        
        // 显示使用特效
        if (useEffect != null && user != null)
        {
            GameObject effect = Object.Instantiate(useEffect, user.transform.position, Quaternion.identity);
            Object.Destroy(effect, 3f);
        }
        
        // 应用效果
        ApplyEffect(user);
        
        return true;
    }
    
    /// <summary>
    /// 应用消耗品效果
    /// </summary>
    private void ApplyEffect(Character user)
    {
        if (user == null) return;
        
        switch (effectType)
        {
            case ConsumableEffectType.InstantHeal:
                ApplyInstantHeal(user);
                break;
            case ConsumableEffectType.InstantMana:
                ApplyInstantMana(user);
                break;
            case ConsumableEffectType.BuffStrength:
                ApplyStrengthBuff(user);
                break;
            case ConsumableEffectType.BuffAgility:
                ApplyAgilityBuff(user);
                break;
            case ConsumableEffectType.BuffStamina:
                ApplyStaminaBuff(user);
                break;
            case ConsumableEffectType.BuffIntelligence:
                ApplyIntelligenceBuff(user);
                break;
            case ConsumableEffectType.BuffSpeed:
                ApplySpeedBuff(user);
                break;
            case ConsumableEffectType.BuffDefense:
                ApplyDefenseBuff(user);
                break;
            case ConsumableEffectType.Poison:
                ApplyPoison(user);
                break;
            case ConsumableEffectType.Regeneration:
                ApplyRegeneration(user);
                break;
            case ConsumableEffectType.ManaRegeneration:
                ApplyManaRegeneration(user);
                break;
            case ConsumableEffectType.Invisibility:
                ApplyInvisibility(user);
                break;
            case ConsumableEffectType.Invincibility:
                ApplyInvincibility(user);
                break;
        }
        
        // 显示效果消息
        ShowEffectMessage(user);
    }
    
    /// <summary>
    /// 瞬间治疗
    /// </summary>
    private void ApplyInstantHeal(Character user)
    {
        int healAmount = Mathf.RoundToInt(effectValue);
        user.Heal(healAmount);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 使用 {itemName}，恢复 {healAmount} 生命值");
        }
    }
    
    /// <summary>
    /// 瞬间回魔
    /// </summary>
    private void ApplyInstantMana(Character user)
    {
        int manaAmount = Mathf.RoundToInt(effectValue);
        user.RestoreMana(manaAmount);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 使用 {itemName}，恢复 {manaAmount} 魔法值");
        }
    }
    
    /// <summary>
    /// 力量增益
    /// </summary>
    private void ApplyStrengthBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyStatBuff(user, "strength", effectValue, duration));
        }
    }
    
    /// <summary>
    /// 敏捷增益
    /// </summary>
    private void ApplyAgilityBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyStatBuff(user, "agility", effectValue, duration));
        }
    }
    
    /// <summary>
    /// 体力增益
    /// </summary>
    private void ApplyStaminaBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyStatBuff(user, "stamina", effectValue, duration));
        }
    }
    
    /// <summary>
    /// 智力增益
    /// </summary>
    private void ApplyIntelligenceBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyStatBuff(user, "intelligence", effectValue, duration));
        }
    }
    
    /// <summary>
    /// 速度增益
    /// </summary>
    private void ApplySpeedBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplySpeedBuffCoroutine(user, effectValue, duration));
        }
    }
    
    /// <summary>
    /// 防御增益
    /// </summary>
    private void ApplyDefenseBuff(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyDefenseBuffCoroutine(user, effectValue, duration));
        }
    }
    
    /// <summary>
    /// 中毒效果
    /// </summary>
    private void ApplyPoison(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyPoisonCoroutine(user, effectValue, duration));
        }
    }
    
    /// <summary>
    /// 生命回复
    /// </summary>
    private void ApplyRegeneration(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyRegenerationCoroutine(user, effectValue, duration));
        }
    }
    
    /// <summary>
    /// 魔法回复
    /// </summary>
    private void ApplyManaRegeneration(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyManaRegenerationCoroutine(user, effectValue, duration));
        }
    }
    
    /// <summary>
    /// 隐身效果
    /// </summary>
    private void ApplyInvisibility(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyInvisibilityCoroutine(user, duration));
        }
    }
    
    /// <summary>
    /// 无敌效果
    /// </summary>
    private void ApplyInvincibility(Character user)
    {
        if (user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(ApplyInvincibilityCoroutine(user, duration));
        }
    }
    
    /// <summary>
    /// 属性增益协程
    /// </summary>
    private IEnumerator ApplyStatBuff(Character user, string statName, float buffValue, float duration)
    {
        int buffAmount = Mathf.RoundToInt(buffValue);
        
        // 应用增益
        switch (statName)
        {
            case "strength":
                user.strength += buffAmount;
                break;
            case "agility":
                user.agility += buffAmount;
                break;
            case "stamina":
                user.stamina += buffAmount;
                break;
            case "intelligence":
                user.intelligence += buffAmount;
                break;
        }
        
        // 重新计算衍生属性
        user.CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 获得 {statName} +{buffAmount} 增益，持续 {duration} 秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        // 移除增益
        switch (statName)
        {
            case "strength":
                user.strength -= buffAmount;
                break;
            case "agility":
                user.agility -= buffAmount;
                break;
            case "stamina":
                user.stamina -= buffAmount;
                break;
            case "intelligence":
                user.intelligence -= buffAmount;
                break;
        }
        
        // 重新计算衍生属性
        user.CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的 {statName} 增益效果结束");
        }
    }
    
    /// <summary>
    /// 速度增益协程
    /// </summary>
    private IEnumerator ApplySpeedBuffCoroutine(Character user, float speedMultiplier, float duration)
    {
        float originalSpeed = user.speed;
        user.speed *= speedMultiplier;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 速度提升 {speedMultiplier}x，持续 {duration} 秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        user.speed = originalSpeed;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的速度增益效果结束");
        }
    }
    
    /// <summary>
    /// 防御增益协程
    /// </summary>
    private IEnumerator ApplyDefenseBuffCoroutine(Character user, float defenseBonus, float duration)
    {
        int defenseAmount = Mathf.RoundToInt(defenseBonus);
        user.defense += defenseAmount;
        user.magicDefense += defenseAmount;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 防御力提升 +{defenseAmount}，持续 {duration} 秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        user.defense -= defenseAmount;
        user.magicDefense -= defenseAmount;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的防御增益效果结束");
        }
    }
    
    /// <summary>
    /// 中毒协程
    /// </summary>
    private IEnumerator ApplyPoisonCoroutine(Character user, float damagePerTick, float duration)
    {
        float elapsed = 0f;
        int damage = Mathf.RoundToInt(damagePerTick);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 中毒，每 {tickInterval} 秒受到 {damage} 伤害，持续 {duration} 秒");
        }
        
        while (elapsed < duration && user.isAlive)
        {
            user.TakeDamage(damage, DamageType.Magical, Vector2.zero, null);
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的中毒效果结束");
        }
    }
    
    /// <summary>
    /// 生命回复协程
    /// </summary>
    private IEnumerator ApplyRegenerationCoroutine(Character user, float healPerTick, float duration)
    {
        float elapsed = 0f;
        int healAmount = Mathf.RoundToInt(healPerTick);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 获得生命回复，每 {tickInterval} 秒恢复 {healAmount} 生命值，持续 {duration} 秒");
        }
        
        while (elapsed < duration && user.isAlive)
        {
            user.Heal(healAmount);
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的生命回复效果结束");
        }
    }
    
    /// <summary>
    /// 魔法回复协程
    /// </summary>
    private IEnumerator ApplyManaRegenerationCoroutine(Character user, float manaPerTick, float duration)
    {
        float elapsed = 0f;
        int manaAmount = Mathf.RoundToInt(manaPerTick);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 获得魔法回复，每 {tickInterval} 秒恢复 {manaAmount} 魔法值，持续 {duration} 秒");
        }
        
        while (elapsed < duration && user.isAlive)
        {
            user.RestoreMana(manaAmount);
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的魔法回复效果结束");
        }
    }
    
    /// <summary>
    /// 隐身协程
    /// </summary>
    private IEnumerator ApplyInvisibilityCoroutine(Character user, float duration)
    {
        SpriteRenderer spriteRenderer = user.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        // 设置半透明
        if (spriteRenderer != null)
        {
            Color invisibleColor = originalColor;
            invisibleColor.a = 0.3f;
            spriteRenderer.color = invisibleColor;
        }
        
        // 设置隐身状态（如果有相关属性）
        // user.isInvisible = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 进入隐身状态，持续 {duration} 秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        // 恢复可见
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 移除隐身状态
        // user.isInvisible = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的隐身效果结束");
        }
    }
    
    /// <summary>
    /// 无敌协程
    /// </summary>
    private IEnumerator ApplyInvincibilityCoroutine(Character user, float duration)
    {
        // 设置无敌状态（如果有相关属性）
        // user.isInvincible = true;
        
        // 视觉效果：闪烁
        SpriteRenderer spriteRenderer = user.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && user is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(FlashEffect(spriteRenderer, duration));
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 进入无敌状态，持续 {duration} 秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        // 移除无敌状态
        // user.isInvincible = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[ConsumableItem] {user.name} 的无敌效果结束");
        }
    }
    
    /// <summary>
    /// 闪烁效果协程
    /// </summary>
    private IEnumerator FlashEffect(SpriteRenderer spriteRenderer, float duration)
    {
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            spriteRenderer.color = effectColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }
        
        spriteRenderer.color = originalColor;
    }
    
    /// <summary>
    /// 显示效果消息
    /// </summary>
    private void ShowEffectMessage(Character user)
    {
        string message = GetEffectMessage();
        if (!string.IsNullOrEmpty(message) && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"{user.name} {message}");
        }
    }
    
    /// <summary>
    /// 获取效果消息
    /// </summary>
    private string GetEffectMessage()
    {
        switch (effectType)
        {
            case ConsumableEffectType.InstantHeal:
                return $"恢复了 {effectValue} 生命值";
            case ConsumableEffectType.InstantMana:
                return $"恢复了 {effectValue} 魔法值";
            case ConsumableEffectType.BuffStrength:
                return $"力量提升 {effectValue} 点";
            case ConsumableEffectType.BuffAgility:
                return $"敏捷提升 {effectValue} 点";
            case ConsumableEffectType.BuffStamina:
                return $"体力提升 {effectValue} 点";
            case ConsumableEffectType.BuffIntelligence:
                return $"智力提升 {effectValue} 点";
            case ConsumableEffectType.BuffSpeed:
                return "速度得到提升";
            case ConsumableEffectType.BuffDefense:
                return "防御力得到提升";
            case ConsumableEffectType.Poison:
                return "中毒了";
            case ConsumableEffectType.Regeneration:
                return "获得生命回复效果";
            case ConsumableEffectType.ManaRegeneration:
                return "获得魔法回复效果";
            case ConsumableEffectType.Invisibility:
                return "进入隐身状态";
            case ConsumableEffectType.Invincibility:
                return "进入无敌状态";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// 获取完整描述
    /// </summary>
    public override string GetFullDescription()
    {
        string baseDesc = base.GetFullDescription();
        string effectDesc = GetEffectDescription();
        
        if (!string.IsNullOrEmpty(effectDesc))
        {
            baseDesc += $"\n\n<color=cyan>效果:</color> {effectDesc}";
        }
        
        if (cooldown > 0)
        {
            baseDesc += $"\n<color=grey>冷却时间: {cooldown} 秒</color>";
        }
        
        return baseDesc;
    }
    
    /// <summary>
    /// 获取效果描述
    /// </summary>
    private string GetEffectDescription()
    {
        switch (effectType)
        {
            case ConsumableEffectType.InstantHeal:
                return $"立即恢复 {effectValue} 生命值";
            case ConsumableEffectType.InstantMana:
                return $"立即恢复 {effectValue} 魔法值";
            case ConsumableEffectType.BuffStrength:
                return $"力量 +{effectValue}，持续 {duration} 秒";
            case ConsumableEffectType.BuffAgility:
                return $"敏捷 +{effectValue}，持续 {duration} 秒";
            case ConsumableEffectType.BuffStamina:
                return $"体力 +{effectValue}，持续 {duration} 秒";
            case ConsumableEffectType.BuffIntelligence:
                return $"智力 +{effectValue}，持续 {duration} 秒";
            case ConsumableEffectType.BuffSpeed:
                return $"速度提升 {effectValue}x，持续 {duration} 秒";
            case ConsumableEffectType.BuffDefense:
                return $"防御力 +{effectValue}，持续 {duration} 秒";
            case ConsumableEffectType.Poison:
                return $"每 {tickInterval} 秒造成 {effectValue} 伤害，持续 {duration} 秒";
            case ConsumableEffectType.Regeneration:
                return $"每 {tickInterval} 秒恢复 {effectValue} 生命值，持续 {duration} 秒";
            case ConsumableEffectType.ManaRegeneration:
                return $"每 {tickInterval} 秒恢复 {effectValue} 魔法值，持续 {duration} 秒";
            case ConsumableEffectType.Invisibility:
                return $"隐身 {duration} 秒";
            case ConsumableEffectType.Invincibility:
                return $"无敌 {duration} 秒";
            default:
                return "未知效果";
        }
    }
    
    /// <summary>
    /// 克隆消耗品
    /// </summary>
    public override Item Clone()
    {
        ConsumableItem clone = new ConsumableItem();
        CopyTo(clone);
        return clone;
    }
    
    /// <summary>
    /// 复制属性到另一个消耗品
    /// </summary>
    protected override void CopyTo(Item target)
    {
        base.CopyTo(target);
        
        if (target is ConsumableItem consumable)
        {
            consumable.effectType = effectType;
            consumable.effectValue = effectValue;
            consumable.duration = duration;
            consumable.isInstant = isInstant;
            consumable.tickInterval = tickInterval;
            consumable.useEffect = useEffect;
            consumable.useSound = useSound;
            consumable.effectColor = effectColor;
        }
    }
}