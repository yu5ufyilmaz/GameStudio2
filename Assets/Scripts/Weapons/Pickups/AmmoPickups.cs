using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    [RequireComponent(typeof(Collider))]
    public class AmmoPickups : MonoBehaviour
    {
        public GunType Type;
        public int AmmoAmount = 5;
        public Vector3 SpinDirection = Vector3.up;

        void Update()
        {
            transform.Rotate(SpinDirection);
        }

        void OnTriggerEnter(Collider other)
        {
            if (
                other.TryGetComponent(out PlayerGunSelector playerGunSelector)
                && playerGunSelector.ActiveGun.Type == Type
            )
            {
                playerGunSelector.ActiveGun.AmmoConfig.AddAmmo(AmmoAmount);
                Destroy(gameObject);
            }
        }
    }
}
