using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BT/Composites/Sequence", fileName = "BT_Sequence")]
public class SequenceSO : CompositeTaskSO
{
    public override Task Create(BTLoader loader)
    {
        var runtimeChildren = new List<Task>(children.Count);
        foreach (var c in children)
        {
            runtimeChildren.Add(loader.GetTask(c));
        }
        return new Sequence(runtimeChildren);
    }
}
