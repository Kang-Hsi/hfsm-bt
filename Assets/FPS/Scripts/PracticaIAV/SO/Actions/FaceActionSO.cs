using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "HFSM/Actions/Face", fileName = "Face_Action")]
public class FaceActionSO : ActionSO
{
    public override IAction Create()
    {
        return new FaceAction();
    }

    private class FaceAction : IAction
    {
        public void Execute(GameObject agent)
        {
            var bot = agent.GetComponent<BotGameplayActions>();
            if (bot == null) return;

            if (bot.Target == null) return;

            // Parar movimiento
            bot.StopNavigation();

            // Apuntado smooth pero rápido (slerp con factor 25), evita el salto de un frame.
            bot.FaceTowardsWorldPointSmooth(bot.Target.transform.position);
        }
    }
}
