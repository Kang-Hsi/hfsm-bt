using Unity.FPS.Game;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/LifeLowerThan", fileName = "Condition_LifeLowerThan")]
public class LifeLowerThanConditionSO : ConditionSO
{
    [SerializeField]
    private float lifeThreshold;

    public override ICondition Create()
    {
        return new LifeLowerThanCondition(this);
    }

    private class LifeLowerThanCondition : ICondition
    {
        private readonly LifeLowerThanConditionSO so;

        public LifeLowerThanCondition(LifeLowerThanConditionSO so)
        {
            this.so = so;
        }

        public bool Test(GameObject agent)
        {
            var health = agent.GetComponent<Health>();
            if (health == null)
                return false;
            return health.CurrentHealth < so.lifeThreshold;
        }
    }
}