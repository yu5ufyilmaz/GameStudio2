using UnityEngine;

public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; }

    public delegate void TakeDamageEvent(int Damage);
    public event TakeDamageEvent OnTakeDamage;

    public delegate void DeathEvent(Vector3 Position);
    public event DeathEvent OnDeath;

    public void TakeDamage(int Damage, Vector3 hitPoint);
}
