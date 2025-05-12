using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    [RequireComponent(typeof(Collider))]
    public class HealthPickup : MonoBehaviour
    {
        public int HealthAmount = 20; // Restore amount
        public Vector3 SpinDirection = Vector3.up; // Spin effect

        void Update()
        {
            transform.Rotate(SpinDirection);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.GainHealth(HealthAmount); // Sağlık artırma
                Destroy(gameObject); // Pickup'ı yok et
            }
        }
    }
}
