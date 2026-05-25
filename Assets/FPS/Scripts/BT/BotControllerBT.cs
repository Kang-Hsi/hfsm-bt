using System.Collections;
using UnityEngine;

public class BotControllerBT : MonoBehaviour
{
    [SerializeField]
    private TaskSO rootTaskSO;

    private Task root;

    IEnumerator Start()
    {
        while (BTLoader.Instance == null) yield return null;
        while (WaypointsManager.Instance == null) yield return null;

        if (rootTaskSO == null)
        {
            Debug.LogWarning("BotControllerBT: rootTaskSO not set");
            yield break;
        }

        root = BTLoader.Instance.GetTask(rootTaskSO);
    }

    void Update()
    {
        if (root == null) return;
        root.Run(gameObject);
    }
}
