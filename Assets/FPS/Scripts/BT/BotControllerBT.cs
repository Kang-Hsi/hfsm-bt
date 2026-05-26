using System.Collections;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

public class BotControllerBT : MonoBehaviour
{
    [SerializeField]
    private TaskSO rootTaskSO;

    [SerializeField]
    private bool showDebugOverlay = true;

    [Header("Passive regen")]
    [SerializeField] private float regenRatePerSecond = 5f;
    [SerializeField] private float regenRateWhileShooting = 2f;

    private Task root;
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

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
        if (root != null) root.Run(gameObject);

        if (health != null
            && health.CurrentHealth > 0f
            && health.CurrentHealth < health.MaxHealth)
        {
            float rate = BTDebug.LastAction == "Task_Shoot" ? regenRateWhileShooting : regenRatePerSecond;
            // Set directly so the OnHealed event (which triggers the blue UI flash) does not fire.
            health.CurrentHealth = Mathf.Min(
                health.CurrentHealth + rate * Time.deltaTime,
                health.MaxHealth);
        }
    }

    void OnGUI()
    {
        if (!showDebugOverlay || root == null) return;
        string action = Strip(BTDebug.LastAction);
        string cond = Strip(BTDebug.LastCondition);
        string result = BTDebug.LastConditionResult ? "true" : "false";
        var style = new GUIStyle(GUI.skin.box) { fontSize = 44, alignment = TextAnchor.UpperLeft, padding = new RectOffset(20, 20, 16, 16) };
        GUI.Box(new Rect(20, 20, 1000, 170),
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
