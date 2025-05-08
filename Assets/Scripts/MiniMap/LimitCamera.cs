using UnityEngine;

public class LimitCamera : MonoBehaviour
{
    public Transform player;
    public float height = 120f;
    public float damping = 2000000f;
    private Vector3 velocity = Vector3.zero;
    private GameObject playerIcon;

    void Start()
    {
        player = GameObject.Find("PlayerArmature").transform;
        playerIcon = GameObject.Find("PlayerIcon");
    }

    void LateUpdate()
    {
        if (player == null)
            return;
        // Desired position above player
        Vector3 targetPosition = new Vector3(player.position.x, height, player.position.z);
        // Smoothly move camera towards the target position using SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            1f / damping
        );
        // Look straight down
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        playerIcon.transform.position = new Vector3(
            player.transform.position.x,
            110,
            player.transform.position.z
        );
    }
}
