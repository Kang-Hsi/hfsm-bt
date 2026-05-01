using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Walk", fileName = "Walk_Action")]
public class WalkActionSO : ActionSO
{
    public float speed = 5f;
    public float searchRadius = 5f;

    public override IAction Create()
    {
        return new WalkAction(speed);
    }

    private class WalkAction : IAction
    {
        private float speed;

        public WalkAction(float speed)
        {
            this.speed = speed;
        }

        public void Execute(GameObject agent)
        {
            var navAgent = agent.GetComponent<NavMeshAgent>();
            var botActions = agent.GetComponent<BotGameplayActions>();

            if (navAgent == null || botActions == null) return;

            // Si no tiene destino o llega al destino actual
            if (!navAgent.hasPath || navAgent.remainingDistance < 0.5f)
            {
                // Buscar nuevo punto
                if (FSM.TryPickAWeapon(agent.transform.position, out var dest))
                {
                    navAgent.speed = speed;
                    botActions.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}