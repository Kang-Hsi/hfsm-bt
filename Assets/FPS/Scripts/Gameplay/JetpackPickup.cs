namespace Unity.FPS.Gameplay
{
    public class JetpackPickup : Pickup
    {
        protected override void OnPicked(PlayerCharacterController byPlayer)
        {
            var jetpack = byPlayer.GetComponent<Jetpack>();
            if (!jetpack)
                return;

            if (jetpack.TryUnlock())
            {
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }

        protected override void RemoveFromWaypoints()
        {
            WaypointsManager.Instance.jetpackWaypoints.Remove(gameObject.transform);
        }
    }
}