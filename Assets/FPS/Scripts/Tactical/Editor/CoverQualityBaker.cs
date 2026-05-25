using UnityEditor;
using UnityEngine;

public static class CoverQualityBaker
{
    private const int ITERATIONS = 32;
    private const float RADIUS = 5f;
    private const float RAND_RADIUS = 1f;

    [MenuItem("BT/Bake Tactical Waypoints")]
    public static void Bake()
    {
        var waypoints = Object.FindObjectsByType<TacticalWaypoint>(FindObjectsSortMode.None);
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("CoverQualityBaker: no TacticalWaypoint components found in scene.");
            return;
        }

        Vector3 characterSize = new Vector3(0.5f, 1.8f, 0.5f);
        foreach (var wp in waypoints)
        {
            float v = GetCoverQuality(wp.transform.position, ITERATIONS, characterSize);
            Undo.RecordObject(wp, "Bake CoverValue");
            wp.coverValue = v;
            EditorUtility.SetDirty(wp);
        }

        Debug.Log($"CoverQualityBaker: baked cover values on {waypoints.Length} waypoints.");
    }

    private static float GetCoverQuality(Vector3 location, int iterations, Vector3 characterSize)
    {
        float angleStep = 2f * Mathf.PI / iterations;
        float theta = 0f;
        int hits = 0;
        int valid = 0;

        for (int i = 0; i < iterations; i++)
        {
            Vector3 from = location;
            from.x += RADIUS * Mathf.Cos(theta) + RandomBinomial() * RAND_RADIUS;
            from.y += Random.value * 2f * RAND_RADIUS;
            from.z += RADIUS * Mathf.Sin(theta) + RandomBinomial() * RAND_RADIUS;

            valid++;

            Vector3 to = location;
            to.x += RandomBinomial() * characterSize.x;
            to.y += Random.value * characterSize.y;
            to.z += RandomBinomial() * characterSize.z;

            if (Physics.Linecast(from, to))
                hits++;

            theta += angleStep;
        }

        return valid > 0 ? (float)hits / valid : 0f;
    }

    private static float RandomBinomial() => Random.value - Random.value;
}
