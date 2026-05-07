using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/HealthPickup", fileName = "HealthPickup_Action")]
public class PickupHeatlhActionSO : ActionSO
{
    [SerializeField]
    float speed = 5.0f;
    public override IAction Create()
    {
        return new PickupHeatlhAction(speed);
    }

    private class PickupHeatlhAction : IAction
    {
        private float speed;

        public PickupHeatlhAction(float speed)
        {
            this.speed = speed;
        }

        public void Execute(GameObject agent)
        {
            Debug.Log("CURAAAa");
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            Debug.Log("Estado pillar cura");
            if (bot.HasReachedCurrentDestination())
            {
                if (bot.TryPickHealth(agent.transform.position, out var dest))
                {
                    bot.NavMeshAgent.speed = speed;
                    bot.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}
