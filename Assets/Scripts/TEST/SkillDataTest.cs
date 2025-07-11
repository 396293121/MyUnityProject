using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

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
public class SkillDataTest : ScriptableObject
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
    [LabelText("矩形宽度")]
    [Range(1f, 20f)]
    public float boxWidth = 5f;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.SingleTargetBox)]
    [LabelText("矩形长度")]
    [Range(1f, 20f)]
    public float boxLength = 5f;

    [TabGroup("技能效果")]
    [ShowIf("skillType", SkillTypeTest.AreaOfEffect)]
    [LabelText("AOE形状是否为圆形")]
    public bool isCircularAOE = true;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("矩形AOE宽度")]
    [Range(1f, 20f)]
    public float aoeWidth = 5f;
    
    [TabGroup("技能效果")]
    [ShowIf("@skillType == SkillTypeTest.AreaOfEffect && !isCircularAOE")]
    [LabelText("矩形AOE高度")]
    [Range(1f, 20f)]
    public float aoeHeight = 5f;
    
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
    /// <param name="castPosition">施法位置</param>
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
        // 矩形攻击，根据角色朝向确定方向
        Vector3 forward = caster.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        List<Collider2D> targets = FindTargetsInBox(castPosition, forward, boxLength, boxWidth, GetTargetLayerMask());
        int hitCount = 0;
        foreach (var target in targets)
        {
            if (IsValidTarget(target.gameObject))
            {
                DealDamageToTarget(target.gameObject, damage);
                hitCount++;
            }
        }
        Debug.Log($"单体攻击(矩形)命中 {hitCount} 个目标，每个造成 {damage} 点伤害，方向：{forward}");
    }
    
    private void ExecuteAreaOfEffectAttack(GameObject caster, Vector3 castPosition)
    {
        Collider2D[] targets;
        
        if (isCircularAOE)
        {
            // 圆形AOE
            targets = Physics2D.OverlapCircleAll(castPosition, range, GetTargetLayerMask());
        }
        else
        {
            // 矩形AOE
            targets = Physics2D.OverlapBoxAll(castPosition, new Vector2(aoeWidth, aoeHeight), 0f, GetTargetLayerMask());
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
        
        Debug.Log($"范围攻击命中 {hitCount} 个目标，每个造成 {damage} 点伤害");
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
            skillName = skillName
        };
    }
}