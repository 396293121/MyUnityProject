using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

/// <summary>
/// 技能系统使用示例
/// 展示如何在角色上集成和使用新的技能系统
/// </summary>
public class SkillSystemExample : MonoBehaviour
{
    [TitleGroup("技能系统示例", "展示新技能系统的使用方法", TitleAlignments.Centered)]
    [InfoBox("此脚本展示如何将技能系统集成到角色中，替代原有的职业特定技能逻辑")]
    
    [FoldoutGroup("组件引用", expanded: true)]
    [SerializeField, Required] private SkillComponent skillComponent;
    [SerializeField, Required] private BuffManager buffManager;
    [SerializeField, Required] private PlayerController playerController;
    [SerializeField, Required] private Character character;
    
    [FoldoutGroup("配置文件", expanded: true)]
    [SerializeField, Required] private CharacterSkillConfig skillConfig;
    [SerializeField] private skillDataConfig[] warriorSkills;
    
    [FoldoutGroup("运行时状态", expanded: false)]
    [ShowInInspector, ReadOnly] private bool isInitialized = false;
    [ShowInInspector, ReadOnly] private int activeBuffCount = 0;
    
    private void Start()
    {
        InitializeSkillSystem();
    }
    
    /// <summary>
    /// 初始化技能系统
    /// </summary>
    [FoldoutGroup("控制面板", expanded: true)]
    [Button("初始化技能系统")]
    [GUIColor(0.4f, 1f, 0.4f)]
    private void InitializeSkillSystem()
    {
        if (isInitialized)
        {
            Debug.LogWarning("技能系统已经初始化过了");
            return;
        }
        
        // 验证组件
        if (!ValidateComponents())
        {
            Debug.LogError("技能系统初始化失败：缺少必要组件");
            return;
        }
        
        // 设置技能数据
        if (skillConfig != null)
        {
            skillComponent.SetSkillConfig(skillConfig);
        }
        
        if (warriorSkills != null && warriorSkills.Length > 0)
        {
            skillComponent.SetSkillData(warriorSkills);
        }
        
        // 设置角色引用
        skillComponent.SetCharacterReferences(character, playerController);
        
        // 设置BUFF管理器引用
        buffManager.SetCharacterController(character);
        
        isInitialized = true;
        Debug.Log("技能系统初始化完成");
    }
    
    /// <summary>
    /// 验证必要组件
    /// </summary>
    private bool ValidateComponents()
    {
        bool isValid = true;
        
        if (skillComponent == null)
        {
            Debug.LogError("缺少SkillComponent组件");
            isValid = false;
        }
        
        if (buffManager == null)
        {
            Debug.LogError("缺少BuffManager组件");
            isValid = false;
        }
        
        if (playerController == null)
        {
            Debug.LogError("缺少PlayerController组件");
            isValid = false;
        }
        
        if (character == null)
        {
            Debug.LogError("缺少Character组件");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 手动使用技能（用于测试）
    /// </summary>
    [FoldoutGroup("控制面板")]
    [Button("使用技能1")]
    [EnableIf("isInitialized")]
    private void UseSkill1()
    {
        if (skillComponent != null)
        {
            skillComponent.TryUseSkill(0);
        }
    }
    
    [FoldoutGroup("控制面板")]
    [Button("使用技能2")]
    [EnableIf("isInitialized")]
    private void UseSkill2()
    {
        if (skillComponent != null)
        {
            skillComponent.TryUseSkill(1);
        }
    }
    
    [FoldoutGroup("控制面板")]
    [Button("使用技能3")]
    [EnableIf("isInitialized")]
    private void UseSkill3()
    {
        if (skillComponent != null)
        {
            skillComponent.TryUseSkill(2);
        }
    }
    
    /// <summary>
    /// 清除所有BUFF
    /// </summary>
    [FoldoutGroup("控制面板")]
    [Button("清除所有BUFF")]
    [EnableIf("isInitialized")]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void ClearAllBuffs()
    {
        if (buffManager != null)
        {
            buffManager.ClearAllBuffs();
            Debug.Log("已清除所有BUFF");
        }
    }
    
    /// <summary>
    /// 获取技能信息
    /// </summary>
    [FoldoutGroup("控制面板")]
    [Button("显示技能信息")]
    [EnableIf("isInitialized")]
    private void ShowSkillInfo()
    {
        if (skillComponent == null) return;
        
        string info = "=== 技能信息 ===\n";
        
        for (int i = 0; i < skillComponent.GetSkillCount(); i++)
        {
            var skillInfo = skillComponent.GetSkillInfo(i);
            if (skillInfo != null)
            {
                info += $"技能{i + 1}: {skillInfo.skillName}\n";
                info += $"  描述: {skillInfo.description}\n";
                info += $"  冷却: {skillInfo.cooldown}秒\n";
                info += $"  法力: {skillInfo.manaCost}\n";
                info += $"  可用: {(skillComponent.IsSkillReady(i) ? "是" : "否")}\n";
                info += "\n";
            }
        }
        
        Debug.Log(info);
    }
    
    /// <summary>
    /// 显示BUFF信息
    /// </summary>
    [FoldoutGroup("控制面板")]
    [Button("显示BUFF信息")]
    [EnableIf("isInitialized")]
    private void ShowBuffInfo()
    {
        if (buffManager == null) return;
        
        var buffs = buffManager.GetAllActiveBuffs();
        
        if (buffs.Count == 0)
        {
            Debug.Log("当前没有激活的BUFF");
            return;
        }
        
        string info = "=== 激活的BUFF ===\n";
        
        foreach (var buff in buffs)
        {
            info += $"BUFF: {buff.name}\n";
            info += $"  类型: {buff.type}\n";
            info += $"  剩余时间: {buff.remainingTime:F1}秒\n";
            info += $"  效果值: {buff.value}\n";
            info += "\n";
        }
        
        Debug.Log(info);
    }
    
    private void Update()
    {
        if (isInitialized && buffManager != null)
        {
            // 更新BUFF数量显示
            activeBuffCount = buffManager.GetAllActiveBuffs().Count;
        }
    }
    
    /// <summary>
    /// 自动设置组件引用
    /// </summary>
    [FoldoutGroup("控制面板")]
    [Button("自动设置组件引用")]
    [GUIColor(0.4f, 0.4f, 1f)]
    private void AutoSetupComponents()
    {
        if (skillComponent == null)
            skillComponent = GetComponent<SkillComponent>();
            
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
            
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        if (character == null)
            character = GetComponent<Character>();
            
        Debug.Log("已自动设置组件引用");
    }
    
    private void OnValidate()
    {
        // 在编辑器中自动查找组件
        if (Application.isPlaying) return;
        
        if (skillComponent == null)
            skillComponent = GetComponent<SkillComponent>();
            
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
            
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        if (character == null)
            character = GetComponent<Character>();
    }
}