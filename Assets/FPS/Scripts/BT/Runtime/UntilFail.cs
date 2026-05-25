using UnityEngine;

public class UntilFail : Decorator
{
    public UntilFail(Task child) : base(child) { }

    public override bool Run(GameObject agent)
    {
        while (child.Run(agent)) { }
        return true;
    }
}
