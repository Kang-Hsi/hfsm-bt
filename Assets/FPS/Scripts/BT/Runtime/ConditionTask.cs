using UnityEngine;

public class ConditionTask : Task
{
    private readonly ICondition condition;

    public ConditionTask(ICondition condition)
    {
        this.condition = condition;
    }

    public override bool Run(GameObject agent)
    {
        return condition.Test(agent);
    }
}
