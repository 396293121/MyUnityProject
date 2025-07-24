using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 神秘人
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class Dasi : Enemy
{
    [TabGroup("神秘人配置", "技能触发")]
    [FoldoutGroup("神秘人配置/技能触发/技能条件", expanded: true)]
    [LabelText("二段斩触发距离")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("当玩家在此距离内时可能二段斩技能")]
    public float twoSwingSkillTriggerDistance = 8f;

    [FoldoutGroup("神秘人配置/技能触发/技能条件")]
    [LabelText("能量球触发距离")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("当玩家在此距离内时可能触发能量球技能")]
    public float energyBallTriggerDistance = 14f;

    private float lastSkillTriggerTime = 0f;
    private DasiConfig dasiConfig;

    public override void Awake()
    {
        // 调用基类初始化
        base.Awake();

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[dasi] 神秘人创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
    /// <summary>
    /// 调试可视化 - 神秘人专用
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

        // 神秘人状态指示器
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
        Gizmos.DrawWireSphere(transform.position, twoSwingSkillTriggerDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, energyBallTriggerDistance);
    }

    /// <summary>
    /// 从系统配置初始化神秘人特定属性
    /// </summary>
    protected override void InitializeFromSystemConfig()
    {
        // 先调用基类初始化
        base.InitializeFromSystemConfig();

        dasiConfig ??= FindObjectOfType<DasiConfig>();
        if (dasiConfig != null)
        {

            // 初始化技能触发条件
            twoSwingSkillTriggerDistance = dasiConfig.twoSwingSkillTriggerDistance;
            energyBallTriggerDistance = dasiConfig.energyBallTriggerDistance;
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[dasi] 神秘人配置初始化完成 - 二段斩触发距离: {twoSwingSkillTriggerDistance}, 能量球触发距离: {energyBallTriggerDistance}");
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

        // 只处理神秘人特有的逻辑
        UpdatedasiSpecificLogic();
    }

    /// <summary>
    /// 更新神秘人特有逻辑
    /// </summary>
    private void UpdatedasiSpecificLogic()
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
        // 检查二段斩技能触发
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (currentState == EnemyState.Chase && distanceToPlayer <= twoSwingSkillTriggerDistance)
        {
            // 30%概率触发二段斩技能
            if (Random.Range(0f, 1f) < 0.3f)
            {
                  lastSkillTriggerTime = Time.time;
                TriggerTwoSwingSkill();
            }
        }
        // 检查能量球技能触发
        else if (currentState == EnemyState.Chase && distanceToPlayer <= energyBallTriggerDistance)
        {
            // 30%概率触发能量球技能
            if (Random.Range(0f, 1f) < 0.3f)
            {
                lastSkillTriggerTime = Time.time;
                TriggerEnergyBallSkill();
            }
        }
    }


    /// <summary>
    /// 触发能量球技能
    /// </summary>
    private void TriggerEnergyBallSkill()
    {

        if (skillComponent.skillDataList[1] != null && skillComponent.TryUseSkill(1)) // 假设能量球技能在索引1

        {
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[dasi] 触发能量球技能");
            }
        }
        else
        {
            Debug.LogWarning($"[dasi] 能量球技能触发失败");
        }
    }
    
    // 在技能触发时统一设置
    private void TriggerTwoSwingSkill()
    {
        if (skillComponent.skillDataList[0] != null && skillComponent.TryUseSkill(0))
        {
                Debug.Log($"[dasi] 触发二段斩技能");
        }
        else
        {
            Debug.LogWarning($"[dasi] 二段斩技能触发失败");
        }
    }

    #region AI状态机系统

    /// <summary>
    /// 重写执行追击行为 - 神秘人版本
    /// </summary>
    protected override void ExecuteChaseState()
    {
        if (!canMove || player == null) return;

        // 如果正在执行技能，不进行状态切换
        if (isSkill) return;

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
    /// 重写执行巡逻行为 - 神秘人版本
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead) return;

        // 采用简化的巡逻逻辑
        Vector2 patrolDirection = GetdasiPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            MoveInDirection(patrolDirection, patrolSpeed);
            UpdateFacing(patrolDirection.x > 0);
        }
    }

    #endregion

    /// <summary>
    /// 获取神秘人巡逻方向 - 采用稳定巡逻机制
    /// </summary>
    private Vector2 GetdasiPatrolDirection()
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
        // if (!IsDead && !hasTriggeredEnrage)
        // {
        //     float healthPercentage = (float)currentHealth / maxHealth;
        //     if (healthPercentage <= enrageSkillTriggerThreshold)
        //     {
        //         TriggerEnrageSkill();
        //     }
        // }
    }
    protected override void InterruptSkill()
    {
        // 被打断时切换到眩晕状态
        base.InterruptSkill();
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

