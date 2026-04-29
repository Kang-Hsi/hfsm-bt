using System.Collections.Generic;
using UnityEngine;

public interface ITransition
{
    bool IsTriggered(GameObject agent);
    IState GetTargetState();
    List<IAction> GetActions();
}