using System.Collections.Generic;
using UnityEngine;

public class HTransition : ITransition
{
    private ICondition condition;
    private State targetState;
    private List<IAction> actions;
    private int level;

    public HTransition(ICondition condition, State targetState, List<IAction> actions, int level)
    {
        this.condition = condition;
        this.targetState = targetState;
        this.actions = actions;
        this.level = level;
    }

    public bool IsTriggered(GameObject agent)
    {
        return condition.Test(agent);
    }

    public IState GetTargetState()
    {
        return targetState;
    }

    public List<IAction> GetActions()
    {
        return actions;
    }

    public int GetLevel()
    {
        return level;
    }
}