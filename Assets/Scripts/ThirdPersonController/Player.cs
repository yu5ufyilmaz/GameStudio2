using DotGalacticos.Guns.Demo;
using UnityEngine;

namespace DotGalacticos
{
    public class Player : MonoBehaviour
    {
        public ThirdPersonController playerController;
        public PlayerHealth health;
        public ShootController playerShootController;
        public PlayerPainResponse painResponse;

        void Start()
        {
            health.OnDeath += Die;
            health.OnTakeDamage += HitResponse;
        }

        private void HitResponse(int Damage)
        {
            painResponse.HandlePain();
        }

        private void Die(Vector3 Position)
        {
            painResponse.HandleDeath();
            playerController.enabled = false;
            playerShootController.enabled = false;
            playerController.Die();
        }
    }
}
