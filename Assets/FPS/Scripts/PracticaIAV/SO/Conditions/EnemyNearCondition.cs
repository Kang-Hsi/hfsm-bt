using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/EnemyNear", fileName = "Condition_EnemyNear")]
public class EnemyNearConditionSO : ConditionSO
{
    [SerializeField]
    private bool inverseLogic = false;
    [SerializeField]
    private float radius;
    public override ICondition Create()
    {
        return new EnemyNearCondition(radius, inverseLogic);
    }

    private class EnemyNearCondition : ICondition
    {
        private float radius;
        bool inverse;
        public EnemyNearCondition(float radius, bool inverse)
        {
            this.radius = radius;
            this.inverse = inverse;
        }
        public bool Test(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null)
                return false;
            bool res = bot.EnemyNear(radius);
            return inverse ? !res : res;
        }
    }
}