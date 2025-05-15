using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int _MaxHealth = 100;
    private RagdollEnabler ragdollEnabler;

    [SerializeField]
    private int _Health;

    [SerializeField]
    private HealthBar healthBar;

    [SerializeField]
    private Animator _animator;
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

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;

    void Start()
    {
        ragdollEnabler = GetComponent<RagdollEnabler>();
    }

    private void OnEnable()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int Damage, Vector3 hitPoint)
    {
        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);

        CurrentHealth -= damageTaken;

        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
            healthBar.SetHealth(CurrentHealth);
        }

        if (CurrentHealth == 0 && damageTaken != 0)
        {
            ragdollEnabler.EnableRagdoll();
            //_animator.SetTrigger("Die");
            OnDeath?.Invoke(transform.position);
            //Destroy(gameObject);
        }
    }

    public void GainHealth(int amount)
    {
        if (amount <= 0)
            return; // Negatif veya sıfır sağlık artışı yok sayılır
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        healthBar.SetHealth(CurrentHealth); // Sağlık çubuğunu güncelle
    }
}
