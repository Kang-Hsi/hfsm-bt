using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Game;
using System.Linq;
/// <summary>
/// Manages all waypoint locations
/// Collects and organizes waypoints by finding GameObjects with specific components
/// Singleton pattern for global access
/// </summary>

namespace Unity.FPS.Gameplay
{
    public class WaypointsManager : MonoBehaviour
    {
        public static WaypointsManager Instance { get; private set; }

        [Header("Weapon Waypoints")]
        [Tooltip("Locations where weapons can be picked up")]
        public List<Transform> weaponWaypoints = new List<Transform>();

        [Header("Utility Waypoints")]
        [Tooltip("Locations where medkits can be found")]
        public List<Transform> medkitWaypoints = new List<Transform>();

        [Tooltip("Locations where jetpacks can be found")]
        public List<Transform> jetpackWaypoints = new List<Transform>();

        [Header("Respawn Waypoints")]
        [Tooltip("Possible respawn points for prisoners after death")]
        public List<Transform> respawnWaypoints = new List<Transform>();

        [Header("Patrol Waypoints")]
        [Tooltip("Points used for patrol routes when no specific target is available")]
        public List<Transform> patrolWaypoints = new List<Transform>();

        [Header("Configuration")]
        [Tooltip("Distance threshold to consider a waypoint as reached")]
        public float waypointRadius = 1.5f;

        /// <summary>
        /// Ensures only one WaypointsManager exists in the scene.
        /// </summary>
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

        /// <summary>
        /// Collects all waypoint references after scene initialization.
        /// </summary>
        void Start()
        {
            CollectWaypoints();
        }

        /// <summary>
        /// Scans the scene for GameObjects with specific components and organizes them
        /// into the appropriate waypoint lists based on their type.
        /// Uses FindObjectsOfType to locate all instances of each component type.
        /// </summary>
        void CollectWaypoints()
        {
            weaponWaypoints.Clear();
            medkitWaypoints.Clear();
            jetpackWaypoints.Clear();
            respawnWaypoints.Clear();
            patrolWaypoints.Clear();

            // Collect all weapon spawn points
            WeaponPickup[] weapons = FindObjectsOfType<WeaponPickup>();
            foreach (WeaponPickup weapon in weapons)
            {
                weaponWaypoints.Add(weapon.transform);
            }

            // Collect all medkit locations
            HealthPickup[] medkits = FindObjectsOfType<HealthPickup>();
            foreach (HealthPickup medkit in medkits)
            {
                medkitWaypoints.Add(medkit.transform);
            }

            // Collect all jetpack locations
            JetpackPickup[] jetpacks = FindObjectsOfType<JetpackPickup>();
            foreach (JetpackPickup jetpack in jetpacks)
            {
                jetpackWaypoints.Add(jetpack.transform);
            }

            //// Collect all respawn points
            var r = GameObject.FindGameObjectsWithTag("RespawnPoint").ToList();
            foreach (GameObject respawn in r)
            {
                respawnWaypoints.Add(respawn.transform);
            }

            //// Collect all patrol points
            var p = GameObject.FindGameObjectsWithTag("PatrolPoint").ToList();
            foreach (GameObject pat in p)
            {
                patrolWaypoints.Add(pat.transform);
            }
        }

        /// <summary>
        /// Returns the nearest waypoint from a given list to a position.
        /// </summary>
        /// <param name="position">Reference position to measure distance from</param>
        /// <param name="waypoints">List of waypoints to search</param>
        /// <returns>The closest waypoint transform, or null if list is empty</returns>
        public Transform GetNearestWaypoint(Vector3 position, List<Transform> waypoints)
        {
            if (waypoints == null || waypoints.Count == 0)
                return null;

            Transform nearest = null;
            float minDistance = float.MaxValue;

            foreach (Transform waypoint in waypoints)
            {
                if (waypoint == null) continue;

                float distance = Vector3.Distance(position, waypoint.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = waypoint;
                }
            }

            return nearest;
        }
    }
}