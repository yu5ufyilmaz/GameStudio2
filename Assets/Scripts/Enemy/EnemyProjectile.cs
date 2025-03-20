using UnityEngine;

namespace StarterAssets
{
    public class EnemyProjectile : MonoBehaviour
    {
        public float Speed = 20f;
        public float Damage = 10f;
        public float LifeTime = 5f;
        public GameObject ImpactEffect;
        
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
                if (playerController != null)
                {
                    // Health sisteminiz varsa burada hasar verebilirsiniz
                    Debug.Log("Player hit with damage: " + Damage);
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