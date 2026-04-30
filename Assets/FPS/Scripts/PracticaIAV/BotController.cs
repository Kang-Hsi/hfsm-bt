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
        // Esperar a que HFSMLoader este listo
        while (HFSMLoader.Instance == null)
        {
            yield return null;
        }

        // Esperar a que WaypointsManager este listo
        while (WaypointsManager.Instance == null)
        {
            yield return null;
        }

        Debug.Log("Singletons inicializados correctamente");

        State initState = HFSMLoader.Instance.GetState(so);
        stateMachine = new HierarchicalStateMachine(initState);
        agent = GetComponent<NavMeshAgent>();

        waypoints = WaypointsManager.Instance.weaponWaypoints;

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("No hay waypoints de armas");
            yield break;
        }
    }

    void Update()
    {
        if (stateMachine == null)
            return;
        Debug.Log(1);

        foreach (var action in stateMachine.Update(gameObject).actions)
        {
            Debug.Log(2);
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