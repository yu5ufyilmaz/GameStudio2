using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    public class GunPickup : MonoBehaviour
    {
        public GunScriptableObject Gun;

        [SerializeField]
        private int currentClipAmmo = 30;

        [SerializeField]
        private int currentAmmo = 120;

        public int CurrentClipAmmo
        {
            get => currentClipAmmo;
            set => currentClipAmmo = Mathf.Clamp(value, 0, Gun?.AmmoConfig.ClipSize ?? 0);
        }

        public int CurrentAmmo
        {
            get => currentAmmo;
            set => currentAmmo = Mathf.Clamp(value, 0, Gun?.AmmoConfig.MaxAmmo ?? 0);
        }

        private void OnValidate()
        {
            if (Gun != null)
            {
                CurrentClipAmmo = Mathf.Clamp(CurrentClipAmmo, 0, Gun.AmmoConfig.ClipSize);
                CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, Gun.AmmoConfig.MaxAmmo);
            }
        }

        public void SetPickupAmmoAmount(PlayerGunSelector DroppedGunBase)
        {
            currentAmmo = DroppedGunBase.ActiveGun.AmmoConfig.CurrentAmmo;
            currentClipAmmo = DroppedGunBase.ActiveGun.AmmoConfig.CurrentClipAmmo;
        }

        public void SetWeaponAmmoAmount(GunScriptableObject ActiveGun)
        {
            GunScriptableObject newGun = ActiveGun;
            newGun.AmmoConfig.CurrentClipAmmo = CurrentClipAmmo;
            newGun.AmmoConfig.CurrentAmmo = CurrentAmmo;
        }
    }
}
