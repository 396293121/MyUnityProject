using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 技能投射物系统
/// 处理技能投射物的移动、碰撞检测、伤害应用和特效播放
/// </summary>
[HideMonoScript]
public class SkillProjectile : MonoBehaviour
{
    [TitleGroup("投射物配置")]
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("移动速度")]
    [ShowIf("projectileType", ProjectileType.Standard)]
    [Tooltip("投射物的移动速度 (单位/秒)")]
    [Range(1f, 50f)]
    [SerializeField] private float speed = 10f;

    [BoxGroup("投射物配置/基础属性")]
    [LabelText("最大飞行距离")]
    [ShowIf("projectileType", ProjectileType.Standard)]
    [Tooltip("投射物的最大飞行距离，超过此距离将自动销毁")]
    [Range(1f, 100f)]
    [SerializeField] private float maxDistance = 20f;
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("投射物类型")]
    [Tooltip("投射物的类型")]
    [SerializeField] private ProjectileType projectileType = ProjectileType.Standard;
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("伤害间隔")]
    [ShowIf("projectileType", ProjectileType.DoT)]
    [Tooltip("投射物伤害应用的时间间隔")]
    [Range(0.1f, 2f)]
    [SerializeField] private float damageInterval = 0.5f;
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("投射物持续时间")]
    [ShowIf("projectileType", ProjectileType.DoT)]
    [Tooltip("")]
    [Range(0.1f, 2f)]
    [SerializeField] private float damageTime = 1f;
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("oming半径")]
    [ShowIf("projectileType", ProjectileType.Homing)]
    [Tooltip("投射物oming半径")]
    [Range(0f, 10f)]
    [SerializeField] private float homingRadius = 0f;
    [BoxGroup("投射物配置/基础属性")]
    [LabelText("发射偏移")]
    [Tooltip("投射物发射偏移")]
    [ShowIf("projectileType", ProjectileType.Homing)]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;
    [BoxGroup("投射物配置/视觉效果")]
    [LabelText("撞击特效")]
    [Tooltip("投射物撞击目标时播放的特效预制体")]
    [AssetsOnly]
    [SerializeField] private GameObject impactEffect;
    [TitleGroup("运行时状态")]
    [BoxGroup("运行时状态/位置信息")]
    [LabelText("已飞行距离")]
    [ReadOnly]
    [ShowInInspector]
    private float traveledDistance;

    [BoxGroup("运行时状态/位置信息")]
    [LabelText("起始位置")]
    [ReadOnly]
    [ShowInInspector]
    private Vector3 startPosition;

    [TitleGroup("战斗属性")]
    [BoxGroup("战斗属性/目标设置")]
    [LabelText("发射者")]
    [ReadOnly]
    [ShowInInspector]
    private GameObject owner;

    [BoxGroup("战斗属性/目标设置")]
    [LabelText("目标类型")]
    [ReadOnly]
    [ShowInInspector]
    private TargetType targetType;

    [BoxGroup("战斗属性/伤害设置")]
    [LabelText("伤害值")]
    [ReadOnly]
    [ShowInInspector]
    private float damage;

    [BoxGroup("战斗属性/音效设置")]
    [LabelText("命中音效")]
    [ShowInInspector]
    public string hitSound;
   [BoxGroup("战斗属性/音效设置")]
    [LabelText("生命周期内音效（最好设置为循环播放）")]
    [ShowInInspector]
    public string allSound;
      [BoxGroup("战斗属性/动画控制")]
    [LabelText("施法者Animator")]
    [ReadOnly]
    [ShowInInspector]
    private Animator casterAnimator;
    [BoxGroup("战斗属性/动画控制")]
    [LabelText("是否施法动画")]
    [ReadOnly]
    [ShowInInspector]
    private bool isCastAnimation;
    private Transform spellPoint; // 新增法术施放点引用
    // DoT 伤害相关变量
    private Dictionary<GameObject, Coroutine> activeDoTCoroutines = new Dictionary<GameObject, Coroutine>();
    private HashSet<GameObject> damagedTargets = new HashSet<GameObject>();
 // 新增销毁事件
    public event Action OnProjectileDestroyed;
    /// <summary>
    /// 初始化投射物参数
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="speed">移动速度</param>
    /// <param name="maxDistance">最大飞行距离</param>
    /// <param name="owner">发射者游戏对象</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="hitSound">撞击音效名称</param>
    [TitleGroup("初始化方法")]
    [Button("初始化投射物", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    public void Initialize(float damage, float speed, float maxDistance, GameObject owner, TargetType targetType, Transform casterTransform, ProjectileType projectileType, float damageInterval, float homingRadius, Vector2 spawnOffset, float damageTime, Transform spellPoint, bool isCastAnimation)

    {
        this.damage = damage;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.owner = owner;
        this.targetType = targetType;
        this.projectileType = projectileType;
        this.damageInterval = damageInterval;
        this.homingRadius = homingRadius;
        this.spawnOffset = spawnOffset;
        this.damageTime = damageTime;
        this.spellPoint = spellPoint;
        this.isCastAnimation = isCastAnimation;
        startPosition = transform.position;
        if (casterTransform != null)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(casterTransform.localScale.x);
            transform.localScale = scale;
        }
            
         // 法杖跟随逻辑
        if (projectileType == ProjectileType.DoT && spellPoint != null)
        {
            // 将投射物绑定到法杖顶端
            transform.SetParent(spellPoint);
            transform.localPosition = Vector3.zero;
            //播放持续施法动画
            if (isCastAnimation )
            {
                casterAnimator = casterTransform.GetComponent<Animator>();

                casterAnimator.SetTrigger("castTrigger");
                casterAnimator.SetBool("isCasting", true);
            }    
            //开始持续时间倒计时
            StartCoroutine(CountdownDamageTime());

        }
        if(allSound != ""){
            PlayerAudioConfig.Instance.PlaySound(allSound);
        }
    }

    /// <summary>
    /// 每帧更新投射物位置和距离检测
    /// </summary>
    // 在SkillProjectile类中添加新功能
    public enum ProjectileType
    {
        Standard,
        DoT,
        Homing,
        AOE
    }

    private IEnumerator ApplyDoTDamage(GameObject target)
    {
        float elapsedTime = 0f;
        while (target != null)
        {
            ApplyDamage(target);
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }
        // 清理协程记录
        // if (activeDoTCoroutines.ContainsKey(target))
        // {
        //     activeDoTCoroutines.Remove(target);
        // }

        Destroy(gameObject);
    }
    private IEnumerator  CountdownDamageTime()

    {
        yield return new WaitForSeconds(damageTime);
        // 伤害应用完成，清理所有DoT协程
        foreach (Coroutine coroutine in activeDoTCoroutines.Values)
        {
            StopCoroutine(coroutine);
        }
        activeDoTCoroutines.Clear();
        if (isCastAnimation)
        {  casterAnimator?.SetBool("isCasting",false); }
      
        Destroy(gameObject);
    }
    private void Update()
    {
        // 新增自动索敌逻辑
        if (projectileType == ProjectileType.Homing)
        {
            //  AcquireHomingTarget();
        }
        else if (projectileType == ProjectileType.DoT)
        {
        }
        else if (projectileType == ProjectileType.Standard)
        {
            ApplyStandardDamage();
        }
    }
    private void LateUpdate()
{
    if (projectileType == ProjectileType.DoT && spellPoint != null)
    {
        // 强制同步位置和旋转
        transform.position = spellPoint.position;
        transform.rotation = spellPoint.rotation;
        
        // 保持缩放一致性（根据需求选择其中一种方案）
        // 方案1：继承父级缩放
       // transform.localScale = spellPoint.lossyScale; 
        
        // 方案2：保持原始缩放
        // transform.localScale = Vector3.one;
    }
}
    private void ApplyStandardDamage()

    {
        Debug.Log(transform.localScale.x + ":啊实打实大大");

        // 原有移动逻辑
        transform.Translate(transform.localScale.x * speed * Time.deltaTime, 0, 0);

        // 计算已飞行距离
        traveledDistance = Vector3.Distance(startPosition, transform.position);

        // 检查是否超过最大飞行距离
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 碰撞检测处理
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (projectileType == ProjectileType.Standard && ShouldHitTarget(other.gameObject))
        {
            ApplyDamage(other.gameObject);
            PlayImpactEffect();
            Destroy(gameObject);
        }
    }
    //持续伤害
    private void OnTriggerStay2D(Collider2D other)
    {
        if (projectileType == ProjectileType.DoT && ShouldHitTarget(other.gameObject))
        {
            // 避免对同一目标重复启动DoT协程
            if (!activeDoTCoroutines.ContainsKey(other.gameObject))
            {
                Coroutine dotCoroutine = StartCoroutine(ApplyDoTDamage(other.gameObject));
                activeDoTCoroutines[other.gameObject] = dotCoroutine;
                PlayImpactEffect();
                BurnEffect burnEffect = ParticleManager.Instance.PlayBurnEffectOnTarget(other.transform, 1f);
            }
        }

    }
    /// <summary>
    /// 判断是否应该命中目标
    /// </summary>
    /// <param name="target">目标游戏对象</param>
    /// <returns>是否应该命中</returns>
    private bool ShouldHitTarget(GameObject target)
    {
        return target != owner &&
               (target.CompareTag("Enemy") && targetType == TargetType.Enemy) ||
               (target.CompareTag("Player") && targetType == TargetType.Self) ||
               (targetType == TargetType.All);
    }

    /// <summary>
    /// 对目标应用伤害
    /// </summary>
    /// <param name="target">目标游戏对象</param>
    private void ApplyDamage(GameObject target)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage((int)damage);
            PlayerAudioConfig.Instance.PlaySound(hitSound);
        }
    }

    /// <summary>
    /// 播放撞击特效
    /// </summary>
    private void PlayImpactEffect()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 停止所有活跃的DoT协程
    /// </summary>
    private void StopAllDoTCoroutines()
    {
        foreach (var coroutine in activeDoTCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeDoTCoroutines.Clear();
        damagedTargets.Clear();
    }

    /// <summary>
    /// 对象销毁时清理资源
    /// </summary>
    private void OnDestroy()
    {
        StopAllDoTCoroutines();
          // 触发销毁事件
        OnProjectileDestroyed?.Invoke();
    }

    #region 调试和验证方法

    [TitleGroup("调试工具")]
    [BoxGroup("调试工具/测试功能")]
    [Button("测试投射物初始化", ButtonSizes.Large)]
    [GUIColor(1f, 0.8f, 0.4f)]
    private void TestInitialize()
    {

    }

    [BoxGroup("调试工具/信息显示")]
    [Button("显示投射物信息", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.4f)]
    private void ShowProjectileInfo()
    {
        Debug.Log($"=== 投射物信息 ===\n" +
                  $"伤害: {damage}\n" +
                  $"速度: {speed}\n" +
                  $"最大距离: {maxDistance}\n" +
                  $"已飞行距离: {traveledDistance:F2}\n" +
                  $"目标类型: {targetType}\n" +
                  $"发射者: {(owner != null ? owner.name : "无")}\n" +
                  $"撞击音效: {hitSound}");
    }

    [BoxGroup("调试工具/验证功能")]
    [Button("验证组件完整性", ButtonSizes.Medium)]
    [GUIColor(0.4f, 1f, 0.8f)]
    private void ValidateComponents()
    {
        bool isValid = true;

        if (speed <= 0)
        {
            Debug.LogError("❌ 速度必须大于0");
            isValid = false;
        }

        if (maxDistance <= 0)
        {
            Debug.LogError("❌ 最大距离必须大于0");
            isValid = false;
        }

        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("❌ 缺少 Collider2D 组件");
            isValid = false;
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogWarning("⚠️ 建议添加 Rigidbody2D 组件");
        }

        if (isValid)
        {
            Debug.Log("✅ 投射物组件验证通过");
        }
    }

    #endregion
}