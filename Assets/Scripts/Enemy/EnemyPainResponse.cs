using System.Collections;
using UnityEngine;

public class EnemyPainResponse : MonoBehaviour
{
    public Animator animator;
    public EnemyHealth health;

    // Update is called once per frame
    public void HandlePain()
    {
        animator.SetTrigger("Hit");
    }
    public void HandleDeath()
    {

        animator.SetTrigger("Die");

    }

}
