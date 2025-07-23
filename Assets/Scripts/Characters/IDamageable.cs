using UnityEngine;

/// <summary>
/// 可受伤害接口 - 定义可以受到伤害的对象的基本行为
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="hitPoint">命中点</param>
    /// <param name="attacker">攻击者</param>
    void TakeDamage(int damage,DamageType damageType=DamageType.Physical, Vector2 hitPoint = default, Character attacker = null);
    /// <summary>
    /// 是否存活
    /// </summary>
    bool IsAlive { get; }
    
    /// <summary>
    /// 当前生命值
    /// </summary>
    int CurrentHealth { get; }
    
    /// <summary>
    /// 最大生命值
    /// </summary>
    int MaxHealth { get; }
}