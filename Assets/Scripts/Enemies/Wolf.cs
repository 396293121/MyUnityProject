using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 狼敌人类 - 具有冲撞攻击能力的近战敌人
/// 继承自重构后的Enemy基类，采用SimpleEnemy的状态机设计
/// 特色功能：冲锋攻击、召唤状态、撞墙眩晕
/// 性能优化：屏幕外减少更新频率
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class Wolf : Enemy
{
    [TabGroup("狼配置", "技能触发")]
    [FoldoutGroup("狼配置/技能触发/技能条件", expanded: true)]
    [LabelText("冲锋触发距离")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("当玩家在此距离内时可能触发冲锋技能")]
    public float chargeSkillTriggerDistance = 6f;

    [FoldoutGroup("狼配置/技能触发/技能条件")]
    [LabelText("召唤血量阈值")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("血量低于此百分比时触发召唤技能")]
    public float enrageSkillTriggerThreshold = 0.3f;
    [FoldoutGroup("狼配置/技能触发/技能条件")]
    [LabelText("召唤预制体")]
    [InfoBox("召唤预制体")]
    public GameObject Summon;
 public GameObject SummonPrefab { get => Summon;set{}}

    [TabGroup("狼配置", "状态监控")]
    [FoldoutGroup("狼配置/状态监控/技能状态", expanded: true)]
    [LabelText("是否已触发召唤")]
    [ReadOnly]
    [ShowInInspector]
    private bool hasTriggeredEnrage = false;

    private WolfConfig wolfConfig;
    // 狼眩晕状态动画

    private float lastSkillTriggerTime = 0f;
    public override void Awake()
    {
        // 调用基类初始化
        base.Awake();

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Wolf] 狼创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
    /// <summary>
    /// 调试可视化 - 狼专用
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);

        // 绘制失去目标范围
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        // 绘制巡逻范围
        if (initialPosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
            Gizmos.DrawWireSphere(leftPatrolPoint, 0.3f);
            Gizmos.DrawWireSphere(rightPatrolPoint, 0.3f);

            // 绘制当前巡逻目标
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolTarget, 0.2f);
        }

        // 绘制屏幕边界检测范围
        if (Camera.main != null)
        {
            Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
            Gizmos.color = isOnScreen ? Color.green : Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        // 狼状态指示器
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.4f);

        // 角色朝向
        Vector3 facingDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector3.left : Vector3.right;
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, facingDirection * 1.5f);

        // 速度向量
        if (rb2D != null && rb2D.velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, rb2D.velocity * 0.5f);
        }

        // 技能触发范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeSkillTriggerDistance);
    }

    /// <summary>
    /// 从系统配置初始化狼特定属性
    /// </summary>
    protected override void InitializeFromSystemConfig()
    {
        // 先调用基类初始化
        base.InitializeFromSystemConfig();

        wolfConfig ??= FindObjectOfType<WolfConfig>();
        if (wolfConfig != null)
        {

            // 初始化技能触发条件
            chargeSkillTriggerDistance = wolfConfig.chargeDistance;
            enrageSkillTriggerThreshold = wolfConfig.enrageHealthThreshold;
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[Wolf] 狼配置初始化完成 - 冲锋触发距离: {chargeSkillTriggerDistance}, 召唤触发阈值: {enrageSkillTriggerThreshold}");
            }
        }
    }

    protected override void Update()
    {
        //敌人放技能,或眩晕期间不更新
        if (isSkill||currentState==EnemyState.Stun) return;

        // 调用基类的优化更新逻辑
        base.Update();

        if (IsDead) return;

        // 只处理狼特有的逻辑
        UpdateWolfSpecificLogic();
    }

    /// <summary>
    /// 更新狼特有逻辑
    /// </summary>
    private void UpdateWolfSpecificLogic()
    {
        // 检查技能触发条件
        CheckSkillTriggers();
    }

    /// <summary>
    /// 检查技能触发条件
    /// </summary>
    private void CheckSkillTriggers()
    {
        if (skillComponent == null || player == null || isSkill) return; // 添加isSkill检查
          // 添加冷却检查，避免频繁触发
    // 添加技能冷却时间字段
    if (Time.time - lastSkillTriggerTime < 1f) return;
        // 检查冲锋技能触发
        float distanceToPlayer = Vector2.Distance(attackPoint.transform.position, player.position);
        if (currentState == EnemyState.Chase && distanceToPlayer <= chargeSkillTriggerDistance)
        {
            // 30%概率触发冲锋技能
            if (Random.Range(0f, 1f) < 0.3f)
            {
                lastSkillTriggerTime = Time.time;
                TriggerChargeSkill();
            }
        }
    }


    /// <summary>
    /// 触发召唤技能
    /// </summary>
    private void TriggerEnrageSkill()
    {

        if (skillComponent.skillDataList[1] != null && skillComponent.TryUseSkill(1)) // 假设召唤技能在索引1

        {
            hasTriggeredEnrage = true;

            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[Wolf] 触发召唤技能");
            }
        }
        else
        {
            Debug.LogWarning($"[Wolf] 召唤技能触发失败");
        }
    }

    /// <summary>
    /// 触发冲锋技能
    /// </summary>
    // 移除冗余的动画参数，统一使用技能系统参数
    private static readonly int isSkillingHash = Animator.StringToHash("isSkilling");




    // 在技能触发时统一设置
    private void TriggerChargeSkill()
    {
        if (skillComponent.skillDataList[0] != null && skillComponent.TryUseSkill(0))
        {
            // 技能系统会自动处理 isSkilling 和 animationTrigger
            // 移除手动的 Charge 触发
                  // 计算玩家在狼的左侧还是右侧
//               bool shouldFaceRight = player.position.x > transform.position.x;
          
//             // UpdateFacing(shouldFaceRight);
//                 // 强制更新朝向状态
//         facingRight = shouldFaceRight;
//                // 同步更新刚体速度方向（可选）
//   // 立即同步物理系统方向
//         transform.localScale = new Vector3(
//             Mathf.Abs(transform.localScale.x) * (shouldFaceRight ? 1 : -1),
//             transform.localScale.y,
//             transform.localScale.z);        
        }
        else
        {
            Debug.LogWarning($"[Wolf] 冲锋技能触发失败");
        }
    }

    #region AI状态机系统

  

    /// <summary>
    /// 重写执行巡逻行为 - 狼版本
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead) return;

        // 采用简化的巡逻逻辑
        Vector2 patrolDirection = GetWolfPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            MoveInDirection(patrolDirection, patrolSpeed);
            UpdateFacing(patrolDirection.x > 0);
        }
    }

    #endregion

    /// <summary>
    /// 获取狼巡逻方向 - 采用稳定巡逻机制
    /// </summary>
    private Vector2 GetWolfPatrolDirection()
    {
        // 使用更长的巡逻周期，减少频繁切换
        float patrolCycle = patrolWaitTime * 6f; // 增加周期长度
        float currentTime = (Time.time + GetInstanceID()) % patrolCycle; // 加入实例ID避免同步

        if (currentTime < patrolCycle * 0.5f)
        {
            return facingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            return facingRight ? Vector2.left : Vector2.right;
        }
    }

    /// <summary>
    /// 使用指定速度向指定方向移动 - 重载方法
    /// </summary>
    protected override void MoveInDirection(Vector2 direction, float speed)
    {
        if (!canMove || isDead || rb2D == null) return;

    base.MoveInDirection(direction, speed);

        // 更新朝向
        UpdateFacing(direction.x > 0);
    }

    public override void onHurtEnd()
    {
        base.onHurtEnd();
        if (!IsDead && !hasTriggeredEnrage)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            if (healthPercentage <= enrageSkillTriggerThreshold)
            {
                TriggerEnrageSkill();
            }
        }
    }
    protected override void InterruptSkill()
    {
        // 被打断时切换到眩晕状态
        // base.InterruptSkill();
        // toBeStun = true;
    }
    /// <summary>
    /// 根据AI状态返回对应颜色
    /// </summary>
    private Color GetStateColor()
    {
        switch (currentState)
        {
            case EnemyState.Idle: return Color.gray;
            case EnemyState.Patrol: return Color.green;
            case EnemyState.Chase: return Color.cyan;
            case EnemyState.Attack: return Color.red;
            case EnemyState.Dead: return Color.black;
            default: return Color.white;
        }
    }

    
}

