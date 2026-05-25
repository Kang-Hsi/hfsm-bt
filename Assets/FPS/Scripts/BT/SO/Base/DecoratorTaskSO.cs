using UnityEngine;

public abstract class DecoratorTaskSO : TaskSO
{
    [SerializeField]
    public TaskSO child;
}
