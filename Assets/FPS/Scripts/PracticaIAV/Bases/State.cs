using System.Collections.Generic;

public class State : HSMBase, IState
{
    protected List<ITransition> transitions = new List<ITransition>();

    public override List<State> GetStates()
    {
        return new List<State> { this };
    }

    public virtual List<IAction> GetEntryActions()
    {
        return new List<IAction>();
    }
    public virtual List<IAction> GetExitActions()
    {
        return new List<IAction>();
    }
    public override List<IAction> GetActions()
    {
        return new List<IAction>();
    }

    public virtual List<ITransition> GetTransitions()
    {
        return transitions;
    }
}