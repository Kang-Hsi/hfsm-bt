using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/LifeHysteresis", fileName = "Condition_LifeHysteresis")]
public class LifeHysteresisConditionSO : ConditionSO
{
    [SerializeField] private float engageBelow = 100f;
    [SerializeField] private float disengageAbove = 160f;

    public override ICondition Create()
    {
        return new LifeHysteresisCondition(engageBelow, disengageAbove);
    }

    // Returns true once the bot drops below engageBelow, and keeps returning
    // true until it climbs above disengageAbove. Used to keep the bot in
    // Survive mode for the full retreat-and-regen arc instead of flipping
    // states the instant it crosses the low threshold.
    private class LifeHysteresisCondition : ICondition
    {
        private readonly float engageBelow;
        private readonly float disengageAbove;
        private bool engaged;

        public LifeHysteresisCondition(float engageBelow, float disengageAbove)
        {
            this.engageBelow = engageBelow;
            this.disengageAbove = disengageAbove;
            this.engaged = false;
        }

        public bool Test(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null || bot.Health == null) return false;
            float h = bot.Health.CurrentHealth;

            if (!engaged && h < engageBelow) engaged = true;
            else if (engaged && h > disengageAbove) engaged = false;

            return engaged;
        }
    }
}
