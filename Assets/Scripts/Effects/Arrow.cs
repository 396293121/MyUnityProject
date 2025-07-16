using UnityEngine;

/// <summary>
/// 箭矢类 - 射手技能使用的箭矢
/// </summary>
public class Arrow : MonoBehaviour
{
    [Header("箭矢属性")]
    public float speed = 10f;
    public float lifetime = 5f;
    public GameObject hitEffect;
    
    private int damage;
    private Character caster;
    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        // 设置箭矢销毁时间
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// 初始化箭矢
    /// </summary>
    public void Initialize(Vector2 targetDirection, int arrowDamage, Character arrowCaster)
    {
        direction = targetDirection.normalized;
        damage = arrowDamage;
        caster = arrowCaster;
        
        // 设置箭矢旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 设置箭矢速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // 检查是否击中敌人
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 对敌人造成伤害
            enemy.TakeDamage(damage);
            
            // 创建击中特效
            CreateHitEffect();
            
            // 播放击中音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("arrow_hit");
            }
            
            hasHit = true;
            Destroy(gameObject);
        }
        // 检查是否击中障碍物
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            CreateHitEffect();
            hasHit = true;
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 创建击中特效
    /// </summary>
    private void CreateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}