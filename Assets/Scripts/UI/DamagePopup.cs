using TMPro;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
}

public class DamagePopup : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float maxHorizontalOffset = 0.5f;

    [Header("Appearance")]
    public float normalSize = 24f;
    public float criticalSize = 32f;
    public Color physical = Color.red;
    public Color magic = Color.blue;

    [Header("Timing")]
    public float lifetime = 1f;
    public float fadeStart = 0.7f;

    private TextMeshProUGUI text;
    private float timer;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(Vector3 position, int damage, DamageType damageType, bool isMerge = false)

    {
        Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.2f, 0.3f),
                0
            );
        transform.position = position + randomOffset;

        // 设置文本属性
        text.text = damage.ToString();
        text.fontSize = normalSize;
        text.color = damageType == DamageType.Physical ? physical : magic;

        // 添加暴击标记

        // 随机移动方向
        // 重置计时器
        timer = lifetime;
    }

    public void SetupExp(Vector3 position, int exp, bool isMerge = false)

    {
        Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.2f, 0.3f),
                0
            );
        transform.position = position + randomOffset;

        // 设置文本属性
        text.text = "Exp:" + exp.ToString();
        text.fontSize = normalSize;
        text.color = Color.yellow;
        timer = lifetime;
    }

    void Update()
    {
    }
    public void ResetState()
    {
        // 重置文本、位置等显示状态
        transform.localPosition = Vector3.zero;
        if(text!=null)
        {
            text.text = "";
        }
    }
    public void OnEnd()
    {
        DamagePool.Instance.ReturnPopup(this);
    }
}