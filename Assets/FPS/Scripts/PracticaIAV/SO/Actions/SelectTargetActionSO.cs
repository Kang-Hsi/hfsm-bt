using UnityEngine;

[CreateAssetMenu(menuName = "HFSM/Actions/SelectTarget", fileName = "SelectTarget_Action")]
public class SelectTargetActionSO : ActionSO
{
    [SerializeField] private float radius = 20f;

    public override IAction Create()
    {
        return new SelectTargetAction(radius);
    }

    private class SelectTargetAction : IAction
    {
        private readonly float radius;

        public SelectTargetAction(float radius)
        {
            this.radius = radius;
        }

        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;
            bot.SelectLowestHealthEnemy(radius);
        }
    }
}
