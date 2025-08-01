using Fungus;
using UnityEngine;
  

    /// <summary>
    /// Fungus自定义命令 - 检查任务状态
    /// 用于在对话中检查任务状态并根据结果执行不同的对话分支
    /// </summary>
    [CommandInfo("Quest",
                 "Check Quest Status",
                 "检查任务状态")]
       [AddComponentMenu("")]
    public class CheckQuestStatusCommand :Command  
    {
        [Tooltip("要检查的任务ID")]
        [QuestId(true, "请选择任务")]
        [SerializeField] protected string questId = "";

        [Tooltip("要检查的任务状态")]
        [SerializeField] protected QuestStatus targetStatus = QuestStatus.InProgress;

        [Tooltip("状态匹配时跳转到的Block")]
        [SerializeField] protected BlockReference targetBlock;

        [Tooltip("状态不匹配时跳转到的Block（可选）")]
        [SerializeField] protected BlockReference elseBlock;
        public override void OnEnter()
        {
            var questManager = QuestManager.Instance;
            if (questManager == null)
            {
                Debug.LogError("[CheckQuestStatusCommand] QuestManager 实例未找到");
                Continue();
                return;
            }

            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogError("[CheckQuestStatusCommand] 任务ID不能为空");
                Continue();
                return;
            }

            var quest = questManager.GetQuest(questId);
            bool statusMatches = false;

            if (quest != null)
            {
                statusMatches = quest.questStatus == targetStatus;
            }
            else
            {
                // 如果任务不存在，检查是否要查找NotStarted状态
                statusMatches = targetStatus == QuestStatus.NotStarted;
            }

            if (statusMatches && targetBlock.block != null)
            {
                // 先调用Continue()确保当前Block正常结束，然后跳转到目标Block
                Continue();
                GetFlowchart().ExecuteBlock(targetBlock.block);
            }
            else if (!statusMatches && elseBlock.block != null)
            {
                // 先调用Continue()确保当前Block正常结束，然后跳转到else Block
                Continue();
                GetFlowchart().ExecuteBlock(elseBlock.block);
            }
            else
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(questId))
            {
                return "错误: 未设置任务ID";
            }
            return $"检查任务 {questId} 状态: {targetStatus}";
        }
    }



