using System.Collections.Generic;
using UnityEngine;

public class BTLoader : MonoBehaviour
{
    [SerializeField] private int targetFps = 60;

    Dictionary<TaskSO, Task> tasks = new Dictionary<TaskSO, Task>();

    public static BTLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0;
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
