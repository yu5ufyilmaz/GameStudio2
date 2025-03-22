using UnityEngine;

public class PlayerPainResponse : MonoBehaviour
{
    public Animator animator;
    public PlayerHealth health;
    // Update is called once per frame
    public void HandlePain()
    {
        Debug.Log("Ah uh");
    }
    public void HandleDeath()
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
