using System.Collections.Generic;
using DotGalacticos;
using DotGalacticos.Guns;
using DotGalacticos.Guns.Demo;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public float reloadSpeed = 1f;
    public float enemyDamageMultiplier = 1.5f;
    public float enemyHealthMultiplier = 1.5f;

    // Add spread multiplier to control aim difficulty
    public float aimSpreadMultiplier = 1f;

    [SerializeField]
    private float speedMultiplier = 0.1f;

    [SerializeField]
    private float defaultSpreadIncrease = 0.1f;

    [SerializeField]
    private float maxSpreadMultiplier = 3f;

    private PlayerGunSelector playerGunSelector; // Silah seçici referansı
    private ThirdPersonController thirdPersonController;

    // Mermi kapasitesi ayarını takip etmek için yeni değişken
    private bool isAmmoCapacityReduced = false;
    Animator animator;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerGunSelector = GetComponent<PlayerGunSelector>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }

    #region IncreaseEnemyHealth
    public void PrideDebuff()
    {
        EnemyHealth[] enemies = GameObject.FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemies)
        {
            enemy.IncreaseHealth(enemyHealthMultiplier);
        }
    }
    #endregion
    #region AmmoCapacity
    // Tüm silahların mermi kapasitelerini yarıya düşüren metod
    public void DecreaseAmmoCapacityAllGuns()
    {
        if (playerGunSelector == null)
        {
            Debug.LogWarning("PlayerGunSelector bulunamadı!");
            return;
        }

        // Eğer mermi kapasitesi zaten azaltılmışsa, işlemi tekrarlama
        if (isAmmoCapacityReduced)
            return;

        isAmmoCapacityReduced = true;
        ModifyAmmoCapacity(true);
    }

    // Tüm silahların mermi kapasitelerini yarıya indiren veya normale döndüren metod
    private void ModifyAmmoCapacity(bool reduce)
    {
        // Aktif silahı güncelle
        if (playerGunSelector.ActiveGun != null)
        {
            UpdateGunAmmoCapacity(playerGunSelector.ActiveGun, reduce);

            // Aktif silahın durumunu güncelle
            int activeGunIndex = GetGunIndex(playerGunSelector.ActiveGun);
            if (activeGunIndex > 0)
            {
                SaveAmmoState(activeGunIndex, playerGunSelector.ActiveGun);
            }
        }

        // Birinci el silahını güncelle
        if (playerGunSelector.FirstHandGun != null)
        {
            UpdateGunAmmoCapacity(playerGunSelector.FirstHandGun, reduce);
            SaveAmmoState(1, playerGunSelector.FirstHandGun);
        }

        // İkinci el silahını güncelle
        if (playerGunSelector.SecondHandGun != null)
        {
            UpdateGunAmmoCapacity(playerGunSelector.SecondHandGun, reduce);
            SaveAmmoState(2, playerGunSelector.SecondHandGun);
        }
    }

    // Belirtilen silahın mermi kapasitelerini günceller
    private void UpdateGunAmmoCapacity(GunScriptableObject gun, bool reduce)
    {
        if (gun != null && gun.AmmoConfig != null)
        {
            if (reduce)
            {
                // Kapasiteleri yarıya indirirken en az 1 olmasını sağla
                gun.AmmoConfig.MaxAmmo = Mathf.Max(1, gun.AmmoConfig.OriginalMaxAmmo / 2);
                gun.AmmoConfig.ClipSize = Mathf.Max(1, gun.AmmoConfig.OriginalClipSize / 2);

                // Mevcut mermi sayılarını yeni sınırlara göre ayarla
                gun.AmmoConfig.CurrentAmmo = Mathf.Min(
                    gun.AmmoConfig.CurrentAmmo,
                    gun.AmmoConfig.MaxAmmo
                );
                gun.AmmoConfig.CurrentClipAmmo = Mathf.Min(
                    gun.AmmoConfig.CurrentClipAmmo,
                    gun.AmmoConfig.ClipSize
                );
            }
            else
            {
                // Normal değerlere döndür
                gun.AmmoConfig.MaxAmmo = gun.AmmoConfig.OriginalMaxAmmo;
                gun.AmmoConfig.ClipSize = gun.AmmoConfig.OriginalClipSize;
            }
        }
    }

    // Silahın indeksini döndürür (1: birinci el, 2: ikinci el, 0: bilinmeyen)
    private int GetGunIndex(GunScriptableObject gun)
    {
        if (
            gun == playerGunSelector.FirstHandGun
            || gun.Type == playerGunSelector.FirstHandGun?.Type
        )
            return 1;
        else if (
            gun == playerGunSelector.SecondHandGun
            || gun.Type == playerGunSelector.SecondHandGun?.Type
        )
            return 2;
        else
            return 0; // Bilinmeyen silah
    }

    // Silah durumunu kaydet
    private void SaveAmmoState(int index, GunScriptableObject gun)
    {
        // AmmoState sınıfı PlayerGunSelector içinde olduğundan, doğrudan erişmek yerine
        // playerGunSelector.ammoStates dictionary'sini güncellememiz gerekir.
        // Ancak bu değişken private olduğu için bu metodu PlayerGunSelector sınıfına eklemeniz gerekebilir
        // veya playerGunSelector için böyle bir metod yazabilirsiniz:

        // Örnek: playerGunSelector.SaveAmmoState(index, gun.AmmoConfig);

        // PlayerGunSelector'da gerekli method yoksa, şu şekilde uygulayabilirsiniz:
        // (Bu durumda PlayerGunSelector sınıfında ammoStates dictionary'sini public yapmanız veya
        // bir property aracılığıyla erişilebilir yapmanız gerekir)
    }
    #endregion
    #region Aim Difficulty Methods
    public void IncreaseAimDifficulty(float amount = 0.1f)
    {
        // Default değer yerine parametre olarak gelen amount değerini kullan
        float increaseAmount = amount > 0 ? amount : defaultSpreadIncrease;

        // Mevcut spread çarpanını arttır, ancak maksimum değeri aşmasın
        aimSpreadMultiplier = Mathf.Min(aimSpreadMultiplier + increaseAmount, maxSpreadMultiplier);

        // Aktif silahın (veya tüm silahların) spread değerlerini güncelle
        UpdateAllGunsSpread();

        Debug.Log($"Aim difficulty increased. New spread multiplier: {aimSpreadMultiplier}");
    }

    // Aim zorluğunu normale döndürme metodu (yeni)
    public void ResetAimDifficulty()
    {
        // Spread çarpanını tekrar 1'e ayarla (varsayılan değer)
        aimSpreadMultiplier = 1f;

        // Tüm silahların spread değerlerini güncelle
        UpdateAllGunsSpread();

        Debug.Log("Aim difficulty reset to normal");
    }

    // Tüm silahların spread değerlerini günceller
    private void UpdateAllGunsSpread()
    {
        // Aktif silahı güncelle
        if (playerGunSelector.ActiveGun != null && playerGunSelector.ActiveGun.ShootConfig != null)
        {
            UpdateGunSpread(playerGunSelector.ActiveGun);
        }

        // Birinci el silahını güncelle
        if (
            playerGunSelector.FirstHandGun != null
            && playerGunSelector.FirstHandGun.ShootConfig != null
        )
        {
            UpdateGunSpread(playerGunSelector.FirstHandGun);
        }

        // İkinci el silahını güncelle
        if (
            playerGunSelector.SecondHandGun != null
            && playerGunSelector.SecondHandGun.ShootConfig != null
        )
        {
            UpdateGunSpread(playerGunSelector.SecondHandGun);
        }
    }

    // Belirli bir silahın spread değerlerini günceller
    private void UpdateGunSpread(GunScriptableObject gun)
    {
        if (gun.ShootConfig != null)
        {
            // ShootConfig'deki spread değerlerini güncelle
            if (gun.ShootConfig.SpreadType == BulletSpreadType.Simple)
            {
                // Eğer simple spread ise spread ve minSpread değerlerini arttır
                gun.ShootConfig.Spread = new Vector3(
                    aimSpreadMultiplier,
                    aimSpreadMultiplier,
                    aimSpreadMultiplier
                );

                // MinSpread değerlerini de arttır, ancak daha düşük bir oranda
                gun.ShootConfig.MinSpread = new Vector3(0.05f, 0.05f, 0.05f);
            }
            else if (gun.ShootConfig.SpreadType == BulletSpreadType.TextureBased)
            {
                // Texture-based spread için SpreadMultiplier değerini arttır
                gun.ShootConfig.SpreadMultiplier = aimSpreadMultiplier;
            }

            // Maksimum spread zamanını azalt (daha hızlı spread)
            gun.ShootConfig.MaxSpreadTime = Mathf.Max(
                0.1f,
                gun.ShootConfig.MaxSpreadTime / aimSpreadMultiplier
            );
        }
    }
    #endregion
    #region Reload Speed
    public void DecreaseReloadSpeed()
    {
        animator.SetFloat("ReloadSpeed", reloadSpeed);
    }
    #endregion
    #region Movement Speed
    public void DecreaseMovementSpeed()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.MoveSpeed *= speedMultiplier;
            thirdPersonController.SprintSpeed *= speedMultiplier;
        }
    }
    #endregion
    #region Enemy Damage
    public void IncreaseEnemyDamage()
    {
        EnemyController[] enemies = GameObject.FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            enemy.IncreaseDamage(enemyDamageMultiplier);
        }
    }
    #endregion
}
