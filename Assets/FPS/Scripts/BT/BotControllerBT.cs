using System.Collections;
using Unity.FPS.Gameplay;
using UnityEngine;

public class BotControllerBT : MonoBehaviour
{
    [SerializeField]
    private TaskSO rootTaskSO;

    [SerializeField]
    private bool showDebugOverlay = true;

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

    void OnGUI()
    {
        if (!showDebugOverlay || root == null) return;
        string action = Strip(BTDebug.LastAction);
        string cond = Strip(BTDebug.LastCondition);
        string result = BTDebug.LastConditionResult ? "true" : "false";
        var style = new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.UpperLeft, padding = new RectOffset(12, 12, 8, 8) };
        GUI.Box(new Rect(10, 10, 520, 90),
            "BT action: " + action + "\nLast condition: " + cond + " = " + result,
            style);
    }

    private static string Strip(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "(none)";
        if (raw.StartsWith("Task_")) return raw.Substring(5);
        return raw;
    }
}
