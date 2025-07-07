using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // 获
    }

    void Update()
    {
        animator.SetBool("IsGrounded", isGrounded);
        // ��ȡ����
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        float currentSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", currentSpeed);
        // �ƶ���ɫ


        // ��Ծ
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 2D 跳跃
            animator.SetTrigger("Jump"); // 触发 Jump 动画
        }

        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
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

        // 当玩家进入敌人触发区域时，可以在这里处理逻辑（比如受到伤害）
        if (other.CompareTag("Enemy")) // 假设敌人有一个"Enemy"标签
        {
            Debug.Log("Player triggered by Enemy!");
            // 添加伤害逻辑
            GameEvents.OnPlayerTakeDamage.Invoke(10f);
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
}