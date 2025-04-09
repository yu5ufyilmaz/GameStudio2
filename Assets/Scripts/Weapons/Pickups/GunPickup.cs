using UnityEngine;
namespace DotGalacticos.Guns.Demo
{
public class GunPickup : MonoBehaviour
{
   public GunScriptableObject Gun;
   public Vector3 SpinDirection = Vector3.up;

        void Update()
        {
            transform.Rotate(SpinDirection);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent(out PlayerGunSelector playerGunSelector))
            {
                playerGunSelector.PickupGun(Gun);
                Destroy(gameObject);
            }
        }
    }
}

