// AtmosphericFog.cs
using UnityEngine;

public class AtmosphericFog : MonoBehaviour
{
    [Header("大气效果")]
    public Color fogColor = new Color(0.7f, 0.8f, 1f, 0.3f);
    public float fogIntensity = 0.3f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        // 应用雾化效果
        Color foggedColor = Color.Lerp(originalColor, fogColor, fogIntensity);
        spriteRenderer.color = foggedColor;
    }
}