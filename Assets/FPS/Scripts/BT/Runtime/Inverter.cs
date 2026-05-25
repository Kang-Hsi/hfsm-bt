using UnityEngine;

public class Inverter : Decorator
{
    public Inverter(Task child) : base(child) { }

    public override bool Run(GameObject agent)
    {
        return !child.Run(agent);
    }
}
