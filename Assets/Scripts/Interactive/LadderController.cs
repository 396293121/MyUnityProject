using UnityEngine;

[ExecuteInEditMode] // 允许在编辑模式下实时调整
public class LadderController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer topPart;
    public SpriteRenderer middlePart;
    public SpriteRenderer bottomPart;
    public BoxCollider2D ladderCollider;

    [Header("Settings")]
    [Min(0.1f)] public float totalHeight = 3f;
    [Tooltip("检测向上偏移距离")]
    public float topOffset = 1f;
    public bool autoUpdate = true;

    // 存储各部分高度
    private float topHeight;
    private float bottomHeight;
    private float middleHeight;

    private void Start()
    {
        CacheHeights();
        UpdateLadder();
    }

    private void OnValidate()
    {
        if (autoUpdate && topPart && middlePart && bottomPart)
        {
            CacheHeights();
           UpdateLadder();
        }
    }

    // 缓存各部分初始高度
    private void CacheHeights()
    {
        topHeight = topPart.bounds.size.y;
        bottomHeight = bottomPart.bounds.size.y;
    }

    // 更新梯子尺寸和位置
    public void UpdateLadder()
    {
        if (totalHeight < topHeight + bottomHeight)
        {
            Debug.LogWarning($"Total height too small! Min height: {topHeight + bottomHeight}");
            totalHeight = topHeight + bottomHeight;
        }

        // 计算中间部分高度
        middleHeight = totalHeight - topHeight - bottomHeight;

        // 设置中间部分尺寸（Y轴拉伸）
        Vector2 middleSize = middlePart.size;
        middleSize.y = middleHeight;
        middlePart.size = middleSize;

        // 定位各部分
        Vector3 bottomPos = Vector3.zero;
        bottomPart.transform.localPosition = bottomPos;

        Vector3 middlePos = bottomPos;
        middlePos.y += bottomHeight ;
        middlePart.transform.localPosition = middlePos;

        Vector3 topPos = bottomPos;
        topPos.y += bottomHeight + middleHeight;
        topPart.transform.localPosition = topPos;

        // 更新碰撞体
        UpdateCollider();
    }

    // 调整碰撞体尺寸
    private void UpdateCollider()
    {
        if (!ladderCollider) return;

        // 设置碰撞体尺寸
        ladderCollider.size = new Vector2(
            middlePart.bounds.size.x, // 宽度取中间部分宽度
            totalHeight+topOffset
        );

        // 定位碰撞体中心
        ladderCollider.offset = new Vector2(
            0,
            bottomHeight + middleHeight/2+topOffset/2
        );
    }

    // 编辑器辅助：在Scene视图中显示高度控制
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * totalHeight;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(start, 0.1f);
        Gizmos.DrawSphere(end, 0.1f);
    }
}