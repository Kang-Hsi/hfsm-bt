using Unity.FPS.Game;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/True", fileName = "Condition_True")]
public class TrueConditionSO : ConditionSO
{
    [SerializeField]
    private bool inverseLogic = false;
    public override ICondition Create()
    {
        return new TrueCondition(inverseLogic);
    }

    private class TrueCondition : ICondition
    {
        bool inverse;
        public TrueCondition(bool inverseLogic)
        {
            this.inverse = inverseLogic;
        }
        public bool Test(GameObject agent)
        {
            return inverse ? false : true;
        }
    }
}