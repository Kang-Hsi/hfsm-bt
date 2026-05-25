using UnityEngine;

public class TacticalWaypoint : MonoBehaviour
{
    [Range(0f, 1f)]
    public float coverValue = 0f;

    // Editor-only debug visual: sphere coloured red (low cover) to green (high cover).
    void OnDrawGizmos()
    {
        Gizmos.color = Color.Lerp(Color.red, Color.green, coverValue);
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}
