using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 技能组件 - 独立的技能系统，从角色脚本中分离技能逻辑
/// 基于SkillManager和skillDataConfig的架构设计
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class SkillComponent : MonoBehaviour
{
    [TitleGroup("技能系统配置", "独立技能组件配置", TitleAlignments.Centered)]
    [FoldoutGroup("技能系统配置/技能列表", expanded: true)]
    [LabelText("技能数据列表")]
    [Required]
    [InfoBox("角色可使用的技能列表，基于skillDataConfig配置")]
    public List<skillDataConfig> skillDataList = new List<skillDataConfig>();
    
    [FoldoutGroup("技能系统配置/输入设置", expanded: true)]
    [LabelText("技能输入动作")]
    [Required]
    [InfoBox("新输入系统的技能按键配置")]
    public InputActionReference[] skillInputActions = new InputActionReference[4];
    
    [FoldoutGroup("技能系统配置/组件引用", expanded: false)]
    [LabelText("技能释放点")]
    [Required]
    [InfoBox("技能效果的生成位置")]
    public Transform skillSpawnPoint;
    
    [FoldoutGroup("技能系统配置/组件引用")]
    [LabelText("角色动画器")]
    [Required]
    [InfoBox("用于播放技能动画")]
    public Animator characterAnimator;
    
    [FoldoutGroup("技能系统配置/组件引用")]
    [LabelText("角色控制器")]
    [Required]
    [InfoBox("角色控制器引用，用于获取角色状态")]
    public Character characterController;
    
    [TitleGroup("技能状态监控", "实时技能状态信息", TitleAlignments.Centered)]
    [FoldoutGroup("技能状态监控/冷却状态", expanded: true)]
    [LabelText("技能冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();
          [FoldoutGroup("技能状态监控/调试信息", expanded: false)]
    [LabelText("显示调试信息")]
    [InfoBox("在Scene视图中显示技能范围等调试信息")]
    public bool showDebugGizmos = true;
    [FoldoutGroup("技能状态监控/执行状态", expanded: false)]
    [LabelText("正在执行技能")]
    [ReadOnly]
    [ShowInInspector]
    public bool isExecutingSkill = false;
    
    [FoldoutGroup("技能状态监控/执行状态")]
    [LabelText("当前执行的技能")]
    [ReadOnly]
    [ShowInInspector]
    private int currentExecutingSkillIndex = -1;
  

    
    private void Awake()
    {
        // 初始化技能冷却字典
        for (int i = 0; i < skillDataList.Count; i++)
        {
            skillCooldowns[i] = 0f;
        }
        
        // 自动获取组件引用
        if (skillSpawnPoint == null)
            skillSpawnPoint = transform;
        
        if (characterAnimator == null)
            characterAnimator = GetComponent<Animator>();
        
        if (characterController == null)
            characterController = GetComponent<Character>();
        
        // 检查技能数据配置
        Debug.Log($"[SkillComponent] Awake - 技能数据列表数量: {skillDataList?.Count ?? 0}");
        if (skillDataList != null && skillDataList.Count > 0)
        {
            for (int i = 0; i < skillDataList.Count; i++)
            {
                if (skillDataList[i] != null)
                {
                    Debug.Log($"[SkillComponent] 技能 {i}: {skillDataList[i].skillName}");
                }
                else
                {
                    Debug.LogWarning($"[SkillComponent] 技能 {i}: 数据为空");
                }
            }
        }
        else
        {
            Debug.LogWarning("[SkillComponent] 技能数据列表为空或未配置");
        }
    }
    
    private void OnEnable()
    {
        // 启用输入动作
        for (int i = 0; i < skillInputActions.Length; i++)
        {
            if (skillInputActions[i] != null)
            {
                int skillIndex = i; // 捕获循环变量
                skillInputActions[i].action.performed += _ => TryUseSkill(skillIndex);
                skillInputActions[i].action.Enable();
            }
        }
    }
    
    private void OnDisable()
    {
        // 禁用输入动作
        for (int i = 0; i < skillInputActions.Length; i++)
        {
            if (skillInputActions[i] != null)
            {
                skillInputActions[i].action.Disable();
            }
        }
        
        // 停止所有持续伤害协程
        for (int i = 0; i < skillDataList.Count; i++)
        {
            if (skillDataList[i] != null)
            {
                skillDataList[i].StopContinuousDamage(this, i);
            }
        }
    }
    
    private void Update()
    {
        // 更新技能冷却时间
        UpdateSkillCooldowns();
    }
    
    /// <summary>
    /// 更新技能冷却时间
    /// </summary>
    private void UpdateSkillCooldowns()
    {
        var keys = new List<int>(skillCooldowns.Keys);
        foreach (int skillIndex in keys)
        {
            if (skillCooldowns[skillIndex] > 0)
            {
                skillCooldowns[skillIndex] -= Time.deltaTime;
                if (skillCooldowns[skillIndex] <= 0)
                {
                    skillCooldowns[skillIndex] = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// 尝试使用技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    public void TryUseSkill(int skillIndex)
    {
        Debug.Log($"[SkillComponent] TryUseSkill 被调用，技能索引: {skillIndex}");
        
        // 性能优化：通知状态机技能输入
        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.NotifySkillInput();
            Debug.Log("[SkillComponent] 已通知状态机技能输入");
        }
        else
        {
            Debug.LogWarning("[SkillComponent] 未找到PlayerStateMachine组件");
        }
        
        // 检查技能索引有效性
        if (skillIndex < 0 || skillIndex >= skillDataList.Count)
        {
            Debug.LogWarning($"[SkillComponent] 技能索引 {skillIndex} 超出范围，技能列表大小: {skillDataList.Count}");
            return;
        }
        
        // 检查技能数据是否存在
        if (skillDataList[skillIndex] == null)
        {
            Debug.LogWarning($"[SkillComponent] 技能索引 {skillIndex} 的技能数据为空");
            return;
        }
        
        Debug.Log($"[SkillComponent] 技能数据检查通过，技能名称: {skillDataList[skillIndex].skillName}");
        
        // 检查是否可以使用技能
        if (!CanUseSkill(skillIndex))
        {
            Debug.LogWarning($"[SkillComponent] 无法使用技能 {skillIndex}，CanUseSkill返回false");
            return;
        }
        
        Debug.Log($"[SkillComponent] 准备执行技能: {skillDataList[skillIndex].skillName}");
        
        // 执行技能
        ExecuteSkill(skillIndex);
    }
    
    /// <summary>
    /// 动画事件：技能伤害时间开始
    /// 由动画控制器的动画事件调用
    /// </summary>
    public void OnSkillDamageTimeStart()
    {
        if (currentExecutingSkillIndex >= 0 && currentExecutingSkillIndex < skillDataList.Count)
        {
            var skillData = skillDataList[currentExecutingSkillIndex];
            if (skillData != null && skillData.damageTime == skillDataConfig.damageTimeType.time)
            {
                // 调用SkillDataConfig中的持续伤害逻辑
                Vector3 castPosition = characterController?.AttackPoint?.position ?? transform.position;
                skillData.StartContinuousDamage(this, currentExecutingSkillIndex, gameObject, castPosition);
                Debug.Log($"[SkillComponent] 开始持续伤害 - 技能: {skillData.skillName}");
            }
        }
    }

    /// <summary>
    /// 动画事件：技能伤害时间结束
    /// 由动画控制器的动画事件调用
    /// </summary>
    public void OnSkillDamageTimeEnd()
    {
        if (currentExecutingSkillIndex >= 0 && currentExecutingSkillIndex < skillDataList.Count)
        {
            var skillData = skillDataList[currentExecutingSkillIndex];
            if (skillData != null && skillData.damageTime == skillDataConfig.damageTimeType.time)
            {
                // 调用SkillDataConfig中的停止持续伤害逻辑
                skillData.StopContinuousDamage(this, currentExecutingSkillIndex);
                Debug.Log($"[SkillComponent] 结束持续伤害 - 技能: {skillData.skillName}");
            }
        }
    }

    /// <summary>
    /// 检查是否可以使用技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>是否可以使用</returns>
    public bool CanUseSkill(int skillIndex)
    {
        // 检查技能索引是否有效
        if (skillIndex < 0 || skillIndex >= skillDataList.Count || skillDataList[skillIndex] == null)
        {
            Debug.LogWarning($"[SkillComponent] 技能索引无效或技能数据为空");
            return false;
        }
        
        // 检查角色是否存活
        if (characterController == null || !characterController.isAlive)
        {
            Debug.LogWarning($"[SkillComponent] 角色控制器为空或角色已死亡");
            return false;
        }
        
        // 检查是否正在执行其他技能
        if (isExecutingSkill)
        {
            Debug.LogWarning($"[SkillComponent] 正在执行其他技能，当前执行技能索引: {currentExecutingSkillIndex}");
            return false;
        }
        
        // 检查技能冷却
        if (IsSkillOnCooldown(skillIndex))
        {
            Debug.LogWarning($"[SkillComponent] 技能 {skillIndex} 正在冷却中，剩余时间: {GetSkillCooldownRemaining(skillIndex):F2}秒");
            return false;
        }
        
        // 检查法力值
        skillDataConfig skillData = skillDataList[skillIndex];
        if (characterController.currentMana < skillData.manaCost)
        {
            Debug.LogWarning($"[SkillComponent] 法力值不足，需要 {skillData.manaCost}，当前 {characterController.currentMana}");
            return false;
        }
        
        // 使用状态机进行状态判断
        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
        {
            bool canTransition = stateMachine.CanTransitionTo(PlayerState.Skill);
            Debug.Log($"[SkillComponent] 状态机检查结果: {canTransition}，当前状态: {stateMachine.GetCurrentState()}");
            return canTransition;
        }
        
        Debug.LogWarning("[SkillComponent] 未找到状态机组件");
        return true;
    }
    
    /// <summary>
    /// 检查技能是否在冷却中
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>是否在冷却中</returns>
    public bool IsSkillOnCooldown(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillDataList.Count)
            return true;
            
        return skillCooldowns.ContainsKey(skillIndex) && skillCooldowns[skillIndex] > 0;
    }
    
    /// <summary>
    /// 获取技能剩余冷却时间
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>剩余冷却时间</returns>
    public float GetSkillCooldownRemaining(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillDataList.Count)
            return 0f;
            
        return skillCooldowns.ContainsKey(skillIndex) ? Mathf.Max(0f, skillCooldowns[skillIndex]) : 0f;
    }
    
    /// <summary>
    /// 检查技能是否准备就绪
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>是否准备就绪</returns>
    public bool IsSkillReady(int skillIndex)
    {
        return CanUseSkill(skillIndex);
    }
    
    /// <summary>
    /// 获取技能数量
    /// </summary>
    /// <returns>技能数量</returns>
    public int GetSkillCount()
    {
        return skillDataList != null ? skillDataList.Count : 0;
    }
    
    /// <summary>
    /// 设置技能配置
    /// </summary>
    /// <param name="config">技能配置</param>
    public void SetSkillConfig(CharacterSkillConfig config)
    {
        if (config != null)
        {
            // 这里可以根据配置设置技能相关参数
            Debug.Log($"已设置技能配置: {config.name}");
        }
    }
    
    /// <summary>
    /// 设置技能数据
    /// </summary>
    /// <param name="skills">技能数据数组</param>
    public void SetSkillData(skillDataConfig[] skills)
    {
        if (skills != null)
        {
            skillDataList.Clear();
            skillDataList.AddRange(skills);
            
            // 重新初始化冷却字典
            skillCooldowns.Clear();
            for (int i = 0; i < skillDataList.Count; i++)
            {
                skillCooldowns[i] = 0f;
            }
            
            Debug.Log($"已设置 {skills.Length} 个技能数据");
        }
    }
    
    /// <summary>
    /// 设置角色引用
    /// </summary>
    /// <param name="character">角色控制器</param>
    /// <param name="playerController">玩家控制器</param>
    public void SetCharacterReferences(Character character, PlayerController playerController)
    {
        this.characterController = character;
        // 这里可以设置其他需要的引用
        Debug.Log("已设置角色引用");
    }
    

    

    
    /// <summary>
    /// 执行技能
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    private void ExecuteSkill(int skillIndex)
    {
        skillDataConfig skillData = skillDataList[skillIndex];
        
        // 设置技能执行状态
        isExecutingSkill = true;
        currentExecutingSkillIndex = skillIndex;
        
        // 通知状态机进入技能状态
        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.ForceChangeState(PlayerState.Skill);
        }
        
        // 消耗法力值
        characterController.currentMana -= (int)skillData.manaCost;
        
        // 播放技能动画
        if (characterAnimator != null && !string.IsNullOrEmpty(skillData.animationTrigger))
        {
            characterAnimator.SetTrigger(skillData.animationTrigger);
        }
        
        // 播放技能音效
        if (AudioManager.Instance != null && skillData.skillSound != null)
        {
            AudioManager.Instance.PlaySFX(skillData.skillSound.name);
        }
        
        // 执行技能位移
        if (skillData.isMove)
        {
            var movementComponent = gameObject.GetComponent<MonoBehaviour>();
            if (movementComponent != null)
            {
                movementComponent.StartCoroutine(skillData.ExecuteMovement(gameObject,characterController.GetFacingDirection()));
            }
        }
        
        // 开始技能冷却
        skillCooldowns[skillIndex] = skillData.cooldown;
        
        Debug.Log($"执行技能: {skillData.skillName}");
    }
    
  
    
    /// <summary>
    /// Animation Event调用：技能伤害帧触发
    /// </summary>
    public void OnSkillDamageFrame()
    {
        if (isExecutingSkill && currentExecutingSkillIndex >= 0)
        {
            skillDataConfig skillData = skillDataList[currentExecutingSkillIndex];
            Vector3 castPosition = transform.position;
               // 生成技能特效
        if (skillData.skillEffect != null)
        {
            GameObject effect = Instantiate(skillData.skillEffect, skillSpawnPoint.position, skillSpawnPoint.rotation);
            Destroy(effect, 3f); // 3秒后销毁特效
        }
           skillData.ExecuteSkillEffect(gameObject, castPosition, skillSpawnPoint);
        
     
        }
    }
    
    /// <summary>
    /// Animation Event调用：技能结束
    /// </summary>
    public void OnSkillEnd()
    {
        // 停止当前技能的持续伤害效果
        if (currentExecutingSkillIndex >= 0 && skillDataList[currentExecutingSkillIndex] != null)
        {
            skillDataList[currentExecutingSkillIndex].StopContinuousDamage(this, currentExecutingSkillIndex);
        }
        
        isExecutingSkill = false;
        currentExecutingSkillIndex = -1;
        
        // 通知状态机技能结束，让状态机自动转换到合适的状态
        // 状态机会根据当前条件自动选择下一个状态（如Idle或Walking）
        Debug.Log("[SkillComponent] 技能执行结束，状态机将自动转换状态");
    }
    /// <summary>
    /// 获取技能信息（用于UI显示）
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能信息</returns>
    public SkillInfo GetSkillInfo(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillDataList.Count || skillDataList[skillIndex] == null)
        {
            return null;
        }
        
        skillDataConfig skillData = skillDataList[skillIndex];
        return new SkillInfo
        {
            skillName = skillData.skillName,
            skillIcon = skillData.skillIcon,
            description = skillData.description,
            manaCost = (int)skillData.manaCost,
            cooldown = skillData.cooldown,
            remainingCooldown = GetSkillCooldownRemaining(skillIndex),
            canUse = CanUseSkill(skillIndex)
        };
    }
    
    /// <summary>
    /// 调试可视化
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || skillDataList == null) return;
        
        for (int i = 0; i < skillDataList.Count; i++)
        {
            if (skillDataList[i] == null) continue;
            
            var gizmoData = skillDataList[i].GetSkillGizmoData();
            
            // 设置颜色
            Color gizmoColor = gizmoData.hitboxColor;
            gizmoColor.a = gizmoData.hitboxAlpha;
            Gizmos.color = gizmoColor;
            
            // 根据技能类型绘制不同的Gizmo
            switch (gizmoData.skillType)
            {
                case SkillTypeTest.SingleTargetNearest:
                    Gizmos.DrawWireSphere(skillSpawnPoint.position, gizmoData.range);
                    break;
                    
                case SkillTypeTest.SingleTargetCone:
                    DrawConeGizmo(skillSpawnPoint.position, gizmoData.coneRadius, gizmoData.coneAngle);
                    break;
                    
                case SkillTypeTest.SingleTargetBox:
                    Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
                    DrawDirectionalBoxGizmo(skillSpawnPoint.position, forward, 
                        gizmoData.boxForward, gizmoData.boxBackward, 
                        transform.localScale.x > 0 ? gizmoData.boxLeft : gizmoData.boxRight,
                        transform.localScale.x > 0 ? gizmoData.boxRight : gizmoData.boxLeft);
                    break;
                    
                case SkillTypeTest.AreaOfEffect:
                    if (gizmoData.isCircularAOE)
                    {
                        Gizmos.DrawWireSphere(skillSpawnPoint.position, gizmoData.range);
                    }
                    else
                    {
                        DrawDirectionalAOEBoxGizmo(skillSpawnPoint.position, 
                            gizmoData.aoeUp, gizmoData.aoeDown, 
                            transform.localScale.x > 0 ? gizmoData.aoeLeft : gizmoData.aoeRight,
                            transform.localScale.x > 0 ? gizmoData.aoeRight : gizmoData.aoeLeft);
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// 绘制扇形Gizmo
    /// </summary>
    private void DrawConeGizmo(Vector3 origin, float radius, float angle)
    {
        Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        float halfAngle = angle * 0.5f;
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * radius;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * radius;
        
        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + rightBoundary);
        
        // 绘制扇形弧线（使用多条线段模拟）
        int segments = 20;
        float angleStep = angle / segments;
        Vector3 prevPoint = origin + leftBoundary;
        
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = halfAngle - (angleStep * i);
            Vector3 currentPoint = origin + Quaternion.Euler(0, 0, currentAngle) * forward * radius;
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
    
    /// <summary>
    /// 绘制矩形Gizmo
    /// </summary>
    private void DrawBoxGizmo(Vector3 origin, float length, float width)
    {
        Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 boxCenter = origin + forward * (length / 2f);
        Gizmos.DrawWireCube(boxCenter, new Vector3(length, width, 0));
    }
    
    /// <summary>
    /// 绘制基于攻击点的方向性矩形（用于单体攻击）
    /// </summary>
    private void DrawDirectionalBoxGizmo(Vector3 attackPoint, Vector3 forward, 
        float forwardDist, float backwardDist, float leftDist, float rightDist)
    {
        // 计算矩形的总尺寸
        float totalLength = forwardDist + backwardDist;
        float totalWidth = leftDist + rightDist;
        
        // 计算矩形中心点（相对于攻击点的偏移）
        Vector3 centerOffset = Vector3.up * (forwardDist - backwardDist) * 0.5f + Vector3.right * (rightDist - leftDist) * 0.5f;
        Vector3 center = attackPoint + centerOffset;
        // 计算矩形的四个角点
        Gizmos.DrawWireCube(center, new Vector3(totalWidth, totalLength, 0));
        
        // 绘制攻击点标记
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint, 0.1f);
    }
    
    /// <summary>
    /// 绘制基于攻击点的方向性矩形AOE（用于范围攻击）
    /// </summary>
    private void DrawDirectionalAOEBoxGizmo(Vector3 attackPoint, 
        float upDist, float downDist, float leftDist, float rightDist)
    {
        // 计算矩形的总尺寸
        float totalHeight = upDist + downDist;
        float totalWidth = leftDist + rightDist;
        
        // 计算矩形中心点（相对于攻击点的偏移）
        Vector3 centerOffset = Vector3.up * (upDist - downDist) * 0.5f + Vector3.right * (rightDist - leftDist) * 0.5f;
        Vector3 center = attackPoint + centerOffset;
        
        // 绘制矩形边框
        Gizmos.DrawWireCube(center, new Vector3(totalWidth, totalHeight, 0));
        
        // 绘制攻击点标记
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint, 0.1f);
    }
}

/// <summary>
/// 技能信息结构（用于UI显示）
/// </summary>
[System.Serializable]
public class SkillInfo
{
    public string skillName;
    public Sprite skillIcon;
    public string description;
    public int manaCost;
    public float cooldown;
    public float remainingCooldown;
    public bool canUse;
}