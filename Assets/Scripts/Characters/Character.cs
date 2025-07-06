using UnityEngine;

/// <summary>
/// 角色基类 - 所有角色的基础类
/// 从原Phaser项目的Character.js迁移而来
/// </summary>
public abstract class Character : MonoBehaviour
{
    [Header("基础属性")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNext = 100;
    
    // 兼容性属性
    public int currentExperience => experience;
    public int maxExperience => experienceToNext;
    
    [Header("生命值和魔法值")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int maxMana = 50;
    public int currentMana = 50;
    
    [Header("核心属性")]
    public int strength = 10;     // 力量
    public int agility = 10;      // 敏捷
    public int stamina = 10;      // 体力
    public int intelligence = 10; // 智力
    
    [Header("衍生属性")]
    public int physicalAttack;    // 物理攻击力
    public int magicalAttack;     // 魔法攻击力
    public float speed = 5f;      // 移动速度
    public int defense;           // 防御力
    public int magicDefense;      // 魔法防御力
    
    [Header("状态")]
    public bool isAlive = true;
    public bool canMove = true;
    public bool canAttack = true;
    
    [Header("组件引用")]
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    
    // 事件系统
    public System.Action<int> OnHealthChanged;
    public System.Action<int> OnManaChanged;
    public System.Action<int> OnLevelUp;
    public System.Action OnDeath;
    
    protected virtual void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化属性
        InitializeStats();
    }
    
    protected virtual void Start()
    {
        // 计算衍生属性
        CalculateDerivedStats();
    }
    
    protected virtual void Update()
    {
        // 基类Update方法，子类可以重写
    }
    
    /// <summary>
    /// 初始化基础属性
    /// </summary>
    protected virtual void InitializeStats()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }
    
    /// <summary>
    /// 计算衍生属性
    /// </summary>
    public virtual void CalculateDerivedStats()
    {
        // 物理攻击力 = 力量 * 2 + 敏捷
        physicalAttack = strength * 2 + agility;
        
        // 魔法攻击力 = 智力 * 2 + 力量 / 2
        magicalAttack = intelligence * 2 + strength / 2;
        
        // 防御力 = 体力 * 1.5 + 力量 / 2
        defense = Mathf.RoundToInt(stamina * 1.5f + strength * 0.5f);
        
        // 魔法防御力 = 智力 + 体力 / 2
        magicDefense = intelligence + stamina / 2;
        
        // 移动速度基于敏捷
        speed = 3f + agility * 0.1f;
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int damage, bool isMagical = false)
    {
        if (!isAlive) return;
        
        // 计算实际伤害
        int actualDamage = isMagical ? 
            Mathf.Max(1, damage - magicDefense) : 
            Mathf.Max(1, damage - defense);
        
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        OnHealthChanged?.Invoke(currentHealth);
        
        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        
        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        
        Debug.Log($"[{gameObject.name}] 受到 {actualDamage} 点伤害，剩余生命值: {currentHealth}");
    }
    
    /// <summary>
    /// 恢复生命值
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (!isAlive) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"[{gameObject.name}] 恢复 {amount} 点生命值，当前生命值: {currentHealth}");
    }
    
    /// <summary>
    /// 消耗魔法值
    /// </summary>
    public virtual bool ConsumeMana(int amount)
    {
        if (currentMana < amount) return false;
        
        currentMana -= amount;
        OnManaChanged?.Invoke(currentMana);
        
        return true;
    }
    
    /// <summary>
    /// 恢复魔法值
    /// </summary>
    public virtual void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        OnManaChanged?.Invoke(currentMana);
    }
    
    /// <summary>
    /// 获得经验值
    /// </summary>
    public virtual void GainExperience(int exp)
    {
        experience += exp;
        
        // 检查升级
        while (experience >= experienceToNext)
        {
            LevelUp();
        }
        
        Debug.Log($"[{gameObject.name}] 获得 {exp} 点经验值，当前经验: {experience}/{experienceToNext}");
    }
    
    /// <summary>
    /// 增加经验值（别名方法）
    /// </summary>
    public virtual void AddExperience(int exp)
    {
        GainExperience(exp);
    }
    
    /// <summary>
    /// 获取下一级所需经验值
    /// </summary>
    public virtual int GetExperienceForNextLevel()
    {
        return experienceToNext;
    }
    
    /// <summary>
    /// 获取指定等级所需的经验值
    /// </summary>
    public virtual int GetExperienceForLevel(int targetLevel)
    {
        if (targetLevel <= level) return 0;
        
        int totalExp = experienceToNext;
        for (int i = level + 1; i < targetLevel; i++)
        {
            totalExp = Mathf.RoundToInt(totalExp * 1.2f);
        }
        return totalExp;
    }
    
    /// <summary>
    /// 获取当前等级的经验值
    /// </summary>
    public virtual int GetCurrentLevelExperience()
    {
        return experience;
    }
    
    /// <summary>
    /// 升级
    /// </summary>
    protected virtual void LevelUp()
    {
        experience -= experienceToNext;
        level++;
        
        // 提升属性
        maxHealth += 10;
        maxMana += 5;
        strength += 2;
        agility += 2;
        stamina += 2;
        intelligence += 2;
        
        // 恢复生命值和魔法值
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        // 计算下一级所需经验
        experienceToNext = Mathf.RoundToInt(experienceToNext * 1.2f);
        
        OnLevelUp?.Invoke(level);
        
        Debug.Log($"[{gameObject.name}] 升级到 {level} 级！");
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;
        canMove = false;
        canAttack = false;
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        OnDeath?.Invoke();
        
        Debug.Log($"[{gameObject.name}] 死亡");
    }
    
    /// <summary>
    /// 复活
    /// </summary>
    public virtual void Revive()
    {
        isAlive = true;
        canMove = true;
        canAttack = true;
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        OnHealthChanged?.Invoke(currentHealth);
        OnManaChanged?.Invoke(currentMana);
        
        Debug.Log($"[{gameObject.name}] 复活");
    }
}