using System;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 角色基类 - 所有角色的基础类
/// 从原Phaser项目的Character.js迁移而来
/// </summary>
[ShowOdinSerializedPropertiesInInspector]

public abstract class Character : MonoBehaviour, IDamageable
{public enum CharacterClass
    {
        Warrior,
        Mage,
        Archer,
        Cleric
    }
    [Header("角色职业类型")]   
      protected CharacterClass characterClass=CharacterClass.Warrior;

    [Header("是否显示调试Gizmos")]
    public bool showDebugGizmos = true;
    
    [LabelText("角色音频类型")]
    public  AudioCategory audioCategory=AudioCategory.Warrior;
    //正在攻击用于调试框
    private float showDebugATTACKINGTime = 0;
        public CharacterClass CHARACTERCLASS { get { return characterClass; } }

    private bool showDebugAttacking => Time.time < showDebugATTACKINGTime;
    [FoldoutGroup("等级系统", expanded: true)]
    [LabelText("角色等级")]
    [PropertyRange(1, 100)]
    [InfoBox("角色当前等级，影响基础属性和技能威力")]
    public int level = 1;

    [FoldoutGroup("等级系统")]
    [LabelText("当前经验值")]
    [ProgressBar(0, "experienceToNext", ColorGetter = "GetExpBarColor")]
    [InfoBox("当前获得的经验值，达到上限时自动升级")]
    public int experience = 0;

    [FoldoutGroup("等级系统")]
    [LabelText("升级所需经验")]
    [ReadOnly]
    [InfoBox("升级到下一级所需的经验值总量")]
    public int experienceToNext = 100;

    // 兼容性属性
    public int currentExperience => experience;
    public int maxExperience => experienceToNext;

    [FoldoutGroup("生命与魔法", expanded: true)]
    [HorizontalGroup("生命与魔法/生命值")]
    [LabelText("最大生命值")]
    [PropertyRange(50, 1000)]
    [SuffixLabel("HP")]
    [InfoBox("角色的最大生命值，升级时会增加")]
    public int maxHealth = 100;

    [HorizontalGroup("生命与魔法/生命值")]
    [LabelText("当前生命值")]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    [InfoBox("角色当前的生命值，归零时死亡")]
    public int currentHealth = 100;

    [HorizontalGroup("生命与魔法/魔法值")]
    [LabelText("最大魔法值")]
    [PropertyRange(20, 500)]
    [SuffixLabel("MP")]
    [InfoBox("角色的最大魔法值，用于释放技能")]
    public int maxMana = 50;

    [HorizontalGroup("生命与魔法/魔法值")]
    [LabelText("当前魔法值")]
    [ReadOnly]
    [InfoBox("角色当前的魔法值，释放技能时消耗")]
    public int currentMana = 50;

    [FoldoutGroup("核心属性", expanded: true)]
    [HorizontalGroup("核心属性/基础属性")]
    [VerticalGroup("核心属性/基础属性/左列")]
    [LabelText("力量")]
    [PropertyRange(1, 100)]
    [ReadOnly]
    [SuffixLabel("STR")]
    [InfoBox("影响物理攻击力和重击伤害")]
    public int strength = 10;

    [VerticalGroup("核心属性/基础属性/左列")]
    [LabelText("敏捷")]
    [PropertyRange(1, 100)]
    [SuffixLabel("AGI")]
    [ReadOnly]
    [InfoBox("影响移动速度、攻击速度和闪避率")]
    public int agility = 10;

    [VerticalGroup("核心属性/基础属性/右列")]
    [LabelText("体力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STA")]
    [ReadOnly]
    [InfoBox("影响生命值上限和物理防御力")]
    public int stamina = 10;

