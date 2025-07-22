using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using Sirenix.Utilities;
using static SkillProjectile;
// 技能类型枚举
public enum SkillTypeTest
{
    [LabelText("单体攻击(最近目标)")]
    SingleTargetNearest,
    [LabelText("单体攻击(扇形)")]
    SingleTargetCone,
    [LabelText("单体攻击(矩形)")]
    SingleTargetBox,
    [LabelText("范围攻击(矩形)")]
    AoeTargetBox,
    [LabelText("范围攻击(扇形)")]
    AoeTargetCone,
    [LabelText("范围伤害")]
    AreaOfEffect,
    [LabelText("增益BUFF")]
    Buff,
    [LabelText("治疗")]
    Heal,
    [LabelText("召唤")]
    Summon,
    [LabelText("远程投射物")]
    Projectile
}
    /// <summary>
    /// 投射物类型枚举
    /// </summary>
    public enum ProjectileType
    {
        Standard,
        DoT,
        Homing,
        AOE
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
    [InfoBox("技能的显示名称")]
    public string skillName = "技能名称";

    [TabGroup("基础信息")]
    [PreviewField(100, ObjectFieldAlignment.Left)]
    [LabelText("技能图标")]
    [InfoBox("技能在UI中显示的图标")]
    public Sprite skillIcon;

    [TabGroup("基础信息")]
    [TextArea(2, 4)]
    [LabelText("技能描述")]
    [InfoBox("技能的详细描述")]
    public string description = "技能描述";

    [TabGroup("属性配置")]
    [ShowIf("@skillType != SkillTypeTest.Buff && skillType != SkillTypeTest.Heal && skillType != SkillTypeTest.Summon")]
    [Range(1f, 500f)]
    [LabelText("伤害值")]
    [InfoBox("技能造成的基础伤害值")]
    public float damage = 50f;

    [TabGroup("属性配置")]
    [Range(0.5f, 30f)]
    [LabelText("冷却时间(秒)")]
    [InfoBox("技能使用后的冷却时间")]
    public float cooldown = 3f;

    [TabGroup("属性配置")]
    [Range(0f, 100f)]
    [LabelText("法力消耗")]
    [InfoBox("释放技能消耗的魔法值")]
    public float manaCost = 20f;

    [TabGroup("属性配置")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetNearest || skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AoeTargetCone || skillType == SkillTypeTest.AoeTargetBox || skillType == SkillTypeTest.AreaOfEffect")]
    [Range(1f, 20f)]
    [LabelText("攻击范围")]
    [InfoBox("技能的有效攻击范围")]
    public float range = 5f;

    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能特效预制体")]
    public GameObject skillEffect;

    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能开始音效（在技能触发音效0.2S后播放）")]
    public AudioClip skillSound;
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能触发音效名称")]
    [InfoBox("在PLAYERAUDIOCONFIG配置的音效名称，默认SKILLSTART")]
    public string skillStartSoundName = "skillStart";

    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能命中音效名称")]
    [InfoBox("在PLAYERAUDIOCONFIG配置的音效名称，默认skillHit")]
    public string skillHitSoundName = "skillHit";
    [TabGroup("特效配置")]
    [LabelText("增益BUFF触发音效名称")]
    [ShowIf("@skillType == SkillTypeTest.Buff")]
    [InfoBox("在PLAYERAUDIOCONFIG配置的音效名称")]
    public string buffSoundName = "";
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("持续伤害技能开始音效名称")]
    [ShowIf("damageTime", damageTimeType.time)]
    [InfoBox("在PLAYERAUDIOCONFIG配置的音效名称")]
    public string skillTimeStartSoundName;

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
    [InfoBox("选择技能的攻击类型")]
    [OnValueChanged("OnSkillTypeChanged")]
    public SkillTypeTest skillType = SkillTypeTest.SingleTargetNearest;

    [TabGroup("技能效果")]
    [LabelText("目标类型")]
    [InfoBox("选择技能的目标类型")]
    [ShowIf("@skillType != SkillTypeTest.Buff && skillType != SkillTypeTest.Heal")]
    public TargetType targetType = TargetType.Enemy;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.AoeTargetCone")]
    [LabelText("扇形角度")]
    [Range(1f, 360f)]
    [InfoBox("扇形攻击的角度范围")]
    public float coneAngle = 90f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetCone || skillType == SkillTypeTest.AoeTargetCone")]
    [LabelText("扇形半径")]
    [Range(1f, 20f)]
    [InfoBox("扇形攻击的半径范围")]
    public float coneRadius = 5f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AoeTargetBox")]
    [LabelText("向上距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形攻击范围向上延伸的距离")]
    public float boxForward = 3f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AoeTargetBox")]
    [LabelText("向下距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形攻击范围向下延伸的距离")]
    public float boxBackward = 1f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AoeTargetBox")]
    [LabelText("向左距离")]
    [Range(0f, 20f)]
    public float boxLeft = 2f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetBox || skillType == SkillTypeTest.AoeTargetBox")]
    [LabelText("向右距离")]
    [Range(0f, 20f)]
    public float boxRight = 2f;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.AreaOfEffect)]
    [LabelText("AOE形状是否为圆形")]
    [InfoBox("选择AOE攻击的形状：圆形或矩形")]
    [PropertySpace(SpaceBefore = 5)]
    public bool isCircularAOE = true;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("AOE向上距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形AOE向上延伸的距离")]
    public float aoeUp = 3f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("AOE向下距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形AOE向下延伸的距离")]
    public float aoeDown = 1f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("AOE向左距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形AOE向左延伸的距离")]
    public float aoeLeft = 2f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("AOE向右距离")]
    [Range(0f, 20f)]
    [InfoBox("矩形AOE向右延伸的距离")]
    public float aoeRight = 2f;

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
    [InfoBox("技能释放时是否包含位移效果")]
    [PropertySpace(SpaceBefore = 10)]
    public bool isMove = false;

    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移距离")]
    [Range(0f, 20f)]
    [InfoBox("位移的距离，单位为Unity单位")]
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
    [InfoBox("选择位移的方向类型")]
    public MoveType moveType;

    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移曲线")]
    [InfoBox("控制位移的速度变化曲线")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("位移时是否无敌")]
    [InfoBox("位移过程中是否获得无敌状态")]
    public bool invincibleDuringMove = false;

    [TabGroup("技能效果")]
    [ShowIf("isMove")]
    [LabelText("碰撞中断位移")]
    [InfoBox("遇到障碍物时是否停止位移")]
    public bool stopOnCollision = true;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("投射物预制体")]
    [AssetsOnly]
    [Required("必须指定投射物预制体")]
    [InfoBox("技能发射的投射物预制体")]
    [PropertySpace(SpaceBefore = 10)]
    public GameObject projectilePrefab;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("投射物类型")]
    public ProjectileType type ;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("持续伤害间隔")]
    [ShowIf("type", ProjectileType.DoT)]
    public float projectileDamageInterval = 0.5f;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("持续期间是否播放动画（动画控制需配置castTrigger和isCasting)")]
    [ShowIf("type", ProjectileType.DoT)]
    public bool isCastAnimation = false;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("持续伤害持续时间")]
    [ShowIf("type", ProjectileType.DoT)]
    public float projectileDamageTime = 1f;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("自动索敌范围")]
    [ShowIf("type", ProjectileType.Homing)]
    public float homingRadius = 5f;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("索敌投射物数量")]
    [ShowIf("type", ProjectileType.Homing)]
    public int homingProjectileCount = 1;
    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.Projectile)]
    [LabelText("生成位置偏移")]
    [ShowIf("type", ProjectileType.Homing)]
    public Vector2 spawnOffset;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.Projectile && type == ProjectileType.Standard")]
    [LabelText("投射速度")]
    [Range(5f, 30f)]
    [InfoBox("投射物的飞行速度")]    public float projectileSpeed = 15f;

    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.Projectile && type == ProjectileType.Standard")]
    [LabelText("最大射程")]
    [Range(5f, 50f)]
    [InfoBox("投射物的最大飞行距离")]
    public float projectileRange = 20f;

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

    public enum damageTimeType
    {
        [LabelText("关键帧")]
        frame,
        [LabelText("时间段")]
        time
    }
    [TabGroup("技能效果")]
    [LabelText("伤害判定时长")]
    [ShowIf("@skillType == SkillTypeTest.SingleTargetNearest || skillType == SkillTypeTest.SingleTargetCone||skillType == SkillTypeTest.SingleTargetBox||skillType == SkillTypeTest.AoeTargetCone||skillType == SkillTypeTest.AoeTargetBox||skillType == SkillTypeTest.AreaOfEffect||skillType == SkillTypeTest.AoeTargetCone")]





    [InfoBox("关键帧：关键帧触发伤害\n时间段：在这段时间内触发伤害")]
    [PropertySpace(SpaceBefore = 10)]
    public damageTimeType damageTime = damageTimeType.frame;

    [TabGroup("技能效果")]
    [ShowIf("damageTime", damageTimeType.time)]
    [LabelText("是否对同一个敌人触发多段伤害")]
    [InfoBox("在持续时间内，是否可以对同一敌人造成多次伤害")]
    public bool isMultiDamage = false;

    [TabGroup("技能效果")]
    [ShowIf("@damageTime == damageTimeType.time && isMultiDamage")]
    [LabelText("伤害触发频率")]
    [Range(0.01f, 1f)]
    [InfoBox("伤害触发的频率，单位秒")]
    public float damageInterval = 0.05f;
    [TabGroup("碰撞框显示")]
    [LabelText("显示技能碰撞框")]
    [InfoBox("在Scene视图中显示技能的攻击范围，便于调试和可视化")]
    [PropertySpace(SpaceBefore = 10)]
    public bool showHitbox = true;

    [TabGroup("碰撞框显示")]
    [ShowIf("@showHitbox && HasVisualHitbox()")]
    [LabelText("碰撞框颜色")]
    [InfoBox("碰撞框在Scene视图中的显示颜色")]
    public Color hitboxColor = Color.red;

    [TabGroup("碰撞框显示")]
    [ShowIf("@showHitbox && HasVisualHitbox()")]
    [LabelText("碰撞框透明度")]
    [Range(0.1f, 1f)]
    [InfoBox("碰撞框的透明度，0.1为几乎透明，1为完全不透明")]
    public float hitboxAlpha = 0.3f;

    /// <summary>
    /// 检查当前技能类型是否有可视化碰撞框
    /// </summary>
    private bool HasVisualHitbox()
    {
        return skillType == SkillTypeTest.SingleTargetNearest ||
               skillType == SkillTypeTest.SingleTargetCone ||
               skillType == SkillTypeTest.SingleTargetBox ||
               skillType == SkillTypeTest.AoeTargetCone ||
               skillType == SkillTypeTest.AoeTargetBox ||
               skillType == SkillTypeTest.AreaOfEffect;
    }

    /// <summary>
    /// 技能类型改变时的回调方法
    /// </summary>
    private void OnSkillTypeChanged()
    {
        // 根据技能类型自动调整默认值
        switch (skillType)
        {
            case SkillTypeTest.Buff:
            case SkillTypeTest.Heal:
                targetType = TargetType.Self;
                break;
            case SkillTypeTest.Summon:
                targetType = TargetType.Self;
                break;
            default:
                if (targetType == TargetType.Self && skillType != SkillTypeTest.Buff && skillType != SkillTypeTest.Heal)
                {
                    targetType = TargetType.Enemy;
                }
                break;
        }
    }

    /// <summary>
    /// 执行技能效果的核心方法
    /// </summary>
    /// <param name="caster">施法者</param>
    /// <param name="castPosition">施法位置/param>
    /// <param name="skillSpawnPoint">技能释放点</param>
    public void ExecuteSkillEffect(GameObject caster, Vector3 castPosition, Transform skillSpawnPoint, SkillComponent skillComponent = null)

    {


        switch (skillType)
        {
            case SkillTypeTest.SingleTargetNearest:
                ExecuteSingleTargetNearestAttack(caster, castPosition);
                break;
            case SkillTypeTest.SingleTargetCone:
                ExecuteSingleTargetConeAttack(caster, castPosition, Vector3.zero, true);
                break;
            case SkillTypeTest.SingleTargetBox:
                ExecuteSingleTargetBoxAttack(caster, castPosition, Vector3.zero, true);

                break;
            case SkillTypeTest.AoeTargetBox:
                ExecuteSingleTargetBoxAttack(caster, castPosition, Vector3.zero, false);

                break;
            case SkillTypeTest.AoeTargetCone:
                ExecuteSingleTargetConeAttack(caster, castPosition, Vector3.zero, false);

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
            case SkillTypeTest.Projectile:
                ExecuteProjectileAttack(caster, skillSpawnPoint, skillComponent);
                break;
        }
    }

    /// <summary>
    /// 执行技能位移的协程
    /// </summary>
    public System.Collections.IEnumerator ExecuteMovement(GameObject caster, bool isFacingRight)
    {
        Vector3 startPosition = caster.transform.position;
        Vector3 moveDirection = GetMoveDirection(caster, isFacingRight);
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
    private Vector3 GetMoveDirection(GameObject caster, bool isFacingRight)

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

    private void ExecuteSingleTargetConeAttack(GameObject caster, Vector3 castPosition, Vector3 forwardDirection, bool isSingle)
    {
        // 扇形攻击，根据角色朝向确定方向
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        List<Collider2D> targets = FindTargetsInCone(castPosition, forward, coneRadius, coneAngle, GetTargetLayerMask(), isSingle);
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

    private void ExecuteSingleTargetBoxAttack(GameObject caster, Vector3 castPosition, Vector3 forwardDirection, bool isSingle)
    {
        // 获取攻击点位置
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);

        // 根据角色朝向确定方向
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // 使用新的方向性矩形配置
        List<Collider2D> targets = FindTargetsInDirectionalBox(attackPosition, forward,
            boxForward, boxBackward, caster.transform.localScale.x > 0 ? boxLeft : boxRight, caster.transform.localScale.x > 0 ? boxRight : boxLeft, GetTargetLayerMask(), isSingle);


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
                aoeUp, aoeDown, caster.transform.localScale.x > 0 ? aoeLeft : aoeRight, caster.transform.localScale.x > 0 ? aoeRight : aoeLeft, GetTargetLayerMask());
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
        // 播放增益BUFF音效
        if (!string.IsNullOrEmpty(buffSoundName))
        {
            PlayerAudioConfig.Instance.PlaySound(buffSoundName);
        }
        buffComponent.ApplyBuff(skillName, attackBonus, speedBonus, buffDuration);
        Debug.Log($"对 {caster.name} 施加BUFF: 攻击力+{attackBonus}, 速度+{speedBonus}, 持续{buffDuration}秒");
    }

    private void ExecuteHealEffect(GameObject caster)
    {
        // 治疗效果
        var healthComponent = caster.GetComponent<PlayerController>();
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
    // 新增投射物攻击执行方法 技能动画结束后执行
    private void ExecuteProjectileAttack(GameObject caster, Transform spawnPoint, SkillComponent skillComponent)
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        // 如果是追踪型投射物，需要先查找目标
        if (type == ProjectileType.Homing)
        {
            // 查找索敌半径内的目标
            List<Transform> targets = FindHomingTargets(caster, spawnPoint.position);

            // 如果没有找到目标，仍然创建一个普通投射物
            if (targets.Count == 0)
            {
                Debug.Log("[SkillDataConfig] 未找到追踪目标，不创建普通投射物");
                return;
            }

            // 限制投射物数量不超过找到的目标数量和配置的最大数量
            int projectileCount = Mathf.Min(targets.Count, homingProjectileCount);

            // 创建多个追踪投射物，每个指向一个目标
            if (projectileCount > 1)
            {
                // 使用协程延迟创建多个投射物，避免同一帧创建过多对象
                skillComponent.StartCoroutine(CreateMultipleProjectiles(caster, spawnPoint, skillComponent, targets, projectileCount));
            }
            else
            {
                // 只有一个目标时直接创建
                CreateSingleProjectile(caster, spawnPoint, skillComponent, targets[0]);
            }

            Debug.Log($"[SkillDataConfig] 准备创建 {projectileCount} 个追踪投射物，目标数量: {targets.Count}");
        }
        else
        {
            // 非追踪型投射物，直接创建单个投射物
            CreateSingleProjectile(caster, spawnPoint, skillComponent, null);
        }
    }

    /// <summary>
    /// 协程：创建多个追踪投射物
    /// </summary>
    private IEnumerator CreateMultipleProjectiles(GameObject caster, Transform spawnPoint, SkillComponent skillComponent, List<Transform> targets, int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateSingleProjectile(caster, spawnPoint, skillComponent, targets[i]);

            // 每创建3个投射物添加一个小延迟，避免同一帧创建过多对象
            if (i > 0 && i % 3 == 0)
            {
                yield return new WaitForSeconds(0.05f);
            }

            Debug.Log($"[SkillDataConfig] 创建了第 {i + 1}/{count} 个追踪投射物，目标: {targets[i].name}");
        }
    }

    /// <summary>
    /// 创建单个投射物并初始化
    /// </summary>
    private void CreateSingleProjectile(GameObject caster, Transform spawnPoint, SkillComponent skillComponent, Transform homingTarget)
    {
        // 计算生成位置（考虑偏移量）
        Vector3 spawnPosition = spawnPoint.position;
        if (type == ProjectileType.Homing && spawnOffset != Vector2.zero)
        {
            // 根据角色朝向调整偏移方向
            float directionMultiplier = caster.transform.localScale.x > 0 ? 1 : -1;
            spawnPosition += new Vector3(spawnOffset.x * directionMultiplier, spawnOffset.y, 0);
        }

        var projectile = Instantiate(projectilePrefab, spawnPosition, spawnPoint.rotation);
        var projectileScript = projectile.GetComponent<SkillProjectile>();

        if (projectileScript == null)
        {
            Debug.LogError($"[SkillDataConfig] 投射物预制体 {projectilePrefab.name} 缺少 SkillProjectile 组件");
            Destroy(projectile);
            return;
        }

        // 绑定销毁事件
        projectileScript.OnProjectileDestroyed += skillComponent.HandleProjectileDestroyed;

        // 设置追踪目标（如果有）
        if (homingTarget != null)
        {
            // 通过反射设置目标（避免修改SkillProjectile类的公共接口）
            var homingTargetField = typeof(SkillProjectile).GetField("homingTarget",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (homingTargetField != null)
            {
                homingTargetField.SetValue(projectileScript, homingTarget);
                Debug.Log($"[SkillDataConfig] 设置追踪目标: {homingTarget.name}");
            }
        }

        // 调用投射物的统一初始化方法
        projectileScript.Initialize(
            damage: damage,
            speed: projectileSpeed,
            maxDistance: projectileRange,
            owner: caster,
            targetType: targetType,
            casterTransform: caster.transform,
            projectileType: type,
            damageInterval: projectileDamageInterval,
            homingRadius: homingRadius,
            homingProjectileCount: homingProjectileCount,
            spawnOffset: spawnOffset,
            damageTime: projectileDamageTime,
            spellPoint: spawnPoint,
            isCastAnimation: isCastAnimation
        );
    }

    /// <summary>
    /// 查找追踪投射物的目标
    /// </summary>
    private List<Transform> FindHomingTargets(GameObject caster, Vector3 position)
    {
        List<Transform> targets = new List<Transform>();

        // 使用OverlapCircleNonAlloc优化性能，避免GC
        Collider2D[] results = new Collider2D[20]; // 预分配合理大小的数组
        int count = Physics2D.OverlapCircleNonAlloc(position, homingRadius, results, GetTargetLayerMask());

        // 根据角色的朝向确定前方方向
        Vector3 forwardDirection = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // 处理找到的目标
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = results[i];

            // 跳过无效目标
            if (collider == null || !IsValidTarget(collider.gameObject) || collider.gameObject == caster)
                continue;

            // 优先选择前方的目标
            Vector3 directionToTarget = (collider.transform.position - position).normalized;
            float dotProduct = Vector3.Dot(forwardDirection, directionToTarget);

            // 如果目标在前方或者我们不限制方向
            if (dotProduct > 0 || homingProjectileCount > 1)
            {
                targets.Add(collider.transform);

                // 如果只需要一个目标且已找到，提前返回
                if (homingProjectileCount == 1)
                    break;
            }
        }

        // 按距离排序，确保最近的目标优先被选择
        if (targets.Count > 1)
        {
            Vector3 finalPosition = position; // 捕获闭包变量
            targets.Sort((a, b) =>
                Vector3.SqrMagnitude(a.position - finalPosition)
                .CompareTo(Vector3.SqrMagnitude(b.position - finalPosition)));
        }

        return targets;
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

    private List<Collider2D> FindTargetsInCone(Vector3 origin, Vector3 forward, float radius, float angle, LayerMask layerMask, bool isSingle = false)
    {
        List<Collider2D> targetsInCone = new List<Collider2D>();
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(origin, radius, layerMask);
        //如果是单体获取最近的目标
        if (isSingle)
        {
            Collider2D nearestTarget = null;
            float minSqrDistance = float.MaxValue;
            Vector3 cachedOrigin = origin; // 缓存原点坐标

            foreach (var target in potentialTargets)
            {
                // 提前过滤无效目标
                if (!IsValidTarget(target.gameObject)) continue;

                Vector3 directionToTarget = (target.transform.position - cachedOrigin).normalized;
                if (Vector3.Dot(forward, directionToTarget) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
                {
                    // 使用平方距离比较
                    float sqrDistance = (target.transform.position - cachedOrigin).sqrMagnitude;
                    if (sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        nearestTarget = target;
                    }
                }
            }

            if (nearestTarget != null)
                targetsInCone.Add(nearestTarget);
        }
        else
        {
            foreach (var target in potentialTargets)
            {
                Vector3 directionToTarget = (target.transform.position - origin).normalized;
                if (Vector3.Dot(forward, directionToTarget) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
                {
                    targetsInCone.Add(target);
                }
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
                return target.CompareTag("Player")|| target.CompareTag("Ally");
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
                return LayerMask.GetMask("Enemy","Player", "Ally");
            default:
                return LayerMask.GetMask("Enemy");
        }
    }

    private void DealDamageToTarget(GameObject target, float damageAmount)
    {
        // 投射物互撞保护
        if (target.GetComponent<SkillProjectile>() != null) return;

        // 尝试对敌人/玩家造成伤害
        if (target.CompareTag("Enemy"))
        {
            var enmey = target.GetComponent<Enemy>();
            enmey?.TakeDamage(damageAmount);

        }
        else if (target.CompareTag("Player"))
        {
                //调用控制器的TAKEDAMAGE方法
            var player = target.GetComponent<PlayerController>();
            player?.TakeDamage((int)damageAmount);

        }
        if (!string.IsNullOrEmpty(skillHitSoundName))
        {
            
        PlayerAudioConfig.Instance.PlaySound(skillHitSoundName);
        }
        // 如果目标有其他生命值组件，可以在这里添加
        Debug.Log($"对 {target.name} 造成 {damageAmount} 点伤害");
           return;
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
        float forwardDist, float backwardDist, float leftDist, float rightDist, LayerMask layerMask, bool isSingle = false)
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
        if (!isSingle)
        {
            targets.AddRange(potentialTargets);
        }
        else
        {
            Collider2D nearestTarget = null;
            float minSqrDistance = float.MaxValue;
            Vector3 attackPosition = attackPoint; // 缓存攻击点坐标

            foreach (var target in potentialTargets)
            {
                // 提前过滤无效目标
                if (!IsValidTarget(target.gameObject)) continue;

                // 使用平方距离比较避免开方运算
                float sqrDistance = (target.transform.position - attackPosition).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearestTarget = target;
                }
            }

            if (nearestTarget != null)
                targets.Add(nearestTarget);
        }

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
            coneAngle = coneAngle,
            coneRadius = coneRadius,
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
        activeContinuousDamageCoroutines[damageKey] = null;
        // 开始新的持续伤害协程
        IEnumerator coroutine = ContinuousDamageCoroutine(damageKey, caster, castPosition);
        activeContinuousDamageCoroutines[damageKey] = skillComponent.StartCoroutine(coroutine);
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
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                PerformContinuousDamage(damageKey, caster, castPosition);
                timer = 0f; // 重置计时器
            }

            // 添加安全退出机制
            if (!activeContinuousDamageCoroutines.ContainsKey(damageKey))
            {
                Debug.Log($"安全退出协程，Key: {damageKey}");
                break;
            }

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
                PerformContinuousConeTargetDamage(damageKey, caster, castPosition, damagedEnemies, true);

                break;
            case SkillTypeTest.SingleTargetBox:
                PerformContinuousBoxTargetDamage(damageKey, caster, castPosition, damagedEnemies, true);

                break;
            case SkillTypeTest.AoeTargetBox:
                PerformContinuousBoxTargetDamage(damageKey, caster, castPosition, damagedEnemies, false);

                break;
            case SkillTypeTest.AoeTargetCone:
                PerformContinuousConeTargetDamage(damageKey, caster, castPosition, damagedEnemies, false);

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
    private void PerformContinuousConeTargetDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies, bool isSingle)
    {
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        List<Collider2D> enemies = FindTargetsInCone(castPosition, forward, coneRadius, coneAngle, GetTargetLayerMask(), isSingle);

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
    private void PerformContinuousBoxTargetDamage(string damageKey, GameObject caster, Vector3 castPosition, HashSet<GameObject> damagedEnemies, bool isSingle)

    {
        Vector3 attackPosition = GetAttackPosition(caster, castPosition);
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        List<Collider2D> enemies = FindTargetsInDirectionalBox(attackPosition, forward,
            boxForward, boxBackward, boxLeft, boxRight, GetTargetLayerMask(), isSingle);


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