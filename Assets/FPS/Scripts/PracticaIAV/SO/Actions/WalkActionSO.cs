using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/Walk", fileName = "Walk_Action")]
public class WalkActionSO : ActionSO
{
    public float speed = 2f;

    public override IAction Create()
    {
        Debug.Log("create walk");
        return new WalkAction();
    }

    private class WalkAction : IAction
    {

        public void Execute(GameObject agent)
        {
            Debug.Log("TE SALUDO");
            if (FSM.TryPickRandomNavMeshPoint(agent.transform.position, 2, out var dest))
                agent.GetComponent<BotGameplayActions>().TryMoveToWorldPosition(dest);
        }
    }
}