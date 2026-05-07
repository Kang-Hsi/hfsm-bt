using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/SwitchWeapon", fileName = "SwitchWeapon_Action")]
public class SwitchWeaponActionSO : ActionSO
{
    public enum Mode { Slot, Next, Previous }

    [SerializeField] private Mode mode = Mode.Next;
    [SerializeField] private int slotIndex = 0;

    public override IAction Create()
    {
        return new SwitchWeaponAction(mode, slotIndex);
    }

    // Pensada para usarse como entryAction; en `actions` (cada frame)
    // los modos Next/Previous ciclarían sin parar.
    private class SwitchWeaponAction : IAction
    {
        private readonly Mode mode;
        private readonly int slotIndex;

        public SwitchWeaponAction(Mode mode, int slotIndex)
        {
            this.mode = mode;
            this.slotIndex = slotIndex;
        }

        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            switch (mode)
            {
                case Mode.Slot:
                    if (bot.GetActiveWeaponSlotIndex() != slotIndex)
                        bot.SwitchToWeaponSlot(slotIndex);
                    break;
                case Mode.Next:
                    bot.SwitchToNextWeaponInInventory();
                    break;
                case Mode.Previous:
                    bot.SwitchToPreviousWeaponInInventory();
                    break;
            }
        }
    }
}
