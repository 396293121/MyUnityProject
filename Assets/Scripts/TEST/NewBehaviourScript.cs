using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System;

[ShowOdinSerializedPropertiesInInspector]
public class SimplePlayerController : MonoBehaviour
{
    #region 角色属性配置
    
   
    private float _baseMoveSpeed = 5f;
    private float _speedMultiplier = 1f;
     [TabGroup("角色配置", "移动设置")]
    [FoldoutGroup("角色配置/移动设置/基础移动", expanded: true)]
    [LabelText("移动速度")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色的移动速度")]
    public float moveSpeed => _baseMoveSpeed * _speedMultiplier;
    
    [FoldoutGroup("角色配置/移动设置/基础移动")]
    [LabelText("跳跃力度")]
    [Range(5f, 30f)]
    [Tooltip("角色跳跃时施加的向上力度")]
    public float jumpForce = 10f;
    
    [TabGroup("角色配置", "战斗设置")]
    [FoldoutGroup("角色配置/战斗设置/生命值", expanded: true)]
    [LabelText("最大生命值")]
    [Range(50f, 500f)]
    [Tooltip("角色的最大生命值")]
    public float maxHealth = 100f;
    
   
    private float _baseAttackDamage = 20f;
    private float _attackMultiplier = 1f;
     [FoldoutGroup("角色配置/战斗设置/攻击属性", expanded: true)]
    [LabelText("攻击伤害")]
    [ShowInInspector, ReadOnly]
    [Tooltip("每次攻击造成的伤害值")]
    public float attackDamage => _baseAttackDamage * _attackMultiplier;
    
    [VerticalGroup("角色配置/战斗设置/攻击属性")]
    [LabelText("攻击宽度")]
    [Range(0.5f, 5f)]
    [Tooltip("攻击判定的宽度范围")]
    public float attackWidth = 2f;
    
    [VerticalGroup("角色配置/战斗设置/攻击属性")]
    [LabelText("攻击高度")]
    [Range(3f, 10f)]
    [Tooltip("攻击判定的高度范围")]
    public float attackHeight = 7f;
    
    [VerticalGroup("角色配置/战斗设置/攻击属性")]
    [LabelText("攻击距离")]
    [Range(2f, 8f)]
    [Tooltip("攻击向前延伸的距离")]
    public float attackRange = 5f;
    
    [VerticalGroup("角色配置/战斗设置/攻击属性")]
    [LabelText("攻击冷却时间")]
    [Range(0.1f, 5f)]
    [Tooltip("两次攻击之间的最小间隔时间")]
    public float attackCooldown = 1f;
    
    [VerticalGroup("角色配置/战斗设置/防御属性")]
    [LabelText("无敌时间")]
    [Range(0.1f, 999f)]
    [Tooltip("受伤后的无敌保护时间")]
    public float invincibilityTime = 0.5f;
    
    [VerticalGroup("角色配置/战斗设置/防御属性")]
    [LabelText("击退力度")]
    [Range(1f, 20f)]
    [Tooltip("攻击敌人时的击退力度")]
    public float knockbackForce = 5f;
    
    #endregion
    
    #region 组件引用
    
    [TabGroup("角色配置", "组件引用")]
    [FoldoutGroup("角色配置/组件引用/核心组件", expanded: true)]
    [LabelText("刚体组件")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色的2D刚体组件")]
    private Rigidbody2D rb;
    
    [FoldoutGroup("角色配置/组件引用/核心组件")]
    [LabelText("动画控制器")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色的动画控制器组件")]
    private Animator animator;
    
    #endregion
    
    #region 运行时状态
    
    [TabGroup("运行状态", "生命状态")]
    [FoldoutGroup("运行状态/生命状态/当前状态", expanded: true)]
    [LabelText("当前生命值")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    [Tooltip("角色当前的生命值")]
    private float currentHealth;
    
    [TabGroup("运行状态", "行为状态")]
    [FoldoutGroup("运行状态/行为状态/移动状态", expanded: true)]
    [LabelText("是否在地面")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否接触地面")]
    private bool isGrounded;
    
    [FoldoutGroup("运行状态/行为状态/战斗状态", expanded: true)]
    [LabelText("正在攻击")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否正在执行攻击动作")]
    private bool isAttacking = false;
    
    [FoldoutGroup("运行状态/行为状态/战斗状态")]
    [LabelText("受伤状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否处于受伤状态")]
    private bool isHurt = false;
    [FoldoutGroup("运行状态/行为状态/战斗状态")]
    [LabelText("技能状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否正在执行技能")]
    private bool isSkill = false;
    [FoldoutGroup("运行状态/行为状态/战斗状态")]
    [LabelText("死亡状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否已死亡")]
    private bool isDead = false;
    
    [FoldoutGroup("运行状态/行为状态/战斗状态")]
    [LabelText("无敌状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否处于无敌状态")]
    private bool isInvincible = false;
    
    [FoldoutGroup("运行状态/行为状态/战斗状态")]
    [LabelText("上次攻击时间")]
    [ShowInInspector, ReadOnly]
    [Tooltip("上次执行攻击的时间戳")]
    private float lastAttackTime = 0f;
    
    #endregion
    
    #region 调试和工具
    
    [TabGroup("调试工具", "状态控制")]
    [FoldoutGroup("调试工具/状态控制/生命值操作", expanded: true)]
    [Button("完全治疗", ButtonSizes.Medium)]
    [Tooltip("将角色生命值恢复到最大值")]
    private void DebugFullHeal()
    {
        if (Application.isPlaying)
        {
            currentHealth = maxHealth;
            Debug.Log("角色已完全治疗！");
        }
    }
    
    [FoldoutGroup("调试工具/状态控制/生命值操作")]
    [Button("受到伤害 (10点)", ButtonSizes.Medium)]
    [Tooltip("让角色受到10点伤害用于测试")]
    private void DebugTakeDamage()
    {
        if (Application.isPlaying)
        {
            TakeDamage(10f);
        }
    }
    
    [TabGroup("调试工具", "状态重置")]
    [FoldoutGroup("调试工具/状态重置/角色重置", expanded: true)]
    [Button("重置角色状态", ButtonSizes.Large)]
    [Tooltip("重置角色到初始状态")]
    private void DebugResetCharacter()
    {
        if (Application.isPlaying)
        {
            currentHealth = maxHealth;
            isDead = false;
            isHurt = false;
            isAttacking = false;
            isInvincible = false;
            rb.velocity = Vector2.zero;
            Debug.Log("角色状态已重置！");
        }
    }
    
    [TabGroup("调试工具", "信息显示")]
    [FoldoutGroup("调试工具/信息显示/当前状态", expanded: true)]
    [ShowInInspector, ReadOnly]
    [LabelText("生命值百分比")]
    [ProgressBar(0, 100, ColorGetter = "GetHealthBarColor")]
    private float HealthPercentage => isDead ? 0 : (currentHealth / maxHealth) * 100f;
    
    [FoldoutGroup("调试工具/信息显示/当前状态")]
    [ShowInInspector, ReadOnly]
    [LabelText("角色状态摘要")]
    private string CharacterStatusSummary => $"生命: {currentHealth:F1}/{maxHealth} | 地面: {isGrounded} | 攻击: {isAttacking} | 受伤: {isHurt} | 死亡: {isDead}";
    
    /// <summary>
    /// 获取生命值进度条的颜色
    /// </summary>
    private Color GetHealthBarColor()
    {
        if (isDead) return Color.black;
        
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    #endregion
    
    #region Unity生命周期方法
    
    /// <summary>
    /// 初始化角色组件和状态
    /// </summary>
    void Start()
    {
        // 获取必要的组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // 初始化角色状态
        currentHealth = maxHealth;
        
        Debug.Log($"角色初始化完成 - 生命值: {currentHealth}/{maxHealth}");
        GameEventsTest.onSkill.AddListener(OnSkill);

    }
    private void OnSkill()
    {
        isSkill = true;
        Debug.Log("事件系统技能事件，999");
    }
    /// <summary>
    /// 每帧更新角色状态和处理输入
    /// </summary>
    private void Update()
    {
        // 死亡状态下停止所有操作
        if (isDead) 
        {
            return;
        }
        if (isSkill)
        {
            return;
        }
        // 更新动画控制器参数
        UpdateAnimatorParameters();
        
        // 根据当前状态处理角色行为
        if (!isHurt && !isAttacking)
        {
            HandleMovement();  // 处理移动输入
            HandleJump();      // 处理跳跃输入
        }
        
        // 始终检测攻击输入（即使在移动中）
        HandleAttack();
    }
    
    /// <summary>
    /// 更新动画控制器的参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isHurt", isHurt);
            // 统一在此处更新速度参数
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("VerticalSpeed", rb.velocity.y);
        }
    }
    
    #endregion
    
    #region 角色行为控制方法
    
    /// <summary>
    /// 处理角色的水平移动和朝向
    /// </summary>
    void HandleMovement()
    {
        // 获取水平输入轴（A/D键或左右箭头键）
        float moveInput = Input.GetAxis("Horizontal");
        
        // 应用移动速度到刚体，保持垂直速度不变
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        
        // 移动速度参数已在UpdateAnimatorParameters中统一更新
        
        // 根据移动方向翻转角色朝向
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);  // 面向右侧
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 面向左侧
        }
    }
    
    /// <summary>
    /// 处理角色跳跃逻辑
    /// </summary>
    void HandleJump()
    {
        // 检测跳跃输入（空格键）且角色在地面上
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 应用跳跃力度，保持水平速度不变
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // 触发跳跃动画
            animator.SetTrigger("Jump");
            
            // 播放跳跃音效
            AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.jumpSound);
            
            Debug.Log("角色执行跳跃");
        }
    }
    
    /// <summary>
    /// 处理攻击输入检测
    /// </summary>
    void HandleAttack()
    {
        // 检测攻击输入（A键），确保冷却时间已过且未在攻击中
        bool attackInput = Input.GetKeyDown(KeyCode.A);
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;
        
        // 添加更严格的状态检查，确保不在受伤、死亡或已在攻击状态时触发攻击
        if (attackInput && canAttack && !isAttacking && !isHurt && !isDead)
        {
            Debug.Log("检测到攻击输入");
            PerformAttack();
        }
    }
    
    #endregion

    #region 动画配置
    
    [TabGroup("角色配置", "动画设置")]
    [FoldoutGroup("角色配置/动画设置/攻击动画", expanded: true)]
    [LabelText("攻击动画名称")]
    [Tooltip("攻击动画在Animator中的状态名称")]
    public string attackAnimationName = "Attack";
    
    [FoldoutGroup("角色配置/动画设置/攻击动画")]
    [LabelText("伤害触发状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("防止在同一次攻击中重复触发伤害")]
    private bool damageTriggered = false;
    
    #endregion
    
    #region 攻击系统
    
    /// <summary>
    /// 执行攻击的主协程，处理攻击状态和动画
    /// </summary>
    /// <returns>协程迭代器</returns>
    void  PerformAttack()
    {
        // 设置攻击状态
        isAttacking = true;
        lastAttackTime = Time.time;
        damageTriggered = false;
        
        Debug.Log("开始攻击序列");
        
        // 触发攻击动画
        animator.SetTrigger("Attack");
        AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.swordSwingSound);

        // 等待动画事件结束攻击状态（主要依赖Animation Event）
        
        // 备用结束逻辑（防止Animation Event失效）
        // 只有在Animation Event未正确触发时才执行
    }
    

    /// <summary>
    /// Animation Event调用：在攻击动画的伤害帧触发
    /// </summary>
    public void OnAttackDamageFrame()
    {
        if (!damageTriggered && isAttacking)
        {
            DetectAndDamageEnemies();
            damageTriggered = true;
            Debug.Log("攻击伤害触发 - Animation Event");
        }
    }
    
    /// <summary>
    /// Animation Event调用：在攻击动画结束时触发
    /// </summary>
    public void OnAttackEnd()
    {
        EndAttack();
        Debug.Log("攻击动画结束 - Animation Event");
    }
    
    /// <summary>
    /// 结束攻击状态，重置相关标志
    /// </summary>
    private void EndAttack()
    {
        // 防止重复调用
        if (!isAttacking)
        {
            Debug.Log("EndAttack被重复调用，攻击状态已为false");
            return;
        }
        
        isAttacking = false;
        damageTriggered = false;
        
        Debug.Log("攻击状态已重置");
    }
    
    #endregion
    
    #region 战斗伤害系统
    
    /// <summary>
    /// 检测攻击范围内的敌人并造成伤害
    /// </summary>
    void DetectAndDamageEnemies()
    {
        // 根据角色朝向确定攻击方向
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        
        // 计算攻击判定区域的中心点
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * (attackRange / 2);
        
        // 定义矩形攻击范围
        Vector2 boxSize = new Vector2(attackRange, attackHeight);
        
        // 检测攻击范围内的所有碰撞体
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(attackCenter, boxSize, 0f);
        
        int enemiesHit = 0;
        
        // 遍历所有被攻击的目标
        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("Enemy"))
            {
                enemiesHit++;
                AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.swordHitSound);
                Debug.Log($"攻击命中敌人: {target.name} (伤害: {attackDamage})");
                
                // 调用敌人的受伤方法（如果存在）
                var enemyController = target.GetComponent<SimpleEnemy>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(attackDamage);
                }
                
                // 应用击退效果
                ApplyKnockback(target);
            }
        }
        
        if (enemiesHit > 0)
        {
            Debug.Log($"本次攻击命中 {enemiesHit} 个敌人");
        }
    }
    
    /// <summary>
    /// 对目标应用击退效果
    /// </summary>
    /// <param name="target">被击退的目标</param>
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // 计算击退方向（从玩家指向敌人）
            Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
            
            // 应用击退力
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log($"对 {target.name} 应用击退力: {knockbackForce}");
        }
    }
    
    #endregion
    
    #region 生命值管理系统
    
    /// <summary>
    /// 角色受到伤害的处理方法
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(float damage)
    {
        // 检查角色是否处于死亡状态、无敌状态、技能攻击状态或正在攻击
        if (isDead || isInvincible||isSkill||isAttacking) 
        {
            Debug.Log($"伤害被忽略 - 死亡状态: {isDead}, 无敌状态: {isInvincible},技能攻击状态:{isSkill},正在攻击:{isAttacking}");
            return;
        }
        
        // 计算实际伤害并更新生命值
        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"玩家受到伤害: {actualDamage:F1}, 剩余生命值: {currentHealth:F1}/{maxHealth:F1}");
        
        // 触发受伤事件通知其他系统
        if (GameEventsTest.OnPlayerTakeDamage != null)
            GameEventsTest.OnPlayerTakeDamage.Invoke(actualDamage);
        
        // 根据生命值状态决定后续行为
        if (currentHealth <= 0)
        {
            Die(); // 生命值归零，执行死亡逻辑
        }
        else
        {
            HurtState(); // 进入受伤状态
        }
    }
    
    /// <summary>
    /// 进入受伤状态，播放动画和音效
    /// </summary>
    void HurtState()
    {
        isHurt = true;
        isInvincible = true;
        
        // 触发受伤动画
        animator.SetTrigger("Hurt");
        
        // 播放受伤音效
        if (AudioManagerTest.Instance != null)
        {
            AudioManagerTest.Instance.PlaySound(AudioManagerTest.Instance.damageSound);
        }
        
        Debug.Log("角色进入受伤状态");
    }
        /// <summary>
    /// Animation Event调用：受伤动画结束时触发
    /// </summary>
    public void OnHurtEnd()
    {
        Debug.Log("受伤动画结束");
        
        // 结束受伤状态
        isHurt = false;
        
        // 启动无敌时间倒计时
        StartCoroutine(EndInvincibilityAfterDelay(invincibilityTime));
    }
        public void onSkillEnd()
    {
        Debug.Log("技能动画结束");
        isSkill = false;
        
    }
    public void onFallEnd()
    {
        Debug.Log("角色落地最后一帧");
        animator.SetBool("fallingOver", true);
    }
    /// <summary>
    /// 延迟结束无敌状态的协程
    /// </summary>
    /// <param name="delay">无敌持续时间</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator EndInvincibilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isInvincible = false;
        Debug.Log($"无敌状态结束，持续时间: {delay:F1}秒");
    }
    /// <summary>
    /// 角色死亡处理方法
    /// </summary>
    void Die()
    {
        // 设置死亡状态
        isDead = true;
        isHurt = false;
        isAttacking = false;
        isInvincible = false;
        
        Debug.Log("玩家死亡!");
            // 触发死亡动画
        animator.SetTrigger("isDead");
        // 停止所有物理运动
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // 防止死亡后继续受物理影响
        
    
        
        // 触发死亡事件（如果需要通知其他系统）
        // GameEvents.OnPlayerDeath?.Invoke();
        
        // 开始死亡后续处理
        StartCoroutine(HandleDeath());
    }
    
    /// <summary>
    /// 死亡后续处理协程
    /// </summary>
    /// <returns>协程迭代器</returns>
    IEnumerator HandleDeath()
    {
        // 等待死亡动画播放完成
        yield return new WaitForSeconds(2f);
        
        Debug.Log("死亡处理完成，可以重生或重新开始游戏");
        
        // 这里可以添加以下逻辑：
        // - 显示重生界面
        // - 自动重生
        // - 返回主菜单
        // - 重新加载关卡
    }
    
    #endregion
    
    #region 碰撞检测系统
    
    [TabGroup("运行状态", "碰撞状态")]
    [BoxGroup("运行状态/碰撞状态/伤害计时")]
    [LabelText("上次受伤时间")]
    [ShowInInspector, ReadOnly]
    [Tooltip("上次受到伤害的时间戳")]
    private float lastDamageTime = 0f;
    
    [BoxGroup("运行状态/碰撞状态/伤害计时")]
    [LabelText("无敌状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("角色是否处于无敌状态（用于碰撞检测）")]
    private bool isInvulnerable => isInvincible;
    
    /// <summary>
    /// 触发器进入事件处理
    /// </summary>
    /// <param name="other">进入的碰撞体</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"触发器检测到: {other.gameObject.name}, 标签: {other.gameObject.tag}");

        
        // 敌人接触伤害
        if (other.CompareTag("Enemy") && !isInvulnerable && !isDead)
        {
            float contactDamage = 10f; // 接触伤害值
            Debug.Log($"与敌人 {other.name} 接触，造成 {contactDamage} 点伤害");
            if(!isAttacking)
            {
                TakeDamage(contactDamage);
            }
        }
        
        // 可收集物品检测
        // if (other.CompareTag("Collectible"))
        // {
        //     Debug.Log($"检测到可收集物品: {other.name}");
        //     // 这里可以添加收集物品的逻辑
        // }
        
        // // 危险区域检测
        // if (other.CompareTag("Hazard") && !isInvulnerable && !isDead)
        // {
        //     float hazardDamage = 25f;
        //     Debug.Log($"进入危险区域 {other.name}，造成 {hazardDamage} 点伤害");
        //     TakeDamage(hazardDamage);
        // }
    }
    
    /// <summary>
    /// 触发器持续接触事件处理
    /// </summary>
    /// <param name="other">持续接触的碰撞体</param>
    void OnTriggerStay2D(Collider2D other)
    {
        // 敌人持续伤害区域
        if (other.CompareTag("Enemy") && !isInvulnerable && !isDead)
        {
            float continuousDamage = 5f; // 持续伤害值
            float damageInterval = 1f;   // 伤害间隔
            
            // 检查是否到达下次伤害时间
            if (Time.time - lastDamageTime >= damageInterval)
            {
                Debug.Log($"受到 {other.name} 的持续伤害: {continuousDamage}");
                TakeDamage(continuousDamage);
                lastDamageTime = Time.time;
            }
        }
        
        // 毒气区域持续伤害
        // if (other.CompareTag("PoisonZone") && !isInvulnerable && !isDead)
        // {
        //     float poisonDamage = 3f;
        //     float poisonInterval = 0.5f;
            
        //     if (Time.time - lastDamageTime >= poisonInterval)
        //     {
        //         Debug.Log($"受到毒气伤害: {poisonDamage}");
        //         TakeDamage(poisonDamage);
        //         lastDamageTime = Time.time;
        //     }
        // }
    }
    
    /// <summary>
    /// 碰撞进入事件处理
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 地面碰撞检测（物理碰撞）
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            this.animator.SetBool("fallingOver", false);
            // 计算碰撞力度
            float impactForce = collision.relativeVelocity.magnitude;
            Debug.Log($"角色通过物理碰撞着地 - 地面: {collision.gameObject.name}, 冲击力: {impactForce:F1}");
            
            // 如果冲击力过大，可以造成坠落伤害
            if (impactForce > 50f && !isDead)
            {
                float fallDamage = (impactForce - 50f) * 2f;
                Debug.Log($"坠落伤害: {fallDamage:F1}");
                TakeDamage(fallDamage);
            }
        }
        
        // 敌人物理碰撞
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"玩家与敌人发生物理碰撞: {collision.gameObject.name}");
            // 可以添加击退效果等物理反应
            
            // 计算击退方向
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
        }
        
        // 墙壁碰撞检测
        // if (collision.gameObject.CompareTag("Wall"))
        // {
        //     Debug.Log($"角色撞到墙壁: {collision.gameObject.name}");
        // }
    }
    
    /// <summary>
    /// 触发器退出事件处理
    /// </summary>
    /// <param name="other">退出的碰撞体</param>
    // void OnTriggerExit2D(Collider2D other)
    // {
    //     // 离开地面检测
    //     if (other.CompareTag("Ground"))
    //     {
    //         isGrounded = false;
    //         Debug.Log($"角色离开地面 - {other.name}");
    //     }
        
    //     // 离开安全区域
    //     // if (other.CompareTag("SafeZone"))
    //     // {
    //     //     Debug.Log($"离开安全区域: {other.name}");
    //     // }
    // }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log($"角色离开地面 - {collision.gameObject.name}");
        }
    }
    #endregion
    
    #region 公共接口方法
    
    /// <summary>
    /// 获取当前生命值
    /// </summary>
    /// <returns>当前生命值</returns>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// 获取最大生命值
    /// </summary>
    /// <returns>最大生命值</returns>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// 获取生命值百分比
    /// </summary>
    /// <returns>生命值百分比 (0-1)</returns>
    public float GetHealthPercentage()
    {
        return isDead ? 0f : currentHealth / maxHealth;
    }
    
    /// <summary>
    /// 治疗角色
    /// </summary>
    /// <param name="healAmount">治疗量</param>
    public void Heal(float healAmount)
    {
        if (isDead) 
        {
            Debug.Log("无法治疗已死亡的角色");
            return;
        }
        
        float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
        currentHealth += actualHeal;
        
        Debug.Log($"玩家治疗: {actualHeal:F1}, 当前生命值: {currentHealth:F1}/{maxHealth:F1}");
        
        // 可以在这里触发治疗事件
        // GameEvents.OnPlayerHeal?.Invoke(actualHeal);
    }
    
    /// <summary>
    /// 角色重生
    /// </summary>
    /// <param name="respawnPosition">重生位置</param>
    public void Respawn(Vector3 respawnPosition)
    {
        // 重置位置
        transform.position = respawnPosition;
        
        // 重置生命值和状态
        currentHealth = maxHealth;
        isDead = false;
        isHurt = false;
        isAttacking = false;
        isInvincible = false;
        
        // 重置物理状态
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        
        // 重置动画状态
        animator.SetBool("isDead", false);
        
        Debug.Log($"玩家重生! 位置: {respawnPosition}, 生命值: {currentHealth}/{maxHealth}");
        
        // 可以在这里触发重生事件
        // GameEvents.OnPlayerRespawn?.Invoke();
    }
    
    /// <summary>
    /// 检查角色是否存活
    /// </summary>
    /// <returns>是否存活</returns>
    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }
    
    #endregion
    
    #region 调试可视化系统
    
    [TabGroup("调试工具", "可视化设置")]
    [BoxGroup("调试工具/可视化设置/Gizmos显示")]
    [LabelText("显示攻击范围")]
    [Tooltip("在Scene视图中显示角色的攻击判定范围")]
    public bool showAttackRange = true;
    
    [BoxGroup("调试工具/可视化设置/Gizmos显示")]
    [LabelText("显示地面检测")]
    [Tooltip("在Scene视图中显示地面检测点")]
    public bool showGroundDetection = true;
    
    [BoxGroup("调试工具/可视化设置/Gizmos显示")]
    [LabelText("显示移动轨迹")]
    [Tooltip("在Scene视图中显示角色移动方向")]
    public bool showMovementDirection = true;
    
    [BoxGroup("调试工具/可视化设置/颜色配置")]
    [LabelText("攻击范围颜色")]
    [Tooltip("攻击范围Gizmos的显示颜色")]
    public Color attackRangeColor = Color.red;
    
    [BoxGroup("调试工具/可视化设置/颜色配置")]
    [LabelText("地面检测颜色")]
    [Tooltip("地面检测Gizmos的显示颜色")]
    public Color groundDetectionColor = Color.green;
    
    [BoxGroup("调试工具/可视化设置/颜色配置")]
    [LabelText("移动方向颜色")]
    [Tooltip("移动方向Gizmos的显示颜色")]
    public Color movementDirectionColor = Color.blue;
    
    /// <summary>
    /// 在Scene视图中绘制调试信息（仅在编辑器中可见）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 只在游戏运行时或编辑器中显示
        if (!Application.isPlaying && !showAttackRange && !showGroundDetection && !showMovementDirection)
            return;
            
        // 绘制攻击范围
        if (showAttackRange)
        {
            DrawAttackRange();
        }
        
        // 绘制地面检测
        if (showGroundDetection)
        {
            DrawGroundDetection();
        }
        
        // 绘制移动方向
        if (showMovementDirection && Application.isPlaying)
        {
            DrawMovementDirection();
        }
    }
    
    /// <summary>
    /// 绘制攻击范围可视化
    /// </summary>
    private void DrawAttackRange()
    {
        // 计算攻击方向和范围
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * (attackRange / 2);
        
        // 设置攻击范围颜色
        Gizmos.color = attackRangeColor;
        
        // 绘制攻击范围矩形
        Gizmos.DrawWireCube(attackCenter, new Vector3(attackRange, attackHeight, 0));
        
        // 如果正在攻击，使用实心显示
        if (Application.isPlaying && isAttacking)
        {
            Color solidColor = attackRangeColor;
            solidColor.a = 0.3f;
            Gizmos.color = solidColor;
            Gizmos.DrawCube(attackCenter, new Vector3(attackRange, attackHeight, 0));
        }
    }
    
    /// <summary>
    /// 绘制地面检测可视化
    /// </summary>
    private void DrawGroundDetection()
    {
        Gizmos.color = groundDetectionColor;
        
        // 绘制地面检测点
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // 如果在地面上，显示不同的颜色
        if (Application.isPlaying && isGrounded)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + Vector3.down * 0.5f, 0.05f);
        }
    }
    
    /// <summary>
    /// 绘制移动方向可视化
    /// </summary>
    private void DrawMovementDirection()
    {
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Gizmos.color = movementDirectionColor;
            
            // 绘制速度向量
            Vector3 velocityDirection = rb.velocity.normalized;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + velocityDirection * 2f;
            
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(endPos, 0.1f);
            
            // 显示速度数值
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(endPos + Vector3.up * 0.5f, 
                $"速度: {rb.velocity.magnitude:F1}\n方向: {velocityDirection}");
            #endif
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = 1f + multiplier; // 假设 multiplier 是一个百分比加成，例如 0.1 代表 10% 加成
        Debug.Log($"移动速度乘数设置为: {_speedMultiplier}");
    }

    public void SetAttackMultiplier(float multiplier)
    {
        _attackMultiplier = 1f + multiplier; // 假设 multiplier 是一个百分比加成
        Debug.Log($"攻击伤害乘数设置为: {_attackMultiplier}");
    }

    public  bool CanUseSkill()
    {
        if (isDead || isHurt || isAttacking||isSkill||!isGrounded)
        {
            return false;
        }
        return true;
    }

    #endregion
}