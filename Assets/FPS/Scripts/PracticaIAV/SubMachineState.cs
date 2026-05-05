using System.Collections.Generic;
using UnityEngine;
public class SubMachineState : State
{
    private HierarchicalStateMachine machine;

    public SubMachineState(HierarchicalStateMachine machine, StateSO aState): base(aState)
    {
        this.machine = machine;
    }

    public override UpdateResult Update(GameObject agent)
    {
        return machine.Update(agent);
    }

    public override List<State> GetStates()
    {
        return machine.GetStates();
    }

    public List<IAction> UpdateDown(State state, int level, GameObject agent)
    {
        if (machine == null)
            return new List<IAction>();
        return machine.UpdateDown(state, level, agent);
    }
}