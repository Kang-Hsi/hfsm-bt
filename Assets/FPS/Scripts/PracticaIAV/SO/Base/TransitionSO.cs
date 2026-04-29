using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "HFSM/Transition", fileName = "-")]
public class TransitionSO : ScriptableObject
{
    public ConditionSO condition;
    public StateSO targetState;
    public int level;
    public List<ActionSO> actions;
}