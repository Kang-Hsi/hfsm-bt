using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Wander", fileName = "Wander_Action")]
public class WanderActionSO : ActionSO
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float radius = 5f;

    public override IAction Create()
    {
        return new WanderAction(speed, radius);
    }

    private class WanderAction : IAction
    {
        private float speed;
        private float radius;
        public WanderAction(float speed, float radius)
        {
            this.speed = speed;
            this.radius = radius;
        }

        public void Execute(GameObject agent)
        {
            Debug.Log("MERODEAR");
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            if (bot.HasReachedCurrentDestination())
            {
                if (bot.TryPickRandomNavMeshPoint(agent.transform.position, radius, out var dest))
                {
                    bot.NavMeshAgent.speed = speed;
                    bot.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}