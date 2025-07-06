using UnityEngine;

/// <summary>
/// 相机跟随脚本 - 让相机平滑跟随目标对象
/// 从原Phaser项目的相机系统迁移而来
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;                 // 跟随目标
    public float smoothSpeed = 0.125f;       // 平滑速度
    public Vector3 offset = new Vector3(0, 0, -10); // 相机偏移
    
    [Header("边界限制")]
    public bool useBounds = false;           // 是否使用边界限制
    public float minX = -10f;                // X轴最小值
    public float maxX = 10f;                 // X轴最大值
    public float minY = -10f;                // Y轴最小值
    public float maxY = 10f;                 // Y轴最大值
    
    [Header("死区设置")]
    public bool useDeadZone = false;         // 是否使用死区
    public float deadZoneWidth = 2f;         // 死区宽度
    public float deadZoneHeight = 2f;        // 死区高度
    
    [Header("预测设置")]
    public bool usePrediction = false;       // 是否使用移动预测
    public float predictionTime = 0.5f;      // 预测时间
    
    [Header("震动设置")]
    public bool canShake = true;             // 是否可以震动
    
    // 私有变量
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    
    // 震动相关
    private bool isShaking = false;
    private float shakeIntensity = 0f;
    private float shakeDuration = 0f;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;
    
    // 死区相关
    private Vector3 deadZoneCenter;
    
    private void Start()
    {
        // 如果没有指定目标，尝试找到玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // 初始化
        if (target != null)
        {
            lastTargetPosition = target.position;
            deadZoneCenter = target.position;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // 计算目标速度
        targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
        lastTargetPosition = target.position;
        
        // 计算目标位置
        Vector3 desiredPosition = CalculateDesiredPosition();
        
        // 应用边界限制
        if (useBounds)
        {
            desiredPosition = ApplyBounds(desiredPosition);
        }
        
        // 平滑移动到目标位置
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        
        // 应用震动效果
        if (isShaking && canShake)
        {
            smoothedPosition += CalculateShakeOffset();
            UpdateShake();
        }
        
        transform.position = smoothedPosition;
    }
    
    /// <summary>
    /// 计算期望位置
    /// </summary>
    private Vector3 CalculateDesiredPosition()
    {
        Vector3 targetPos = target.position;
        
        // 使用移动预测
        if (usePrediction)
        {
            targetPos += targetVelocity * predictionTime;
        }
        
        // 使用死区
        if (useDeadZone)
        {
            targetPos = ApplyDeadZone(targetPos);
        }
        
        return targetPos + offset;
    }
    
    /// <summary>
    /// 应用死区逻辑
    /// </summary>
    private Vector3 ApplyDeadZone(Vector3 targetPos)
    {
        Vector3 currentPos = transform.position - offset;
        Vector3 deltaFromCenter = targetPos - deadZoneCenter;
        
        // 检查是否超出死区
        if (Mathf.Abs(deltaFromCenter.x) > deadZoneWidth / 2f)
        {
            deadZoneCenter.x = targetPos.x - Mathf.Sign(deltaFromCenter.x) * deadZoneWidth / 2f;
        }
        
        if (Mathf.Abs(deltaFromCenter.y) > deadZoneHeight / 2f)
        {
            deadZoneCenter.y = targetPos.y - Mathf.Sign(deltaFromCenter.y) * deadZoneHeight / 2f;
        }
        
        return deadZoneCenter;
    }
    
    /// <summary>
    /// 应用边界限制
    /// </summary>
    private Vector3 ApplyBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        return position;
    }
    
    /// <summary>
    /// 计算震动偏移
    /// </summary>
    private Vector3 CalculateShakeOffset()
    {
        float x = Random.Range(-1f, 1f) * shakeIntensity;
        float y = Random.Range(-1f, 1f) * shakeIntensity;
        return new Vector3(x, y, 0);
    }
    
    /// <summary>
    /// 更新震动状态
    /// </summary>
    private void UpdateShake()
    {
        shakeTimer += Time.deltaTime;
        
        if (shakeTimer >= shakeDuration)
        {
            isShaking = false;
            shakeTimer = 0f;
        }
        else
        {
            // 震动强度随时间衰减
            float progress = shakeTimer / shakeDuration;
            shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, progress);
        }
    }
    
    /// <summary>
    /// 开始震动
    /// </summary>
    public void Shake(float duration, float intensity)
    {
        if (!canShake) return;
        
        shakeDuration = duration;
        shakeIntensity = intensity;
        shakeTimer = 0f;
        isShaking = true;
        originalPosition = transform.position;
    }
    
    /// <summary>
    /// 停止震动
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        shakeTimer = 0f;
        shakeIntensity = 0f;
    }
    
    /// <summary>
    /// 手动更新跟随（供外部调用）
    /// </summary>
    public void LateUpdateFollow()
    {
        // 这个方法可以为空，因为LateUpdate()已经处理了相机跟随
        // 或者可以用于强制更新相机位置
    }
    
    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
            deadZoneCenter = target.position;
        }
    }
    
    /// <summary>
    /// 立即移动到目标位置
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        
        if (useBounds)
        {
            desiredPosition = ApplyBounds(desiredPosition);
        }
        
        transform.position = desiredPosition;
        velocity = Vector3.zero;
    }
    
    /// <summary>
    /// 设置边界
    /// </summary>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
    
    /// <summary>
    /// 禁用边界
    /// </summary>
    public void DisableBounds()
    {
        useBounds = false;
    }
    
    /// <summary>
    /// 设置偏移
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    /// <summary>
    /// 设置平滑速度
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Clamp01(speed);
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // 绘制跟随线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position + offset);
        
        // 绘制死区
        if (useDeadZone)
        {
            Gizmos.color = Color.green;
            Vector3 deadZonePos = deadZoneCenter;
            Gizmos.DrawWireCube(deadZonePos, new Vector3(deadZoneWidth, deadZoneHeight, 0));
        }
        
        // 绘制边界
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 boundsCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
            Vector3 boundsSize = new Vector3(maxX - minX, maxY - minY, 0);
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}

/// <summary>
/// 相机震动组件 - 独立的震动组件
/// </summary>
public class CameraShake : MonoBehaviour
{
    private bool isShaking = false;
    private float shakeIntensity = 0f;
    private float shakeDuration = 0f;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;
    
    /// <summary>
    /// 开始震动
    /// </summary>
    public void Shake(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
        shakeTimer = 0f;
        isShaking = true;
        originalPosition = transform.position;
    }
    
    /// <summary>
    /// 停止震动
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        shakeTimer = 0f;
        transform.position = originalPosition;
    }
    
    private void Update()
    {
        if (!isShaking) return;
        
        shakeTimer += Time.deltaTime;
        
        if (shakeTimer >= shakeDuration)
        {
            StopShake();
        }
        else
        {
            // 计算震动偏移
            float progress = shakeTimer / shakeDuration;
            float currentIntensity = Mathf.Lerp(shakeIntensity, 0f, progress);
            
            float x = Random.Range(-1f, 1f) * currentIntensity;
            float y = Random.Range(-1f, 1f) * currentIntensity;
            
            transform.position = originalPosition + new Vector3(x, y, 0);
        }
    }
}