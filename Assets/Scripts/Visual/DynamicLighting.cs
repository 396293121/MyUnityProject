// DynamicLighting.cs
using UnityEngine;

public class DynamicLighting : MonoBehaviour
{
    [Header("光照设置")]
    public Gradient lightingGradient;
    public float dayDuration = 120f;
    
    private SpriteRenderer spriteRenderer;
    private float currentTime;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        currentTime += Time.deltaTime;
        float timeOfDay = (currentTime % dayDuration) / dayDuration;
        
        Color lightColor = lightingGradient.Evaluate(timeOfDay);
        spriteRenderer.color = lightColor;
    }
}