using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Walk", fileName = "Walk_Action")]
public class WalkActionSO : ActionSO
{
    public float speed = 5f;
    public float searchRadius = 5f;

    public override IAction Create()
    {
        return new WalkAction(speed, searchRadius);
    }

    private class WalkAction : IAction
    {
        private float speed;
        private float searchRadius;
        private Vector3 currentDestination= Vector3.zero;
        private bool hasDestination = false;

        public WalkAction(float speed, float searchRadius)
        {
            this.speed = speed;
            this.searchRadius = searchRadius;
        }

        public void Execute(GameObject agent)
        {
            var navAgent = agent.GetComponent<NavMeshAgent>();
            var botActions = agent.GetComponent<BotGameplayActions>();

            if (navAgent == null || botActions == null) return;

            // Si no tiene destino o llegó al destino actual
            if (!hasDestination || navAgent.remainingDistance < 0.5f)
            {
                // Buscar nuevo punto
                if (FSM.TryPickAWeapon(agent.transform.position, out var dest))
                {
                    currentDestination = dest;
                    hasDestination = true;
                    navAgent.speed = speed;
                    botActions.TryMoveToWorldPosition(dest);
                    Debug.Log($"Nuevo destino: {dest}, Distancia: {Vector3.Distance(agent.transform.position, dest)}");
                }
                else
                {
                    hasDestination = false;
                    Debug.Log($"No se encontró punto NavMesh en radio {searchRadius}");
                }
            }
        }
    }
}