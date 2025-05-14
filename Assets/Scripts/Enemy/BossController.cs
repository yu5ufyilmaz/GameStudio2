using System;
using System.Collections;
using DotGalacticos;
using UnityEngine;

public class BossController : EnemyController
{
    [Header("Boss Özellikleri")]
    public Sin sin; // Bossun temsil ettiği günah

    [Tooltip("Bossun savaşabilir olup olmadığını belirler.")]
    public bool canFight = true; // Bu değer Unity Inspector'da değiştirilebilir

    private bool nerfApplied = false;
    private SinMenu sinMenu;

    protected override void Start()
    {
        base.Start();
        sinMenu = GameObject.Find("SinMenu").GetComponent<SinMenu>();
    }

    protected override void Update()
    {
        if (!isDie)
        {
            if (canFight)
            {
                base.Update();
            }
            else
            {
                UpdateAnimation();
                // Sadece devriye davranışı, saldırıya geçmez
                PatrolOnlyBehavior();
            }
        }
        else
        {
            if (!nerfApplied) // Eğer nerf uygulanmadıysa
            {
                ApplyNerf(); // Nerf'i uygula
                nerfApplied = true; // Nerf'in uygulandığını işaretle
                if (sinMenu != null)
                {
                    sinMenu.RevealCardForSin(sin);
                }
            }
            EnemyDieActions(); // Boss öldüğünde yapılacak işlemler
        }
    }

    private void PatrolOnlyBehavior()
    {
        // Oyuncu algılama ve saldırı olmadan sadece devriye
        // Burada devriye durumuna zorla geçilebilir veya farklı özel hareketler yapılabilir
        if (currentState != EnemyState.Patrol)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected override void UpdateAnimation()
    {
        Debug.Log($"NAber BOSSS");
        base.UpdateAnimation();
    }

    public void ApplyNerf()
    {
        switch (sin)
        {
            case Sin.Kıskançlık:
                PlayerStats.Instance.IncreaseEnemyDamage();
                break;
            case Sin.Kibir:
                PlayerStats.Instance.PrideDebuff();
                // Kibir için özel etki ekleyebilirsiniz
                break;
            case Sin.Açgözlülük:
                PlayerStats.Instance.DecreaseAmmoCapacityAllGuns();
                break;
            case Sin.Şehvet:
                PlayerStats.Instance.IncreaseAimDifficulty(0.5f);
                break;
            case Sin.Oburluk:
                PlayerStats.Instance.DecreaseMovementSpeed();
                break;
            case Sin.Tembellik:
                PlayerStats.Instance.DecreaseReloadSpeed();
                break;
        }
    }

    protected override void UpdateAttackState()
    {
        if (!canFight)
            return;

        base.UpdateAttackState();

        if (canFire && playerVisible)
        {
            StartCoroutine(FireRoutine());
        }
    }

    protected override IEnumerator FireRoutine()
    {
        canFire = false;

        if (bulletPrefab != null && weaponMuzzle != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                weaponMuzzle.position,
                weaponMuzzle.rotation
            );
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = weaponMuzzle.forward * bulletSpeed;
            }
            Destroy(bullet, 3f);
        }

        yield return new WaitForSeconds(1f / fireRate);
        canFire = true;
    }
}
