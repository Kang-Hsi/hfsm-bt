using System.Collections.Generic;
using System.Diagnostics;

public class State : HSMBase, IState
{
    StateSO myState;
    //transitions
    List<ITransition> transitions;

    //actions
    List<IAction> entryActions;
    List<IAction> exitActions;
    List<IAction> actions;

    public State(StateSO aState)
    {
        myState = aState;
        entryActions = new List<IAction>();
        exitActions = new List<IAction>();
        actions = new List<IAction>();
        transitions = new List<ITransition>();

        //crear acciones de entrada
        foreach (ActionSO a in myState.entryActions)
        {
            entryActions.Add(a.Create());
        }

        //crear acciones de salida
        foreach (ActionSO a in myState.exitActions)
        {
            exitActions.Add(a.Create());
        }

        //crear acciones
        foreach (ActionSO a in myState.actions)
        {
            Debug.Print("CREANDOOOO");
            actions.Add(a.Create());
        }


        //crear transiciones
        foreach (TransitionSO t in myState.transitions)
        {
            ICondition aCondition = t.condition.Create();
            State targetState = HFSMLoader.Instance.GetState(t.targetState);

            List<IAction> transitionActions = new();
            foreach (ActionSO a in t.actions)
            {
                transitionActions.Add(a.Create());
            }

            HTransition aTransition = new HTransition(aCondition, targetState, transitionActions, t.level);
            transitions.Add(aTransition);
        }
    }
    public override List<State> GetStates()
    {
        return new List<State> { this };
    }

    public virtual List<IAction> GetEntryActions()
    {
        return entryActions;
    }
    public virtual List<IAction> GetExitActions()
    {
        return exitActions;
    }
    public override List<IAction> GetActions()
    {
        return actions;
    }

    public virtual List<ITransition> GetTransitions()
    {
        return transitions;
    }
}