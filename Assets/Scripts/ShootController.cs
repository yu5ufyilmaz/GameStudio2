using UnityEngine;
using UnityEngine.InputSystem;
                
public class ShootController : MonoBehaviour 
{ 
    private Animator animator; 
    private bool isShooting;
    [SerializeField]
    private InputAction shootAction;
    private PlayerInput playerInput;
                
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
        shootAction.Enable();
        shootAction.performed += OnShootPerformed;
    }
                
    // Update is called once per frame
    void Update()
    {
        isShooting = animator.GetBool("isShooting");
                
        // You can add additional logic here to handle shooting behavior
        if (isShooting)
        {
            // Handle shooting logic
        }
        else
        {
            // Handle non-shooting logic
        }
    }
                
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        // Handle the shooting action
        isShooting = true;
        animator.SetBool("isShooting", true);
                
        // Add your shooting logic here
    }
                
    private void OnDestroy()
    {
        shootAction.performed -= OnShootPerformed;
    }
}