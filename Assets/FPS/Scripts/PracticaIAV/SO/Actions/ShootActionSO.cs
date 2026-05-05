using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Shoot", fileName = "Shoot_Action")]
public class ShootActionSO : ActionSO
{
    public override IAction Create()
    {
        return new ShootAction();
    }

    private class ShootAction : IAction
    {
        public void Execute(GameObject agent)
        {
            var botActions = agent.GetComponent<BotGameplayActions>();
            if (botActions == null) return;

            botActions.TryFireCurrentWeaponPrimary(true, true, false);
        }
    }
}