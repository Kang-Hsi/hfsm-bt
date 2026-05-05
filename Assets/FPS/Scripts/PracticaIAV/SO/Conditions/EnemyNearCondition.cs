using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Conditions/EnemyNear", fileName = "Condition_EnemyNear")]
public class EnemyNearConditionSO : ConditionSO
{
    [SerializeField]
    private float radius;
    public override ICondition Create()
    {
        return new EnemyNearCondition(radius);
    }

    private class EnemyNearCondition : ICondition
    {
        private float radius;
        public EnemyNearCondition(float radius)
        {
            this.radius = radius;
        }
        public bool Test(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null)
                return false;
            bool res = bot.EnemyNear(radius);
            Debug.Log($"hay enemigo cerca?: {res}");
            return res;
        }
    }
}