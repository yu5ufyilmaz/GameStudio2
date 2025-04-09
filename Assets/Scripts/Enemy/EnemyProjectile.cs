using UnityEngine;

namespace DotGalacticos.Enemy
{
    public class EnemyProjectile : MonoBehaviour
    {
        public float Speed = 20f;
        public float Damage = 10f;
        public float LifeTime = 5f;
        public GameObject ImpactEffect;
        void OnTriggerEnter(Collider other)
        {
            // Çarpışma efekti oluştur
            if (ImpactEffect != null)
            {
                Instantiate(ImpactEffect, transform.position, Quaternion.identity);
            }

            // Player ile çarpışma kontrolü
            if (other.gameObject.CompareTag("Player"))
            {
                // Player'a hasar ver
                ThirdPersonController playerController = other.gameObject.GetComponent<ThirdPersonController>();
                PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)Damage, transform.position);
                }
                else
                {
                    Debug.Log($"PlayerHealth component not found on {other.gameObject.name}");
                }
            }
            else
            {
                // Mermiyi yok et
                Debug.Log("Projectile not hit player");
            }

            // Mermiyi yok et
            Destroy(gameObject);
        }
        private void Start()
        {
            // Belirli bir süre sonra mermiyi yok et
            Destroy(gameObject, LifeTime);
        }

        private void Update()
        {
            // İleri doğru hareket et
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Çarpışma efekti oluştur
            if (ImpactEffect != null)
            {
                Instantiate(ImpactEffect, transform.position, Quaternion.identity);
            }

            // Player ile çarpışma kontrolü
            if (collision.gameObject.CompareTag("Player"))
            {
                // Player'a hasar ver
                ThirdPersonController playerController = collision.gameObject.GetComponent<ThirdPersonController>();
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)Damage, transform.position);
                }
                else
                {
                    Debug.Log($"PlayerHealth component not found on {collision.gameObject.name}");
                }
            }
            else
            {
                // Mermiyi yok et
                Debug.Log("Projectile not hit player");
            }

            // Mermiyi yok et
            Destroy(gameObject);
        }
    }
}