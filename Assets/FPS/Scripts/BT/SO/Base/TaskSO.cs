using UnityEngine;

public abstract class TaskSO : ScriptableObject
{
    public abstract Task Create(BTLoader loader);
}
