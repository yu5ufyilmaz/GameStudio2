using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Optional for NavMeshAgent integration
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState { Idle, Patrol, Attack }
    private EnemyState currentState = EnemyState.Idle;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private Transform eyePosition;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float patrolDuration = 3f;
    private Vector3 patrolDirection;
    private float stateTimer;
    private Vector3 lastPosition;
    private float currentSpeed;

    [Header("Combat")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float shootCooldown = 1.5f;
    private bool canShoot = true;

    [Header("Animation")]
    [SerializeField] private float animationBlendSpeed = 8.0f;
    [SerializeField] private AudioClip[] footstepAudioClips;
    [SerializeField] private AudioClip landingAudioClip;
    [SerializeField] private float footstepAudioVolume = 0.5f;

    // Animation Parameters - similar to ThirdPersonController
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int IsShooting = Animator.StringToHash("isShooting");

    private Transform player;
    private Animator animator;
    private AudioSource audioSource;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float speedChangeRate = 10.0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (eyePosition == null)
            eyePosition = transform;

        lastPosition = transform.position;
        animator.SetBool(Grounded, true);

        StartCoroutine(UpdateState());
    }

    private void Update()
    {
        // Calculate movement speed for animations
        Vector3 currentPosition = transform.position;
        float moveDistance = Vector3.Distance(lastPosition, currentPosition);
        currentSpeed = moveDistance / Time.deltaTime;
        lastPosition = currentPosition;

        // Update animation parameters
        UpdateAnimation();

        // Handle state behavior
        switch (currentState)
        {
            case EnemyState.Idle:
                // Idle animation is handled by setting speed to 0
                break;

            case EnemyState.Patrol:
                transform.Translate(patrolDirection * moveSpeed * Time.deltaTime);
                break;

            case EnemyState.Attack:
                if (CanSeePlayer())
                {
                    // Look at player
                    Vector3 lookDirection = player.position - transform.position;
                    lookDirection.y = 0;
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(lookDirection),
                        Time.deltaTime * 5f
                    );

                    // Shoot at player
                    if (canShoot)
                        StartCoroutine(Shoot());
                }
                else
                {
                    // Lost sight of player, go back to idle
                    currentState = EnemyState.Idle;
                    stateTimer = idleDuration;
                }
                break;
        }

        // Check for player detection regardless of current state
        if (currentState != EnemyState.Attack && CanSeePlayer())
        {
            currentState = EnemyState.Attack;
        }
    }

    private void UpdateAnimation()
    {
        // Calculate target speed based on state
        float targetSpeed = 0;

        if (currentState == EnemyState.Patrol)
            targetSpeed = moveSpeed;
        else if (currentState == EnemyState.Attack)
            targetSpeed = runSpeed;

        // Normalize input direction
        float inputMagnitude = 1f;

        // Calculate animation blend similar to ThirdPersonController
        float targetBlend;

        if (currentState != EnemyState.Idle)
            targetBlend = targetSpeed;
        else
            targetBlend = 0f;

        // Apply smooth animation blending
        if (animationBlend < targetBlend)
        {
            animationBlend = Mathf.Lerp(animationBlend, targetBlend, Time.deltaTime * animationBlendSpeed);
        }
        else if (animationBlend > targetBlend)
        {
            animationBlend = Mathf.Lerp(animationBlend, targetBlend, Time.deltaTime * animationBlendSpeed);
        }

        if (animationBlend < 0.01f)
            animationBlend = 0f;

        // Update animator parameters
        animator.SetFloat(Speed, animationBlend);
        animator.SetFloat(MotionSpeed, inputMagnitude);

        // Rotate character to face movement direction
        if (currentState == EnemyState.Patrol || currentState == EnemyState.Attack)
        {
            Vector3 movementDirection = (transform.position - lastPosition).normalized;
            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speedChangeRate);
            }
        }
    }
    private IEnumerator UpdateState()
    {
        while (true)
        {
            // Don't change state if attacking
            if (currentState != EnemyState.Attack)
            {
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0)
                {
                    // Switch between idle and patrol
                    if (currentState == EnemyState.Idle)
                    {
                        currentState = EnemyState.Patrol;
                        stateTimer = patrolDuration;
                        // Choose random direction (left or right)
                        patrolDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    }
                    else
                    {
                        currentState = EnemyState.Idle;
                        stateTimer = idleDuration;
                    }
                }
            }

            yield return null;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        // Check if player is within range
        Vector3 directionToPlayer = player.position - eyePosition.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
            return false;

        // Check if player is within field of view
        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angle > fieldOfView / 2f)
            return false;

        // Check if there's no obstacle between enemy and player
        if (Physics.Raycast(eyePosition.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayer))
            return false;

        return true;
    }

    private IEnumerator Shoot()
    {
        canShoot = false;

        if (bulletPrefab != null && muzzlePoint != null)
        {
            // Play shooting animation
            animator.SetTrigger(IsShooting);

            // Instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Calculate direction to player with slight inaccuracy
                Vector3 direction = (player.position - muzzlePoint.position).normalized;

                // Add slight random spread
                direction += new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    Random.Range(-0.05f, 0.05f),
                    Random.Range(-0.05f, 0.05f)
                );

                rb.linearVelocity = direction * bulletSpeed;
            }
        }

        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    // Animation event methods (called by animation clips)
    public void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && footstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.position, footstepAudioVolume);
        }
    }

    public void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && landingAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.position, footstepAudioVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw field of view
        Gizmos.color = Color.red;
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2f, Vector3.up) * transform.forward * detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView / 2f, Vector3.up) * transform.forward * detectionRange;

        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
    }
}