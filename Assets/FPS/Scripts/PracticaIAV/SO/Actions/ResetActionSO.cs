using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Reset", fileName = "Reset_Action")]
public class ResetActionSO : ActionSO
{
    public override IAction Create()
    {
        return new ResetAction();
    }

    private class ResetAction : IAction
    {
        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            bot.ResetAI();
        }
    }
}
