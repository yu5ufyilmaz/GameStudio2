using UnityEngine;

public class EnemyPainResponse : MonoBehaviour
{
    public Animator animator;
    public EnemyHealth health;
    // Update is called once per frame
    public void TakeDamage()
    {

    }
    public void HandleDeath()
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");
    }
}
