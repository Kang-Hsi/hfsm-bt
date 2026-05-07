using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/Evade", fileName = "Evade_Action")]
public class EvadeActionSO : ActionSO
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float fleeDistance = 12f;

    public override IAction Create()
    {
        return new EvadeAction(speed, fleeDistance);
    }

    private class EvadeAction : IAction
    {
        private readonly float speed;
        private readonly float fleeDistance;

        public EvadeAction(float speed, float fleeDistance)
        {
            this.speed = speed;
            this.fleeDistance = fleeDistance;
        }

        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            if (bot.HasReachedCurrentDestination())
            {
                bool canFlee = bot.TryFleeFromTarget(speed, fleeDistance);
                if (!canFlee && bot.Target != null)
                {
                    bot.FaceTowardsWorldPointSmooth(bot.Target.transform.position);
                    bot.TryFireCurrentWeaponPrimary(true, true, false);
                }
            }
        }
    }
}
