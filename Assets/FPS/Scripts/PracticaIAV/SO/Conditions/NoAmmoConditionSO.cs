using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/NoAmmo", fileName = "Condition_NoAmmo")]
public class NoAmmoConditionSo : ConditionSO
{
    public override ICondition Create()
    {
        return new NoAmmoCondition();
    }

    private class NoAmmoCondition : ICondition
    {
        public bool Test(GameObject agent)
        {
            PlayerWeaponsManager m_weapons = agent.GetComponent<PlayerWeaponsManager>();
            if (m_weapons == null) return false;

            WeaponController weapon = m_weapons.GetActiveWeapon();
            if (weapon == null) return false;

            return weapon.CurrentAmmoRatio <= 0.1;
        }
    }
}