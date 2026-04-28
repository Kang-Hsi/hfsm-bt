using System.Collections.Generic;

public class HierarchicalStateMachine : HSMBase
{
    protected State _initialState;
    protected State _currentState;

    public HierarchicalStateMachine Parent { get; set; }

    public HierarchicalStateMachine(State initialState)
    {
        _initialState = initialState;
        _currentState = initialState;
    }

    public override List<State> GetStates()
    {
        if (_currentState != null)
            return _currentState.GetStates();

        return new List<State>();
    }

    public override UpdateResult Update()
    {
        if (_currentState == null)
        {
            _currentState = _initialState;

            return new UpdateResult
            {
                actions = _currentState.GetEntryActions()
            };
        }

        HTransition triggered = null;

        foreach (HTransition t in _currentState.GetTransitions())
        {
            if (t.IsTriggered())
            {
                triggered = t;
                break;
            }
        }

        UpdateResult result;

        if (triggered != null)
        {
            result = new UpdateResult
            {
                transition = triggered,
                level = triggered.GetLevel()
            };
        }
        else
        {
            result = _currentState.Update();
        }

        if (result.transition != null)
        {
            if (result.level == 0)
            {
                State targetState = (State)result.transition.GetTargetState();

                result.actions.AddRange(_currentState.GetExitActions());
                result.actions.AddRange(result.transition.GetActions());
                result.actions.AddRange(targetState.GetEntryActions());

                _currentState = targetState;

                result.actions.AddRange(GetActions());
                result.transition = null;
            }
            else if (result.level > 0)
            {
                result.actions.AddRange(_currentState.GetExitActions());
                _currentState = null;
                result.level -= 1;
            }
            else
            {
                State targetState = (State)result.transition.GetTargetState();

                result.actions.AddRange(result.transition.GetActions());

                if (targetState is SubMachineState subMachine)
                {
                    // transición submáquina
                    result.actions.AddRange(subMachine.UpdateDown(targetState, -result.level));
                }
                else
                {
                    // transición normal
                    result.actions.AddRange(_currentState.GetExitActions());
                    result.actions.AddRange(targetState.GetEntryActions());

                    _currentState = targetState;
                }

                result.transition = null;
            }
        }
        else
        {
            result.actions.AddRange(GetActions());
        }

        return result;
    }

    public List<IAction> UpdateDown(State state, int level)
    {
        List<IAction> actions = new List<IAction>();

        // subir en la jerarquía
        if (level > 0 && Parent != null)
        {
            actions.AddRange(Parent.UpdateDown(_currentState, level - 1));
        }

        // salir del estado actual
        if (_currentState != null)
        {
            actions.AddRange(_currentState.GetExitActions());
        }

        // entrar al nuevo estado
        _currentState = state;
        actions.AddRange(state.GetEntryActions());

        return actions;
    }
}