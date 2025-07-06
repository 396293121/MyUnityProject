using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家控制器 - 整合角色系统和输入管理，控制玩家角色
/// 从原Phaser项目的Player.js迁移而来
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("角色引用")]
    public Character playerCharacter;        // 玩家角色引用
    
    [Header("移动设置")]
    public float moveSpeed = 5f;             // 移动速度
    public float jumpForce = 10f;            // 跳跃力度
    public float groundCheckDistance = 0.1f; // 地面检测距离
    public LayerMask groundLayerMask = 1;    // 地面图层
    
    [Header("战斗设置")]
    public float attackRange = 2f;           // 攻击范围
    public LayerMask enemyLayerMask = 1;     // 敌人图层
    public Transform attackPoint;            // 攻击点
    
    [Header("交互设置")]
    public float interactRange = 1.5f;       // 交互范围
    public LayerMask interactableLayerMask = 1; // 可交互物体图层
    
    [Header("UI设置")]
    public bool showDebugGizmos = true;      // 显示调试辅助线
    
    // 组件引用
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Inventory inventory;
    
    // 输入状态
    private Vector2 moveInput;
    private bool jumpInput;
    private bool attackInput;
    private bool interactInput;
    private bool skill1Input;
    private bool skill2Input;
    private bool skill3Input;
    private bool skill4Input;
    
    // 状态
    private bool isGrounded;
    private bool facingRight = true;
    private bool canMove = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    
    // 攻击冷却
    private float lastAttackTime;
    private float attackCooldown = 0.5f;
    
    // 动画哈希
    private int animMoveSpeed;
    private int animIsGrounded;
    private int animAttack;
    private int animJump;
    private int animHurt;
    private int animDeath;
    
    private void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventory = GetComponent<Inventory>();
        
        // 如果没有指定角色，尝试获取当前对象的角色组件
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<Character>();
        }
        
        // 初始化动画哈希
        InitializeAnimationHashes();
    }
    
    private void Start()
    {
        // 设置UI管理器的当前角色
        if (UIManager.Instance != null && playerCharacter != null)
        {
            UIManager.Instance.SetCurrentCharacter(playerCharacter);
        }
        
        // 监听输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed += OnJumpInput;
            InputManager.Instance.OnAttackPressed += OnAttackInput;
            InputManager.Instance.OnInteractPressed += OnInteractInput;
            InputManager.Instance.OnSkillPressed += OnSkillInput;
        }
        
        // 监听角色事件
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged += OnHealthChanged;
            playerCharacter.OnManaChanged += OnManaChanged;
            playerCharacter.OnLevelUp += OnLevelUp;
            playerCharacter.OnDeath += OnPlayerDeath;
        }
    }
    
    private void Update()
    {
        // 检查地面
        CheckGrounded();
        
        // 处理移动
        HandleMovement();
        
        // 处理攻击
        HandleAttack();
        
        // 处理交互
        HandleInteraction();
        
        // 更新动画
        UpdateAnimations();
    }
    
    private void FixedUpdate()
    {
        // 物理移动
        if (canMove && !isAttacking)
        {
            ApplyMovement();
        }
    }
    
    /// <summary>
    /// 初始化动画哈希
    /// </summary>
    private void InitializeAnimationHashes()
    {
        animMoveSpeed = Animator.StringToHash("MoveSpeed");
        animIsGrounded = Animator.StringToHash("IsGrounded");
        animAttack = Animator.StringToHash("Attack");
        animJump = Animator.StringToHash("Jump");
        animHurt = Animator.StringToHash("Hurt");
        animDeath = Animator.StringToHash("Death");
    }
    
    /// <summary>
    /// 检查是否在地面
    /// </summary>
    private void CheckGrounded()
    {
        Vector2 raycastOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
        isGrounded = hit.collider != null;
    }
    
    /// <summary>
    /// 处理移动输入
    /// </summary>
    private void HandleMovement()
    {
        // 获取移动输入
        if (InputManager.Instance != null)
        {
            moveInput = InputManager.Instance.MoveInput;
        }
        
        // 处理转向
        if (moveInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && facingRight)
        {
            Flip();
        }
        
        // 处理跳跃
        if (jumpInput && isGrounded && canMove)
        {
            Jump();
            jumpInput = false;
        }
    }
    
    /// <summary>
    /// 应用物理移动
    /// </summary>
    private void ApplyMovement()
    {
        if (rb != null)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = moveInput.x * moveSpeed;
            rb.velocity = velocity;
        }
    }
    
    /// <summary>
    /// 跳跃
    /// </summary>
    private void Jump()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // 播放跳跃音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("player_jump");
            }
        }
    }
    
    /// <summary>
    /// 翻转角色
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }
    
    /// <summary>
    /// 处理攻击
    /// </summary>
    private void HandleAttack()
    {
        if (attackInput && canAttack && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            attackInput = false;
        }
    }
    
    /// <summary>
    /// 执行攻击
    /// </summary>
    private void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger(animAttack);
        }
        
        // 检测攻击范围内的敌人
        Vector2 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayerMask);
        
        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null && playerCharacter != null)
            {
                // 计算伤害
                int damage = playerCharacter.physicalAttack;
                enemyComponent.TakeDamage(damage);
            }
        }
        
        // 播放攻击音效
        if (AudioManager.Instance != null && playerCharacter != null)
        {
            string soundName = GetAttackSoundName();
            AudioManager.Instance.PlaySFX(soundName);
        }
        
        // 重置攻击状态
        StartCoroutine(ResetAttackState());
    }
    
    /// <summary>
    /// 重置攻击状态
    /// </summary>
    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.3f); // 攻击动画持续时间
        isAttacking = false;
    }
    
    /// <summary>
    /// 获取攻击音效名称
    /// </summary>
    private string GetAttackSoundName()
    {
        if (playerCharacter is Warrior)
        {
            return "warrior_attack";
        }
        else if (playerCharacter is Mage)
        {
            return "mage_attack";
        }
        else if (playerCharacter is Archer)
        {
            return "archer_attack";
        }
        return "player_attack";
    }
    
    /// <summary>
    /// 处理交互
    /// </summary>
    private void HandleInteraction()
    {
        if (interactInput)
        {
            PerformInteraction();
            interactInput = false;
        }
    }
    
    /// <summary>
    /// 执行交互
    /// </summary>
    private void PerformInteraction()
    {
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayerMask);
        
        foreach (Collider2D interactable in interactables)
        {
            // 检查是否为物品
            ItemPickup itemPickup = interactable.GetComponent<ItemPickup>();
            if (itemPickup != null && inventory != null)
            {
                if (inventory.AddItem(itemPickup.item, itemPickup.quantity))
                {
                    itemPickup.OnPickedUp();
                    Destroy(interactable.gameObject);
                }
                continue;
            }
            
            // 检查其他可交互对象
            IInteractable interactableComponent = interactable.GetComponent<IInteractable>();
            if (interactableComponent != null)
            {
                interactableComponent.Interact(this);
            }
        }
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 移动速度
        float moveSpeedAnim = Mathf.Abs(moveInput.x);
        animator.SetFloat(animMoveSpeed, moveSpeedAnim);
        
        // 是否在地面
        animator.SetBool(animIsGrounded, isGrounded);
        
        // 跳跃
        if (jumpInput)
        {
            animator.SetTrigger(animJump);
        }
    }
    
    /// <summary>
    /// 输入事件处理
    /// </summary>
    private void OnMoveInput(Vector2 input)
    {
        moveInput = input;
    }
    
    private void OnJumpInput()
    {
        jumpInput = true;
    }
    
    private void OnAttackInput()
    {
        attackInput = true;
    }
    
    private void OnInteractInput()
    {
        interactInput = true;
    }
    
    private void OnSkillInput()
    {
        // 默认使用第一个技能，可以根据需要扩展
        UseSkill(0);
    }
    
    /// <summary>
    /// 使用技能
    /// </summary>
    private void UseSkill(int skillIndex)
    {
        if (playerCharacter == null) return;
        
        // 根据角色类型使用技能
        if (playerCharacter is Warrior warrior)
        {
            UseWarriorSkill(warrior, skillIndex);
        }
        else if (playerCharacter is Mage mage)
        {
            UseMageSkill(mage, skillIndex);
        }
        else if (playerCharacter is Archer archer)
        {
            UseArcherSkill(archer, skillIndex);
        }
    }
    
    /// <summary>
    /// 使用战士技能
    /// </summary>
    private void UseWarriorSkill(Warrior warrior, int skillIndex)
    {
        switch (skillIndex)
        {
            case 0:
                warrior.PerformHeavySlash();
                break;
            case 1:
                warrior.PerformWhirlwind();
                break;
            case 2:
                warrior.PerformBattleCry();
                break;
        }
    }
    
    /// <summary>
    /// 使用法师技能
    /// </summary>
    private void UseMageSkill(Mage mage, int skillIndex)
    {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        switch (skillIndex)
        {
            case 0:
                mage.CastFireball(targetPos);
                break;
            case 1:
                // 查找最近的敌人
                GameObject nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    mage.CastLightningBolt(nearestEnemy.transform);
                }
                break;
            case 2:
                mage.CastHeal();
                break;
            case 3:
                mage.CastTeleport(targetPos);
                break;
        }
    }
    
    /// <summary>
    /// 使用射手技能
    /// </summary>
    private void UseArcherSkill(Archer archer, int skillIndex)
    {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        switch (skillIndex)
        {
            case 0:
                archer.PerformMultiShot(targetPos);
                break;
            case 1:
                archer.PerformPiercingShot(targetPos);
                break;
            case 2:
                archer.PerformRapidFire();
                break;
            case 3:
                archer.PerformExplosiveArrow(targetPos);
                break;
        }
    }
    
    /// <summary>
    /// 查找最近的敌人
    /// </summary>
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// 角色事件处理
    /// </summary>
    private void OnHealthChanged(int currentHealth)
    {
        // 生命值变化时的处理
        if (playerCharacter != null && currentHealth <= playerCharacter.maxHealth * 0.2f) // 生命值低于20%
        {
            // 播放低血量音效或显示警告
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("low_health_warning");
            }
        }
    }
    
    private void OnManaChanged(int currentMana)
    {
        // 魔法值变化时的处理
    }
    
    private void OnLevelUp(int newLevel)
    {
        // 升级时的处理
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"恭喜！升级到 {newLevel} 级！");
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("level_up");
        }
    }
    
    private void OnPlayerDeath()
    {
        // 玩家死亡时的处理
        canMove = false;
        canAttack = false;
        
        if (animator != null)
        {
            animator.SetTrigger(animDeath);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("player_death");
        }
        
        // 显示游戏结束界面
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameOver();
        }
        
        // 通知游戏管理器
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
    
    /// <summary>
    /// 受到伤害时的处理
    /// </summary>
    public void OnTakeDamage(int damage)
    {
        if (animator != null)
        {
            animator.SetTrigger(animHurt);
        }
        
        // 播放受伤音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("player_hurt");
        }
        
        // 屏幕震动效果（如果有相机震动组件）
        CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.Shake(0.2f, 0.1f);
        }
    }
    
    /// <summary>
    /// 设置移动能力
    /// </summary>
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }
    
    /// <summary>
    /// 设置攻击能力
    /// </summary>
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    
    /// <summary>
    /// 获取玩家角色
    /// </summary>
    public Character GetPlayerCharacter()
    {
        return playerCharacter;
    }
    
    /// <summary>
    /// 获取背包
    /// </summary>
    public Inventory GetInventory()
    {
        return inventory;
    }
    
    /// <summary>
    /// 绘制调试辅助线
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // 攻击范围
        Gizmos.color = Color.red;
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(attackPos, attackRange);
        
        // 交互范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
        // 地面检测
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
    
    private void OnDestroy()
    {
        // 取消监听输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= OnJumpInput;
            InputManager.Instance.OnAttackPressed -= OnAttackInput;
            InputManager.Instance.OnInteractPressed -= OnInteractInput;
            InputManager.Instance.OnSkillPressed -= OnSkillInput;
        }
        
        // 取消监听角色事件
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged -= OnHealthChanged;
            playerCharacter.OnManaChanged -= OnManaChanged;
            playerCharacter.OnLevelUp -= OnLevelUp;
            playerCharacter.OnDeath -= OnPlayerDeath;
        }
    }
}

/// <summary>
/// 可交互接口
/// </summary>
public interface IInteractable
{
    void Interact(PlayerController player);
}

/// <summary>
/// 物品拾取组件
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int quantity = 1;
    
    public void OnPickedUp()
    {
        if (item != null)
        {
            item.OnPickedUp();
        }
    }
}