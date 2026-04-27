using UnityEngine;

public class CameraFollor : MonoBehaviour
{
    private GameObject player;

    [SerializeField]
    private Vector3 cameraOffset;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void LateUpdate()
    {
        if (!player)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (!player) return;
        }

        transform.position = cameraOffset + player.transform.position;
    }
}
