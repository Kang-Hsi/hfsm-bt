using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Conditions/HasAllWeapons", fileName = "Condition_HasAllWeapons")]
public class HasAllWeaponsConditionSO : ConditionSO
{
    [SerializeField]
    private bool inverseLogic = false;
    public override ICondition Create()
    {
        return new HasAllWeaponsCondition(inverseLogic);
    }

    private class HasAllWeaponsCondition : ICondition
    {
        bool inverse;
        public HasAllWeaponsCondition(bool inverseLogic)
        {
            this.inverse = inverseLogic;
        }
        public bool Test(GameObject agent)
        {
            PlayerWeaponsManager m_weapons = agent.GetComponent<PlayerWeaponsManager>();

            if (m_weapons == null)
                return false;

            int filled = 0;
            int total = m_weapons.WeaponSlotNumber;
            for (int i = 0; i < total; i++)
            {
                if (m_weapons.GetWeaponAtSlotIndex(i) != null)
                    filled++;
            }

            bool res = filled >= total;
            Debug.Log("tiene todas armas: " + res + " (" + filled + "/" + total + ")");
            return inverse ? !res : res;
        }
    }
}