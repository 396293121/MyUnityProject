using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 角色选择UI控制器
/// 从Phaser项目的CharacterSelectScene.js迁移而来
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Button[] characterButtons; // 角色选择按钮
    public Image[] characterPortraits; // 角色头像
    public Text[] characterNames; // 角色名称
    public Text characterDescription; // 角色描述
    public Button confirmButton; // 确认按钮
    public Button backButton; // 返回按钮
    public Image backgroundImage; // 背景图片
    
    [Header("角色预览")]
    public Transform characterPreviewParent; // 角色预览父对象
    public GameObject[] characterPreviews; // 角色预览模型
    
    [Header("音频设置")]
    public bool playBackgroundMusic = true;
    public bool playButtonSounds = true;
    
    [Header("动画设置")]
    public float selectionAnimationTime = 0.3f;
    public float characterPreviewScale = 1.2f;
    
    // 当前选中的角色
    private int selectedCharacterIndex = -1;
    private string selectedCharacterType = "";
    
    // 角色配置数据
    private List<CharacterConfigData> availableCharacters;
    
    // 组件引用
    private Animator[] characterAnimators;
    
    void Start()
    {
        InitializeCharacterSelect();
    }
    
    /// <summary>
    /// 初始化角色选择界面
    /// </summary>
    void InitializeCharacterSelect()
    {
        // 加载角色配置
        LoadCharacterConfigs();
        
        // 设置UI组件
        SetupUI();
        
        // 设置按钮事件
        SetupButtonEvents();
        
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
        
        // 默认选择第一个角色
        if (availableCharacters.Count > 0)
        {
            SelectCharacter(0);
        }
        
        Debug.Log("[CharacterSelectUI] 角色选择界面初始化完成");
    }
    
    /// <summary>
    /// 加载角色配置
    /// </summary>
    void LoadCharacterConfigs()
    {
        availableCharacters = new List<CharacterConfigData>();
        
        if (ConfigManager.Instance != null)
        {
            var characterTypes = ConfigManager.Instance.GetAvailableCharacters();
            foreach (var characterType in characterTypes)
            {
                var config = ConfigManager.Instance.GetCharacterConfig(characterType);
                if (config != null)
                {
                    availableCharacters.Add(config);
                }
            }
        }
        else
        {
            // 默认角色配置
            availableCharacters.Add(new CharacterConfigData
            {
                characterType = "warrior",
                displayName = "战士",
                health = 120,
                mana = 30,
                strength = 15,
                weaponType = "sword"
            });
            
            availableCharacters.Add(new CharacterConfigData
            {
                characterType = "mage",
                displayName = "法师",
                health = 80,
                mana = 100,
                intelligence = 15,
                weaponType = "staff"
            });
            
            availableCharacters.Add(new CharacterConfigData
            {
                characterType = "archer",
                displayName = "射手",
                health = 100,
                mana = 50,
                agility = 15,
                weaponType = "bow"
            });
        }
    }
    
    /// <summary>
    /// 设置UI组件
    /// </summary>
    void SetupUI()
    {
        // 设置角色按钮和头像
        for (int i = 0; i < characterButtons.Length && i < availableCharacters.Count; i++)
        {
            var character = availableCharacters[i];
            
            // 设置角色名称
            if (characterNames != null && i < characterNames.Length && characterNames[i] != null)
            {
                characterNames[i].text = character.displayName;
            }
            
            // 设置角色头像（这里需要根据实际资源路径加载）
            if (characterPortraits != null && i < characterPortraits.Length && characterPortraits[i] != null)
            {
                // 尝试加载角色头像
                var portrait = Resources.Load<Sprite>($"Characters/{character.characterType}_portrait");
                if (portrait != null)
                {
                    characterPortraits[i].sprite = portrait;
                }
            }
            
            // 激活按钮
            if (characterButtons[i] != null)
            {
                characterButtons[i].gameObject.SetActive(true);
            }
        }
        
        // 隐藏多余的按钮
        for (int i = availableCharacters.Count; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                characterButtons[i].gameObject.SetActive(false);
            }
        }
        
        // 初始化角色预览动画器
        if (characterPreviews != null)
        {
            characterAnimators = new Animator[characterPreviews.Length];
            for (int i = 0; i < characterPreviews.Length; i++)
            {
                if (characterPreviews[i] != null)
                {
                    characterAnimators[i] = characterPreviews[i].GetComponent<Animator>();
                    characterPreviews[i].SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    void SetupButtonEvents()
    {
        // 角色选择按钮
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                int index = i; // 闭包变量
                characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
                AddButtonHoverEffect(characterButtons[i]);
            }
        }
        
        // 确认按钮
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            AddButtonHoverEffect(confirmButton);
        }
        
        // 返回按钮
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
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
            button.transform.localScale = Vector3.one * 1.05f;
            PlayButtonSound("button_select");
        });
        eventTrigger.triggers.Add(pointerEnter);
        
        // 鼠标离开
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        eventTrigger.triggers.Add(pointerExit);
    }
    
    /// <summary>
    /// 角色按钮点击事件
    /// </summary>
    void OnCharacterButtonClicked(int index)
    {
        PlayButtonSound("button_click");
        SelectCharacter(index);
    }
    
    /// <summary>
    /// 选择角色
    /// </summary>
    void SelectCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return;
        
        selectedCharacterIndex = index;
        selectedCharacterType = availableCharacters[index].characterType;
        
        // 更新UI显示
        UpdateCharacterSelection();
        
        // 播放角色预览动画
        PlayCharacterPreviewAnimation(index);
        
        // 更新角色描述
        UpdateCharacterDescription(availableCharacters[index]);
        
        Debug.Log($"[CharacterSelectUI] 选择角色: {selectedCharacterType}");
    }
    
    /// <summary>
    /// 更新角色选择显示
    /// </summary>
    void UpdateCharacterSelection()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                // 高亮选中的角色按钮
                var colors = characterButtons[i].colors;
                if (i == selectedCharacterIndex)
                {
                    colors.normalColor = Color.yellow;
                    colors.selectedColor = Color.yellow;
                }
                else
                {
                    colors.normalColor = Color.white;
                    colors.selectedColor = Color.white;
                }
                characterButtons[i].colors = colors;
            }
        }
        
        // 启用确认按钮
        if (confirmButton != null)
        {
            confirmButton.interactable = selectedCharacterIndex >= 0;
        }
    }
    
    /// <summary>
    /// 播放角色预览动画
    /// </summary>
    void PlayCharacterPreviewAnimation(int index)
    {
        // 隐藏所有角色预览
        if (characterPreviews != null)
        {
            for (int i = 0; i < characterPreviews.Length; i++)
            {
                if (characterPreviews[i] != null)
                {
                    characterPreviews[i].SetActive(i == index);
                    
                    // 播放行走动画
                    if (i == index && characterAnimators[i] != null)
                    {
                        characterAnimators[i].SetTrigger("Walk");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 更新角色描述
    /// </summary>
    void UpdateCharacterDescription(CharacterConfigData character)
    {
        if (characterDescription != null)
        {
            string description = $"职业: {character.displayName}\n";
            description += $"生命值: {character.health}\n";
            description += $"魔法值: {character.mana}\n";
            description += $"武器类型: {GetWeaponTypeName(character.weaponType)}\n";
            description += $"攻击类型: {GetAttackTypeName(character.attackType)}";
            
            characterDescription.text = description;
        }
    }
    
    /// <summary>
    /// 获取武器类型名称
    /// </summary>
    string GetWeaponTypeName(string weaponType)
    {
        switch (weaponType)
        {
            case "sword": return "剑";
            case "staff": return "法杖";
            case "bow": return "弓";
            default: return weaponType;
        }
    }
    
    /// <summary>
    /// 获取攻击类型名称
    /// </summary>
    string GetAttackTypeName(string attackType)
    {
        switch (attackType)
        {
            case "melee": return "近战";
            case "ranged": return "远程";
            default: return attackType;
        }
    }
    
    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnConfirmButtonClicked()
    {
        if (selectedCharacterIndex < 0) return;
        
        PlayButtonSound("button_click");
        
        Debug.Log($"[CharacterSelectUI] 确认选择角色: {selectedCharacterType}");
        
        // 保存选择的角色
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacterType);
        PlayerPrefs.Save();
        
        // 停止背景音乐
        StopBackgroundMusic();
        
        // 加载测试场景
        LoadScene("TestScene");
    }
    
    /// <summary>
    /// 返回按钮点击事件
    /// </summary>
    void OnBackButtonClicked()
    {
        PlayButtonSound("button_click");
        
        Debug.Log("[CharacterSelectUI] 返回主菜单");
        
        // 停止背景音乐
        StopBackgroundMusic();
        
        // 返回主菜单
        LoadScene("MainMenuScene");
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    void PlayBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            var audioConfig = ConfigManager.Instance?.GetAudioConfig("character_select_music");
            if (audioConfig != null)
            {
                AudioManager.Instance.PlayMusic("character_select_music", audioConfig.volume, audioConfig.loop);
            }
            else
            {
                AudioManager.Instance.PlayMusic("character_select_music", 0.6f, true);
            }
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    void StopBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
    
    /// <summary>
    /// 播放按钮音效
    /// </summary>
    void PlayButtonSound(string soundKey)
    {
        if (!playButtonSounds) return;
        
        if (AudioManager.Instance != null)
        {
            var audioConfig = ConfigManager.Instance?.GetAudioConfig(soundKey);
            if (audioConfig != null)
            {
                AudioManager.Instance.PlaySFX(soundKey, audioConfig.volume);
            }
            else
            {
                AudioManager.Instance.PlaySFX(soundKey, 0.6f);
            }
        }
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
    
    void OnDestroy()
    {
        // 清理事件监听
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                characterButtons[i].onClick.RemoveAllListeners();
            }
        }
        
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
    }
}