    [VerticalGroup("核心属性/基础属性/右列")]
    [LabelText("智力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("INT")]
    [ReadOnly]
    [InfoBox("影响魔法攻击力、魔法值上限和魔法防御力")]
    public int intelligence = 10;

    [FoldoutGroup("衍生属性", expanded: false)]
    [HorizontalGroup("衍生属性/攻击力")]
    [VerticalGroup("衍生属性/攻击力/左列")]
    [LabelText("物理攻击力")]
    [ReadOnly]
    [SuffixLabel("ATK")]
    [InfoBox("基于力量和敏捷计算的物理伤害值")]
    public int physicalAttack;

    [VerticalGroup("衍生属性/攻击力/右列")]
    [LabelText("魔法攻击力")]
    [ReadOnly]
    [SuffixLabel("MAG")]
    [InfoBox("基于智力和力量计算的魔法伤害值")]
    public int magicalAttack;

    [HorizontalGroup("衍生属性/防御力")]
    [VerticalGroup("衍生属性/防御力/左列")]
    [LabelText("物理防御力")]
    [ReadOnly]
    [SuffixLabel("DEF")]
    [InfoBox("基于体力和力量计算的物理防御值")]
    public int defense;

    [VerticalGroup("衍生属性/防御力/右列")]
    [LabelText("魔法防御力")]
    [ReadOnly]
    [SuffixLabel("MDEF")]
    [InfoBox("基于智力和体力计算的魔法防御值")]
    public int magicDefense;

    [FoldoutGroup("衍生属性")]
    [LabelText("移动速度")]
    [ReadOnly]
    [SuffixLabel("SPD")]
    [InfoBox("基于敏捷计算的移动速度")]
    public float speed = 5f;

    [FoldoutGroup("衍生属性")]
    [LabelText("显示速度")]
    [ReadOnly]
    [SuffixLabel("SPD")]
    [InfoBox("基于敏捷计算的移动速度")]
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    [FoldoutGroup("状态控制", expanded: false)]
    [HorizontalGroup("状态控制/基础状态")]
    [VerticalGroup("状态控制/基础状态/左列")]
    [LabelText("存活状态")]
    [ReadOnly]
    [InfoBox("角色是否存活，死亡时为false")]
    public bool isAlive = true;



    [FoldoutGroup("状态控制")]
    [LabelText("交互范围")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("米")]
    [InfoBox("角色与物品、NPC等的交互范围")]
    public float interactionRange = 2f;

    [FoldoutGroup("组件引用", expanded: false)]
    [HorizontalGroup("组件引用/核心组件")]
    [VerticalGroup("组件引用/核心组件/左列")]
    [LabelText("刚体组件")]
    [ReadOnly]
    [InfoBox("用于物理移动和碰撞检测")]
    protected Rigidbody2D rb;
    public Rigidbody2D Rigidbody2D { get { return rb; } }
    protected new Collider2D collider2D;
    public Collider2D Collider2D { get { return collider2D; } }
    [VerticalGroup("组件引用/核心组件/右列")]
    [LabelText("动画控制器")]
    [ReadOnly]
    [InfoBox("控制角色动画播放")]
    protected Animator animator;

    public Animator Animator { get { return animator; } }
    [FoldoutGroup("组件引用")]
    [LabelText("精灵渲染器")]
    [ReadOnly]
    [InfoBox("渲染角色贴图")]
    protected SpriteRenderer spriteRenderer;

    [FoldoutGroup("攻击配置", expanded: false)]
    [HorizontalGroup("攻击配置/基础攻击")]
    [VerticalGroup("攻击配置/基础攻击/左列")]
    [LabelText("攻击范围")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("米")]
    [ReadOnly]
    [InfoBox("角色的基础攻击范围")]
    [SerializeField] protected float attackRange = 2f;

    [VerticalGroup("攻击配置/基础攻击/右列")]
    [LabelText("攻击高度")]
    [PropertyRange(0.5f, 5f)]
    [SuffixLabel("米")]
    [ReadOnly]
    [InfoBox("攻击判定的垂直范围")]
    [SerializeField] protected float attackHeight = 2f;

    [HorizontalGroup("攻击配置/击退与检测")]
    [VerticalGroup("攻击配置/击退与检测/左列")]
    [LabelText("击退力度")]
    [PropertyRange(0f, 20f)]
    [SuffixLabel("力")]
    [ReadOnly]
    [InfoBox("攻击时对敌人的击退力度")]
    [SerializeField] protected float knockbackForce = 5f;

    [FoldoutGroup("攻击配置")]
    [LabelText("攻击点")]
    [InfoBox("攻击判定的起始位置，如果为空则使用角色中心点")]
    [SerializeField] protected Transform attackPoint;

    // 攻击相关属性的公共访问器
    public virtual float AttackRange => attackRange;
    public virtual float AttackHeight => attackHeight;
    public virtual float KnockbackForce => knockbackForce;
    public virtual Transform AttackPoint => attackPoint;

    // IDamageable接口实现
    public bool IsAlive => isAlive;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // 事件系统
    public System.Action<int, int> OnHealthChanged;
    public System.Action<int, int> OnManaChanged;
    public System.Action<int, int> OnExpChanged;
    public System.Action<int> OnLevelUp;
    public System.Action OnDeath;
    public System.Action OnRevive;
    protected virtual void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化属性
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
    /// 受到伤害 - IDamageable接口实现
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="hitPoint">命中点</param>
    /// <param name="attacker">攻击者</param>
    public virtual void TakeDamage(int damage, DamageType damageType = DamageType.Physical, Vector2 hitPoint = default, Character attacker = null)
    {
        if (!isAlive) 
        {
            Debug.Log($"[{gameObject.name}] 已死亡，忽略伤害");
            return;
        }

        // 计算实际伤害
        int actualDamage = damageType == DamageType.Magical ?
            Mathf.Max(1, damage - magicDefense) :
            Mathf.Max(1, damage - defense);
        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        // 检查死亡 - 在显示伤害数字前检查
        bool willDie = currentHealth <= 0;

        // 只有在角色还活着或者刚死亡时才显示伤害数字
        if (isAlive || willDie)
        {
              Vector3 damageNumberPosition = transform.position;
        
            if (collider2D != null)
            {
                damageNumberPosition += Vector3.up * collider2D.bounds.size.y;
            }
        DamagePopup popup = DamagePool.Instance.GetPopup();
        popup.Setup(damageNumberPosition, actualDamage, damageType);

         
        }
        
        OnHealthChanged?.Invoke(oldHealth, currentHealth);
        
        // 处理死亡
        if (willDie && isAlive)
        {
            Die();
            return;
        }

        Debug.Log($"[{gameObject.name}] 受到 {actualDamage} 点{damageType}伤害，剩余生命值: {currentHealth}");
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (!isAlive) return;
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(oldHealth, currentHealth);

        Debug.Log($"[{gameObject.name}] 恢复 {amount} 点生命值，当前生命值: {currentHealth}");
    }

    /// <summary>
    /// 消耗魔法值
    /// </summary>
    public virtual bool ConsumeMana(int amount)
    {
        if (currentMana < amount) return false;
        int oldMana = currentMana;
        currentMana -= amount;
        OnManaChanged?.Invoke(oldMana, currentMana);

        return true;
    }

    /// <summary>
    /// 恢复魔法值
    /// </summary>
    public virtual void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        OnManaChanged?.Invoke(oldMana, currentMana);
    }

    /// <summary>
    /// 获得经验值
    /// </summary>
    public virtual void GainExperience(int exp)
    {
        experience += exp;
        OnExpChanged?.Invoke(experienceToNext, experience);
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
        if (!isAlive) return; // 防止重复死亡
        
        isAlive = false;
        

        OnDeath?.Invoke();

        Debug.Log($"[{gameObject.name}] 死亡");
    }

    /// <summary>
    /// 复活
    /// </summary>
    public virtual void Revive()
    {
        isAlive = true;
        currentHealth = maxHealth;
        currentMana = maxMana;
        int oldHealth = currentHealth;
        int oldMana = currentMana;
        OnHealthChanged?.Invoke(oldHealth, currentHealth);
        OnManaChanged?.Invoke(oldMana, currentMana);

        Debug.Log($"[{gameObject.name}] 复活");
    }

    /// <summary>
    /// 检测并对敌人造成伤害
    /// </summary>
    public virtual void DetectAndDamageEnemies(Action onAttackHit)
    {

        // 获取角色朝向
        bool facingRight = GetFacingDirection();

        // 根据角色朝向确定攻击方向
        Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;

        // 计算攻击判定区域的中心点
        Vector2 attackCenter;
        if (attackPoint != null)
        {
            attackCenter = (Vector2)attackPoint.position + attackDirection * (attackRange / 2);
        }
        else
        {
            attackCenter = (Vector2)transform.position + attackDirection * (attackRange / 2);
        }

        // 定义矩形攻击范围
        Vector2 boxSize = new Vector2(attackRange, attackHeight);

        // 检测攻击范围内的所有碰撞体 图层默认敌人
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(attackCenter, boxSize, 0f, LayerMask.GetMask("Enemy"));
        int enemiesHit = 0;

        // 遍历所有被攻击的目标
        foreach (Collider2D target in hitTargets)
        {
            if (IsValidTarget(target))
            {
                enemiesHit++;

                // 播放命中音效
                onAttackHit?.Invoke();
                Debug.Log($"攻击命中目标: {target.name} (伤害: {physicalAttack})");

                // 获取IDamageable组件
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // 对目标造成伤害
                    DealDamageToTarget(damageable, target.transform.position);
                }

                // 应用击退效果
                Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
                    ApplyKnockbackToTarget(targetRb, knockbackDirection);
                }
            }
        }

