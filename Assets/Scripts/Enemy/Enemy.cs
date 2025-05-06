using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyHealth health;
    public EnemyController controller;
    public EnemyPainResponse painResponse;

    void Start()
    {
        health.OnDeath += Die;
        health.OnTakeDamage += HitResponse;
    }

    private void HitResponse(int Damage)
    {
        painResponse.HandlePain();
    }

    private void Die(Vector3 Position)
    {
        painResponse.HandleDeath();
        controller.OnDieSound();
        controller.isDie = true;
    }
}
