using DotGalacticos;
using DotGalacticos.Guns.Demo;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public float reloadSpeed = 1f;
    public float enemyDamage = 10f;

    [SerializeField]
    private float speedMultiplier = 0.1f;

    private PlayerGunSelector playerGunSelector; // Silah seçici referansı
    private ThirdPersonController thirdPersonController;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerGunSelector = GetComponent<PlayerGunSelector>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    public void DecreaseMovementSpeed()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.MoveSpeed *= speedMultiplier;
            thirdPersonController.SprintSpeed *= speedMultiplier;
        }
    }

    public void IncreaseEnemyDamage(float amount = 5f)
    {
        enemyDamage += amount; // Düşman hasarını artır
    }

    public void DecreaseAmmoCapacityAllGuns()
    {
        // Oyundaki tüm silahlara erişip MaxAmmo ve ClipSize değerlerini yarıya düşür
        if (playerGunSelector != null)
        {
            var guns = playerGunSelector.Guns; // PlayerGunSelector içindeki tüm silahlar listesi

            if (guns != null)
            {
                foreach (var gun in guns)
                {
                    if (gun != null && gun.AmmoConfig != null)
                    {
                        // MaxAmmo'yu yarıya düşür
                        gun.AmmoConfig.MaxAmmo = Mathf.Max(0, gun.AmmoConfig.MaxAmmo / 2);

                        // ClipSize'ı yarıya düşür
                        gun.AmmoConfig.ClipSize = Mathf.Max(0, gun.AmmoConfig.ClipSize / 2);

                        // Eğer mevcut mermi sayısı MaxAmmo'dan fazlaysa, onu da sınırla
                        if (gun.AmmoConfig.CurrentAmmo > gun.AmmoConfig.MaxAmmo)
                        {
                            gun.AmmoConfig.CurrentAmmo = gun.AmmoConfig.MaxAmmo;
                        }

                        // Eğer mevcut şarjör mermisi sayısı ClipSize'dan fazlaysa, onu da sınırla
                        if (gun.AmmoConfig.CurrentClipAmmo > gun.AmmoConfig.ClipSize)
                        {
                            gun.AmmoConfig.CurrentClipAmmo = gun.AmmoConfig.ClipSize;
                        }
                    }
                }
            }

            // Aktif silahın değerlerini de güncelle
            var activeGun = playerGunSelector.ActiveGun; // Aktif silahı al
            if (activeGun != null && activeGun.AmmoConfig != null)
            {
                // MaxAmmo'yu yarıya düşür
                activeGun.AmmoConfig.MaxAmmo = Mathf.Max(0, activeGun.AmmoConfig.MaxAmmo / 2);

                // ClipSize'ı yarıya düşür
                activeGun.AmmoConfig.ClipSize = Mathf.Max(0, activeGun.AmmoConfig.ClipSize / 2);

                // Eğer mevcut mermi sayısı MaxAmmo'dan fazlaysa, onu da sınırla
                if (activeGun.AmmoConfig.CurrentAmmo > activeGun.AmmoConfig.MaxAmmo)
                {
                    activeGun.AmmoConfig.CurrentAmmo = activeGun.AmmoConfig.MaxAmmo;
                }

                // Eğer mevcut şarjör mermisi sayısı ClipSize'dan fazlaysa, onu da sınırla
                if (activeGun.AmmoConfig.CurrentClipAmmo > activeGun.AmmoConfig.ClipSize)
                {
                    activeGun.AmmoConfig.CurrentClipAmmo = activeGun.AmmoConfig.ClipSize;
                }
            }
        }

        // Dünya üzerindeki silahları kontrol et
        var allGunsInWorld = FindObjectsOfType<GunPickup>(); // Tüm silahları bul
        foreach (var gunPickup in allGunsInWorld)
        {
            if (gunPickup != null && gunPickup.Gun != null && gunPickup.Gun.AmmoConfig != null)
            {
                // MaxAmmo'yu yarıya düşür
                gunPickup.Gun.AmmoConfig.MaxAmmo = Mathf.Max(
                    0,
                    gunPickup.Gun.AmmoConfig.MaxAmmo / 2
                );

                // ClipSize'ı yarıya düşür
                gunPickup.Gun.AmmoConfig.ClipSize = Mathf.Max(
                    0,
                    gunPickup.Gun.AmmoConfig.ClipSize / 2
                );

                // Eğer mevcut mermi sayısı MaxAmmo'dan fazlaysa, onu da sınırla
                if (gunPickup.Gun.AmmoConfig.CurrentAmmo > gunPickup.Gun.AmmoConfig.MaxAmmo)
                {
                    gunPickup.Gun.AmmoConfig.CurrentAmmo = gunPickup.Gun.AmmoConfig.MaxAmmo;
                }

                // Eğer mevcut şarjör mermisi sayısı ClipSize'dan fazlaysa, onu da sınırla
                if (gunPickup.Gun.AmmoConfig.CurrentClipAmmo > gunPickup.Gun.AmmoConfig.ClipSize)
                {
                    gunPickup.Gun.AmmoConfig.CurrentClipAmmo = gunPickup.Gun.AmmoConfig.ClipSize;
                }
            }
        }
    }

    public void IncreaseAimDifficulty(float amount = 0.1f)
    {
        // Aim zorluğunu artırma işlemi
        // Örneğin, aim doğruluğunu azaltabilirsiniz
    }

    public void DecreaseReloadSpeed(float amount = 0.5f)
    {
        // Reload hızını azaltma işlemi
        // Örneğin, reload süresini artırabilirsiniz
    }
}
