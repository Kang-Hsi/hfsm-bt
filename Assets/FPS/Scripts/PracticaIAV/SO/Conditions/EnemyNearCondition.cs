using Unity.FPS.Game;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
            RaycastHit hit;
            if (Physics.SphereCast(agent.transform.position, radius, agent.transform.forward, out hit, 30, 1))
            {
                Debug.Log("Enemigo cerca");
                return true;
            }
            Debug.Log($"Hit: {hit.distance}");
            return false;
        }
    }
}