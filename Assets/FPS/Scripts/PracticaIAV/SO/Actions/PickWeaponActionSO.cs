using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/PickWeaponAction", fileName = "PickWeapon_Action")]
public class PickWeaponActionSo : ActionSO
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float searchRadius = 5f;

    public override IAction Create()
    {
        return new PickWeaponAction(speed);
    }

    private class PickWeaponAction : IAction
    {
        private float speed;

        public PickWeaponAction(float speed)
        {
            this.speed = speed;
        }

        public void Execute(GameObject agent)
        {
            Debug.Log("PILLAR ARMA");
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            if (bot.HasReachedCurrentDestination())
            {
                if (bot.TryPickAWeapon(agent.transform.position, out var dest))
                {
                    bot.NavMeshAgent.speed = speed;
                    bot.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}