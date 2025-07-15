using System.Collections;
using UnityEngine;

/// <summary>
/// 火球效果类
/// </summary>
public class Fireball : MonoBehaviour
{
    [Header("火球设置")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionEffect;
    
    private Vector2 targetPosition;
    private int damage;
    private Character caster;
    private Vector2 direction;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
    }
    
    /// <summary>
    /// 初始化火球
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="damageAmount">伤害值</param>
    /// <param name="casterCharacter">施法者</param>
    public void Initialize(Vector2 target, int damageAmount, Character casterCharacter)
    {
        targetPosition = target;
        damage = damageAmount;
        caster = casterCharacter;
        
        // 计算方向
        direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // 设置旋转朝向目标
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 设置速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // 设置生命周期
        Destroy(gameObject, lifetime);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Fireball] 火球初始化 - 目标: {targetPosition}, 伤害: {damage}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否击中敌人
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 造成伤害
            enemy.TakePlayerDamage(damage);
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[Fireball] 火球击中敌人: {other.name}, 造成伤害: {damage}");
            }
            
            // 创建爆炸效果
            CreateExplosion();
            
            // 销毁火球
            Destroy(gameObject);
        }
        // 检查是否击中障碍物
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // 创建爆炸效果
            CreateExplosion();
            
            // 销毁火球
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 创建爆炸效果
    /// </summary>
    private void CreateExplosion()
    {
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }
        
        // 播放爆炸音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("fireball_explosion");
        }
    }
    
    private void Update()
    {
        // 检查是否到达目标位置附近
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.5f)
        {
            // 创建爆炸效果
            CreateExplosion();
            
            // 销毁火球
            Destroy(gameObject);
        }
    }
}