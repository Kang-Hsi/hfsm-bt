using System.Collections.Generic;
public interface IState
{
    List<IAction> GetActions();
    List<IAction> GetEntryActions();
    List<IAction> GetExitActions();
    List<ITransition> GetTransitions();
}