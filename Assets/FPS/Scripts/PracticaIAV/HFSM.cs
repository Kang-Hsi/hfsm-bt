using System.Collections.Generic;

/// <summary>
/// Clase abstracta de la máquina de estados jerárquica
/// </summary>
public abstract class HSMBase
{
    public class UpdateResult
    {
        public List<IAction> actions = new List<IAction>();
        public ITransition transition = null;
        public int level = 0;
    }

    public virtual List<IAction> GetActions()
    {
        return new List<IAction>();
    }

    public virtual UpdateResult Update()
    {
        return new UpdateResult
        {
            actions = GetActions(),
            transition = null,
            level = 0
        };
    }

    public abstract List<State> GetStates();
}

/// <summary>
/// transición jerárquica
/// </summary>
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

    public int GetLevel()
    {
        return level;
    }
}