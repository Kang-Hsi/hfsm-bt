using System.Collections.Generic;
using UnityEngine;

public class Sequence : Task
{
    private readonly List<Task> children;

    public Sequence(List<Task> children)
    {
        this.children = children;
    }

    public override bool Run(GameObject agent)
    {
        foreach (var c in children)
        {
            if (!c.Run(agent))
                return false;
        }
        return true;
    }
}
