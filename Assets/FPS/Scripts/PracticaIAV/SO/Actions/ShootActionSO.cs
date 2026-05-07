using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Shoot", fileName = "Shoot_Action")]
public class ShootActionSO : ActionSO
{
    [SerializeField]
    float chargeWeaponTime = 1.5f;
    public override IAction Create()
    {
        return new ShootAction(chargeWeaponTime);
    }

    private class ShootAction : IAction
    {
        private float chargeTime;
        private float currentTime;
        private bool charging;
        public ShootAction(float chargeTime)
        {
            this.chargeTime = chargeTime;
            currentTime = 0f;
            charging = false;
        }

        public void Execute(GameObject agent)
        {
            var botActions = agent.GetComponent<BotGameplayActions>();
            if (botActions == null) return;

            WeaponController weapon = botActions.GetActiveWeaponOrNull();
            if (weapon == null) return;

            switch (weapon.ShootType)
            {
                case WeaponShootType.Manual:
                case WeaponShootType.Burst:
                    botActions.TryFireCurrentWeaponPrimary(true, false, false);
                    break;

                case WeaponShootType.Automatic:
                    botActions.TryFireCurrentWeaponPrimary(false, true, false);
                    break;

                case WeaponShootType.Charge:
                    currentTime += Time.deltaTime;

                    if (currentTime < chargeTime)
                    {
                        charging = true;
                        botActions.TryFireCurrentWeaponPrimary(false, true, false);
                    }
                    else
                    {
                        if (charging)
                        {
                            botActions.TryFireCurrentWeaponPrimary(false, false, true);
                            charging = false;
                            currentTime = 0f;
                        }
                    }
                    break;
            }
        }
    }
}