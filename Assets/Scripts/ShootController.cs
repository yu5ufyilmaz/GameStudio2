using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShootController : MonoBehaviour
{
    private Animator animator;
    private bool isShooting;
    private bool isAiming;

    [SerializeField]
    private InputAction shootAction;

    [SerializeField]
    private InputAction aimAction;

    [SerializeField]
    private Image aimImage;

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
        isShooting = animator.GetBool("isShooting");

        if (isShooting)
        {
            // Shooting logic...
        }
        else
        {
            // Non-shooting logic...
        }

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
        isShooting = true;
        animator.SetBool("isShooting", true);
        // Additional shooting logic
    }

    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        isAiming = true;
        aimImage.enabled = true;
        if (thirdPersonController !=null)
        {
            thirdPersonController.IsAiming = true;
        }
    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        isAiming = false;
        aimImage.enabled = false;

        if (thirdPersonController!=null)
        {
            thirdPersonController.IsAiming = false;
        }
    }

    private void OnDestroy()
    {
        shootAction.performed -= OnShootPerformed;
        aimAction.performed -= OnAimPerformed;
        aimAction.canceled -= OnAimCanceled;
    }
}