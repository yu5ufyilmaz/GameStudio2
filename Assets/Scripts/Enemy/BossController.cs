using System.Collections;
using UnityEngine;

public class BossController : EnemyController
{
    [Header("Boss Özellikleri")]
    public Sin sin; // Bossun temsil ettiği günah

    [SerializeField]
    private int bossHealth = 200; // Bossun başlangıç canı
    private int currentHealth; // Mevcut can

    [Tooltip("Bossun savaşabilir olup olmadığını belirler.")]
    public bool canFight = true; // Bu değer Unity Inspector'da değiştirilebilir

    protected override void Start()
    {
        base.Start();
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
                // Sadece devriye davranışı, saldırıya geçmez
                PatrolOnlyBehavior();
            }
        }
        else
        {
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        ApplyNerf();
        OnDieSound();
        isDie = true;
        navMeshAgent.enabled = false;
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    private void ApplyNerf()
    {
        /* switch (sin)
         {
             case Sin.Kıskançlık:
                 PlayerStats.Instance.IncreaseEnemyDamage();
                 break;
             case Sin.Kibir:
                 // Kibir için özel etki ekleyebilirsiniz
                 break;
             case Sin.Açgözlülük:
                 PlayerStats.Instance.DecreaseAmmoCapacity();
                 break;
             case Sin.Şehvet:
                 PlayerStats.Instance.IncreaseAimDifficulty();
                 break;
             case Sin.Oburluk:
                 PlayerStats.Instance.DecreaseMovementSpeed();
                 break;
             case Sin.Tembellik:
                 PlayerStats.Instance.DecreaseReloadSpeed();
                 break;
         }*/
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
