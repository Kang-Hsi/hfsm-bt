using System.Collections.Generic;
using UnityEngine;

public class Selector : Task
{
    private readonly List<Task> children;

    public Selector(List<Task> children)
    {
        this.children = children;
    }

    public override bool Run(GameObject agent)
    {
        foreach (var c in children)
        {
            if (c.Run(agent))
                return true;
        }
        return false;
    }
}
