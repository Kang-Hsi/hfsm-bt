using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
using System.Collections;

public class BotController : MonoBehaviour
{
    private List<Transform> waypoints;

    NavMeshAgent agent;
    int currentIndex;

    public float reachDistance = 2f;

    HierarchicalStateMachine stateMachine;

    [SerializeField]
    private StateSO so;
    IEnumerator Start()
    {
        State initState = HFSMLoader.Instance.GetState(so);
        stateMachine = new HierarchicalStateMachine(initState);
        agent = GetComponent<NavMeshAgent>();

        while (WaypointsManager.Instance == null)
            yield return null;

        waypoints = WaypointsManager.Instance.weaponWaypoints;

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("No hay waypoints de armas");
            yield break;
        }

        PickRandomWaypoint();

    }

    void Update()
    {
        foreach (var action in stateMachine.Update(gameObject).actions)
        {
            action.Execute(gameObject);
        }
    }

    void PickRandomWaypoint()
    {
        if (waypoints.Count == 1)
        {
            currentIndex = 0;
        }
        else
        {
            int newIndex;

            do
            {
                newIndex = Random.Range(0, waypoints.Count);
            } while (newIndex == currentIndex);

            currentIndex = newIndex;
        }

        agent.SetDestination(waypoints[currentIndex].position);
    }
}