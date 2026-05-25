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

            TacticalWaypoint best = null;
            float bestScore = float.NegativeInfinity;
            Vector3 botPos = agent.transform.position;

            foreach (var wp in mgr.waypoints)
            {
                if (wp == null) continue;
                float dist = Vector3.Distance(botPos, wp.transform.position);
                if (dist < 0.5f) continue;
                float score = wp.coverValue / dist;
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
