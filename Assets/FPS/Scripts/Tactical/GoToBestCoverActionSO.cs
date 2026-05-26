using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/GoToBestCover", fileName = "GoToBestCover_Action")]
public class GoToBestCoverActionSO : ActionSO
{
    public override IAction Create()
    {
        return new GoToBestCoverAction();
    }

    private class GoToBestCoverAction : IAction
    {
        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            var mgr = TacticalWaypointsManager.Instance;
            if (mgr == null || mgr.waypoints.Count == 0) return;

            // Need a known enemy to test line of sight against.
            var target = bot.Target;
            if (target == null) return;
            Vector3 enemyPos = target.position;

            TacticalWaypoint best = null;
            float bestScore = float.NegativeInfinity;
            Vector3 botPos = agent.transform.position;

            foreach (var wp in mgr.waypoints)
            {
                if (wp == null) continue;
                Vector3 wpPos = wp.transform.position;
                // Skip waypoints that the enemy can see — not real cover right now.
                if (!Physics.Linecast(wpPos, enemyPos)) continue;
                float distToBot = Vector3.Distance(botPos, wpPos);
                if (distToBot < 0.5f) continue;
                float distFromEnemy = Vector3.Distance(enemyPos, wpPos);
                // Prefer high-cover waypoints far from the enemy and not too far from the bot.
                float score = wp.coverValue * distFromEnemy / distToBot;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = wp;
                }
            }

            if (best == null) return;
            bot.TryMoveToWorldPosition(best.transform.position);
        }
    }
}
