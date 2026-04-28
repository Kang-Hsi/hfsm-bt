using System.Collections.Generic;

public class Transition : ITransition
{
    private ICondition condition;
    private IState targetState;
    private List<IAction> actions;

    public Transition(ICondition condition, IState targetState, List<IAction> actions)
    {
        this.condition = condition;
        this.targetState = targetState;
        this.actions = actions;
    }

    public bool IsTriggered()
    {
        return condition.Test();
    }

    public IState GetTargetState()
    {
        return targetState;
    }

    public List<IAction> GetActions()
    {
        return actions;
    }
}