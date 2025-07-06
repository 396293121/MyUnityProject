using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 暂停菜单UI
/// 显示游戏暂停时的菜单界面，包括继续游戏、存档、读档、设置和退出按钮
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    #region 私有字段
    private Button continueButton;
    private Button saveButton;
    private Button loadButton;
    private Button optionsButton;
    private Button mainMenuButton;
    private Text titleText;
    private Text messageText;

    [Header("UI配置")]
    [SerializeField] private string title = "游戏暂停";
    [SerializeField] private float messageDisplayDuration = 2f;
    [SerializeField] private float buttonHeight = 50f;
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private float panelWidth = 300f;
    [SerializeField] private float panelHeight = 400f;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.7f); // 半透明黑色
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color buttonTextColor = Color.white;
    [SerializeField] private int buttonFontSize = 24;
    [SerializeField] private int titleFontSize = 36;
    [SerializeField] private int messageFontSize = 20;
    #endregion

    #region Unity生命周期
    void Awake()
    {
        CreateUIElements();
        Hide();
    }

    void OnDestroy()
    {
        // 移除按钮点击事件监听器，防止内存泄漏
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        if (loadButton != null) loadButton.onClick.RemoveListener(OnLoadButtonClicked);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(OnOptionsButtonClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
    }
    #endregion

    #region UI创建
    private void CreateUIElements()
    {
        // 创建主面板
        GameObject panelObj = new GameObject("PausePanel");
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.SetParent(transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = panelColor;

        // 创建标题文本
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelRect, false);
        titleText = titleObj.AddComponent<Text>();
        titleText.font = Font.CreateDynamicFontFromOSFont("Arial", titleFontSize);
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = title;
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(panelWidth - 40, 60);

        // 创建按钮容器
        GameObject buttonContainerObj = new GameObject("ButtonContainer");
        RectTransform buttonContainerRect = buttonContainerObj.AddComponent<RectTransform>();
        buttonContainerRect.SetParent(panelRect, false);
        buttonContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonContainerRect.pivot = new Vector2(0.5f, 0.5f);
        buttonContainerRect.sizeDelta = new Vector2(panelWidth - 40, panelHeight - 150); // 调整大小以适应按钮
        VerticalLayoutGroup layoutGroup = buttonContainerObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = buttonSpacing;
        layoutGroup.padding = new RectOffset(0, 0, 10, 10);
        buttonContainerObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 创建按钮
        continueButton = CreateButton(buttonContainerRect, "ContinueButton", "继续游戏", OnContinueButtonClicked);
        saveButton = CreateButton(buttonContainerRect, "SaveButton", "保存游戏", OnSaveButtonClicked);
        loadButton = CreateButton(buttonContainerRect, "LoadButton", "读取游戏", OnLoadButtonClicked);
        optionsButton = CreateButton(buttonContainerRect, "OptionsButton", "设置", OnOptionsButtonClicked);
        mainMenuButton = CreateButton(buttonContainerRect, "MainMenuButton", "返回主菜单", OnMainMenuButtonClicked);

        // 创建消息文本
        GameObject messageObj = new GameObject("MessageText");
        messageObj.transform.SetParent(panelRect, false);
        messageText = messageObj.AddComponent<Text>();
        messageText.font = Font.CreateDynamicFontFromOSFont("Arial", messageFontSize);
        messageText.color = Color.yellow;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.text = "";
        RectTransform messageRect = messageText.rectTransform;
        messageRect.anchorMin = new Vector2(0.5f, 0f);
        messageRect.anchorMax = new Vector2(0.5f, 0f);
        messageRect.pivot = new Vector2(0.5f, 0f);
        messageRect.anchoredPosition = new Vector2(0, 50);
        messageRect.sizeDelta = new Vector2(panelWidth - 40, 40);
        messageText.gameObject.SetActive(false);
    }

    private Button CreateButton(Transform parent, string name, string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        Button button = buttonObj.AddComponent<Button>();
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(panelWidth - 60, buttonHeight);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Font.CreateDynamicFontFromOSFont("Arial", buttonFontSize);
        text.color = buttonTextColor;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = buttonText;
        text.GetComponent<RectTransform>().sizeDelta = button.GetComponent<RectTransform>().sizeDelta;

        button.onClick.AddListener(onClickAction);
        return button;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 显示暂停菜单
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        // 暂停游戏时间流逝
        Time.timeScale = 0f;
        if (titleText != null) titleText.text = title;
    }

    /// <summary>
    /// 隐藏暂停菜单
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        // 恢复游戏时间流逝
        Time.timeScale = 1f;
    }
    #endregion

    #region 按钮点击事件处理
    private void OnContinueButtonClicked()
    {
        Debug.Log("继续游戏按钮点击");
        Hide();
        // 可以在这里添加恢复游戏逻辑，例如取消暂停状态
    }

    private void OnSaveButtonClicked()
    {
        Debug.Log("保存游戏按钮点击");
        // 模拟保存游戏逻辑
        ShowMessage("游戏已保存！");
        // 实际保存逻辑应通过 GameManager 或 SaveSystem 调用
    }

    private void OnLoadButtonClicked()
    {
        Debug.Log("读取游戏按钮点击");
        // 模拟检查存档数据
        bool hasSaveData = true; // 假设有存档数据
        if (hasSaveData)
        {
            ShowMessage("游戏已加载！");
            Hide();
            // 实际加载逻辑应通过 GameManager 或 SaveSystem 调用
        }
        else
        {
            ShowMessage("没有找到存档数据！");
        }
    }

    private void OnOptionsButtonClicked()
    {
        Debug.Log("设置按钮点击");
        // 可以在这里添加显示设置菜单的逻辑
        // 例如: UIManager.Instance.ShowPanel("OptionsUI");
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log("返回主菜单按钮点击");
        // 确保时间流逝恢复，否则场景加载会受影响
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); // 假设主菜单场景名为 MainMenuScene
    }
    #endregion

    #region 消息提示
    private void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(messageDisplayDuration));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // 使用Realtime，不受Time.timeScale影响
        if (messageText != null) messageText.gameObject.SetActive(false);
    }
    #endregion
}