using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 增强版角色选择UI控制器
/// 完全重现Phaser项目的角色选择场景功能
/// </summary>
public class CharacterSelectUIEnhanced : MonoBehaviour
{
    [Header("角色选择项")]
    public CharacterOption[] characterOptions;
    
    [Header("对话框系统")]
    public GameObject dialogPrefab;         // 对话框预制体
    public Transform dialogParent;          // 对话框父对象
    
    [Header("UI组件")]
    public Image backgroundImage;           // 背景图片
    public Image titleImage;                // 标题图片
    public Button backButton;               // 返回按钮
    public Text statusText;                 // 状态文字（可选）
    
    [Header("音频设置")]
    public AudioClip selectSound;           // 选择音效
    public AudioClip clickSound;            // 点击音效
    public AudioClip backgroundMusic;       // 背景音乐
    public bool playBackgroundMusic = true;
    public bool playButtonSounds = true;
    
    [Header("动画设置")]
    public float titleAnimationDelay = 0.5f;    // 标题动画延迟
    public float characterAnimationDelay = 1f;  // 角色动画延迟
    
    // 状态变量
    private CharacterOption selectedCharacter;
    private GameObject currentDialog;
    private AudioSource audioSource;
    private bool isInitialized = false;
    
    // 角色配置数据
    private Dictionary<string, CharacterConfigData> characterConfigs;
    
