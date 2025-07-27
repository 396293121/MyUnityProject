// ParallaxBackground.cs
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{

    [Tooltip("水平滚动速度系数")]
    [Range(0f, 1f)]
    public float horizontalSpeed = 0.1f;

    [Tooltip("垂直滚动速度系数")]
    [Range(0f, 1f)]
    public float verticalSpeed = 0f;
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;


    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // 修改移动计算方式
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(
            deltaMovement.x * horizontalSpeed,
            deltaMovement.y * verticalSpeed,
            0
        );

        lastCameraPosition = cameraTransform.position;
    }
}