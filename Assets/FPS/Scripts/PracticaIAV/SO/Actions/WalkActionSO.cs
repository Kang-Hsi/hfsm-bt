using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Walk", fileName = "Walk_Action")]
public class WalkActionSO : ActionSO
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float searchRadius = 5f;

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
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            Debug.Log("Anda");

            // Si ha llegado o no tiene path
            if (bot.HasReachedCurrentDestination())
            {
                if (FSM.TryPickAWeapon(agent.transform.position, out var dest))
                {
                    bot.NavMeshAgent.speed = speed; // OK tocarlo aquí
                    bot.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}