using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Shoot", fileName = "Shoot_Action")]
public class ShootActionSO : ActionSO
{
    public float searchRadius = 5f;

    public override IAction Create()
    {
        return new ShootAction();
    }

    private class ShootAction : IAction
    {
        public void Execute(GameObject agent)
        {
            Debug.Log("Dispara");
            var navAgent = agent.GetComponent<NavMeshAgent>();
            var botActions = agent.GetComponent<BotGameplayActions>();
            if (navAgent == null || botActions == null) return;

            botActions.TryFireCurrentWeaponPrimary(true, true, false);
        }
    }
}