using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/TargetDead", fileName = "Condition_TargetDead")]
public class TargetDeadConditionSO : ConditionSO
{
    public override ICondition Create()
    {
        return new TargetDeadCondition();
    }

    private class TargetDeadCondition : ICondition
    {
        public bool Test(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null)
                return false;
            return bot.Target == null;
        }
    }
}