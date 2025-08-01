using Fungus;
using UnityEngine;

    /// <summary>
    /// Fungus自定义命令 - 检查任务是否可完成
    /// 用于在对话中检查任务是否可以完成
    /// </summary>
    [CommandInfo("Quest",
                 "Check Quest Completable",
                 "检查任务是否可完成")]
       [AddComponentMenu("")]
    public class CheckQuestCompletableCommand :Command 
    {
        [Tooltip("要检查的任务ID")]
        [QuestId(true, "请选择任务")]
        [SerializeField] protected string questId = "";

        [Tooltip("任务可完成时跳转到的Block")]
        [SerializeField] protected BlockReference completableBlock;

        [Tooltip("任务不可完成时跳转到的Block（可选）")]
        [SerializeField] protected BlockReference notCompletableBlock;

        public override void OnEnter()
        {
            var questManager = QuestManager.Instance;
            if (questManager == null)
            {
                Debug.LogError("[CheckQuestCompletableCommand] QuestManager 实例未找到");
                Continue();
                return;
            }

            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogError("[CheckQuestCompletableCommand] 任务ID不能为空");
                Continue();
                return;
            }

            bool canComplete = questManager.CanComplete(questId);

            if (canComplete && completableBlock.block != null)
            {
                // 先调用Continue()确保当前Block正常结束，然后跳转到目标Block
                Continue();
                GetFlowchart().ExecuteBlock(completableBlock.block);
            }
            else if (!canComplete && notCompletableBlock.block != null)
            {
                // 先调用Continue()确保当前Block正常结束，然后跳转到else Block
                Continue();
                GetFlowchart().ExecuteBlock(notCompletableBlock.block);
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
            return $"检查任务 {questId} 是否可完成";
        }
    }


