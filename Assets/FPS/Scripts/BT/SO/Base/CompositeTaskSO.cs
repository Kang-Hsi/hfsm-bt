using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeTaskSO : TaskSO
{
    [SerializeField]
    public List<TaskSO> children = new List<TaskSO>();
}
