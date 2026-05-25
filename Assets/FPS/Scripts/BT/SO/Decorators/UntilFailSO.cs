using UnityEngine;

[CreateAssetMenu(menuName = "BT/Decorators/UntilFail", fileName = "BT_UntilFail")]
public class UntilFailSO : DecoratorTaskSO
{
    public override Task Create(BTLoader loader)
    {
        return new UntilFail(loader.GetTask(child));
    }
}
