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

        //StoreOriginalGunAmmoValues();
    }

    private void Start()
    {
        // Oyun başladığında orijinal değerleri ayarla
    }

    private void StoreOriginalGunAmmoValues()
    {
        if (playerGunSelector == null || playerGunSelector.Guns == null)
            return;

        foreach (var gun in playerGunSelector.Guns)
        {
            if (gun != null && gun.AmmoConfig != null)
            {
                // Orijinal değerleri sakla
                gun.AmmoConfig.OriginalMaxAmmo = gun.AmmoConfig.MaxAmmo;
                gun.AmmoConfig.OriginalClipSize = gun.AmmoConfig.ClipSize;
            }
        }
    }

    // Örnek nerf metodu
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
                        // MaxAmmo ve ClipSize değerlerini yarıya düşürmeden önce kontrol et
                        if (gun.AmmoConfig.MaxAmmo > 0)
                        {
                            gun.AmmoConfig.MaxAmmo = Mathf.Max(0, gun.AmmoConfig.MaxAmmo / 2);
                        }

                        if (gun.AmmoConfig.ClipSize > 0)
                        {
                            gun.AmmoConfig.ClipSize = Mathf.Max(
                                0,
                                gun.AmmoConfig.OriginalClipSize / 2
                            );
                        }

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
                // MaxAmmo ve ClipSize değerlerini yarıya düşürmeden önce kontrol et
                if (activeGun.AmmoConfig.MaxAmmo > 0)
                {
                    activeGun.AmmoConfig.MaxAmmo = Mathf.Max(0, activeGun.AmmoConfig.MaxAmmo / 2);
                }

                if (activeGun.AmmoConfig.ClipSize > 0)
                {
                    activeGun.AmmoConfig.ClipSize = Mathf.Max(
                        0,
                        activeGun.AmmoConfig.OriginalClipSize / 2
                    );
                }

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
            var secondHandGun = playerGunSelector.SecondHandGun; // Ikinci el silahı al
            if (secondHandGun != null && secondHandGun.AmmoConfig != null)
            {
                // MaxAmmo ve ClipSize değerlerini yarıya düşürmeden önce kontrol et
                if (secondHandGun.AmmoConfig.MaxAmmo > 0)
                {
                    secondHandGun.AmmoConfig.MaxAmmo = Mathf.Max(
                        0,
                        secondHandGun.AmmoConfig.MaxAmmo / 2
                    );
                }

                if (secondHandGun.AmmoConfig.ClipSize > 0)
                {
                    secondHandGun.AmmoConfig.ClipSize = Mathf.Max(
                        0,
                        secondHandGun.AmmoConfig.OriginalClipSize / 2
                    );
                }

                // Eğer mevcut mermi sayısı MaxAmmo'dan fazlaysa, onu da sınırla
                if (secondHandGun.AmmoConfig.CurrentAmmo > secondHandGun.AmmoConfig.MaxAmmo)
                {
                    secondHandGun.AmmoConfig.CurrentAmmo = secondHandGun.AmmoConfig.MaxAmmo;
                }

                // Eğer mevcut şarjör mermisi sayısı ClipSize'dan fazlaysa, onu da sınırla
                if (secondHandGun.AmmoConfig.CurrentClipAmmo > secondHandGun.AmmoConfig.ClipSize)
                {
                    secondHandGun.AmmoConfig.CurrentClipAmmo = secondHandGun.AmmoConfig.ClipSize;
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
}
