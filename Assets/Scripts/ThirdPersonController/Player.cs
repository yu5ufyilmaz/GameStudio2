using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ThirdPersonController playerController;
    public PlayerHealth health;
    public ShootController playerShootController;
    public PlayerPainResponse painResponse;

    void Start()
    {
        health.OnDeath += Die;
    }

    private void Die(Vector3 Position)
    {
        painResponse.HandleDeath();
    }
}
