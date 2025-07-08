using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectionRange = 5f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance < detectionRange)
            {
                // ×·»÷Íæ¼Ò
                Vector2 direction = (player.position - transform.position).normalized;
                rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

                // ·­×ª³¯Ïò
                if (direction.x > 0)
                    transform.localScale = new Vector3(1, 1, 1);
                else if (direction.x < 0)
                    transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // ÔÚSceneÊÓÍ¼ÖÐÏÔÊ¾¼ì²â·¶Î§
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}