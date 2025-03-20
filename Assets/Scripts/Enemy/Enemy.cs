using UnityEngine;

public class Enemy : MonoBehaviour
{
 public EnemyHealth health;
 public EnemyController controller;
 public EnemyPainResponse painResponse;
 void Start()
 {
    
    health.OnDeath += Die;
 }

 private void Die(Vector3 Position)
 {
    controller.Die();
 }
}
