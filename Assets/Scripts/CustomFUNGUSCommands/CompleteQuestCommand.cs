using Fungus;
using UnityEngine;
    /// <summary>
    /// Fungus自定义命令 - 分发任务
    /// 用于在对话中分发任务给玩家
    /// </summary>

    /// <summary>
    /// Fungus自定义命令 - 完成任务
    /// 用于在对话中完成任务
    /// </summary>
    [CommandInfo("Quest",
                 "Complete Quest",
                 "完成指定任务")]
       [AddComponentMenu("")]
    public class CompleteQuestCommand :Command 
    {
        [Tooltip("要完成的任务ID")]
        [QuestId(true, "请选择任务")]
        [SerializeField] protected string questId = "";

        [Tooltip("完成成功时的消息")]
        [SerializeField] protected StringData successMessage = new StringData("任务已完成！");

        [Tooltip("完成失败时的消息")]
        [SerializeField] protected StringData failureMessage = new StringData("任务无法完成。");

        [Tooltip("是否显示结果消息")]
        [SerializeField] protected bool showResultMessage = true;

        public override void OnEnter()
        {
            var questManager = QuestManager.Instance;
            if (questManager == null)
            {
                Debug.LogError("[CompleteQuestCommand] QuestManager 实例未找到");
                Continue();
                return;
            }

            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogError("[CompleteQuestCommand] 任务ID不能为空");
                Continue();
                return;
            }

            string reward = questManager.CompleteQuestString(questId);
            if (string.IsNullOrEmpty(reward))
            {
                reward="任务已完成！";
            }
            QuestData quest=questManager.GetQuestById(questId);
            if (showResultMessage)
            {
                string message =quest.questName+"\n"+ reward;    

                // 通过Fungus的Say系统显示消息
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
                    Debug.Log($"[CompleteQuestCommand] {message}");
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
            return "完成任务: " + questId;
        }
    }



