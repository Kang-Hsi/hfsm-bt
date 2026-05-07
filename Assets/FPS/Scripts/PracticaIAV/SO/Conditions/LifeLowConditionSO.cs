using Unity.FPS.Game;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/LifeLowerThan", fileName = "Condition_LifeLowerThan")]
public class LifeLowerThanConditionSO : ConditionSO
{
    [SerializeField]
    private float lifeThreshold;

    public override ICondition Create()
    {
        return new LifeLowerThanCondition(lifeThreshold);
    }

    private class LifeLowerThanCondition : ICondition
    {
        private float lifeThreshold;

        public LifeLowerThanCondition(float lifeThreshold)
        {
            this.lifeThreshold = lifeThreshold;
        }

        public bool Test(GameObject agent)
        {
            bool needSurvival = agent.GetComponent<Health>().CurrentHealth < lifeThreshold;
            Debug.Log("Vida baja: " +needSurvival +"Vida" +agent.GetComponent<Health>().CurrentHealth);
            return agent.GetComponent<Health>().CurrentHealth < lifeThreshold;
        }
    }
}