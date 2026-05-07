using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/SelectWeapon", fileName = "SelectWeapon_Action")]
public class SelectWeaponActionSO : ActionSO
{

    public override IAction Create()
    {
        return new SelectWeaponAction();
    }

    private class SelectWeaponAction : IAction
    {
        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;
            WeaponController current = bot.GetActiveWeaponOrNull();

            int weaponIndex = bot.FindValidWeapon();
            if (weaponIndex != -1)
            {
                bot.SwitchToWeaponSlot(weaponIndex);
            }
            else bot.SwitchToNextWeaponInInventory();
        }
    }
}