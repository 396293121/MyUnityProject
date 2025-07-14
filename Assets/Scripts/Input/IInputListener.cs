using UnityEngine;

/// <summary>
/// 输入监听器接口 - 用于自动注册和接收输入事件
/// </summary>
public interface IInputListener
{
    /// <summary>
    /// 移动输入事件
    /// </summary>
    /// <param name="moveInput">移动输入向量</param>
    void OnMoveInput(Vector2 moveInput);
    
    /// <summary>
    /// 跳跃输入事件
    /// </summary>
    void OnJumpInput();
    
    /// <summary>
    /// 跳跃释放事件
    /// </summary>
    void OnJumpReleased();
    
    /// <summary>
    /// 攻击输入事件
    /// </summary>
    void OnAttackInput();
    
    /// <summary>
    /// 技能输入事件
    /// </summary>
    void OnSkillInput();
    
    /// <summary>
    /// 交互输入事件
    /// </summary>
    void OnInteractInput();
    
    /// <summary>
    /// 暂停输入事件
    /// </summary>
    void OnPauseInput();
}