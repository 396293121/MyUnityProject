using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 角色面板 - 显示角色的基本属性和状态信息
/// 实现IPauseMenuContent接口，可作为暂停菜单的一个标签页
/// </summary>
public class CharacterPanel : MonoBehaviour, IPauseMenuContent
{
    [Header("UI引用")]
    [SerializeField] private GameObject characterPanel;
    
    [Header("角色信息显示")]
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterLevelText;
    [SerializeField] private TextMeshProUGUI characterClassText;
    
    [Header("属性显示")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI expText;
    
    [Header("基础属性")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI vitalityText;
    
    [Header("战斗属性")]
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI magicAttackText;
    [SerializeField] private TextMeshProUGUI magicDefenseText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    
    [Header("其他信息")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    
    // 当前显示的角色
    private Character currentCharacter;
    
    private void Awake()
    {
        // 注册到暂停菜单管理器
        RegisterToPauseMenu();
    }
    
    private void Start()
    {
        // 默认隐藏面板
        if (characterPanel != null)
        {
            characterPanel.SetActive(false);
        }
        
        // 获取玩家角色
        GetPlayerCharacter();
    }
    
    /// <summary>
    /// 注册到暂停菜单管理器
    /// </summary>
    private void RegisterToPauseMenu()
    {
        var pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        if (pauseMenuManager != null)
        {
            pauseMenuManager.RegisterTabContent("Character", this);
        }
    }
    
    /// <summary>
    /// 获取玩家角色
    /// </summary>
    private void GetPlayerCharacter()
    {
        // 从UIManager获取当前角色
        // if (UIManager.Instance != null)
        // {
        //     var character = UIManager.Instance.GetCurrentCharacter();
        //     if (character != null)
        //     {
        //         SetCharacter(character);
        //     }
        // }
        
        // 如果UIManager没有角色，尝试从PlayerController获取
        if (currentCharacter == null)
        {
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                var character = playerController.GetPlayerCharacter();
                if (character != null)
                {
                    SetCharacter(character);
                }
            }
        }
    }
    
    /// <summary>
    /// 设置要显示的角色
    /// </summary>
    public void SetCharacter(Character character)
    {
        currentCharacter = character;
        RefreshContent();
    }
    
    /// <summary>
    /// 显示内容
    /// </summary>
    public void ShowContent()
    {
        if (characterPanel != null)
        {
            characterPanel.SetActive(true);
        }
        RefreshContent();
    }
    
    /// <summary>
    /// 隐藏内容
    /// </summary>
    public void HideContent()
    {
        if (characterPanel != null)
        {
            characterPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 刷新内容
    /// </summary>
    public void RefreshContent()
    {
        if (currentCharacter == null)
        {
            GetPlayerCharacter();
            if (currentCharacter == null) return;
        }
        
        UpdateCharacterInfo();
        UpdateAttributes();
        UpdateCombatStats();
        UpdateOtherInfo();
    }
    
    /// <summary>
    /// 更新角色基本信息
    /// </summary>
    private void UpdateCharacterInfo()
    {
        // 角色名称
        if (characterNameText != null)
        {
            characterNameText.text = currentCharacter.name;
        }
        
        // 角色等级
        if (characterLevelText != null)
        {
            characterLevelText.text = $"等级 {currentCharacter.level}";
        }
        
        // 角色职业（如果有的话）
        if (characterClassText != null)
        {
            // 这里可以根据Character类的实际实现来获取职业信息
            characterClassText.text = "战士"; // 默认值，可以根据实际情况修改
        }
        
        // 生命值
        if (healthSlider != null)
        {
            healthSlider.maxValue = currentCharacter.maxHealth;
            healthSlider.value = currentCharacter.currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{currentCharacter.currentHealth}/{currentCharacter.maxHealth}";
        }
        
        // 魔法值
        if (manaSlider != null)
        {
            manaSlider.maxValue = currentCharacter.maxMana;
            manaSlider.value = currentCharacter.currentMana;
        }
        if (manaText != null)
        {
            manaText.text = $"{currentCharacter.currentMana}/{currentCharacter.maxMana}";
        }
        
        // 经验值
        if (expSlider != null)
        {
            expSlider.maxValue = currentCharacter.experienceToNext;
            expSlider.value = currentCharacter.experience;
        }
        if (expText != null)
        {
            expText.text = $"{currentCharacter.experience}/{currentCharacter.experienceToNext}";
        }
    }
    
    /// <summary>
    /// 更新基础属性
    /// </summary>
    private void UpdateAttributes()
    {
        if (strengthText != null)
        {
            strengthText.text = currentCharacter.strength.ToString();
        }
        
        if (agilityText != null)
        {
            agilityText.text = currentCharacter.agility.ToString();
        }
        
        if (intelligenceText != null)
        {
            intelligenceText.text = currentCharacter.intelligence.ToString();
        }
        
        // if (vitalityText != null)
        // {
        //     vitalityText.text = currentCharacter.vitality.ToString();
        // }
    }
    
    /// <summary>
    /// 更新战斗属性
    /// </summary>
    private void UpdateCombatStats()
    {
        if (attackPowerText != null)
        {
            attackPowerText.text = currentCharacter.physicalAttack.ToString();
        }
        
        if (defenseText != null)
        {
            defenseText.text = currentCharacter.defense.ToString();
        }
        
        if (magicAttackText != null)
        {
            magicAttackText.text = currentCharacter.magicalAttack.ToString();
        }
        
        if (magicDefenseText != null)
        {
            magicDefenseText.text = currentCharacter.magicDefense.ToString();
        }
        
        if (moveSpeedText != null)
        {
            moveSpeedText.text = currentCharacter.speed.ToString("F1");
        }
        
        // if (attackSpeedText != null)
        // {
        //     attackSpeedText.text = currentCharacter.attackSpeed.ToString("F1");
        // }
    }
    
    /// <summary>
    /// 更新其他信息
    /// </summary>
    private void UpdateOtherInfo()
    {
        // 金币信息
        if (goldText != null)
        {
            if (GameManager.Instance != null)
            {
                goldText.text = GameManager.Instance.GetPlayerGold().ToString();
            }
            else
            {
                goldText.text = "0";
            }
        }
        
        // 游戏时间
        if (playTimeText != null)
        {
            if (GameManager.Instance != null)
            {
                float playTime = Time.time; // 这里应该使用实际的游戏时间
                int hours = Mathf.FloorToInt(playTime / 3600);
                int minutes = Mathf.FloorToInt((playTime % 3600) / 60);
                playTimeText.text = $"{hours:00}:{minutes:00}";
            }
            else
            {
                playTimeText.text = "00:00";
            }
        }
    }
    
    /// <summary>
    /// 设置角色头像
    /// </summary>
    public void SetCharacterPortrait(Sprite portrait)
    {
        if (characterPortrait != null)
        {
            characterPortrait.sprite = portrait;
        }
    }
}