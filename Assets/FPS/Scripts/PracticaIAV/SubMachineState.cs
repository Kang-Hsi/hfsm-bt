using System.Collections.Generic;
using UnityEngine;

public class SubMachineState : State
{
    private HierarchicalStateMachine machine;

    public SubMachineState(HierarchicalStateMachine machine)
    {
        this.machine = machine;
    }

    public override UpdateResult Update(GameObject agent)
    {
        var result = machine.Update(agent);
        result.actions.AddRange(base.GetActions());
        return result;
    }

    public override List<State> GetStates()
    {
        if (machine != null)
        {
            var states = new List<State> { this };
            states.AddRange(machine.GetStates());
            return states;
        }
        return new List<State> { this };
    }

    public List<IAction> UpdateDown(State state, int level, GameObject agent)
    {
        if (machine == null)
            return new List<IAction>();
        return machine.UpdateDown(state, level, agent);
    }
}