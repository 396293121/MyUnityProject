
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void OnEnable()
    {
        GameEventsTest.OnPlayerTakeDamage.AddListener(TakeDamage);
    }

    void OnDisable()
    {
        GameEventsTest.OnPlayerTakeDamage.RemoveListener(TakeDamage);
    }

    public void TakeDamage(float damage)
    {
        
        Debug.Log("大安老师");
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }
}