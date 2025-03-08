using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShootController : MonoBehaviour
{
    private static readonly int IsShooting = Animator.StringToHash("isShooting");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private Animator animator;
    private bool isShooting;
    private bool isAiming;
    private bool isShootingInProgress;
    private bool canShoot = true;

    private InputAction shootAction;
    private InputAction aimAction;

    [SerializeField]
    private Image aimImage;
    [SerializeField]
    private GameObject bulletPrefab;          
    [SerializeField]
    private Transform muzzlePoint;            
    [SerializeField]
    private float bulletSpeed = 20f;           
    [SerializeField]
    private float recoilAmount = 0.1f;
    [SerializeField]
    private float bulletSpread = 0.05f;
    [SerializeField]
    private float shootCooldown = 0.5f;

    private PlayerInput playerInput;
    private Camera mainCamera;
    private ThirdPersonController thirdPersonController;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        thirdPersonController = GetComponent<ThirdPersonController>();

        shootAction = playerInput.actions["Shoot"];
        aimAction = playerInput.actions["Aim"];

        shootAction.Enable();
        aimAction.Enable();

        shootAction.performed += OnShootPerformed;
        aimAction.performed += OnAimPerformed;
        aimAction.canceled += OnAimCanceled;

        aimImage.enabled = false;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isAiming)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            aimImage.transform.position = mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPosition = hit.point;
                Vector3 direction = (targetPosition - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                }
            }
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (!isShootingInProgress && canShoot)
        {
            isShootingInProgress = true;
            isShooting = true;
            animator.SetBool(IsShooting, true);
            float animationDuration = GetAnimationDuration("isShooting");
            StartCoroutine(ResetShooting(animationDuration));
            ApplyRecoil();
            ApplyBulletSpread();
            Shoot();  // Yeni: Ateş etme fonksiyonunu çağır
            StartCoroutine(ShootCooldown());
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Calculate direction from muzzle to target
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 targetPosition = hit.point;
                    Vector3 direction = (targetPosition - muzzlePoint.position).normalized;

                    // Apply bullet spread
                    direction += new Vector3(
                        Random.Range(-bulletSpread, bulletSpread),
                        Random.Range(-bulletSpread, bulletSpread),
                        Random.Range(-bulletSpread, bulletSpread));
                
                    // Use velocity instead of linearVelocity
                    rb.linearVelocity = direction * bulletSpeed;
                }
                else
                {
                    rb.linearVelocity = muzzlePoint.forward * bulletSpeed;
                }
            }
        
        }
    }

    private float GetAnimationDuration(string animationName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
                return clip.length;
        }
        return 0.5f;
    }

    private IEnumerator ResetShooting(float duration)
    {
        yield return new WaitForSeconds(duration);
        isShooting = false;
        isShootingInProgress = false;
        animator.SetBool(IsShooting, false);
    }

    private void ApplyRecoil()
    {
        mainCamera.transform.localPosition += Random.insideUnitSphere * recoilAmount;
    }

    private void ApplyBulletSpread()
    {
        Vector3 spread = new Vector3(
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread),
            0);
        mainCamera.transform.forward += spread;
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        isAiming = true;
        aimImage.enabled = true;
        if (thirdPersonController != null)
        {
            thirdPersonController.IsAiming = true;
        }
        animator.SetBool(IsAiming, true);
    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        isAiming = false;
        aimImage.enabled = false;
        if (thirdPersonController != null)
        {
            thirdPersonController.IsAiming = false;
        }
        animator.SetBool(IsAiming, false);
    }

    private void OnDestroy()
    {
        shootAction.performed -= OnShootPerformed;
        aimAction.performed -= OnAimPerformed;
        aimAction.canceled -= OnAimCanceled;

        shootAction.Disable();
        aimAction.Disable();
    }
}
