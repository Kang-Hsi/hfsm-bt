using UnityEngine;

[CreateAssetMenu(menuName = "BT/Leaves/Action", fileName = "BT_Action")]
public class ActionTaskSO : TaskSO
{
    [SerializeField]
    public ActionSO action;

    public override Task Create(BTLoader loader)
    {
        return new ActionTask(action.Create());
    }
}
