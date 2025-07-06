using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

/// <summary>
/// 对话系统 - 管理游戏中的对话和剧情
/// 从原Phaser项目的对话系统迁移而来
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [Header("对话配置")]
    public List<DialogueData> dialogues = new List<DialogueData>();
    
    [Header("对话设置")]
    public float textSpeed = 0.05f;              // 文字显示速度
    public bool autoAdvance = false;             // 自动推进对话
    public float autoAdvanceDelay = 3f;          // 自动推进延迟
    public bool showSpeakerName = true;          // 显示说话者名称
    public bool enableVoiceOver = true;          // 启用语音
    public bool allowSkipText = true;            // 允许跳过文字动画
    
    // 单例
    public static DialogueSystem Instance { get; private set; }
    
    // 对话状态
    private Dictionary<string, DialogueData> dialogueDatabase = new Dictionary<string, DialogueData>();
    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private int currentNodeIndex;
    private bool isDialogueActive;
    private bool isTextAnimating;
    private Coroutine textAnimationCoroutine;
    private Coroutine autoAdvanceCoroutine;
    
    // UI引用
    private GameObject dialogueUI;
    private UnityEngine.UI.Text dialogueText;
    private UnityEngine.UI.Text speakerNameText;
    private UnityEngine.UI.Image speakerPortrait;
    private Transform choiceButtonContainer;
    private List<UnityEngine.UI.Button> choiceButtons = new List<UnityEngine.UI.Button>();
    
    // 事件
    public event Action<DialogueData> OnDialogueStarted;
    public event Action<DialogueData> OnDialogueEnded;
    public event Action<DialogueNode> OnNodeChanged;
    public event Action<DialogueChoice> OnChoiceSelected;
    public event Action<string> OnDialogueVariableChanged;
    
    // 对话变量
    private Dictionary<string, object> dialogueVariables = new Dictionary<string, object>();
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDialogueSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化对话系统
    /// </summary>
    private void InitializeDialogueSystem()
    {
        // 构建对话数据库
        foreach (DialogueData dialogue in dialogues)
        {
            if (!string.IsNullOrEmpty(dialogue.dialogueId))
            {
                dialogueDatabase[dialogue.dialogueId] = dialogue;
            }
        }
        
        // 创建默认对话
        if (dialogues.Count == 0)
        {
            CreateDefaultDialogues();
        }
        
        // 初始化UI
        InitializeDialogueUI();
    }
    
    /// <summary>
    /// 创建默认对话
    /// </summary>
    private void CreateDefaultDialogues()
    {
        // NPC问候对话
        DialogueData greetingDialogue = new DialogueData
        {
            dialogueId = "npc_greeting",
            dialogueName = "NPC问候",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    nodeId = "greeting_1",
                    speakerName = "村民",
                    text = "你好，勇敢的冒险者！欢迎来到我们的村庄。",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { text = "你好，这里有什么任务吗？", nextNodeId = "quest_info" },
                        new DialogueChoice { text = "谢谢，我只是路过。", nextNodeId = "farewell" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "quest_info",
                    speakerName = "村民",
                    text = "确实有一些野猪在骚扰我们的农田，如果你能帮忙清理它们就太好了！",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { text = "我接受这个任务。", nextNodeId = "accept_quest", action = "start_quest:kill_001" },
                        new DialogueChoice { text = "我考虑一下。", nextNodeId = "farewell" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "accept_quest",
                    speakerName = "村民",
                    text = "太好了！野猪通常在村庄东边的森林里出没。小心点，它们很凶猛！",
                    isEndNode = true
                },
                new DialogueNode
                {
                    nodeId = "farewell",
                    speakerName = "村民",
                    text = "祝你旅途愉快！如果改变主意了，随时来找我。",
                    isEndNode = true
                }
            }
        };
        
        // 商人对话
        DialogueData merchantDialogue = new DialogueData
        {
            dialogueId = "merchant_trade",
            dialogueName = "商人交易",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    nodeId = "merchant_1",
                    speakerName = "商人",
                    text = "欢迎光临！我这里有各种冒险用品。",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { text = "我想看看你的商品。", nextNodeId = "open_shop", action = "open_shop:general_shop" },
                        new DialogueChoice { text = "有什么特别推荐的吗？", nextNodeId = "recommendation" },
                        new DialogueChoice { text = "谢谢，我先看看。", nextNodeId = "farewell_merchant" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "recommendation",
                    speakerName = "商人",
                    text = "我推荐这些治疗药水，在冒险中非常有用！",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { text = "打开商店", nextNodeId = "open_shop", action = "open_shop:general_shop" },
                        new DialogueChoice { text = "我再考虑考虑。", nextNodeId = "farewell_merchant" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "open_shop",
                    speakerName = "商人",
                    text = "请随意挑选！",
                    isEndNode = true
                },
                new DialogueNode
                {
                    nodeId = "farewell_merchant",
                    speakerName = "商人",
                    text = "随时欢迎回来！",
                    isEndNode = true
                }
            }
        };
        
        dialogues.AddRange(new[] { greetingDialogue, merchantDialogue });
        
        // 更新数据库
        foreach (DialogueData dialogue in dialogues)
        {
            dialogueDatabase[dialogue.dialogueId] = dialogue;
        }
    }
    
    /// <summary>
    /// 初始化对话UI
    /// </summary>
    private void InitializeDialogueUI()
    {
        // 这里需要根据实际的UI系统设置
        // 暂时使用查找的方式，实际项目中应该通过Inspector分配
        
        // 查找对话UI
        dialogueUI = GameObject.Find("DialogueUI");
        if (dialogueUI != null)
        {
            dialogueText = dialogueUI.transform.Find("DialogueText")?.GetComponent<UnityEngine.UI.Text>();
            speakerNameText = dialogueUI.transform.Find("SpeakerName")?.GetComponent<UnityEngine.UI.Text>();
            speakerPortrait = dialogueUI.transform.Find("SpeakerPortrait")?.GetComponent<UnityEngine.UI.Image>();
            choiceButtonContainer = dialogueUI.transform.Find("ChoiceButtons");
            
            // 初始时隐藏对话UI
            dialogueUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 开始对话
    /// </summary>
    public bool StartDialogue(string dialogueId, string startNodeId = null)
    {
        if (!dialogueDatabase.ContainsKey(dialogueId))
        {
            Debug.LogWarning($"Dialogue '{dialogueId}' not found!");
            return false;
        }
        
        if (isDialogueActive)
        {
            EndDialogue();
        }
        
        currentDialogue = dialogueDatabase[dialogueId];
        
        // 查找起始节点
        if (!string.IsNullOrEmpty(startNodeId))
        {
            currentNode = currentDialogue.nodes.FirstOrDefault(n => n.nodeId == startNodeId);
            currentNodeIndex = currentDialogue.nodes.IndexOf(currentNode);
        }
        else
        {
            currentNode = currentDialogue.nodes.FirstOrDefault();
            currentNodeIndex = 0;
        }
        
        if (currentNode == null)
        {
            Debug.LogWarning($"Start node not found in dialogue '{dialogueId}'!");
            return false;
        }
        
        isDialogueActive = true;
        
        // 显示对话UI
        ShowDialogueUI();
        
        // 显示当前节点
        DisplayCurrentNode();
        
        // 触发事件
        OnDialogueStarted?.Invoke(currentDialogue);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("dialogue_start");
        }
        
        return true;
    }
    
    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (!isDialogueActive) return;
        
        DialogueData endingDialogue = currentDialogue;
        
        // 停止所有协程
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }
        
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
        
        // 重置状态
        isDialogueActive = false;
        isTextAnimating = false;
        currentDialogue = null;
        currentNode = null;
        currentNodeIndex = 0;
        
        // 隐藏对话UI
        HideDialogueUI();
        
        // 触发事件
        OnDialogueEnded?.Invoke(endingDialogue);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("dialogue_end");
        }
    }
    
    /// <summary>
    /// 显示当前节点
    /// </summary>
    private void DisplayCurrentNode()
    {
        if (currentNode == null) return;
        
        // 检查条件
        if (!EvaluateConditions(currentNode.conditions))
        {
            // 如果条件不满足，尝试下一个节点或结束对话
            AdvanceToNextNode();
            return;
        }
        
        // 执行节点动作
        ExecuteActions(currentNode.actions);
        
        // 显示说话者名称
        if (showSpeakerName && speakerNameText != null)
        {
            speakerNameText.text = currentNode.speakerName;
        }
        
        // 显示说话者头像
        if (speakerPortrait != null && !string.IsNullOrEmpty(currentNode.portraitPath))
        {
            // 加载头像精灵
            Sprite portrait = Resources.Load<Sprite>(currentNode.portraitPath);
            if (portrait != null)
            {
                speakerPortrait.sprite = portrait;
                speakerPortrait.gameObject.SetActive(true);
            }
        }
        
        // 显示文本
        DisplayText(currentNode.text);
        
        // 播放语音
        if (enableVoiceOver && !string.IsNullOrEmpty(currentNode.voiceClipPath))
        {
            PlayVoiceOver(currentNode.voiceClipPath);
        }
        
        // 触发事件
        OnNodeChanged?.Invoke(currentNode);
    }
    
    /// <summary>
    /// 显示文本
    /// </summary>
    private void DisplayText(string text)
    {
        if (dialogueText == null) return;
        
        // 停止之前的文本动画
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        
        // 处理文本变量
        string processedText = ProcessTextVariables(text);
        
        // 开始文本动画
        textAnimationCoroutine = StartCoroutine(AnimateText(processedText));
    }
    
    /// <summary>
    /// 文本动画协程
    /// </summary>
    private IEnumerator AnimateText(string text)
    {
        isTextAnimating = true;
        dialogueText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            dialogueText.text = text.Substring(0, i);
            
            // 播放打字音效
            if (i < text.Length && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("typewriter", 0.1f);
            }
            
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTextAnimating = false;
        
        // 显示选择按钮或设置自动推进
        if (currentNode.choices.Count > 0)
        {
            ShowChoiceButtons();
        }
        else if (autoAdvance && !currentNode.isEndNode)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
        else if (currentNode.isEndNode)
        {
            // 延迟结束对话
            yield return new WaitForSeconds(1f);
            EndDialogue();
        }
    }
    
    /// <summary>
    /// 自动推进协程
    /// </summary>
    private IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        AdvanceToNextNode();
    }
    
    /// <summary>
    /// 显示选择按钮
    /// </summary>
    private void ShowChoiceButtons()
    {
        if (choiceButtonContainer == null) return;
        
        // 清除现有按钮
        ClearChoiceButtons();
        
        // 创建选择按钮
        foreach (DialogueChoice choice in currentNode.choices)
        {
            // 检查选择条件
            if (!EvaluateConditions(choice.conditions)) continue;
            
            // 创建按钮（这里需要根据实际UI系统实现）
            GameObject buttonObj = CreateChoiceButton(choice.text);
            if (buttonObj != null)
            {
                UnityEngine.UI.Button button = buttonObj.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    // 添加点击事件
                    DialogueChoice capturedChoice = choice; // 闭包捕获
                    button.onClick.AddListener(() => SelectChoice(capturedChoice));
                    choiceButtons.Add(button);
                }
            }
        }
    }
    
    /// <summary>
    /// 创建选择按钮
    /// </summary>
    private GameObject CreateChoiceButton(string text)
    {
        // 这里需要根据实际的UI系统实现
        // 可以使用预制体或动态创建
        return null;
    }
    
    /// <summary>
    /// 清除选择按钮
    /// </summary>
    private void ClearChoiceButtons()
    {
        foreach (UnityEngine.UI.Button button in choiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        choiceButtons.Clear();
    }
    
    /// <summary>
    /// 选择选项
    /// </summary>
    public void SelectChoice(DialogueChoice choice)
    {
        if (!isDialogueActive) return;
        
        // 执行选择动作
        ExecuteActions(choice.actions);
        
        // 触发事件
        OnChoiceSelected?.Invoke(choice);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("choice_select");
        }
        
        // 跳转到下一个节点
        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            JumpToNode(choice.nextNodeId);
        }
        else
        {
            AdvanceToNextNode();
        }
    }
    
    /// <summary>
    /// 跳转到指定节点
    /// </summary>
    public void JumpToNode(string nodeId)
    {
        if (currentDialogue == null) return;
        
        DialogueNode targetNode = currentDialogue.nodes.FirstOrDefault(n => n.nodeId == nodeId);
        if (targetNode != null)
        {
            currentNode = targetNode;
            currentNodeIndex = currentDialogue.nodes.IndexOf(targetNode);
            
            // 清除选择按钮
            ClearChoiceButtons();
            
            // 显示新节点
            DisplayCurrentNode();
        }
        else
        {
            Debug.LogWarning($"Node '{nodeId}' not found!");
            EndDialogue();
        }
    }
    
    /// <summary>
    /// 推进到下一个节点
    /// </summary>
    public void AdvanceToNextNode()
    {
        if (currentDialogue == null) return;
        
        currentNodeIndex++;
        if (currentNodeIndex < currentDialogue.nodes.Count)
        {
            currentNode = currentDialogue.nodes[currentNodeIndex];
            
            // 清除选择按钮
            ClearChoiceButtons();
            
            // 显示新节点
            DisplayCurrentNode();
        }
        else
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// 跳过文本动画
    /// </summary>
    public void SkipTextAnimation()
    {
        if (!allowSkipText || !isTextAnimating) return;
        
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }
        
        // 立即显示完整文本
        if (dialogueText != null && currentNode != null)
        {
            dialogueText.text = ProcessTextVariables(currentNode.text);
        }
        
        isTextAnimating = false;
        
        // 显示选择按钮或设置自动推进
        if (currentNode.choices.Count > 0)
        {
            ShowChoiceButtons();
        }
        else if (autoAdvance && !currentNode.isEndNode)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }
    
    /// <summary>
    /// 处理文本变量
    /// </summary>
    private string ProcessTextVariables(string text)
    {
        string processedText = text;
        
        // 替换变量 {variable_name}
        foreach (var kvp in dialogueVariables)
        {
            string placeholder = "{" + kvp.Key + "}";
            if (processedText.Contains(placeholder))
            {
                processedText = processedText.Replace(placeholder, kvp.Value.ToString());
            }
        }
        
        // 替换玩家名称
        if (GameManager.Instance != null)
        {
            processedText = processedText.Replace("{player_name}", GameManager.Instance.GetPlayerName());
            processedText = processedText.Replace("{player_level}", GameManager.Instance.GetPlayerLevel().ToString());
        }
        
        return processedText;
    }
    
    /// <summary>
    /// 评估条件
    /// </summary>
    private bool EvaluateConditions(List<DialogueCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        
        foreach (DialogueCondition condition in conditions)
        {
            if (!EvaluateCondition(condition))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 评估单个条件
    /// </summary>
    private bool EvaluateCondition(DialogueCondition condition)
    {
        switch (condition.type)
        {
            case ConditionType.Variable:
                return EvaluateVariableCondition(condition);
            case ConditionType.QuestStatus:
                return EvaluateQuestCondition(condition);
            case ConditionType.PlayerLevel:
                return EvaluatePlayerLevelCondition(condition);
            case ConditionType.HasItem:
                return EvaluateItemCondition(condition);
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 评估变量条件
    /// </summary>
    private bool EvaluateVariableCondition(DialogueCondition condition)
    {
        if (!dialogueVariables.ContainsKey(condition.variableName))
            return false;
        
        object value = dialogueVariables[condition.variableName];
        
        switch (condition.comparison)
        {
            case ComparisonType.Equal:
                return value.ToString() == condition.value;
            case ComparisonType.NotEqual:
                return value.ToString() != condition.value;
            case ComparisonType.GreaterThan:
                if (float.TryParse(value.ToString(), out float val1) && float.TryParse(condition.value, out float val2))
                    return val1 > val2;
                break;
            case ComparisonType.LessThan:
                if (float.TryParse(value.ToString(), out float val3) && float.TryParse(condition.value, out float val4))
                    return val3 < val4;
                break;
        }
        
        return false;
    }
    
    /// <summary>
    /// 评估任务条件
    /// </summary>
    private bool EvaluateQuestCondition(DialogueCondition condition)
    {
        if (QuestSystem.Instance == null) return false;
        
        switch (condition.value)
        {
            case "completed":
                return QuestSystem.Instance.IsQuestCompleted(condition.questId);
            case "active":
                return QuestSystem.Instance.IsQuestActive(condition.questId);
            case "not_started":
                return !QuestSystem.Instance.IsQuestCompleted(condition.questId) && 
                       !QuestSystem.Instance.IsQuestActive(condition.questId);
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 评估玩家等级条件
    /// </summary>
    private bool EvaluatePlayerLevelCondition(DialogueCondition condition)
    {
        if (GameManager.Instance == null) return false;
        
        int playerLevel = GameManager.Instance.GetPlayerLevel();
        if (int.TryParse(condition.value, out int requiredLevel))
        {
            switch (condition.comparison)
            {
                case ComparisonType.GreaterThan:
                    return playerLevel > requiredLevel;
                case ComparisonType.LessThan:
                    return playerLevel < requiredLevel;
                case ComparisonType.Equal:
                    return playerLevel == requiredLevel;
                case ComparisonType.NotEqual:
                    return playerLevel != requiredLevel;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 评估物品条件
    /// </summary>
    private bool EvaluateItemCondition(DialogueCondition condition)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Inventory inventory = player.GetInventory();
            if (inventory != null)
            {
                Item item = GetItemById(condition.itemId);
                 if (item != null)
                 {
                     int itemCount = inventory.GetItemCount(item);
                     if (int.TryParse(condition.value, out int requiredCount))
                     {
                         return itemCount >= requiredCount;
                     }
                 }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 执行动作
    /// </summary>
    private void ExecuteActions(List<DialogueAction> actions)
    {
        if (actions == null) return;
        
        foreach (DialogueAction action in actions)
        {
            ExecuteAction(action);
        }
    }
    
    /// <summary>
    /// 执行单个动作
    /// </summary>
    private void ExecuteAction(DialogueAction action)
    {
        switch (action.type)
        {
            case ActionType.SetVariable:
                SetDialogueVariable(action.variableName, action.value);
                break;
            case ActionType.StartQuest:
                if (QuestSystem.Instance != null)
                    QuestSystem.Instance.StartQuest(action.questId);
                break;
            case ActionType.GiveItem:
                GiveItemToPlayer(action.itemId, int.Parse(action.value));
                break;
            case ActionType.OpenShop:
                if (ShopSystem.Instance != null)
                {
                    PlayerController player = FindObjectOfType<PlayerController>();
                    if (player != null)
                        ShopSystem.Instance.OpenShop(action.shopId, player);
                }
                break;
            case ActionType.PlaySound:
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(action.soundId);
                break;
        }
    }
    
    /// <summary>
    /// 根据ID获取物品对象
    /// </summary>
    private Item GetItemById(string itemId)
    {
        if (GameConfig.Instance != null)
        {
            ItemData itemData = GameConfig.Instance.GetItemData(itemId);
            if (itemData != null)
            {
                return new Item(itemData.itemId, itemData.itemName, itemData.description, itemData.itemType, itemData.rarity);
            }
        }
        return null;
    }
    
    /// <summary>
    /// 给予物品给玩家
    /// </summary>
    private void GiveItemToPlayer(string itemId, int amount)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Inventory inventory = player.GetInventory();
            if (inventory != null)
            {
                Item item = GetItemById(itemId);
                if (item != null)
                {
                    inventory.AddItem(item, amount);
                }
            }
        }
    }
    
    /// <summary>
    /// 播放语音
    /// </summary>
    private void PlayVoiceOver(string voiceClipPath)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVoice(voiceClipPath);
        }
    }
    
    /// <summary>
    /// 显示对话UI
    /// </summary>
    private void ShowDialogueUI()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }
        
        // 暂停游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }
    
    /// <summary>
    /// 隐藏对话UI
    /// </summary>
    private void HideDialogueUI()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
        
        // 清除选择按钮
        ClearChoiceButtons();
        
        // 恢复游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    /// <summary>
    /// 设置对话变量
    /// </summary>
    public void SetDialogueVariable(string variableName, object value)
    {
        dialogueVariables[variableName] = value;
        OnDialogueVariableChanged?.Invoke(variableName);
    }
    
    /// <summary>
    /// 获取对话变量
    /// </summary>
    public object GetDialogueVariable(string variableName)
    {
        return dialogueVariables.ContainsKey(variableName) ? dialogueVariables[variableName] : null;
    }
    
    /// <summary>
    /// 检查对话是否激活
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
    
    /// <summary>
    /// 获取对话数据
    /// </summary>
    public DialogueData GetDialogueData(string dialogueId)
    {
        return dialogueDatabase.ContainsKey(dialogueId) ? dialogueDatabase[dialogueId] : null;
    }
    
    private void Update()
    {
        // 处理输入
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTextAnimating)
                {
                    SkipTextAnimation();
                }
                else if (currentNode != null && currentNode.choices.Count == 0 && !currentNode.isEndNode)
                {
                    AdvanceToNextNode();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
    }
}

/// <summary>
/// 对话数据
/// </summary>
[Serializable]
public class DialogueData
{
    public string dialogueId;
    public string dialogueName;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

/// <summary>
/// 对话节点
/// </summary>
[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string speakerName;
    public string text;
    public string portraitPath;
    public string voiceClipPath;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public List<DialogueCondition> conditions = new List<DialogueCondition>();
    public List<DialogueAction> actions = new List<DialogueAction>();
    public bool isEndNode = false;
}

/// <summary>
/// 对话选择
/// </summary>
[Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeId;
    public List<DialogueCondition> conditions = new List<DialogueCondition>();
    public List<DialogueAction> actions = new List<DialogueAction>();
    public string action; // 简化的动作字符串，格式："action_type:parameter"
}

/// <summary>
/// 对话条件
/// </summary>
[Serializable]
public class DialogueCondition
{
    public ConditionType type;
    public string variableName;
    public string questId;
    public string itemId;
    public ComparisonType comparison;
    public string value;
}

/// <summary>
/// 对话动作
/// </summary>
[Serializable]
public class DialogueAction
{
    public ActionType type;
    public string variableName;
    public string questId;
    public string itemId;
    public string shopId;
    public string soundId;
    public string value;
}

/// <summary>
/// 条件类型枚举
/// </summary>
public enum ConditionType
{
    Variable,     // 变量条件
    QuestStatus,  // 任务状态
    PlayerLevel,  // 玩家等级
    HasItem       // 拥有物品
}

/// <summary>
/// 比较类型枚举
/// </summary>
public enum ComparisonType
{
    Equal,        // 等于
    NotEqual,     // 不等于
    GreaterThan,  // 大于
    LessThan      // 小于
}

/// <summary>
/// 动作类型枚举
/// </summary>
public enum ActionType
{
    SetVariable,  // 设置变量
    StartQuest,   // 开始任务
    GiveItem,     // 给予物品
    OpenShop,     // 打开商店
    PlaySound     // 播放音效
}