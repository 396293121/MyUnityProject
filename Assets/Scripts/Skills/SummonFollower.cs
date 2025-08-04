using UnityEngine;

/// <summary>
/// 召唤物跟随组件
/// 负责让召唤物跟随召唤者移动
/// </summary>
public class SummonFollower : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform summoner;           // 召唤者
    [SerializeField] private float followDistance = 3f;   // 跟随距离
    [SerializeField] private float followSpeed = 5f;      // 跟随速度
    [SerializeField] private float stopDistance = 1f;     // 停止距离
    
    [Header("性能优化")]
    [SerializeField] private float updateInterval = 0.1f; // 更新间隔
    
    private float lastUpdateTime;
    private Vector3 targetPosition;
    private bool isInitialized = false;
    
    /// <summary>
    /// 初始化跟随组件
    /// </summary>
    /// <param name="summonerTransform">召唤者Transform</param>
    /// <param name="distance">跟随距离</param>
    /// <param name="speed">跟随速度</param>
    public void Initialize(Transform summonerTransform, float distance, float speed)
    {
        summoner = summonerTransform;
        followDistance = distance;
        followSpeed = speed;
        stopDistance = distance * 0.5f; // 停止距离为跟随距离的一半
        isInitialized = true;
        
        // 设置初始目标位置
        if (summoner != null)
        {
            targetPosition = GetFollowPosition();
        }
        
        Debug.Log($"[SummonFollower] {gameObject.name} 初始化跟随组件，跟随距离: {followDistance}，速度: {followSpeed}");
    }
    
    private void Update()
    {
        if (!isInitialized || summoner == null)
        {
            return;
        }
        
        // 性能优化：限制更新频率
        if (Time.time - lastUpdateTime < updateInterval)
        {
            return;
        }
        lastUpdateTime = Time.time;
        
        UpdateFollowBehavior();
    }
    
    /// <summary>
    /// 更新跟随行为
    /// </summary>
    private void UpdateFollowBehavior()
    {
        Vector3 currentFollowPosition = GetFollowPosition();
        float distanceToTarget = Vector3.Distance(transform.position, currentFollowPosition);
        
        // 如果距离过远，需要跟随
        if (distanceToTarget > followDistance)
        {
            targetPosition = currentFollowPosition;
            MoveTowardsTarget();
        }
        // 如果距离太近，停止移动
        else if (distanceToTarget < stopDistance)
        {
            // 可以在这里添加停止移动的逻辑
        }
    }
    
    /// <summary>
    /// 获取跟随位置
    /// </summary>
    /// <returns>目标跟随位置</returns>
    private Vector3 GetFollowPosition()
    {
        if (summoner == null) return transform.position;
        
        // 在召唤者周围随机选择一个位置，避免所有召唤物聚集在同一点
        Vector3 basePosition = summoner.position;
        
        // 添加一些随机偏移，让多个召唤物分散开
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(followDistance * 0.5f, followDistance);
        
        Vector3 offset = new Vector3(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance,
            0f
        );
        
        return basePosition + offset;
    }
    
    /// <summary>
    /// 向目标位置移动
    /// </summary>
    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float moveDistance = followSpeed * Time.deltaTime;
        
        // 使用插值移动，让移动更平滑
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveDistance);
        transform.position = newPosition;
        
        // 让召唤物面向移动方向
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
    
    /// <summary>
    /// 设置新的召唤者
    /// </summary>
    /// <param name="newSummoner">新的召唤者</param>
    public void SetSummoner(Transform newSummoner)
    {
        summoner = newSummoner;
        if (summoner != null)
        {
            targetPosition = GetFollowPosition();
        }
    }
    
    /// <summary>
    /// 停止跟随
    /// </summary>
    public void StopFollowing()
    {
        isInitialized = false;
        summoner = null;
    }
    
    private void OnDestroy()
    {
        StopFollowing();
    }
    
    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        if (summoner == null) return;
        
        // 绘制跟随距离
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(summoner.position, followDistance);
        
        // 绘制停止距离
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(summoner.position, stopDistance);
        
        // 绘制到召唤者的连线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, summoner.position);
        
        // 绘制目标位置
        if (isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }
}