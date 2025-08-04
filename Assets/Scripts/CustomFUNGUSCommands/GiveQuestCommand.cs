using Fungus;
using UnityEngine;
    /// <summary>
    /// Fungus自定义命令 - 分发任务
    /// 用于在对话中分发任务给玩家
    /// </summary>
    [CommandInfo("Quest",
                 "Give Quest",
                 "分发任务给玩家")]
                  [AddComponentMenu("")]
    public class GiveQuestCommand :Command 
    {
        [Tooltip("要分发的任务ID")]
        [QuestId(true, "请选择任务")]
        [SerializeField] protected string questId = "";

        [Tooltip("分发成功时的消息")]
        [SerializeField] protected StringData successMessage = new StringData("任务已接受！");

        [Tooltip("分发失败时的消息")]
        [SerializeField] protected StringData failureMessage = new StringData("无法接受此任务。");

        [Tooltip("是否显示结果消息")]
        [SerializeField] protected bool showResultMessage = true;

        public override void OnEnter()
        {
            var questManager = QuestManager.Instance;
            if (questManager == null)
            {
                Debug.LogError("[GiveQuestCommand] QuestManager 实例未找到");
                Continue();
                return;
            }

            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogError("[GiveQuestCommand] 任务ID不能为空");
                Continue();
                return;
            }

            bool success = questManager.StartQuest(questId);
            QuestData quest=questManager.GetQuestById(questId);
            if (showResultMessage)
            {
                string message =quest.questName+" "+ (success ? successMessage.Value : failureMessage.Value);

                // 使用SayDialog显示消息
                var sayDialog = SayDialog.GetSayDialog();
                if (sayDialog != null)
                {
                    sayDialog.SetActive(true);
                    sayDialog.Say(message, true, true, true, true, false, null, delegate {
                        // 消息显示完成后的回调，这里不需要额外操作
                    });
                }
                else
                {
                    Debug.Log($"[GiveQuestCommand] {message}");
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(questId))
            {
                return "错误: 未设置任务ID";
            }
            return "分发任务: " + questId;
        }
    }


