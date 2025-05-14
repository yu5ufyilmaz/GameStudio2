using System.Collections;
using UnityEngine;

public class EnemyPainResponse : MonoBehaviour
{
    public Animator animator;
    public EnemyHealth health;

    private int painAnimationCount = 3;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
    }

    // Update is called once per frame
    public void HandlePain()
    {
        // Choose a random pain trigger from the list
        int painIndex = Random.Range(0, painAnimationCount);
        animator.SetFloat("PainIndex", painIndex);

        animator.SetTrigger("Hit");
    }

    public void HandleDeath()
    {
        animator.SetTrigger("Die");
    }
}
