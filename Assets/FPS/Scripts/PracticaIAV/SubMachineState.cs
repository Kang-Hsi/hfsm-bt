using System.Collections.Generic;

public class SubMachineState : State
{
    private HierarchicalStateMachine machine;

    public SubMachineState(HierarchicalStateMachine machine)
    {
        this.machine = machine;
    }

    public override UpdateResult Update()
    {
        var result = machine.Update();

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

    public List<IAction> UpdateDown(State state, int level)
    {
        if (machine == null)
            return new List<IAction>();

        return machine.UpdateDown(state, level);
    }
}