        if (enemiesHit > 0)
        {
            Debug.Log($"本次攻击命中 {enemiesHit} 个目标");
        }
        showDebugATTACKINGTime = Time.time + 0.5f;
    }

    /// <summary>
    /// 获取角色朝向（子类需要重写）
    /// </summary>
    /// <returns>true表示朝右，false表示朝左</returns>
    public virtual bool GetFacingDirection()
    {
        return transform.localScale.x > 0;
    }

    /// <summary>
    /// 判断是否为有效攻击目标（子类可重写）
    /// </summary>
    /// <param name="target">目标碰撞体</param>
    /// <returns>是否为有效目标</returns>
    protected virtual bool IsValidTarget(Collider2D target)
    {
        return target.CompareTag("Enemy");
    }
    /// <summary>
    /// 对目标造成伤害（子类可重写）
    /// </summary>
    /// <param name="target">目标碰撞体</param>
    /// <param name="hitPoint">命中点</param>
    public virtual void DealDamageToTarget(IDamageable target, Vector2 hitPoint)
    {
        //默认物理
        target.TakeDamage(physicalAttack, DamageType.Physical, hitPoint, this);
    }

    /// <summary>
    /// 对目标应用击退效果
    /// </summary>
    /// <param name="targetRb">目标刚体</param>
    /// <param name="direction">击退方向</param>
    protected virtual void ApplyKnockbackToTarget(Rigidbody2D targetRb, Vector2 direction)
    {
        if (targetRb != null)
        {
            // 应用击退力
            Vector2 knockback = direction.normalized * knockbackForce;
            targetRb.AddForce(knockback, ForceMode2D.Impulse);

            Debug.Log($"对 {targetRb.name} 应用击退力: {knockbackForce}");
        }
    }

    public virtual void InitializeWithConfig(string selectedCharacterType)
    {

    }

    #region Odin颜色获取方法
    private Color GetExpBarColor()
    {
        float ratio = (float)experience / experienceToNext;
        return Color.Lerp(Color.yellow, Color.green, ratio);
    }

    private Color GetHealthBarColor()
    {
        float ratio = (float)currentHealth / maxHealth;
        if (ratio > 0.6f) return Color.green;
        if (ratio > 0.3f) return Color.yellow;
        return Color.red;
    }
    /// <summary>
    /// 调试可视化
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (showDebugGizmos)
        {
            // 基础攻击范围
            Gizmos.color = showDebugAttacking ? Color.red : Color.green;
            Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 basicAttackCenter = (Vector2)attackPoint.position + attackDirection * (AttackRange / 2);
            Vector3 basicAttackSize = new Vector3(AttackRange, AttackHeight, 0);
            Gizmos.DrawWireCube(basicAttackCenter, basicAttackSize);





            // 角色朝向
            Gizmos.color = Color.white;
            Vector3 facingDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(transform.position, facingDirection * 0.8f);



            // 交互范围
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
    #endregion
}