using UnityEngine;

[CreateAssetMenu(menuName = "BT/Decorators/Inverter", fileName = "BT_Inverter")]
public class InverterSO : DecoratorTaskSO
{
    public override Task Create(BTLoader loader)
    {
        return new Inverter(loader.GetTask(child));
    }
}