    void Start()
    {
        InitializeUI();
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        if (isInitialized) return;
        
        // 获取音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 加载角色配置
        LoadCharacterConfigs();
        
        // 设置UI组件
        SetupUIComponents();
        
        // 设置按钮事件
        SetupButtonEvents();
        
        // 播放入场动画
        PlayEntranceAnimation();
        
        // 播放背景音乐
        if (playBackgroundMusic)
        {
            PlayBackgroundMusic();
        }
        
        // 设置游戏状态
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.CharacterSelect);
        }
        
        isInitialized = true;
        Debug.Log("[CharacterSelectUIEnhanced] 增强版角色选择UI初始化完成");
    }
    
    /// <summary>
    /// 加载角色配置
    /// </summary>
    void LoadCharacterConfigs()
    {
        characterConfigs = new Dictionary<string, CharacterConfigData>();
        
        // 默认角色配置
        var defaultConfigs = new CharacterConfigData[]
        {
            new CharacterConfigData
            {
                characterType = "warrior",
                displayName = "战士",
                health = 120,
                mana = 30,
                strength = 15,
                weaponType = "sword",
                attackType = "melee"
            },
            new CharacterConfigData
            {
                characterType = "mage",
                displayName = "法师",
                health = 80,
                mana = 100,
                intelligence = 15,
                weaponType = "staff",
                attackType = "ranged"
            },
            new CharacterConfigData
            {
                characterType = "archer",
                displayName = "射手",
                health = 100,
                mana = 50,
                agility = 15,
                weaponType = "bow",
                attackType = "ranged"
            }
        };
        
        // 从ConfigManager加载或使用默认配置
        if (ConfigManager.Instance != null)
        {
            var availableCharacters = ConfigManager.Instance.GetAvailableCharacters();
            foreach (var characterType in availableCharacters)
            {
                var config = ConfigManager.Instance.GetCharacterConfig(characterType);
                if (config != null)
                {
                    characterConfigs[characterType] = config;
                }
            }
        }
        
        // 如果没有从ConfigManager加载到配置，使用默认配置
        if (characterConfigs.Count == 0)
        {
            foreach (var config in defaultConfigs)
            {
                characterConfigs[config.characterType] = config;
            }
        }
        
        Debug.Log($"[CharacterSelectUIEnhanced] 加载了 {characterConfigs.Count} 个角色配置");
    }
    
    /// <summary>
    /// 设置UI组件
    /// </summary>
    void SetupUIComponents()
    {
        // 设置背景图片
        if (backgroundImage != null)
        {
            var bgSprite = Resources.Load<Sprite>("UI/backgrounds/character_select_bg");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
        }
        
        // 设置标题图片
        if (titleImage != null)
        {
            var titleSprite = Resources.Load<Sprite>("UI/texts/class_selection_title");
            if (titleSprite != null)
            {
                titleImage.sprite = titleSprite;
            }
        }
        
        // 设置角色选择项
        SetupCharacterOptions();
        
        Debug.Log("[CharacterSelectUIEnhanced] UI组件设置完成");
    }
    
    /// <summary>
    /// 设置角色选择项
    /// </summary>
    void SetupCharacterOptions()
    {
        if (characterOptions == null) return;
        
        var characterTypes = new string[] { "warrior", "mage", "archer" };
        var characterNames = new string[] { "战士", "法师", "射手" };
        var shortDescriptions = new string[]
        {
            "近战职业，拥有强大的攻击力和防御力",
            "远程法术职业，擅长元素魔法攻击",
            "远程物理职业，拥有敏捷和精准射击"
        };
        var detailedDescriptions = new string[]
        {
            "战士是游戏中的近战职业，拥有强大的攻击力和防御力。擅长使用各种武器进行近距离战斗，是团队中的主要输出和肉盾。\n\n技能特点：\n• 高生命值和防御力\n• 强大的近战攻击\n• 多种武器精通\n• 战斗怒气系统",
            "法师是游戏中的远程法术职业，擅长使用各种元素魔法进行攻击。虽然防御力较低，但拥有强大的魔法伤害和控制能力。\n\n技能特点：\n• 高魔法值和智力\n• 强大的元素魔法\n• 范围攻击法术\n• 魔法护盾防护",
            "射手是游戏中的远程物理职业，拥有出色的敏捷性和精准的射击技巧。能够在远距离对敌人造成致命伤害。\n\n技能特点：\n• 高敏捷和暴击率\n• 精准的远程攻击\n• 多种箭矢技能\n• 陷阱和追踪能力"
        };
        
        for (int i = 0; i < characterOptions.Length && i < characterTypes.Length; i++)
        {
            if (characterOptions[i] != null)
            {
                // 设置角色信息
                characterOptions[i].SetCharacterInfo(
                    characterTypes[i],
                    characterNames[i],
                    shortDescriptions[i],
                    detailedDescriptions[i]
                );
                
                // 加载角色图片
                LoadCharacterImages(characterOptions[i], characterTypes[i]);
            }
        }
    }
    
    /// <summary>
    /// 加载角色图片
    /// </summary>
    void LoadCharacterImages(CharacterOption option, string characterType)
    {
        // 加载角色完整展示图
        if (option.characterImage != null)
        {
            var characterSprite = Resources.Load<Sprite>($"Characters/{characterType}_character_show");
            if (characterSprite != null)
            {
                option.characterImage.sprite = characterSprite;
            }
        }
        
        // 加载角色文字图片
        if (option.characterText != null)
        {
            var textSprite = Resources.Load<Sprite>($"UI/texts/{characterType}_text");
            if (textSprite != null)
            {
                option.characterText.sprite = textSprite;
            }
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    void SetupButtonEvents()
    {
        // 返回按钮
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
            AddButtonHoverEffect(backButton);
        }
    }
    
    /// <summary>
    /// 添加按钮悬停效果
    /// </summary>
    void AddButtonHoverEffect(Button button)
    {
        var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 鼠标进入
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            button.transform.DOScale(Vector3.one * 1.05f, 0.2f);
            PlaySelectSound();
        });
        eventTrigger.triggers.Add(pointerEnter);
        
        // 鼠标离开
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            button.transform.DOScale(Vector3.one, 0.2f);
        });
        eventTrigger.triggers.Add(pointerExit);
    }
    
    /// <summary>
    /// 播放入场动画
    /// </summary>
    void PlayEntranceAnimation()
    {
        var sequence = DOTween.Sequence();
        
        // 标题动画
        if (titleImage != null)
        {
            titleImage.transform.localScale = Vector3.zero;
            sequence.AppendInterval(titleAnimationDelay);
            sequence.Append(titleImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }
        
        // 角色选择项动画
        if (characterOptions != null)
        {
            sequence.AppendInterval(characterAnimationDelay);
            
            for (int i = 0; i < characterOptions.Length; i++)
            {
                if (characterOptions[i] != null)
                {
                    var option = characterOptions[i];
                    // 动画应该从其当前大小开始，而不是从零开始，因为CharacterOption已经处理了初始缩放
                    
                    sequence.Append(option.transform.DOScale(Vector3.one , 0.3f)
                        .SetEase(Ease.OutBack));
                    
                    if (i < characterOptions.Length - 1)
                    {
                        sequence.AppendInterval(0.1f);
                    }
                }
            }
        }
        
        Debug.Log("[CharacterSelectUIEnhanced] 播放入场动画");
    }
    
    #region 角色选择逻辑
    
    /// <summary>
    /// 选择角色
    /// </summary>
    public void SelectCharacter(CharacterOption character)
    {
        // 播放点击音效
        PlayClickSound();
        
        // 重置所有角色状态
        ResetAllCharacters();
        
        // 设置选中角色
        selectedCharacter = character;
        character.SetSelected(true);
        
        // 显示角色详情对话框
        ShowCharacterDialog(character);
        
        // 更新状态文字
        if (statusText != null)
        {
            statusText.text = $"已选择: {character.GetCharacterName()}";
        }
        
        Debug.Log($"[CharacterSelectUIEnhanced] 选择角色: {character.GetCharacterName()}");
    }
    
    /// <summary>
    /// 重置所有角色状态
    /// </summary>
    void ResetAllCharacters()
    {
        if (characterOptions == null) return;
        
        foreach (var option in characterOptions)
        {
            if (option != null)
            {
                option.SetSelected(false);
            }
        }
    }
    
    /// <summary>
    /// 显示角色详情对话框
    /// </summary>
    void ShowCharacterDialog(CharacterOption character)
    {
        // 如果已有对话框，先关闭
        if (currentDialog != null)
        {
            CloseDialog();
        }
        
        // 创建新对话框
        if (dialogPrefab != null && dialogParent != null)
        {
            currentDialog = Instantiate(dialogPrefab, dialogParent);
            var dialogScript = currentDialog.GetComponent<CharacterDialog>();
            
            if (dialogScript != null)
            {
                // 设置对话框内容
                dialogScript.SetCharacterInfo(
                    character.GetCharacterType(),
                    character.GetCharacterName(),
                    character.GetDetailedDescription()
                );
                
                // 设置回调
                dialogScript.SetStartGameCallback(() => StartGame(character.GetCharacterType()));
                dialogScript.SetCloseCallback(() => {
                    selectedCharacter = null;
                    ResetAllCharacters();
                    if (statusText != null)
                    {
                        statusText.text = "请选择角色";
                    }
                });
                
                // 显示对话框
                dialogScript.ShowDialog();
            }
        }
        
        Debug.Log($"[CharacterSelectUIEnhanced] 显示角色对话框: {character.GetCharacterName()}");
    }
    
    /// <summary>
    /// 关闭对话框
    /// </summary>
    void CloseDialog()
    {
        if (currentDialog != null)
        {
            var dialogScript = currentDialog.GetComponent<CharacterDialog>();
            if (dialogScript != null)
            {
                dialogScript.HideDialog();
            }
            else
            {
                Destroy(currentDialog);
            }
            currentDialog = null;
        }
    }
    
    #endregion
    
    #region 音效播放
    
    /// <summary>
    /// 播放选择音效
    /// </summary>
    public void PlaySelectSound()
    {
        if (!playButtonSounds || selectSound == null || audioSource == null) return;
        
        audioSource.PlayOneShot(selectSound, 0.5f);
    }
    
    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickSound()
    {
        if (!playButtonSounds || clickSound == null || audioSource == null) return;
        
        audioSource.PlayOneShot(clickSound, 0.6f);
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || audioSource == null) return;
        
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
        
        Debug.Log("[CharacterSelectUIEnhanced] 播放背景音乐");
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    void StopBackgroundMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    #endregion
    
    #region 场景切换
    
    /// <summary>
    /// 开始游戏
    /// </summary>
    void StartGame(string characterType)
    {
        Debug.Log($"[CharacterSelectUIEnhanced] 开始游戏，选择角色: {characterType}");
        
        // 保存选择的角色
        PlayerPrefs.SetString("SelectedCharacter", characterType);
        PlayerPrefs.Save();
        
        // 停止背景音乐
        StopBackgroundMusic();
        
        // 加载游戏场景
        LoadScene("TestScene");
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMainMenu()
    {
        PlayClickSound();
        
        Debug.Log("[CharacterSelectUIEnhanced] 返回主菜单");
        
        // 停止背景音乐
        StopBackgroundMusic();
        
        // 返回主菜单
        LoadScene("MainMenuScene");
    }
    
    /// <summary>
    /// 加载场景
    /// </summary>
    void LoadScene(string sceneName)
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    #endregion
    
    #region 生命周期
    
    void OnDestroy()
    {
        // 清理DOTween动画
        DOTween.Kill(this);
        
        // 清理按钮事件
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        
        // 清理对话框
        if (currentDialog != null)
        {
            Destroy(currentDialog);
        }
        
        Debug.Log("[CharacterSelectUIEnhanced] 增强版角色选择UI销毁");
    }
    
    void OnDisable()
    {
        // 停止背景音乐
        StopBackgroundMusic();
    }
    
    #endregion
}