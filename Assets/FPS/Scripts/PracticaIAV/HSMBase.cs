using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase abstracta de la maquina de estados jerarquica
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

    public virtual UpdateResult Update(GameObject agent)
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