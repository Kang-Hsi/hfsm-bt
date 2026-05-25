using UnityEngine;

[CreateAssetMenu(menuName = "BT/Leaves/Condition", fileName = "BT_Condition")]
public class ConditionTaskSO : TaskSO
{
    [SerializeField]
    public ConditionSO condition;

    public override Task Create(BTLoader loader)
    {
        return new ConditionTask(condition.Create());
    }
}
