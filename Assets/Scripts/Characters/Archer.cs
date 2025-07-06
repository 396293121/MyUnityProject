using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 射手职业类 - 高敏捷高速度的远程物理攻击角色
/// 从原Phaser项目的Archer.js迁移而来
/// </summary>
public class Archer : Character
{
    [Header("射手特有属性")]
    public float shootRange = 10f;
    public float arrowSpeed = 15f;
    public int maxArrows = 30;
    public float reloadTime = 2f;
    
    [Header("射手技能冷却")]
    public float multiShotCooldown = 6f;
    public float piercingShotCooldown = 8f;
    public float rapidFireCooldown = 12f;
    public float explosiveArrowCooldown = 15f;
    
    [Header("技能预制件")]
    public GameObject arrowPrefab;
    public GameObject piercingArrowPrefab;
    public GameObject explosiveArrowPrefab;
    
    // 箭矢系统
    private int currentArrows;
    private bool isReloading = false;
    
    // 技能状态
    private bool canUseMultiShot = true;
    private bool canUsePiercingShot = true;
    private bool canUseRapidFire = true;
    private bool canUseExplosiveArrow = true;
    
    // 快速射击状态
    private bool rapidFireActive = false;
    private float originalAttackSpeed;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 射手特有属性设置
        strength = 10;      // 中等力量
        agility = 15;       // 高敏捷
        stamina = 10;       // 中等体力
        intelligence = 8;   // 较低智力
        
