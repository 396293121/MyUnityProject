using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 精英山贼敌人类 - 具有冲撞攻击能力的近战敌人
/// 继承自重构后的Enemy基类，采用SimpleEnemy的状态机设计
/// 特色功能：后撤步攻击、召唤状态、撞墙眩晕
/// 性能优化：屏幕外减少更新频率
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class CaptainBandits : Enemy
{
    [TabGroup("精英山贼配置", "技能触发")]
    [FoldoutGroup("精英山贼配置/技能触发/技能条件", expanded: true)]
    [LabelText("后撤步触发距离")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("当玩家在此距离内时可能触发后撤步技能")]
    public float backChargeDistance = 6f;

    [FoldoutGroup("精英山贼配置/技能触发/技能条件")]
     [LabelText("飞刀触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发飞刀技能。建议设置为保持距离的1.5-2倍，避免与反向移动冲突")]
    public float knifeDinstance = 12f;
    private CaptainBanditsConfig CaptainBanditsConfig;    private float lastSkillTriggerTime = 0f;
    public override void Awake()
    {
        // 调用基类初始化
        base.Awake();

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[CaptainBandits] 精英山贼创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
    /// <summary>
    /// 调试可视化 - 精英山贼专用
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

        // 精英山贼状态指示器
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
        Gizmos.DrawWireSphere(transform.position, backChargeDistance);
    }

    /// <summary>
    /// 从系统配置初始化精英山贼特定属性
    /// </summary>
    protected override void InitializeFromSystemConfig()
    {
        // 先调用基类初始化
        base.InitializeFromSystemConfig();

        CaptainBanditsConfig ??= FindObjectOfType<CaptainBanditsConfig>();
        if (CaptainBanditsConfig != null)
        {

            // 初始化技能触发条件
            backChargeDistance = CaptainBanditsConfig.backChargeDistance;
            knifeDinstance = CaptainBanditsConfig.knifeDinstance;
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {

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

        // 只处理精英山贼特有的逻辑
        UpdateCaptainBanditsSpecificLogic();
    }

    /// <summary>
    /// 更新精英山贼特有逻辑
    /// </summary>
    private void UpdateCaptainBanditsSpecificLogic()
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
        // 检查后撤步技能触发
        float distanceToPlayer = Vector2.Distance(attackPoint.transform.position, player.position);
        if (currentState == EnemyState.Chase && distanceToPlayer <= backChargeDistance)
        {
            // 80%概率触发后撤步技能
            if (Random.Range(0f, 1f) < 0.8f)
            {
                lastSkillTriggerTime = Time.time;
                TriggerChargeSkill(0);
            }
        }
        // else if (currentState == EnemyState.Chase && distanceToPlayer <= knifeDinstance)
        // {
        //       // 60%概率触发飞刀技能
        //     if (Random.Range(0f, 1f) < 0.6f)
        //     {
        //         lastSkillTriggerTime = Time.time;
        //         TriggerChargeSkill(1);
        //     }
        // }
        
    }



    /// <summary>
    /// 触发后撤步技能
    /// </summary>

    // 在技能触发时统一设置
    private void TriggerChargeSkill(int index)
    {
        if (skillComponent.skillDataList[index] != null && skillComponent.TryUseSkill(index))
        {
        }
        else
        {
        }
        
    }

    #region AI状态机系统

  

    /// <summary>
    /// 重写执行巡逻行为 - 精英山贼版本
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead) return;

        // 采用简化的巡逻逻辑
        Vector2 patrolDirection = GetCaptainBanditsPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            MoveInDirection(patrolDirection, patrolSpeed);
            UpdateFacing(patrolDirection.x > 0);
        }
    }

    #endregion

    /// <summary>
    /// 获取精英山贼巡逻方向 - 采用稳定巡逻机制
    /// </summary>
    private Vector2 GetCaptainBanditsPatrolDirection()
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

