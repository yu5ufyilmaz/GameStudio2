using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int _MaxHealth = 100;

    [SerializeField]
    private int _Health;
    public int CurrentHealth
    {
        get => _Health;
        set => _Health = value;
    }
    public int MaxHealth
    {
        get => _MaxHealth;
        set => _MaxHealth = value;
    }

    [SerializeField]
    private RagdollEnabler ragdollEnabler;

    [SerializeField]
    private float fadeOutDelay = 10f;

    // Kan partikül prefab'ı için referans
    [SerializeField]
    private GameObject bloodParticlePrefab;

    [SerializeField]
    private GameObject Icon;

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;

    private void OnEnable()
    {
        CurrentHealth = MaxHealth;
    }

    // Arayüzdeki TakeDamage metodunu uyguluyoruz
    public void TakeDamage(int damage, Vector3 hitPoint)
    {
        int damageTaken = Mathf.Clamp(damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;

        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
            SpawnBloodEffect(hitPoint); // Vurulduğu noktada kan efekti oluştur
        }

        if (CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
            if (ragdollEnabler != null)
            {
                ragdollEnabler.EnableRagdoll();
                Icon.SetActive(false);
                StartCoroutine(FadeOut());
            }
        }
    }

    private void SpawnBloodEffect(Vector3 hitPoint)
    {
        // Vurulduğu noktada kan partikülünü oluştur
        if (bloodParticlePrefab != null)
        {
            GameObject bloodEffect = Instantiate(
                bloodParticlePrefab,
                hitPoint,
                Quaternion.identity
            );
            Destroy(bloodEffect, 2f); // 2 saniye sonra yok et
        }
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(fadeOutDelay);
        if (ragdollEnabler != null)
        {
            ragdollEnabler.DisableAllRigidbodies();
        }
        float time = 0f;
        while (time < 1f)
        {
            transform.position += Vector3.down * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
