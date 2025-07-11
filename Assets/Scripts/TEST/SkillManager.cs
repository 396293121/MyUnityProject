using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class SkillManager : MonoBehaviour
{
    [TabGroup("技能配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "skillName")]
    [LabelText("可用技能列表")]
    public List<SkillDataTest> availableSkills = new List<SkillDataTest>();
    
    [TabGroup("技能配置")]
    [Required]
    [LabelText("技能释放点")]
    public Transform skillSpawnPoint;
    
    [TabGroup("状态监控")]
    [ProgressBar(0, "maxMana", ColorMember = "GetManaBarColor")]
    [LabelText("当前法力值")]
    public float currentMana = 100f;
    
    [TabGroup("状态监控")]
    [Range(50f, 500f)]
    [LabelText("最大法力值")]
    public float maxMana = 100f;
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "技能", ValueLabel = "剩余冷却时间")]
    private Dictionary<SkillDataTest, float> skillCooldowns = new Dictionary<SkillDataTest, float>();
    
    private SimplePlayerController playerController;
    private Animator playerAnimator;
    
    private Color GetManaBarColor()
    {
        float ratio = currentMana / maxMana;
        if (ratio > 0.6f) return Color.blue;
        if (ratio > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    void Start()
    {
        playerController = GetComponent<SimplePlayerController>();
        playerAnimator = GetComponent<Animator>();
        
        // 检查动画组件
        if (playerAnimator == null)
        {
            Debug.LogWarning("未找到Animator组件，技能动画将无法播放！");
        }
        
        // 初始化技能冷却时间
        foreach (var skill in availableSkills)
        {
            skillCooldowns[skill] = 0f;
        }
    }
    
    void Update()
    {
        // 更新技能冷却时间
        UpdateCooldowns();
        
        // 检测技能输入
        HandleSkillInput();
        
        // 恢复法力值
        RegenerateMana();
    }
    
    void HandleSkillInput()
    {
        // Q键 - 技能1
        if (Input.GetKeyDown(KeyCode.Q) && availableSkills.Count > 0)
        {
            UseSkill(availableSkills[0]);
        }
        
        // E键 - 技能2
        if (Input.GetKeyDown(KeyCode.E) && availableSkills.Count > 1)
        {
            UseSkill(availableSkills[1]);
        }
    }
    
    public bool UseSkill(SkillDataTest skill)
    {
        // 检查技能是否可使用
        if (!playerController.CanUseSkill())
        {
            Debug.Log($"玩家组件判断技能无法使用");
            return false;
        }
        
        // 检查冷却时间和法力值
        if (skillCooldowns[skill] > 0 || currentMana < skill.manaCost)
        {
            Debug.Log($"技能 {skill.skillName} 无法使用：冷却中或法力不足");
            return false;
        }
        
        // 消耗法力值
        currentMana -= skill.manaCost;
        currentMana = Mathf.Max(0, currentMana);
        
        // 设置冷却时间
        skillCooldowns[skill] = skill.cooldown;
        
        // 执行技能效果
        ExecuteSkill(skill);
        
        Debug.Log($"使用技能: {skill.skillName}");
        return true;
    }
    
    void ExecuteSkill(SkillDataTest skill)
    {
        // 播放技能动画
        if (playerAnimator != null && !string.IsNullOrEmpty(skill.animationTrigger))
        {
            playerAnimator.SetTrigger(skill.animationTrigger);
            Debug.Log($"播放技能动画: {skill.animationTrigger}");
        }
        
        // 生成技能特效
        if (skill.skillEffect != null && skillSpawnPoint != null)
        {
            GameObject effect = Instantiate(skill.skillEffect, skillSpawnPoint.position, skillSpawnPoint.rotation);
            Destroy(effect, 2f); // 2秒后销毁特效
        }
        
        // 播放技能音效
        if (skill.skillSound != null && AudioManagerTest.Instance != null)
        {
            AudioManagerTest.Instance.PlaySound(skill.skillSound);
        }
        GameEventsTest.onSkill.Invoke();
        // 技能效果执行 依赖技能动画关键帧
    }
    
    /// <summary>
    /// 延迟执行技能效果，配合动画时机
    /// </summary>
    public void OnDelayedSkillEffect(SkillDataTest skill)
    {
        
        // 委托给SkillDataTest执行具体的技能效果
        skill.ExecuteSkillEffect(gameObject, skillSpawnPoint.position, skillSpawnPoint);
    }
    
    void UpdateCooldowns()
    {
        var keys = new List<SkillDataTest>(skillCooldowns.Keys);
        foreach (var skill in keys)
        {
            if (skillCooldowns[skill] > 0)
            {
                skillCooldowns[skill] -= Time.deltaTime;
                skillCooldowns[skill] = Mathf.Max(0, skillCooldowns[skill]);
            }
        }
    }
    
    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += 10f * Time.deltaTime; // 每秒恢复10点法力
            currentMana = Mathf.Min(maxMana, currentMana);
        }
    }
    
    public float GetSkillCooldown(SkillDataTest skill)
    {
        return skillCooldowns.ContainsKey(skill) ? skillCooldowns[skill] : 0f;
    }

#if UNITY_EDITOR
    // 在Scene视图中绘制Gizmos，用于调试技能碰撞框
    private void OnDrawGizmosSelected()
    {
        if (skillSpawnPoint == null) return;

        foreach (var skill in availableSkills)
        {
            if (skill.showHitbox)
            {
                SkillDataTest.SkillGizmoData gizmoData = skill.GetSkillGizmoData();

                Color visualColor = gizmoData.hitboxColor;
                visualColor.a = gizmoData.hitboxAlpha;
                Gizmos.color = visualColor;

                Vector3 drawPosition = skillSpawnPoint.position;

                switch (gizmoData.skillType)
                {
                    case SkillTypeTest.SingleTargetNearest:
                        // 单体攻击显示圆形范围
                        Gizmos.DrawSphere(drawPosition, gizmoData.range);
                        Gizmos.DrawWireSphere(drawPosition, gizmoData.range);
                        break;
                    case SkillTypeTest.SingleTargetCone:
                        {
                            // 根据角色的localScale.x判断朝向，与角色脚本保持一致
                            Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
                            DrawConeGizmo(drawPosition, forward, gizmoData.coneRadius, gizmoData.coneAngle);
                            break;
                        }
                    case SkillTypeTest.SingleTargetBox:
                        {
                            // 根据角色的localScale.x判断朝向，与角色脚本保持一致
                            Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
                            DrawBoxGizmo(drawPosition, forward, gizmoData.boxLength, gizmoData.boxWidth);
                            break;
                        }
                    case SkillTypeTest.AreaOfEffect:
                        if (gizmoData.isCircularAOE)
                        {
                            // 圆形AOE
                            Gizmos.DrawSphere(drawPosition, gizmoData.range);
                            Gizmos.DrawWireSphere(drawPosition, gizmoData.range);
                        }
                        else
                        {
                            // 矩形AOE
                            Vector3 center = drawPosition + transform.forward * (gizmoData.aoeHeight / 2f);
                            Quaternion rotation = Quaternion.LookRotation(transform.forward);
                            Matrix4x4 oldMatrix = Gizmos.matrix;
                            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                            Gizmos.DrawCube(Vector3.zero, new Vector3(gizmoData.aoeWidth, 0.1f, gizmoData.aoeHeight));
                            Gizmos.DrawWireCube(Vector3.zero, new Vector3(gizmoData.aoeWidth, 0.1f, gizmoData.aoeHeight));
                            Gizmos.matrix = oldMatrix;
                        }
                        break;
                }

                // 显示范围标签
                UnityEditor.Handles.Label(drawPosition + Vector3.up * 0.5f, gizmoData.skillType == SkillTypeTest.AreaOfEffect && !gizmoData.isCircularAOE ? $"{gizmoData.aoeWidth}x{gizmoData.aoeHeight}" : $"R:{gizmoData.range}");
            }
        }
    }

    private void DrawConeGizmo(Vector3 origin, Vector3 forward, float radius, float angle)
    {
        // 绘制扇形弧线
        // 对于2D游戏，我们通常在XY平面上绘制，所以使用Vector3.forward作为法线
        Vector3 leftDir = Quaternion.Euler(0, 0, angle / 2) * forward;
        Vector3 rightDir = Quaternion.Euler(0, 0, -angle / 2) * forward;

        // 设置Gizmos颜色
        Gizmos.color = Color.red;
        
        // 绘制扇形弧线
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(origin, Vector3.forward, leftDir, angle, radius);

        // 绘制扇形的两条边
        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);
        
        // 绘制扇形填充区域（半透明）
        Color fillColor = Color.red;
        fillColor.a = 0.2f;
        Gizmos.color = fillColor;
        
        // 绘制扇形的填充三角形
        int segments = Mathf.Max(1, Mathf.RoundToInt(angle / 10f)); // 每10度一个分段
        float angleStep = angle / segments;
        Vector3 prevDir = leftDir;
        
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + angleStep * i;
            Vector3 currentDir = Quaternion.Euler(0, 0, currentAngle) * forward;
            
            // 绘制三角形填充
            Vector3[] triangle = new Vector3[3]
            {
                origin,
                origin + prevDir * radius,
                origin + currentDir * radius
            };
            
            // 使用Handles绘制填充三角形
            UnityEditor.Handles.color = fillColor;
            UnityEditor.Handles.DrawAAConvexPolygon(triangle);
            
            prevDir = currentDir;
        }
    }

    private void DrawBoxGizmo(Vector3 origin, Vector3 forward, float length, float width)
    {
        // 对于2D游戏，我们通常在XY平面上绘制
        Vector3 center = origin + forward * (length / 2f);
        
        // 计算矩形的四个角点
        Vector3 perpendicular = new Vector3(-forward.y, forward.x, 0); // 垂直于forward的方向
        Vector3 halfLength = forward * (length / 2f);
        Vector3 halfWidth = perpendicular * (width / 2f);
        
        Vector3[] corners = new Vector3[4]
        {
            center + halfLength + halfWidth,  // 右上
            center + halfLength - halfWidth,  // 右下
            center - halfLength - halfWidth,  // 左下
            center - halfLength + halfWidth   // 左上
        };
        
        // 绘制矩形边框
        Gizmos.color = Color.red;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
        
        // 绘制填充矩形（半透明）
        Color fillColor = Color.red;
        fillColor.a = 0.2f;
        UnityEditor.Handles.color = fillColor;
        UnityEditor.Handles.DrawAAConvexPolygon(corners);
    }
#endif
}
