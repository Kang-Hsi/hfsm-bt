using System.Collections.Generic;

public interface IState
{
    List<IAction> GetActions();
    List<IAction> GetEntryActions();
    List<IAction> GetExitActions();
    List<ITransition> GetTransitions();
}

public interface ITransition
{
    bool IsTriggered();
    IState GetTargetState();
    List<IAction> GetActions();
}

public interface ICondition
{
    bool Test();
}

public interface IAction
{
    void Execute();
}