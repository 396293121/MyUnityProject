using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 野猪敌人类 - 具有冲撞攻击能力的近战敌人
/// 继承自重构后的Enemy基类，采用SimpleEnemy的状态机设计
/// 特色功能：冲锋攻击、狂暴状态、撞墙眩晕
/// 性能优化：屏幕外减少更新频率
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class WildBoar : Enemy
{
    [TabGroup("野猪配置", "技能触发")]
    [FoldoutGroup("野猪配置/技能触发/技能条件", expanded: true)]
    [LabelText("冲锋触发距离")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("当玩家在此距离内时可能触发冲锋技能")]
    public float chargeSkillTriggerDistance = 6f;

    [FoldoutGroup("野猪配置/技能触发/技能条件")]
    [LabelText("狂暴血量阈值")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("血量低于此百分比时触发狂暴技能")]
    public float enrageSkillTriggerThreshold = 0.3f;
    [FoldoutGroup("野猪配置/技能触发/技能条件")]
    [LabelText("眩晕持续时间")]
    [ReadOnly]
    [ShowInInspector]
    private float stunDuration;
    [TabGroup("野猪配置", "状态监控")]
    [FoldoutGroup("野猪配置/状态监控/技能状态", expanded: true)]
    [LabelText("是否已触发狂暴")]
    [ReadOnly]
    [ShowInInspector]
    private bool hasTriggeredEnrage = false;
    private bool toBeStun=false;

    // 野猪眩晕状态动画
    private static readonly int IsStunnedHash = Animator.StringToHash("IsStunned");
    private static readonly int StunTriggerHash = Animator.StringToHash("StunTrigger");

    public override void Awake()
    {
        // 设置敌人类型
        enemyType = "WildBoar";

        // 调用基类初始化
        base.Awake();

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] 野猪创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
    /// <summary>
    /// 调试可视化 - 野猪专用
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

        // 野猪状态指示器
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
    /// 重写获取基础配置方法，返回野猪特定配置
    /// </summary>
    protected override enmeyConfig GetBaseConfig(EnemySystemConfig systemConfig)
    {
        return systemConfig.wildBoarConfig;
    }

    /// <summary>
    /// 从系统配置初始化野猪特定属性
    /// </summary>
    protected override void InitializeFromSystemConfig()
    {
        // 先调用基类初始化
        base.InitializeFromSystemConfig();

        enemySystem ??= FindObjectOfType<EnemySystemConfig>();
        if (enemySystem != null)
        {
            var wildBoarConfig = enemySystem.wildBoarConfig;

            // 初始化技能触发条件
            chargeSkillTriggerDistance = wildBoarConfig.chargeDistance;
            enrageSkillTriggerThreshold = wildBoarConfig.enrageHealthThreshold;
            stunDuration = wildBoarConfig.stunDuration;
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 野猪配置初始化完成 - 冲锋触发距离: {chargeSkillTriggerDistance}, 狂暴触发阈值: {enrageSkillTriggerThreshold}");
            }
        }
    }

    protected override void Update()
    {
        //敌人放技能,或眩晕期间不更新
        Debug.Log(currentState + "999");
        if (isSkill||currentState==EnemyState.Stun) return;

        // 调用基类的优化更新逻辑
        base.Update();

        if (IsDead) return;

        // 只处理野猪特有的逻辑
        UpdateWildBoarSpecificLogic();
    }

    /// <summary>
    /// 更新野猪特有逻辑
    /// </summary>
    private void UpdateWildBoarSpecificLogic()
    {
        // 检查技能触发条件
        CheckSkillTriggers();
    }

    /// <summary>
    /// 检查技能触发条件
    /// </summary>
    private void CheckSkillTriggers()
    {
        if (skillComponent == null || player == null) return;
        // 检查冲锋技能触发
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (currentState == EnemyState.Chase && distanceToPlayer <= chargeSkillTriggerDistance)

        {
            // 30%概率触发冲锋技能
            if (Random.Range(0f, 1f) < 0.3f)
            {
                //转向玩家
                TriggerChargeSkill();
            }

        }
    }


    /// <summary>
    /// 触发狂暴技能
    /// </summary>
    private void TriggerEnrageSkill()
    {

        if (skillComponent.skillDataList[1] != null && skillComponent.TryUseSkill(1)) // 假设狂暴技能在索引1

        {
            hasTriggeredEnrage = true;

            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 触发狂暴技能");
            }
        }
        else
        {
            Debug.LogWarning($"[WildBoar] 狂暴技能触发失败");
        }
    }

    /// <summary>
    /// 触发冲锋技能
    /// </summary>
    private void TriggerChargeSkill()
    {
        if (skillComponent.skillDataList[0] != null && skillComponent.TryUseSkill(0)) // 假设冲锋技能在索引0
        {

            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 触发冲锋技能");
            }
        }
        else
        {
            Debug.LogWarning($"[WildBoar] 冲锋技能触发失败");
        }
    }

    #region AI状态机系统

    /// <summary>
    /// 重写执行追击行为 - 野猪版本
    /// </summary>
    protected override void ExecuteChaseState()
    {
        if (!canMove || player == null) return;

        // 修改为攻击点检测
        if (IsPlayerInAttackRange())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 检查是否失去目标
        if (distanceToPlayer > loseTargetRange)
        {
            if (stateTimer > 2f) // 至少追击2秒后才能失去目标
            {
                ChangeState(EnemyState.Patrol);
            }
            return;
        }

        // 追击移动
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb2D.velocity = new Vector2(directionToPlayer.x * moveSpeed * chaseSpeedRate, rb2D.velocity.y);
        UpdateFacing(directionToPlayer.x > 0);
    }

    /// <summary>
    /// 重写执行巡逻行为 - 野猪版本
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead) return;

        // 采用简化的巡逻逻辑
        Vector2 patrolDirection = GetWildBoarPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            MoveInDirection(patrolDirection, patrolSpeed);
            UpdateFacing(patrolDirection.x > 0);
        }
    }

    #endregion

    /// <summary>
    /// 获取野猪巡逻方向 - 采用稳定巡逻机制
    /// </summary>
    private Vector2 GetWildBoarPatrolDirection()
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
    private void MoveInDirection(Vector2 direction, float speed)
    {
        if (!canMove || isDead || rb2D == null) return;

        Vector2 targetVelocity = new Vector2(direction.x * speed, rb2D.velocity.y);
        rb2D.velocity = targetVelocity;

        isMoving = targetVelocity.magnitude > 0.1f;

        // 更新朝向
        UpdateFacing(direction.x > 0);
    }

    /// <summary>
    /// 受到玩家伤害 - 野猪版本
    /// </summary>
    public override void TakeDamage(float damage, Vector2 knockbackForce = default)
    {
        base.TakeDamage(damage, knockbackForce);

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
        if (toBeStun)
        {
               toBeStun = false;
            ExecuteStunState();
         
        }
    }
    protected override void InterruptSkill()
    {
        // 被打断时切换到眩晕状态
        base.InterruptSkill();
        toBeStun = true;
    }
    private void ExecuteStunState()
    {
          ChangeState(EnemyState.Stun);
           // 眩晕时停止移动
        if (rb2D != null)
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
        }
        // 播放眩晕动画
        animator.SetBool(IsStunnedHash, true);
        animator.SetTrigger(StunTriggerHash);
        float now = Time.time;
        // 眩晕时间结束后回到空闲状态
        while (Time.time - now <= stunDuration)
        {
            animator.SetBool(IsStunnedHash, false);
            ChangeState(EnemyState.Idle);
        }
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

