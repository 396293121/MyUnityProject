using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

// 技能类型枚举
public enum SkillTypeTest
{
    [LabelText("单体攻击(最近目标)")]
    SingleTargetNearest,
    [LabelText("单体攻击(扇形)")]
    SingleTargetCone,
    [LabelText("单体攻击(矩形)")]
    SingleTargetBox,
    [LabelText("范围伤害")]
    AreaOfEffect,
    [LabelText("增益BUFF")]
    Buff,
    [LabelText("治疗")]
    Heal,
    [LabelText("召唤")]
    Summon
}

// 目标类型枚举
public enum TargetType
{
    [LabelText("敌人")]
    Enemy,
    [LabelText("自己")]
    Self,
    [LabelText("友军")]
    Ally,
    [LabelText("所有单位")]
    All
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class skillDataConfig : ScriptableObject
{
    [TabGroup("基础信息")]
    [LabelText("技能名称")]
    public string skillName = "技能名称";
    
    [TabGroup("基础信息")]
    [PreviewField(100, ObjectFieldAlignment.Left)]
    [LabelText("技能图标")]
    public Sprite skillIcon;
    
    [TabGroup("基础信息")]
    [TextArea(2, 4)]
    [LabelText("技能描述")]
    public string description = "技能描述";
    
    [TabGroup("属性配置")]
    [Range(1f, 200f)]
    [LabelText("伤害值")]
    public float damage = 30f;
    
    [TabGroup("属性配置")]
    [Range(0.5f, 30f)]
    [LabelText("冷却时间(秒)")]
    public float cooldown = 3f;
    
    [TabGroup("属性配置")]
    [Range(0f, 100f)]
    [LabelText("法力消耗")]
    public float manaCost = 20f;
    
    [TabGroup("属性配置")]
    [Range(1f, 20f)]
    [LabelText("技能范围")]
    public float range = 8f;
    
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能特效预制体")]
    public GameObject skillEffect;
    
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能音效")]
    public AudioClip skillSound;
    
    [TabGroup("动画配置")]
    [LabelText("技能动画触发器")]
    [InfoBox("在Animator Controller中对应的技能动画触发器名称")]
    public string animationTrigger = "Skill1";
    
    [TabGroup("动画配置")]
    [LabelText("动画持续时间(秒)")]
    [Range(0.1f, 5f)]
    public float animationDuration = 1f;
    
    [TabGroup("技能效果")]
    [LabelText("技能类型")]
    public SkillTypeTest skillType = SkillTypeTest.SingleTargetNearest;
    
    [TabGroup("技能效果")]
    [LabelText("目标类型")]
    public TargetType targetType = TargetType.Enemy;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetCone)]
    [LabelText("扇形角度")]
    [Range(1f, 360f)]
    public float coneAngle = 90f;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetCone)]
    [LabelText("扇形半径")]
    [Range(1f, 20f)]
    public float coneRadius = 5f;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetBox)]
    [LabelText("向上距离")]
    [Range(0f, 20f)]
    public float boxForward = 3f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetBox)]
    [LabelText("向下距离")]
    [Range(0f, 20f)]
    public float boxBackward = 1f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetBox)]
    [LabelText("向左距离")]
    [Range(0f, 20f)]
    public float boxLeft = 2f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetBox)]
    [LabelText("向右距离")]
    [Range(0f, 20f)]
    public float boxRight = 2f;
    // 兼容性属性
    [HideInInspector]
    public float boxWidth => boxLeft + boxRight;
    [HideInInspector]
    public float boxLength => boxForward + boxBackward;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.AreaOfEffect)]
    [LabelText("AOE形状是否为圆形")]
    public bool isCircularAOE = true;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("向上距离")]
    [Range(0f, 20f)]
    public float aoeUp = 2.5f;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("向下距离")]
    [Range(0f, 20f)]
    public float aoeDown = 2.5f;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("向左距离")]
    [Range(0f, 20f)]
    public float aoeLeft = 2.5f;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("向右距离")]
    [Range(0f, 20f)]
    public float aoeRight = 2.5f;
    
    // 兼容性属性
    [HideInInspector]
    public float aoeWidth => aoeLeft + aoeRight;
    [HideInInspector]
    public float aoeHeight => aoeUp + aoeDown;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Buff)]
    [LabelText("BUFF持续时间(秒)")]
    [Range(1f, 60f)]
    public float buffDuration = 10f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Buff)]
    [LabelText("攻击力加成（百分比）")]
    public float attackBonus = 0f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Buff)]
    [LabelText("移动速度加成（百分比）")]
    public float speedBonus = 0f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Heal)]
    [LabelText("治疗量")]
    [Range(1f, 200f)]
    public float healAmount = 50f;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Summon)]
    [LabelText("召唤物预制体")]
    [AssetsOnly]
    public GameObject summonPrefab;
    
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Summon)]
    [LabelText("召唤物存在时间(秒)")]
    [Range(5f, 120f)]
    public float summonDuration = 30f;
        [TabGroup("技能效果")]
    [LabelText("是否位移")]
    public bool isMove = false;
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移距离")]
    [Range(0f, 20f)]
    public float moveDistance = 5f;
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移持续时间(秒)")]
    [Range(0.1f, 2f)]
    [InfoBox("位移动作的持续时间，影响位移速度")]
    public float moveDuration = 0.3f;
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移类型")]
    public MoveType moveType;
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移曲线")]
    [InfoBox("控制位移的速度变化曲线")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移时是否无敌")]
    public bool invincibleDuringMove = false;
    
    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("碰撞中断位移")]
    public bool stopOnCollision = true;
    
    public enum MoveType
    {
        [LabelText("向前冲撞")]
        dash,
        [LabelText("后撤步")]
        backstep,
        [LabelText("垂直向上")]
        vertical,
        [LabelText("水平固定方向")]
        horizontal
    }
    
    public enum damageTimeType{
        [LabelText("关键帧")]
        frame,
        [LabelText("时间段")]
        time
    }
    [TabGroup("技能效果")]
    [LabelText("伤害判定时长")]
    [InfoBox("关键帧：关键帧触发伤害\n时间段：在这段时间内触发伤害")]
    public damageTimeType damageTime = damageTimeType.frame;
    [ShowIf("damageTime", damageTimeType.time)]
    [LabelText("是否对同一个敌人触发多段伤害")]
    public bool isMultiDamage = false;
    [ShowIf("isMultiDamage",true)]
    [LabelText("伤害触发频率")]
    [Range(0.1f, 2f)]
    [InfoBox("伤害触发的频率，单位秒")]
    public float damageInterval = 0.5f;
    [TabGroup("碰撞框显示")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetNearest || skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AreaOfEffect")]
    [LabelText("显示技能碰撞框")]
    [InfoBox("在Scene视图中显示技能的攻击范围，便于调试")]
    public bool showHitbox = true;
    
    [TabGroup("碰撞框显示")]
    [ShowIf("@showHitbox && (skillType == SkillTypeTest.SingleTargetNearest || skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AreaOfEffect)")]
    [LabelText("碰撞框颜色")]
    public Color hitboxColor = Color.red;
    
    [TabGroup("碰撞框显示")]
    [ShowIf("@showHitbox && (skillType == SkillTypeTest.SingleTargetNearest || skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AreaOfEffect)")]
    [LabelText("碰撞框透明度")]
    [Range(0.1f, 1f)]
    public float hitboxAlpha = 0.3f;

    /// <summary>
    /// 执行技能效果的核心方法
    /// </summary>
    /// <param name="caster">施法者</param>
    /// <param name="castPosition">施法位置/param>
    /// <param name="skillSpawnPoint">技能释放点</param>
    public void ExecuteSkillEffect(GameObject caster, Vector3 castPosition, Transform skillSpawnPoint)
    {

     
        switch (skillType)
        {
            case SkillTypeTest.SingleTargetNearest:
                ExecuteSingleTargetNearestAttack(caster, castPosition);
                break;
            case SkillTypeTest.SingleTargetCone:
                ExecuteSingleTargetConeAttack(caster, castPosition, Vector3.zero);
                break;
            case SkillTypeTest.SingleTargetBox:
                ExecuteSingleTargetBoxAttack(caster, castPosition, Vector3.zero);
                break;
            case SkillTypeTest.AreaOfEffect:
                ExecuteAreaOfEffectAttack(caster, castPosition);
                break;
            case SkillTypeTest.Buff:
                ExecuteBuffEffect(caster);
                break;
            case SkillTypeTest.Heal:
                ExecuteHealEffect(caster);
                break;
            case SkillTypeTest.Summon:
                ExecuteSummonEffect(caster, skillSpawnPoint);
                break;
        }
    }
    
    /// <summary>
    /// 执行技能位移的协程
    /// </summary>
    public System.Collections.IEnumerator ExecuteMovement(GameObject caster,bool isFacingRight)
    {
        Vector3 startPosition = caster.transform.position;
        Vector3 moveDirection = GetMoveDirection(caster,isFacingRight);
        Vector3 targetPosition = startPosition + moveDirection * moveDistance;
        
        float elapsed = 0f;
        bool movementInterrupted = false;
        
        // 如果启用无敌帧，设置无敌状态
        if (invincibleDuringMove)
        {
            SetInvincible(caster, true);
        }
        
        while (elapsed < moveDuration && !movementInterrupted)
        {
            float progress = elapsed / moveDuration;
            float curveValue = moveCurve.Evaluate(progress);
            
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            // 碰撞检测
            if (stopOnCollision && CheckCollisionAtPosition(caster, newPosition))
            {
                movementInterrupted = true;
                Debug.Log($"{caster.name} 的冲撞被障碍物阻挡");
                break;
            }
            
            caster.transform.position = newPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 关闭无敌帧
        if (invincibleDuringMove)
        {
            SetInvincible(caster, false);
        }
        
        Debug.Log($"{caster.name} 完成{moveType}位移，距离: {Vector3.Distance(startPosition, caster.transform.position):F2}");
    }
    
    /// <summary>
    /// 获取位移方向
    /// </summary>
    private Vector3 GetMoveDirection(GameObject caster,bool isFacingRight)

    {
        switch (moveType)
        {
            case MoveType.dash:
                // 向角色面朝方向冲撞
                return isFacingRight ? Vector3.right : Vector3.left;
            case MoveType.backstep:
                // 向角色背后方向后撤
                return isFacingRight ? Vector3.left : Vector3.right;
            case MoveType.vertical:
                return Vector3.up;
            case MoveType.horizontal:
                return Vector3.right;
            default:
                return Vector3.right;
        }
    }
    
    /// <summary>
    /// 检查指定位置是否有碰撞
    /// </summary>
    private bool CheckCollisionAtPosition(GameObject caster, Vector3 position)
    {
        // 获取角色的碰撞体
        Collider2D casterCollider = caster.GetComponent<Collider2D>();
        if (casterCollider == null) return false;
        
        // 检查在新位置是否会与环境碰撞
        LayerMask obstacleLayer = LayerMask.GetMask("Ground", "Wall", "Obstacle");
        Vector3 offset = position - caster.transform.position;
        
        // 使用射线检测或重叠检测
        RaycastHit2D hit = Physics2D.Raycast(caster.transform.position, offset.normalized, offset.magnitude, obstacleLayer);
        return hit.collider != null;
    }
    
    /// <summary>
    /// 设置角色无敌状态
    /// </summary>
    private void SetInvincible(GameObject caster, bool invincible)
    {
        // 尝试获取角色的生命值组件并设置无敌状态
        var playerController = caster.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // 假设SimplePlayerController有SetInvincible方法
            // playerController.SetInvincible(invincible);
            Debug.Log($"{caster.name} 无敌状态: {invincible}");
        }
    }
    
    private void ExecuteSingleTargetNearestAttack(GameObject caster, Vector3 castPosition)
    {
        // 寻找最近的敌人（只在面朝方向）
        Collider2D nearestEnemy = FindNearestTargetInDirection(castPosition, caster.transform, targetType);
        if (nearestEnemy != null)
        {
            DealDamageToTarget(nearestEnemy.gameObject, damage);
            Debug.Log($"单体攻击(最近目标)对 {nearestEnemy.name} 造成 {damage} 点伤害");
        }
        else
        {
            Debug.Log("未找到有效目标");
        }
    }

    private void ExecuteSingleTargetConeAttack(GameObject caster, Vector3 castPosition, Vector3 forwardDirection)
    {
        // 扇形攻击，根据角色朝向确定方向
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        List<Collider2D> targets = FindTargetsInCone(castPosition, forward, coneRadius, coneAngle, GetTargetLayerMask());
        int hitCount = 0;
        foreach (var target in targets)
        {
            if (IsValidTarget(target.gameObject))
            {
                DealDamageToTarget(target.gameObject, damage);
                hitCount++;
            }
        }
        Debug.Log($"单体攻击(扇形)命中 {hitCount} 个目标，每个造成 {damage} 点伤害");
    }

    private void ExecuteSingleTargetBoxAttack(GameObject caster, Vector3 castPosition, Vector3 forwardDirection)
    {
        // 获取攻击点位置
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);
        
        // 根据角色朝向确定方向
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        
        // 使用新的方向性矩形配置
        List<Collider2D> targets = FindTargetsInDirectionalBox(attackPosition, forward, 
            boxForward, boxBackward, boxLeft, boxRight, GetTargetLayerMask());
        
        int hitCount = 0;
        foreach (var target in targets)
        {
            if (IsValidTarget(target.gameObject))
            {
                DealDamageToTarget(target.gameObject, damage);
                hitCount++;
            }
        }
        Debug.Log($"单体攻击(矩形)命中 {hitCount} 个目标，每个造成 {damage} 点伤害，攻击点：{attackPosition}");
    }
    
    private void ExecuteAreaOfEffectAttack(GameObject caster, Vector3 castPosition)
    {
        // 获取攻击点位置
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);
        
        Collider2D[] targets;
        
        if (isCircularAOE)
        {
            // 圆形AOE
            targets = Physics2D.OverlapCircleAll(attackPosition, range, GetTargetLayerMask());
        }
        else
        {
            // 使用新的方向性矩形AOE配置
            List<Collider2D> boxTargets = FindTargetsInDirectionalAOEBox(attackPosition, 
                aoeUp, aoeDown, aoeLeft, aoeRight, GetTargetLayerMask());
            targets = boxTargets.ToArray();
        }
        
        int hitCount = 0;
        foreach (var target in targets)
        {
            if (IsValidTarget(target.gameObject))
            {
                DealDamageToTarget(target.gameObject, damage);
                hitCount++;
            }
        }
        
        Debug.Log($"范围攻击命中 {hitCount} 个目标，每个造成 {damage} 点伤害，攻击点：{attackPosition}");
    }
    
    private void ExecuteBuffEffect(GameObject caster)
    {
        // 为施法者添加BUFF效果
        var buffComponent = caster.GetComponent<BuffManager>();
        if (buffComponent == null)
        {
            buffComponent = caster.AddComponent<BuffManager>();
        }
        
        buffComponent.ApplyBuff(skillName, attackBonus, speedBonus, buffDuration);
        Debug.Log($"对 {caster.name} 施加BUFF: 攻击力+{attackBonus}, 速度+{speedBonus}, 持续{buffDuration}秒");
    }
    
    private void ExecuteHealEffect(GameObject caster)
    {
        // 治疗效果
        var healthComponent = caster.GetComponent<SimplePlayerController>();
        if (healthComponent != null)
        {
            // 假设SimplePlayerController有治疗方法
            // healthComponent.Heal(healAmount);
            Debug.Log($"对 {caster.name} 治疗 {healAmount} 点生命值");
        }
    }
    
    private void ExecuteSummonEffect(GameObject caster, Transform skillSpawnPoint)
    {
        if (summonPrefab != null && skillSpawnPoint != null)
        {
            GameObject summon = Instantiate(summonPrefab, skillSpawnPoint.position, skillSpawnPoint.rotation);
            
            // 设置召唤物的生存时间
            MonoBehaviour.Destroy(summon, summonDuration);
            
            Debug.Log($"召唤了 {summonPrefab.name}，存在时间: {summonDuration}秒");
        }
    }
    
    private Collider2D FindNearestTarget(Vector3 position, TargetType targetType)
    {
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(position, range, GetTargetLayerMask());
        Collider2D nearestTarget = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var target in potentialTargets)
        {
            if (IsValidTarget(target.gameObject))
            {
                float distance = Vector3.Distance(position, target.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = target;
                }
            }
        }
        
        return nearestTarget;
    }

    private Collider2D FindNearestTargetInDirection(Vector3 position, Transform casterTransform, TargetType targetType)
    {
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(position, range, GetTargetLayerMask());
        Collider2D nearestTarget = null;
        float nearestDistance = float.MaxValue;
        
        // 根据角色的localScale.x判断朝向，与角色脚本保持一致
        Vector3 forwardDirection = casterTransform.localScale.x > 0 ? Vector3.right : Vector3.left;
        
        foreach (var target in potentialTargets)
        {
            if (IsValidTarget(target.gameObject))
            {
                Vector3 directionToTarget = (target.transform.position - position).normalized;
                // 检查目标是否在角色面朝方向（使用点积判断，大于0表示在前方）
                if (Vector3.Dot(forwardDirection, directionToTarget) > 0)
                {
                    float distance = Vector3.Distance(position, target.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTarget = target;
                    }
                }
            }
        }
        
        return nearestTarget;
    }

    private List<Collider2D> FindTargetsInCone(Vector3 origin, Vector3 forward, float radius, float angle, LayerMask layerMask)
    {
        List<Collider2D> targetsInCone = new List<Collider2D>();
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(origin, radius, layerMask);

        foreach (var target in potentialTargets)
        {
            Vector3 directionToTarget = (target.transform.position - origin).normalized;
            if (Vector3.Dot(forward, directionToTarget) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
            {
                targetsInCone.Add(target);
            }
        }
        return targetsInCone;
    }

    private List<Collider2D> FindTargetsInBox(Vector3 origin, Vector3 forward, float length, float width, LayerMask layerMask)
    {
        List<Collider2D> targetsInBox = new List<Collider2D>();
        // 计算矩形中心点，位于施法者前方
        Vector3 boxCenter = origin + forward * (length / 2f);
        // 计算旋转角度
        float rotationAngle = Vector2.SignedAngle(Vector2.up, new Vector2(forward.x, forward.y));
        
        Collider2D[] potentialTargets = Physics2D.OverlapBoxAll(boxCenter, new Vector2(width, length), rotationAngle, layerMask);

        foreach (var target in potentialTargets)
        {
            targetsInBox.Add(target);
        }
        return targetsInBox;
    }
    
    private bool IsValidTarget(GameObject target)
    {
        switch (targetType)
        {
            case TargetType.Enemy:
                return target.CompareTag("Enemy");
            case TargetType.Self:
                return target.CompareTag("Player");
            case TargetType.Ally:
                return target.CompareTag("Player") || target.CompareTag("Ally");
            case TargetType.All:
                return true;
            default:
                return false;
        }
    }
    
    private LayerMask GetTargetLayerMask()
    {
        switch (targetType)
        {
            case TargetType.Enemy:
                return LayerMask.GetMask("Enemy");
            case TargetType.Self:
            case TargetType.Ally:
                return LayerMask.GetMask("Player", "Ally");
            case TargetType.All:
                return LayerMask.GetMask("Enemy", "Player", "Ally");
            default:
                return LayerMask.GetMask("Enemy");
        }
    }
    
    private void DealDamageToTarget(GameObject target, float damageAmount)
    {
        // 尝试对敌人造成伤害
        var enemy = target.GetComponent<SimpleEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount);
            return;
        }
        
        // 如果目标有其他生命值组件，可以在这里添加
        Debug.Log($"对 {target.name} 造成 {damageAmount} 点伤害");
    }
    
    /// <summary>
    /// 获取攻击点位置
    /// </summary>
    private Vector3 GetAttackPosition(GameObject caster, Vector3 fallbackPosition)
    {
        // 尝试从角色组件获取攻击点
        var character = caster.GetComponent<Character>();
        if (character != null)
        {
            return character.AttackPoint.position;
        }
        
        // 如果没有Character组件，使用传入的位置
        return fallbackPosition;
    }
    
    /// <summary>
    /// 基于攻击点的方向性矩形攻击检测（用于单体攻击）
    /// </summary>
    private List<Collider2D> FindTargetsInDirectionalBox(Vector3 attackPoint, Vector3 forward, 
        float forwardDist, float backwardDist, float leftDist, float rightDist, LayerMask layerMask)
    {
        List<Collider2D> targets = new List<Collider2D>();
        
        // 计算矩形的总尺寸
        float totalLength = forwardDist + backwardDist;
        float totalWidth = leftDist + rightDist;
        
        // 计算矩形中心点（相对于攻击点的偏移）
        // Vector3 centerOffset = forward * (forwardDist - backwardDist) * 0.5f;
        // Vector3 boxCenter = attackPoint + centerOffset;
                Vector3 centerOffset = Vector3.up * (forwardDist - backwardDist) * 0.5f + Vector3.right * (rightDist - leftDist) * 0.5f;
        Vector3 boxCenter = attackPoint + centerOffset;
        // 计算旋转角度
        // float rotationAngle = Vector2.SignedAngle(Vector2.up, new Vector2(forward.x, forward.y));
        
        // 使用Physics2D.OverlapBoxAll检测
        Collider2D[] potentialTargets = Physics2D.OverlapBoxAll(boxCenter, 
            new Vector2(totalWidth, totalLength), 0f, layerMask);
        
        targets.AddRange(potentialTargets);
        return targets;
    }
    
    /// <summary>
    /// 基于攻击点的方向性矩形AOE检测（用于范围攻击）
    /// </summary>
    private List<Collider2D> FindTargetsInDirectionalAOEBox(Vector3 attackPoint, 
        float upDist, float downDist, float leftDist, float rightDist, LayerMask layerMask)
    {
        List<Collider2D> targets = new List<Collider2D>();
        
        // 计算矩形的总尺寸
        float totalHeight = upDist + downDist;
        float totalWidth = leftDist + rightDist;
        
        // 计算矩形中心点（相对于攻击点的偏移）
        Vector3 centerOffset = Vector3.up * (upDist - downDist) * 0.5f + Vector3.right * (rightDist - leftDist) * 0.5f;
        Vector3 boxCenter = attackPoint + centerOffset;
        
        // AOE矩形不需要旋转，始终与世界坐标轴对齐
        Collider2D[] potentialTargets = Physics2D.OverlapBoxAll(boxCenter, 
            new Vector2(totalWidth, totalHeight), 0f, layerMask);
        
        targets.AddRange(potentialTargets);
        return targets;
    }

    // 定义一个结构体来存储Gizmo绘制所需的数据
    public struct SkillGizmoData
    {
        public SkillTypeTest skillType;
        public bool isCircularAOE;
        public float range;
        public float aoeWidth;
        public float aoeHeight;
        public float coneAngle;
        public float coneRadius;
        public float boxWidth;
        public float boxLength;
        public Color hitboxColor;
        public float hitboxAlpha;
        public string skillName;
        
        // 新增方向性配置字段
        public float boxForward;
        public float boxBackward;
        public float boxLeft;
        public float boxRight;
        public float aoeUp;
        public float aoeDown;
        public float aoeLeft;
        public float aoeRight;
    }

    /// <summary>
    /// 获取技能Gizmo绘制所需的数据
    /// </summary>
    public SkillGizmoData GetSkillGizmoData()
    {
        return new SkillGizmoData
        {
            skillType = skillType,
            isCircularAOE = isCircularAOE,
            range = range,
            aoeWidth = aoeWidth,
            aoeHeight = aoeHeight,
            coneAngle = coneAngle,
            coneRadius = coneRadius,
            boxWidth = boxWidth,
            boxLength = boxLength,
            hitboxColor = hitboxColor,
            hitboxAlpha = hitboxAlpha,
            skillName = skillName,
            // 新增方向性配置数据
            boxForward = boxForward,
            boxBackward = boxBackward,
            boxLeft = boxLeft,
            boxRight = boxRight,
            aoeUp = aoeUp,
            aoeDown = aoeDown,
            aoeLeft = aoeLeft,
            aoeRight = aoeRight
        };
    }
    
    // 静态持续伤害管理器
    private static Dictionary<string, Coroutine> activeContinuousDamageCoroutines = new Dictionary<string, Coroutine>();
    private static Dictionary<string, HashSet<GameObject>> activeDamagedEnemies = new Dictionary<string, HashSet<GameObject>>();
    
    /// <summary>
    /// 启动持续伤害效果
    /// </summary>
    /// <param name="skillComponent">技能组件引用</param>
    /// <param name="skillIndex">技能索引</param>
    /// <param name="caster">施法者</param>
    /// <param name="castPosition">施法位置</param>
    public void StartContinuousDamage(MonoBehaviour skillComponent, int skillIndex, GameObject caster, Vector3 castPosition)
    {
        if (damageTime != damageTimeType.time)
        {
            Debug.LogWarning($"[SkillDataConfig] 技能 {skillName} 不是时间段伤害类型，无法启动持续伤害");
            return;
        }
        
        string damageKey = $"{caster.GetInstanceID()}_{skillIndex}";
        
        // 如果已经有相同的持续伤害在进行，先停止它
        if (activeContinuousDamageCoroutines.ContainsKey(damageKey))
        {
            StopContinuousDamage(skillComponent, skillIndex);
        }
        
        // 初始化受伤敌人列表
        if (!activeDamagedEnemies.ContainsKey(damageKey))
        {
            activeDamagedEnemies[damageKey] = new HashSet<GameObject>();
        }
        else
        {
            activeDamagedEnemies[damageKey].Clear();
        }
        
        // 开始新的持续伤害协程
        var coroutine = skillComponent.StartCoroutine(ContinuousDamageCoroutine(damageKey, caster, castPosition));
        activeContinuousDamageCoroutines[damageKey] = coroutine;
        
        Debug.Log($"[SkillDataConfig] 启动技能 {skillName} 的持续伤害效果，Key: {damageKey}");
    }
    
    /// <summary>
    /// 停止持续伤害效果
    /// </summary>
    /// <param name="skillComponent">技能组件引用</param>
    /// <param name="skillIndex">技能索引</param>
    public void StopContinuousDamage(MonoBehaviour skillComponent, int skillIndex)
    {
        string damageKey = $"{skillComponent.gameObject.GetInstanceID()}_{skillIndex}";
        
        // 停止协程
        if (activeContinuousDamageCoroutines.ContainsKey(damageKey))
        {
            if (activeContinuousDamageCoroutines[damageKey] != null)
            {
                skillComponent.StopCoroutine(activeContinuousDamageCoroutines[damageKey]);
            }
            activeContinuousDamageCoroutines.Remove(damageKey);
        }
        
        // 清理受伤敌人列表
        if (activeDamagedEnemies.ContainsKey(damageKey))
        {
            activeDamagedEnemies[damageKey].Clear();
            activeDamagedEnemies.Remove(damageKey);
        }
        
        Debug.Log($"[SkillDataConfig] 停止技能 {skillName} 的持续伤害效果，Key: {damageKey}");
    }
    
    /// <summary>
    /// 持续伤害协程
    /// </summary>
    private IEnumerator ContinuousDamageCoroutine(string damageKey, GameObject caster, Vector3 castPosition)
    {
        float elapsed = 0f;
        float nextDamageTime = 0f;
        
        while (activeContinuousDamageCoroutines.ContainsKey(damageKey))
        {
            if (elapsed >= nextDamageTime)
            {
                PerformContinuousDamage(damageKey, caster, castPosition);
                nextDamageTime = elapsed + damageInterval;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        

        
        Debug.Log($"[SkillDataConfig] 技能 {skillName} 持续伤害协程结束");
    }
    
    /// <summary>
    /// 执行持续伤害
    /// </summary>
    private void PerformContinuousDamage(string damageKey, GameObject caster, Vector3 castPosition)
    {
        if (!activeDamagedEnemies.ContainsKey(damageKey))
        {
            return;
        }
        
        var damagedEnemies = activeDamagedEnemies[damageKey];
        
        switch (skillType)
        {
            case SkillTypeTest.SingleTargetNearest:
                PerformContinuousSingleTargetDamage(damageKey, caster, castPosition, damagedEnemies);
                break;
            case SkillTypeTest.AreaOfEffect:
                PerformContinuousAOEDamage(damageKey, caster, castPosition, damagedEnemies);
                break;
            case SkillTypeTest.SingleTargetCone:
                PerformContinuousConeTargetDamage(damageKey, caster, castPosition, damagedEnemies);
                break;
            case SkillTypeTest.SingleTargetBox:
                PerformContinuousBoxTargetDamage(damageKey, caster, castPosition, damagedEnemies);
                break;
        }
    }
    
    /// <summary>
    /// 执行持续单体伤害
    /// </summary>
    private void PerformContinuousSingleTargetDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies)
    {
        Collider2D nearestEnemy = FindNearestTargetInDirection(castPosition, caster.transform, targetType);
        if (nearestEnemy == null) return;
        
        if (!isMultiDamage && damagedEnemies.Contains(nearestEnemy.gameObject))
            return;
        
        DealDamageToTarget(nearestEnemy.gameObject, damage);
        damagedEnemies.Add(nearestEnemy.gameObject);
        Debug.Log($"[SkillDataConfig] 单体持续伤害 - 对 {nearestEnemy.name} 造成 {damage} 伤害");
    }
    
    /// <summary>
    /// 执行持续AOE伤害
    /// </summary>
    private void PerformContinuousAOEDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies)
    {
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);
        Collider2D[] enemies;
        
        if (isCircularAOE)
        {
            enemies = Physics2D.OverlapCircleAll(attackPosition, range, GetTargetLayerMask());
        }
        else
        {
            List<Collider2D> boxTargets = FindTargetsInDirectionalAOEBox(attackPosition, 
                aoeUp, aoeDown, aoeLeft, aoeRight, GetTargetLayerMask());
            enemies = boxTargets.ToArray();
        }
        
        foreach (var enemy in enemies)
        {
            if (!IsValidTarget(enemy.gameObject)) continue;
            if (!isMultiDamage && damagedEnemies.Contains(enemy.gameObject))
                continue;
            
            DealDamageToTarget(enemy.gameObject, damage);
            damagedEnemies.Add(enemy.gameObject);
            Debug.Log($"[SkillDataConfig] AOE持续伤害 - 对 {enemy.name} 造成 {damage} 伤害");
        }
    }
    
    /// <summary>
    /// 执行持续扇形伤害
    /// </summary>
    private void PerformContinuousConeTargetDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies)
    {
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        List<Collider2D> enemies = FindTargetsInCone(castPosition, forward, coneRadius, coneAngle, GetTargetLayerMask());
        
        foreach (var enemy in enemies)
        {
            if (!IsValidTarget(enemy.gameObject)) continue;
            if (!isMultiDamage && damagedEnemies.Contains(enemy.gameObject))
                continue;
            
            DealDamageToTarget(enemy.gameObject, damage);
            damagedEnemies.Add(enemy.gameObject);
            Debug.Log($"[SkillDataConfig] 扇形持续伤害 - 对 {enemy.name} 造成 {damage} 伤害");
        }
    }
    
    /// <summary>
    /// 执行持续矩形伤害
    /// </summary>
    private void PerformContinuousBoxTargetDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies)
    {
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        
        List<Collider2D> enemies = FindTargetsInDirectionalBox(attackPosition, forward, 
            boxForward, boxBackward, boxLeft, boxRight, GetTargetLayerMask());
        
        foreach (var enemy in enemies)
        {
            if (!IsValidTarget(enemy.gameObject)) continue;
            if (!isMultiDamage && damagedEnemies.Contains(enemy.gameObject))
                continue;
            
            DealDamageToTarget(enemy.gameObject, damage);
            damagedEnemies.Add(enemy.gameObject);
            Debug.Log($"[SkillDataConfig] 矩形持续伤害 - 对 {enemy.name} 造成 {damage} 伤害");
        }
    }
    
}