        // 初始化箭矢数量
        currentArrows = maxArrows;
        
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射手创建完成 - 攻击力: {physicalAttack}, 速度: {speed}, 箭矢: {currentArrows}/{maxArrows}");
        }
    }
    
    public override void CalculateDerivedStats()
    {
        base.CalculateDerivedStats();
        
        // 射手额外加成
        physicalAttack += 3;  // 额外物理攻击力
        speed += 1f;          // 额外速度
        defense -= 2;         // 防御力较低
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 处理射手特有的更新逻辑
        HandleArcherInput();
    }
    
    /// <summary>
    /// 处理射手特有输入
    /// </summary>
    private void HandleArcherInput()
    {
        if (!canMove || !isAlive) return;
        
        // 检查技能输入
        if (InputManager.Instance != null)
        {
            // 这里可以添加射手特有的输入处理
            // 例如：多重射击、穿透射击、快速射击、爆炸箭等技能的快捷键
        }
    }
    
    /// <summary>
    /// 普通射击
    /// </summary>
    public bool Shoot(Vector2 targetPosition)
    {
        if (!canAttack || !isAlive || isReloading || currentArrows <= 0) return false;
        
        canAttack = false;
        currentArrows--;
        
        // 播放射击动画
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        // 创建箭矢
        StartCoroutine(CreateArrow(targetPosition, arrowPrefab, physicalAttack));
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(ReloadArrows());
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射击，剩余箭矢: {currentArrows}/{maxArrows}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 多重射击 - 同时射出3支箭
    /// </summary>
    public bool PerformMultiShot(Vector2 targetPosition)
    {
        if (!canUseMultiShot || !canAttack || !isAlive || isReloading || currentArrows < 3) return false;
        
        canUseMultiShot = false;
        canAttack = false;
        currentArrows -= 3;
        
        // 播放多重射击动画
        if (animator != null)
        {
            animator.SetTrigger("MultiShot");
        }
        
        // 创建多支箭矢
        StartCoroutine(CreateMultipleArrows(targetPosition));
        
        // 开始冷却
        StartCoroutine(MultiShotCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(ReloadArrows());
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 多重射击，剩余箭矢: {currentArrows}/{maxArrows}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 穿透射击 - 射出能穿透敌人的箭矢
    /// </summary>
    public bool PerformPiercingShot(Vector2 targetPosition)
    {
        if (!canUsePiercingShot || !canAttack || !isAlive || isReloading || currentArrows <= 0) return false;
        
        canUsePiercingShot = false;
        canAttack = false;
        currentArrows--;
        
        // 播放穿透射击动画
        if (animator != null)
        {
            animator.SetTrigger("PiercingShot");
        }
        
        // 创建穿透箭矢
        int damage = Mathf.RoundToInt(physicalAttack * 1.3f);
        StartCoroutine(CreateArrow(targetPosition, piercingArrowPrefab, damage));
        
        // 开始冷却
        StartCoroutine(PiercingShotCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(ReloadArrows());
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 穿透射击，伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 快速射击 - 提高攻击速度，持续8秒
    /// </summary>
    public bool PerformRapidFire()
    {
        if (!canUseRapidFire || !isAlive || rapidFireActive) return false;
        
        canUseRapidFire = false;
        
        // 播放快速射击动画
        if (animator != null)
        {
            animator.SetTrigger("RapidFire");
        }
        
        // 应用快速射击效果
        StartCoroutine(ApplyRapidFireEffect());
        
        // 开始冷却
        StartCoroutine(RapidFireCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 激活快速射击模式");
        }
        
        return true;
    }
    
    /// <summary>
    /// 爆炸箭 - 射出造成范围伤害的箭矢
    /// </summary>
    public bool PerformExplosiveArrow(Vector2 targetPosition)
    {
        if (!canUseExplosiveArrow || !canAttack || !isAlive || isReloading || currentArrows <= 0) return false;
        
        canUseExplosiveArrow = false;
        canAttack = false;
        currentArrows--;
        
        // 播放爆炸箭动画
        if (animator != null)
        {
            animator.SetTrigger("ExplosiveArrow");
        }
        
        // 创建爆炸箭矢
        int damage = Mathf.RoundToInt(physicalAttack * 1.5f);
        StartCoroutine(CreateArrow(targetPosition, explosiveArrowPrefab, damage));
        
        // 开始冷却
        StartCoroutine(ExplosiveArrowCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(ReloadArrows());
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 爆炸箭，伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 手动装填箭矢
    /// </summary>
    public bool ManualReload()
    {
        if (isReloading || currentArrows >= maxArrows) return false;
        
        StartCoroutine(ReloadArrows());
        return true;
    }
    
    /// <summary>
    /// 创建箭矢
    /// </summary>
    private System.Collections.IEnumerator CreateArrow(Vector2 targetPosition, GameObject arrowPrefab, int damage)
    {
        // 等待射击动画
        yield return new WaitForSeconds(0.2f);
        
        if (arrowPrefab != null)
        {
            // 创建箭矢实例
            GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            
            // 设置箭矢属性（假设箭矢有Arrow组件）
            var arrowComponent = arrow.GetComponent<Arrow>();
            if (arrowComponent != null)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                arrowComponent.Initialize(direction, damage, this);
            }
        }
        
        // 恢复攻击能力（考虑快速射击效果）
        float attackDelay = rapidFireActive ? 0.1f : 0.3f;
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }
    
    /// <summary>
    /// 创建多支箭矢
    /// </summary>
    private System.Collections.IEnumerator CreateMultipleArrows(Vector2 targetPosition)
    {
        // 等待射击动画
        yield return new WaitForSeconds(0.3f);
        
        if (arrowPrefab != null)
        {
            // 计算三个方向
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 创建三支箭矢，角度稍有不同
            for (int i = -1; i <= 1; i++)
            {
                float arrowAngle = angle + (i * 15f); // 每支箭相差15度
                Vector2 arrowDirection = new Vector2(Mathf.Cos(arrowAngle * Mathf.Deg2Rad), Mathf.Sin(arrowAngle * Mathf.Deg2Rad));
                Vector2 arrowTarget = (Vector2)transform.position + arrowDirection * shootRange;
                
                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
                var arrowComponent = arrow.GetComponent<Arrow>();
                if (arrowComponent != null)
                {
                    Vector2 arrowDir = (arrowTarget - (Vector2)transform.position).normalized;
                    arrowComponent.Initialize(arrowDir, physicalAttack, this);
                }
            }
        }
        
        // 恢复攻击能力
        yield return new WaitForSeconds(0.4f);
        canAttack = true;
    }
    
    /// <summary>
    /// 装填箭矢
    /// </summary>
    private System.Collections.IEnumerator ReloadArrows()
    {
        isReloading = true;
        canAttack = false;
        
        // 播放装填动画
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 开始装填箭矢...");
        }
        
        // 等待装填时间
        yield return new WaitForSeconds(reloadTime);
        
        // 恢复箭矢
        currentArrows = maxArrows;
        isReloading = false;
        canAttack = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 箭矢装填完成");
        }
    }
    
    /// <summary>
    /// 应用快速射击效果
    /// </summary>
    private System.Collections.IEnumerator ApplyRapidFireEffect()
    {
        rapidFireActive = true;
        
        // 提高动画播放速度
        if (animator != null)
        {
            animator.speed = 1.5f;
        }
        
        // 持续8秒
        yield return new WaitForSeconds(8f);
        
        // 恢复正常速度
        if (animator != null)
        {
            animator.speed = 1f;
        }
        
        rapidFireActive = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 快速射击效果结束");
        }
    }
    
    /// <summary>
    /// 多重射击冷却
    /// </summary>
    private System.Collections.IEnumerator MultiShotCooldown()
    {
        yield return new WaitForSeconds(multiShotCooldown);
        canUseMultiShot = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 多重射击冷却完成");
        }
    }
    
    /// <summary>
    /// 穿透射击冷却
    /// </summary>
    private System.Collections.IEnumerator PiercingShotCooldown()
    {
        yield return new WaitForSeconds(piercingShotCooldown);
        canUsePiercingShot = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 穿透射击冷却完成");
        }
    }
    
    /// <summary>
    /// 快速射击冷却
    /// </summary>
    private System.Collections.IEnumerator RapidFireCooldown()
    {
        yield return new WaitForSeconds(rapidFireCooldown);
        canUseRapidFire = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 快速射击冷却完成");
        }
    }
    
    /// <summary>
    /// 爆炸箭冷却
    /// </summary>
    private System.Collections.IEnumerator ExplosiveArrowCooldown()
    {
        yield return new WaitForSeconds(explosiveArrowCooldown);
        canUseExplosiveArrow = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 爆炸箭冷却完成");
        }
    }
    
    /// <summary>
    /// 获取技能状态信息
    /// </summary>
    public ArcherSkillStatus GetSkillStatus()
    {
        return new ArcherSkillStatus
        {
            canUseMultiShot = this.canUseMultiShot,
            canUsePiercingShot = this.canUsePiercingShot,
            canUseRapidFire = this.canUseRapidFire,
            canUseExplosiveArrow = this.canUseExplosiveArrow,
            currentArrows = this.currentArrows,
            maxArrows = this.maxArrows,
            isReloading = this.isReloading,
            rapidFireActive = this.rapidFireActive
        };
    }
}

/// <summary>
/// 射手技能状态结构
/// </summary>
[System.Serializable]
public struct ArcherSkillStatus
{
    public bool canUseMultiShot;
    public bool canUsePiercingShot;
    public bool canUseRapidFire;
    public bool canUseExplosiveArrow;
    public int currentArrows;
    public int maxArrows;
    public bool isReloading;
    public bool rapidFireActive;
}