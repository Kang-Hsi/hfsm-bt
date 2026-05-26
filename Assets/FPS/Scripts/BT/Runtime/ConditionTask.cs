using UnityEngine;

public class ConditionTask : Task
{
    private readonly ICondition condition;
    private readonly string conditionName;

    public ConditionTask(ICondition condition, string name = "")
    {
        this.condition = condition;
        this.conditionName = name;
    }

    public override bool Run(GameObject agent)
    {
        bool result = condition.Test(agent);
        BTDebug.LastCondition = conditionName;
        BTDebug.LastConditionResult = result;
        return result;
    }
}
