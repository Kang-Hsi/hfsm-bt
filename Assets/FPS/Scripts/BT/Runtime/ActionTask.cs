using UnityEngine;

public class ActionTask : Task
{
    private readonly IAction action;
    private readonly string actionName;

    public ActionTask(IAction action, string name = "")
    {
        this.action = action;
        this.actionName = name;
    }

    public override bool Run(GameObject agent)
    {
        BTDebug.LastAction = actionName;
        action.Execute(agent);
        return true;
    }
}
