using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BT/Composites/Selector", fileName = "BT_Selector")]
public class SelectorSO : CompositeTaskSO
{
    public override Task Create(BTLoader loader)
    {
        var runtimeChildren = new List<Task>(children.Count);
        foreach (var c in children)
        {
            runtimeChildren.Add(loader.GetTask(c));
        }
        return new Selector(runtimeChildren);
    }
}
