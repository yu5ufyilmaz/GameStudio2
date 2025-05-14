using System.Collections;
using DotGalacticos.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // Referanslar
    [Header("Referanslar")]
    [SerializeField]
    protected Transform playerTransform;

    [SerializeField]
    protected Animator animator;

    [SerializeField]
    protected NavMeshAgent navMeshAgent;

    [SerializeField]
    protected Transform weaponMuzzle;

    [SerializeField]
    protected GameObject bulletPrefab;

    // Düşman Özellikleri
    [Header("Düşman Özellikleri")]
    [SerializeField]
    protected float Damage = 1;

    [SerializeField]
    protected float detectionRange = 10f;

    [SerializeField]
    protected float attackRange = 8f;

    [SerializeField]
    protected float fireRate = 1f;

    [SerializeField]
    protected float patrolSpeed = 2f;

    [SerializeField]
    protected float chaseSpeed = 6f;

    [SerializeField]
    protected float bulletSpeed = 20f;

    [SerializeField]
    protected float speedChangeRate = 10.0f;

    [Header("Mermi Düşürme")]
    [Range(0f, 100f)]
    [SerializeField]
    protected float dropChanceOverall = 50f; // Genel düşürme olasılığı (örn %50)

    [SerializeField]
    private AmmoDrop[] ammoDrops; // Düşman öldüğünde düşürülecek mermiler

    // Devriye Noktaları
    [Header("Devriye")]
    [SerializeField]
    protected Transform[] patrolPoints;

    [SerializeField]
    protected float waypointStopDistance = 0.5f;

    [SerializeField]
    protected float waitTime = 2f;

    [Header("Sesler")]
    public AudioClip[] FootstepAudioClips;
    public AudioClip[] DieAudioClips;

    [Range(0, 1)]
    public float FootstepAudioVolume = 0.5f;

    [Range(0, 1)]
    public float DieAudioVolume = 0.5f;

    [SerializeField]
    protected AudioClip shootSound;

    [Range(0, 1)]
    public float shootAudioVolume = 0.5f;

    // Animator Parametreleri
    protected int _animIDSpeed;
    protected int _animIDFire;

    // Durum Yönetimi
    protected enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
    }

    protected EnemyState currentState = EnemyState.Idle;

    // Özel Değişkenler
    protected int currentPatrolIndex = 0;
    protected bool canFire = true;
    protected bool isWaiting = false;
    protected float distanceToPlayer;
    protected Vector3 lastKnownPlayerPosition;
    protected bool playerVisible = false;

    [SerializeField]
    protected float _animationBlend;
    public bool isDie = false;

    protected virtual void Start()
    {
        // Eğer player referansı verilmediyse, tag ile bulma
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Eğer NavMeshAgent referansı verilmediyse, kendinden alma
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Eğer Animator referansı verilmediyse, kendinden alma
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Animator parametrelerini ayarla
        AssignAnimationIDs();

        // Başlangıç durumu
        ChangeState(EnemyState.Patrol);
    }

    protected virtual void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDFire = Animator.StringToHash("isShooting");
    }

    protected virtual void Update()
    {
        if (!IsStopEnemy())
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Oyuncuyu görüş kontrolü
            CheckPlayerVisibility();
            UpdateAnimation();

            // Durum makinesi
            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;
                case EnemyState.Patrol:
                    UpdatePatrolState();
                    break;
                case EnemyState.Chase:
                    UpdateChaseState();
                    break;
                case EnemyState.Attack:
                    UpdateAttackState();
                    break;
            }
        }
        else
        {
            EnemyDieActions();
        }
    }

    protected virtual void EnemyDieActions()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        characterController.enabled = false;
        navMeshAgent.enabled = false;
        animator.SetLayerWeight(1, 0f);
    }

    public virtual bool IsStopEnemy()
    {
        return isDie;
    }

    public virtual void OnDieSound()
    {
        DieAudioVolume = PlayerPrefs.GetFloat("SFXVolume");

        if (DieAudioClips.Length > 0)
        {
            var index = Random.Range(0, DieAudioClips.Length);
            AudioSource.PlayClipAtPoint(DieAudioClips[index], transform.position, DieAudioVolume);
        }
    }

    protected virtual void OnFootStep(AnimationEvent animationEvent)
    {
        FootstepAudioVolume = PlayerPrefs.GetFloat("SFXVolume") * 0.5f;
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(
                    FootstepAudioClips[index],
                    transform.position,
                    FootstepAudioVolume
                );
            }
        }
    }

    protected virtual void UpdateAnimation()
    {
        float currentHorizontalSpeed = new Vector3(
            navMeshAgent.velocity.x,
            0f,
            navMeshAgent.velocity.z
        ).magnitude;

        float animSpeed = currentHorizontalSpeed;
        if (currentHorizontalSpeed < 0.1f)
            animSpeed = 0f;
        if (currentHorizontalSpeed > 6f)
            animSpeed = 6f;

        _animationBlend = Mathf.Lerp(_animationBlend, animSpeed, Time.deltaTime * speedChangeRate);
        animator.SetFloat("Speed", _animationBlend);
    }

    protected virtual void CheckPlayerVisibility()
    {
        playerVisible = false;

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            if (
                Physics.Raycast(
                    transform.position + Vector3.up * 0.4f,
                    directionToPlayer,
                    out RaycastHit hit,
                    detectionRange
                )
            )
            {
                if (hit.transform == playerTransform)
                {
                    playerVisible = true;
                    lastKnownPlayerPosition = playerTransform.position;

                    if (distanceToPlayer <= attackRange)
                    {
                        if (currentState != EnemyState.Attack)
                        {
                            ChangeState(EnemyState.Attack);
                        }
                    }
                    else
                    {
                        if (currentState != EnemyState.Chase)
                        {
                            ChangeState(EnemyState.Chase);
                        }
                    }
                }
            }
        }

        if (
            !playerVisible
            && (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        )
        {
            if (currentState == EnemyState.Attack)
            {
                ChangeState(EnemyState.Chase);
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
            }

            if (
                Vector3.Distance(transform.position, lastKnownPlayerPosition) < waypointStopDistance
                || !navMeshAgent.hasPath
            )
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    protected virtual void UpdateIdleState()
    {
        if (!isWaiting)
        {
            StartCoroutine(WaitAndTransition(waitTime, EnemyState.Patrol));
        }
    }

    protected virtual void UpdatePatrolState()
    {
        if (patrolPoints.Length == 0)
            return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= waypointStopDistance)
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndTransition(waitTime, EnemyState.Patrol));
            }
        }
    }

    protected virtual void UpdateChaseState()
    {
        if (playerVisible)
        {
            navMeshAgent.SetDestination(playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyState.Attack);
            }
        }
    }

    protected virtual void UpdateAttackState()
    {
        if (playerVisible)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );

            if (canFire)
            {
                StartCoroutine(FireRoutine());
            }
        }
        else if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Attack)
        {
            animator.SetBool(_animIDFire, false);
        }

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true;
                break;

            case EnemyState.Patrol:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolSpeed;
                SetNextPatrolPoint();
                break;

            case EnemyState.Chase:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = chaseSpeed;
                if (playerTransform != null)
                {
                    navMeshAgent.SetDestination(playerTransform.position);
                }
                break;

            case EnemyState.Attack:
                navMeshAgent.isStopped = true;
                animator.SetBool(_animIDFire, true);
                break;
        }
    }

    protected virtual void SetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    protected virtual IEnumerator WaitAndTransition(float time, EnemyState nextState)
    {
        isWaiting = true;
        yield return new WaitForSeconds(time);
        isWaiting = false;

        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
        {
            ChangeState(nextState);
        }
    }

    protected virtual IEnumerator FireRoutine()
    {
        canFire = false;

        if (bulletPrefab != null && weaponMuzzle != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                weaponMuzzle.position,
                weaponMuzzle.rotation
            );

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            EnemyProjectile enemyProjectile = bullet.GetComponent<EnemyProjectile>();
            enemyProjectile.Damage = Damage;
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bullet.transform.forward * bulletSpeed;
            }

            PlayShootSound();
            Destroy(bullet, 3f);
        }

        yield return new WaitForSeconds(1f / fireRate);
        canFire = true;
    }

    protected virtual void PlayShootSound()
    {
        shootAudioVolume = PlayerPrefs.GetFloat("SFXVolume");
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, shootAudioVolume);
        }
    }

    public void IncreaseDamage(float amount)
    {
        Damage *= amount;
    }

    public void DropAmmo()
    {
        float roll = Random.Range(0f, 100f);
        if (roll > dropChanceOverall)
        {
            // Düşürme şansı tutmadı, çık.
            return;
        }
        // Weighted random seçim için toplam ağırlığı hesapla
        float totalWeight = 0f;
        foreach (var ammoDrop in ammoDrops)
        {
            totalWeight += ammoDrop.dropChance;
        }
        float randomWeight = Random.Range(0f, totalWeight);
        float currentSum = 0f;
        foreach (var ammoDrop in ammoDrops)
        {
            currentSum += ammoDrop.dropChance;
            if (randomWeight <= currentSum)
            {
                Instantiate(ammoDrop.ammoPrefab, transform.position, Quaternion.identity);
                break;
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
