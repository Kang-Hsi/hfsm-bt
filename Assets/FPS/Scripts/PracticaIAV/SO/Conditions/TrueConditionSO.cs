using Unity.FPS.Game;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/True", fileName = "Condition_True")]
public class TrueConditionSO : ConditionSO
{
    public override ICondition Create()
    {
        return new TrueCondition();
    }

    private class TrueCondition : ICondition
    {
        public bool Test(GameObject agent)
        {
            return true;
        }
    }
}