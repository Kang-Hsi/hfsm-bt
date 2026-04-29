using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/Walk", fileName = "Walk_Action")]
public class WalkActionSO : ActionSO
{
    public float speed = 2f;

    public override IAction Create()
    {
        return new WalkAction(speed);
    }

    private class WalkAction : IAction
    {
        private float speed;

        public WalkAction(float speed)
        {
            this.speed = speed;
        }

        public void Execute(GameObject agent)
        {
            // Logica
        }
    }
}