using System.Collections.Generic;
using UnityEngine;

public class BTLoader : MonoBehaviour
{
    Dictionary<TaskSO, Task> tasks = new Dictionary<TaskSO, Task>();

    public static BTLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Task GetTask(TaskSO so)
    {
        if (so == null) return null;
        if (!tasks.ContainsKey(so))
        {
            tasks[so] = null;
            tasks[so] = so.Create(this);
        }
        return tasks[so];
    }
}
