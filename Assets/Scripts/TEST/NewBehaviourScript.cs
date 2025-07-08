using UnityEngine;
using System.Collections;

public class SimplePlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("战斗参数")]
    public float maxHealth = 100f;
    public float attackDamage = 20f;
    public float attackWidth = 2f;  // 攻击宽度
    public float attackHeight = 1f; // 攻击高度
    public float attackRange = 1.5f; // 攻击距离（向前延伸）
    public float attackCooldown = 1f;
    public float invincibilityTime = 1f;
    public float knockbackForce = 5f;
    
    [Header("组件引用")]
    private Rigidbody2D rb;
    private Animator animator;
    
    [Header("状态变量")]
    private float currentHealth;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isDead = false;
    private bool isInvincible = false;
    private float lastAttackTime = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth; // 初始化生命值
    }

    void Update()
    {
        // 如果死亡，停止所有操作
        if (isDead) return;
        
        // 更新动画参数
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("isAttacking", isAttacking);
        
        // 如果正在受伤或攻击，限制移动
        if (!isHurt && !isAttacking)
        {
            HandleMovement();
            HandleJump();
        }
        
        // 处理攻击输入
        HandleAttack();
    }
    
    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        float currentSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", currentSpeed);
        
        // 角色翻转
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
    
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
            if (AudioManagerTest.Instance != null)
                AudioManagerTest.Instance.PlaySound(AudioManagerTest.Instance.jumpSound);
        }
    }
    
    void HandleAttack()
    {
        // 检测攻击输入（鼠标左键或X键）
        if (Input.GetKeyDown(KeyCode.A) && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    [Header("攻击动画参数")]
    public string attackAnimationName = "Attack"; // 攻击动画状态名称
    private bool damageTriggered = false; // 防止重复触发伤害
    
    // 攻击协程（简化版，主要依赖Animation Event）
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        damageTriggered = false; // 重置伤害触发标志
        
        // 触发攻击动画
        animator.SetTrigger("Attack");
        
        // 等待动画事件结束攻击状态
        // 如果没有使用Animation Event，可以作为备用方案
        yield return StartCoroutine(WaitForAttackAnimationComplete());
        
        // 备用结束逻辑（如果Animation Event没有触发）
        if (isAttacking)
        {
            EndAttack();
        }
    }
    
    // 等待攻击动画完成的协程（备用方案）
    IEnumerator WaitForAttackAnimationComplete()
    {
        // 等待动画开始播放
        yield return null;
        
        // 持续检查动画状态
        while (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查动画是否播放完成
            if (stateInfo.IsName(attackAnimationName) && stateInfo.normalizedTime >= 1.0f)
            {
                break;
            }
            
            yield return null;
        }
    }
    
    // Animation Event调用：在第7帧触发伤害
    public void OnAttackDamageFrame()
    {
        if (!damageTriggered && isAttacking)
        {
            DetectAndDamageEnemies();
            damageTriggered = true;
            Debug.Log("攻击伤害触发 - 第7帧");
        }
    }
    
    // Animation Event调用：在最后帧结束攻击状态
    public void OnAttackEnd()
    {
        EndAttack();
        Debug.Log("攻击动画结束");
    }
    
    // 结束攻击状态的方法
    private void EndAttack()
    {
        isAttacking = false;
        damageTriggered = false;
    }
    
    void DetectAndDamageEnemies()
    {
        // 获取角色面向方向
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * (attackRange / 2);
        
        // 创建矩形攻击范围
        Vector2 boxSize = new Vector2(attackRange, attackHeight);
        
        // 检测矩形范围内的敌人
        Collider2D[] enemies = Physics2D.OverlapBoxAll(attackCenter, boxSize, 0f);
        
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("攻击命中敌人: " + enemy.name);
                
                // 这里可以调用敌人的受伤方法
                // enemy.GetComponent<EnemyController>()?.TakeDamage(attackDamage);
                
                // 给敌人添加击退效果
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    
    // 受伤方法
    public void TakeDamage(float damage)
    {
        // 如果处于无敌状态或已死亡，不受伤害
        if (isInvincible || isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log("玩家受到伤害: " + damage + ", 剩余生命值: " + currentHealth);
        
        // 触发受伤事件
        if (GameEvents.OnPlayerTakeDamage != null)
            GameEvents.OnPlayerTakeDamage.Invoke(damage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            HurtState();
        }
    }
    
    // 受伤状态协程
    void HurtState()
    {
        isHurt = true;
        isInvincible = true;
        animator.SetTrigger("isHurt");
        // 播放受伤音效
        if (AudioManagerTest.Instance != null)
            AudioManagerTest.Instance.PlaySound(AudioManagerTest.Instance.damageSound);
        
    }
    public void OnHurtEnd()
{  Debug.Log("受伤动画结束");
    // 动画结束时执行的逻辑
    isHurt = false;
    // 如果需要无敌时间，可以在这里启动一个协程
    StartCoroutine(EndInvincibilityAfterDelay(invincibilityTime ));
}

private IEnumerator EndInvincibilityAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
      Debug.Log("恢复无敌时间");
    isInvincible = false;
}
    // 死亡方法
    void Die()
    {
        isDead = true;
        isHurt = false;
        isAttacking = false;
        
        Debug.Log("玩家死亡!");
        
        // 停止所有移动
        rb.velocity = Vector2.zero;
        
        // 这里可以添加死亡事件
        // GameEvents.OnPlayerDeath?.Invoke();
        animator.SetTrigger("isDead");
        // 可以在这里添加重生逻辑或游戏结束逻辑
        StartCoroutine(HandleDeath());
    }
    
    // 死亡处理协程
    IEnumerator HandleDeath()
    {
        // 等待死亡动画播放
        yield return new WaitForSeconds(2f);
        
        // 这里可以添加重生或重新开始游戏的逻辑
        Debug.Log("可以重生或重新开始游戏");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger detected with: " + other.gameObject.name + ", Tag: " + other.gameObject.tag);

        // 检测地面
        if (other.CompareTag("Ground"))
        {
            Debug.Log("Ground detected! Setting isGrounded to true");
            isGrounded = true;
        }

    }
    void OnTriggerStay2D(Collider2D other) {
        if(other.CompareTag("Enemy")) {
            TakeDamage(10f);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 处理物理碰撞（非 Trigger 碰撞体）
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player physically collided with Enemy!");
            // 可以添加击退效果等物理反应
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log("Left ground! Setting isGrounded to false");
            isGrounded = false;
        }
    }
    
    // 公共方法：获取当前生命值
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // 公共方法：获取最大生命值
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    // 公共方法：治疗
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log("玩家治疗: " + healAmount + ", 当前生命值: " + currentHealth);
    }
    
    // 公共方法：重生
    public void Respawn(Vector3 respawnPosition)
    {
        transform.position = respawnPosition;
        currentHealth = maxHealth;
        isDead = false;
        isHurt = false;
        isAttacking = false;
        isInvincible = false;
        rb.velocity = Vector2.zero;
        
        Debug.Log("玩家重生!");
    }
    
    // 在Scene视图中绘制攻击范围（仅在编辑器中可见）
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // 绘制矩形攻击范围
            Gizmos.color = Color.red;
            Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 attackCenter = (Vector2)transform.position + attackDirection * (attackRange / 2);
            
            // 绘制矩形框
            Vector3 boxSize = new Vector3(attackRange, attackHeight, 0f);
            Gizmos.DrawWireCube(attackCenter, boxSize);
        }
    }
}