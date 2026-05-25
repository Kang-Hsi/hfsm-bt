using System.Collections.Generic;
using UnityEngine;

public class TacticalWaypointsManager : MonoBehaviour
{
    public static TacticalWaypointsManager Instance { get; private set; }

    public List<TacticalWaypoint> waypoints = new List<TacticalWaypoint>();

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

    void Start()
    {
        Collect();
    }

    public void Collect()
    {
        waypoints.Clear();
        var found = FindObjectsByType<TacticalWaypoint>(FindObjectsSortMode.None);
        waypoints.AddRange(found);
    }
}
