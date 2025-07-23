using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 法师职业类 - 高魔攻高魔防但生命值较低的远程魔法角色
/// 从原Phaser项目的Mage.js迁移而来
/// </summary>
public class Mage : Character
{
    [Header("法师配置")]
    // [SerializeField, Header("法师配置文件")]
    // [InfoBox("配置法师的所有属性和技能参数")]
    public MageConfig config;

    [Header("法师恢复量")]
    private float manaRegenRate = 2f;
    protected override void ApplyKnockbackToTarget(Rigidbody2D targetRb, Vector2 direction)
    {
        // 法师的击退效果带有魔法特效
        Vector2 knockback = direction.normalized * KnockbackForce;
        targetRb.AddForce(knockback, ForceMode2D.Impulse);

        // 可以在这里添加魔法击退特效
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师魔法击退，力度: {KnockbackForce}");
        }
    }



    
    // 魔法值自动回复
    private float manaRegenTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        characterClass=CharacterClass.Mage;
        // 法师特有属性设置
        strength = config != null ? config.strength : 5;
        agility = config != null ? config.agility : 10;
        stamina = config != null ? config.stamina : 8;
        intelligence = config != null ? config.intelligence : 15;
        maxMana = config != null ? config.maxMana : 100;
        maxHealth = config != null ? config.maxHealth : 100;

        // 重新计算衍生属性
        CalculateDerivedStats();
        //
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师创建完成 - 魔法攻击力: {magicalAttack}, 魔法防御力: {magicDefense}");
        }
    }


    public override void CalculateDerivedStats()
    {
        base.CalculateDerivedStats();

        // 法师额外加成
        magicalAttack += 8;     // 额外魔法攻击力
        magicDefense += 5;    // 额外魔法防御力
        maxHealth -= 10;      // 生命值较低
        maxMana += 20;        // 额外魔法值

        // 确保当前生命值和魔法值不超过最大值
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    protected override void Update()
    {
        base.Update();

        // 处理法师特有的更新逻辑
        HandleManaRegeneration();
        HandleMageInput();
    }

    /// <summary>
    /// 处理魔法值自动回复
    /// </summary>
    private void HandleManaRegeneration()
    {
        if (!isAlive || currentMana >= maxMana) return;

        manaRegenTimer += Time.deltaTime;
        if (manaRegenTimer >= 1f) // 每秒回复
        {
            float regenRate = config != null ? config.manaRegenRate : manaRegenRate;
            RestoreMana(Mathf.RoundToInt(regenRate));
            manaRegenTimer = 0f;
        }
    }

    /// <summary>
    /// 处理法师特有输入
    /// </summary>
    private void HandleMageInput()
    {
        if (!isAlive) return;

        // 检查技能输入
        if (InputManager.Instance != null)
        {
            // 这里可以添加法师特有的输入处理
            // 例如：火球术、闪电术、治疗术、传送术等技能的快捷键
        }
    }












}

