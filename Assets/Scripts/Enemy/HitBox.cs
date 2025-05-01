using DotGalacticos.Guns;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public EnemyHealth health;

    public void OnEnemyHit(IDamageable damageable, int damage, Vector3 Hit)
    {
        damageable.TakeDamage(damage, Hit);
    }
}
