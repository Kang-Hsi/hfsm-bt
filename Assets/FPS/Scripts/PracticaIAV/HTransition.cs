using System.Collections.Generic;
using UnityEngine;

public class HTransition : ITransition
{
    private ICondition condition;
    private StateSO targetStateSO;
    private List<IAction> actions;
    private int level;

    public HTransition(ICondition condition, StateSO targetStateSO, List<IAction> actions, int level)
    {
        this.condition = condition;
        this.targetStateSO = targetStateSO;
        this.actions = actions;
        this.level = level;
    }

    public bool IsTriggered(GameObject agent)
    {
        return condition.Test(agent);
    }

    public IState GetTargetState()
    {
        return HFSMLoader.Instance.GetState(targetStateSO);
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