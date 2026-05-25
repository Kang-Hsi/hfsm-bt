using UnityEngine;

public class ActionTask : Task
{
    private readonly IAction action;

    public ActionTask(IAction action)
    {
        this.action = action;
    }

    public override bool Run(GameObject agent)
    {
        action.Execute(agent);
        return true;
    }
}
