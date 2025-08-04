using Fungus;
using UnityEngine;
    /// <summary>
    /// Fungus自定义命令 - 检查任务是否是当前任务
    /// 用于在对话中检查任务是否是当前任务
    /// </summary>
    [CommandInfo("Quest",
                 "checkQuestIsCurrent",
                 "检查任务是否是当前任务")]

                  [AddComponentMenu("")]
    public class checkQuestIsCurrent :Command 
    {
        [Tooltip("要检查的任务ID")]
        [QuestId(true, "请选择任务")]
        [SerializeField] protected string questId = "";

          [Tooltip("状态匹配时跳转到的Block")]
        [SerializeField] protected BlockReference targetBlock;

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

            bool isCurrent = questManager.CanStartQuest(questId);

           if (isCurrent && targetBlock.block != null)
            {
                // StopParentBlock()确保当前Block正常结束，然后跳转到目标Block
                StopParentBlock();
                GetFlowchart().ExecuteBlock(targetBlock.block);
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
            return "检查任务是否是当前任务: " + questId;
        }
    }


