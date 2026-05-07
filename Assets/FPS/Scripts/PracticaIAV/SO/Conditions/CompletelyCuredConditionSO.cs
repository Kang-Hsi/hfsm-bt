using Unity.FPS.Game;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/CompletelyCured", fileName = "Condition_CompletelyCured")]
public class CompletelyCuredConditionSO : ConditionSO
{
    [SerializeField]
    private bool inverseLogic = false;
    public override ICondition Create()
    {
        return new CompletelyCuredCondition(inverseLogic);
    }

    private class CompletelyCuredCondition : ICondition
    {
        bool inverse;
        public CompletelyCuredCondition(bool inverseLogic)
        {
            this.inverse = inverseLogic;
        }
        public bool Test(GameObject agent)
        {
            var health = agent.GetComponent<Health>();
            if (health == null)
                return false;

            bool isFullHealth = health.CurrentHealth >= health.MaxHealth/2 ;
            bool dead = health.CurrentHealth <= 0.0f;
            return inverse ? dead : isFullHealth;
        }
    }
}