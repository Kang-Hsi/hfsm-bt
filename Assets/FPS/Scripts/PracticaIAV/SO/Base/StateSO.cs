using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/State", fileName = "State")]
public class StateSO : ScriptableObject
{
    [Header("Transitions")]
    public List<TransitionSO> transitions = new();

    [Header("Actions")]
    public List<ActionSO> entryActions = new();
    public List<ActionSO> exitActions = new();
    public List<ActionSO> actions = new();
}