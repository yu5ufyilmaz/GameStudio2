using UnityEngine;

public class PlayerPainResponse : MonoBehaviour
{
    public Animator animator;
    public PlayerHealth health;

    // Update is called once per frame
    public void HandlePain()
    {
        animator.SetTrigger("Hit");
        //CameraShake.Instance.ShakeCamera(1f, 0.5f);
        VignetteController.SpecialEffects.SetIntensity(Random.Range(0.35f, 0.8f));
    }

    public void HandleDeath()
    {
        //CameraShake.Instance.ShakeCamera(3f, 1f);
        VignetteController.SpecialEffects.SetIntensity(Random.Range(0.35f, 0.8f));
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");
        // Destroy(gameObject, 3f);
    }
}
