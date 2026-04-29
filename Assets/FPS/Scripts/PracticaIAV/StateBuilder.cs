using System.Collections.Generic;
using UnityEngine;

public static class StateBuilder
{
    private static Dictionary<StateSO, State> builtStates = new Dictionary<StateSO, State>();

    /// <summary>
    /// Construye un estado concreto desde un StateSO
    /// </summary>
    public static State BuildFromSO(StateSO stateSO, HierarchicalStateMachine parentMachine = null)
    {
        if (stateSO == null) return null;

        // tad para evitar construir el mismo estado multiples veces
        if (builtStates.ContainsKey(stateSO))
            return builtStates[stateSO];

        // crear acciones utilizando las fabricas de los ScriptableObjects
        List<IAction> entryActions = CreateActions(stateSO.entryActions);
        List<IAction> exitActions = CreateActions(stateSO.exitActions);
        List<IAction> actions = CreateActions(stateSO.actions);

        // Crear transiciones
        List<ITransition> transitions = new List<ITransition>();

        foreach (var transitionSO in stateSO.transitions)
        {
            if (transitionSO == null) continue;

            // Construir la condición
            ICondition condition = transitionSO.condition?.Create();
            if (condition == null) continue;

            // Construir el estado destino
            State targetState = BuildFromSO(transitionSO.targetState, parentMachine);

            // Crear acciones de la transición
            List<IAction> transitionActions = CreateActions(transitionSO.actions);

            // Si tiene level != 0, es una transición jerárquica (HTransition)
            ITransition transition;
            if (transitionSO.level != 0)
            {
                transition = new HTransition(condition, targetState, transitionActions, transitionSO.level);
            }
            else
            {
                transition = new Transition(condition, targetState, transitionActions);
            }

            transitions.Add(transition);
        }

        // Determinar si es un estado normal o una submáquina
        // Nota: Necesitarás agregar un campo a StateSO para indicar si tiene submáquina
        // Por ahora asumimos estados normales

        var state = new StateWithActions(entryActions, exitActions, actions, transitions);
        builtStates[stateSO] = state;

        return state;
    }

    private static List<IAction> CreateActions(List<ActionSO> actionSOs)
    {
        List<IAction> actions = new List<IAction>();
        if (actionSOs == null) return actions;

        foreach (var actionSO in actionSOs)
        {
            if (actionSO != null)
            {
                IAction action = actionSO.Create();
                if (action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    public static void ClearCache()
    {
        builtStates.Clear();
    }
}

/// <summary>
/// Implementación concreta de State que guarda sus acciones y transiciones
/// </summary>
public class StateWithActions : State
{
    private List<IAction> entryActions;
    private List<IAction> exitActions;
    private List<IAction> actions;
    private List<ITransition> transitions;

    public StateWithActions(
        List<IAction> entryActions,
        List<IAction> exitActions,
        List<IAction> actions,
        List<ITransition> transitions)
    {
        this.entryActions = entryActions ?? new List<IAction>();
        this.exitActions = exitActions ?? new List<IAction>();
        this.actions = actions ?? new List<IAction>();
        this.transitions = transitions ?? new List<ITransition>();
    }

    public override List<IAction> GetEntryActions() => entryActions;
    public override List<IAction> GetExitActions() => exitActions;
    public override List<IAction> GetActions() => actions;
    public override List<ITransition> GetTransitions() => transitions;
}