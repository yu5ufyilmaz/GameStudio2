using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // Referanslar
    [Header("Referanslar")]
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private Transform weaponMuzzle;

    [SerializeField]
    private GameObject bulletPrefab;

    // Düşman Özellikleri
    [Header("Düşman Özellikleri")]
    [SerializeField]
    private float health = 100f;

    [SerializeField]
    private float detectionRange = 10f;

    [SerializeField]
    private float attackRange = 8f;

    [SerializeField]
    private float fireRate = 1f;

    [SerializeField]
    private float patrolSpeed = 2f;

    [SerializeField]
    private float chaseSpeed = 6f;

    [SerializeField]
    private float bulletSpeed = 20f;

    [SerializeField]
    private float speedChangeRate = 10.0f;

    // Devriye Noktaları
    [Header("Devriye")]
    [SerializeField]
    private Transform[] patrolPoints;

    [SerializeField]
    private float waypointStopDistance = 0.5f;

    [SerializeField]
    private float waitTime = 2f;

    [Header("Sesler")]
    public AudioClip[] FootstepAudioClips;

    [Range(0, 1)]
    public float FootstepAudioVolume = 0.5f;

    [SerializeField]
    private AudioClip shootSound;

    [Range(0, 1)]
    public float shootAudioVolume = 0.5f;

    // Animator Parametreleri
    private int _animIDSpeed;
    private int _animIDFire;

    // Durum Yönetimi
    private enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
    }

    private EnemyState currentState = EnemyState.Idle;

    // Özel Değişkenler
    private int currentPatrolIndex = 0;
    private bool canFire = true;
    private bool isWaiting = false;
    private float distanceToPlayer;
    private Vector3 lastKnownPlayerPosition;
    private bool playerVisible = false;
    private float _targetSpeed;
    private float _currentSpeed;
    private float _animationBlend;
    public bool isDie = false;

    private void Start()
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

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDFire = Animator.StringToHash("isShooting");
    }

    private void Update()
    {
        if (IsStopEnemy() == false)
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

    private void EnemyDieActions()
    {
        navMeshAgent.enabled = false;
        animator.SetLayerWeight(1, 0f);
        //Destroy(gameObject, 4f);
    }

    public bool IsStopEnemy()
    {
        return isDie;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
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

    private void UpdateAnimation()
    {
        // Mevcut hızı hesapla
        float currentHorizontalSpeed = new Vector3(
            navMeshAgent.velocity.x,
            0f,
            navMeshAgent.velocity.z
        ).magnitude;

        // Duruma göre hedef hızı ayarla
        switch (currentState)
        {
            case EnemyState.Idle:
                _targetSpeed = 0f; // Idle = 0
                break;
            case EnemyState.Patrol:
                _targetSpeed = 2f; // Walk = 2
                break;
            case EnemyState.Chase:
                _targetSpeed = 6f; // Run = 6
                break;
            case EnemyState.Attack:
                _targetSpeed = 0f; // Idle = 0
                break;
        }

        // Gerçek hızdan animasyon hızını hesapla (0-6 arasında)
        float animSpeed = currentHorizontalSpeed;
        // Eğer 0.1'den küçükse tamamen durduğunu varsay
        if (currentHorizontalSpeed < 0.1f)
            animSpeed = 0f;
        // 6'dan büyükse 6 ile sınırla
        if (currentHorizontalSpeed > 6f)
            animSpeed = 6f;

        _animationBlend = Mathf.Lerp(_animationBlend, animSpeed, Time.deltaTime * speedChangeRate);

        // Animatör parametresini güncelle
        animator.SetFloat(_animIDSpeed, _animationBlend);
    }

    private void CheckPlayerVisibility()
    {
        playerVisible = false;

        if (distanceToPlayer <= detectionRange)
        {

            // Oyuncuya yönelik bir ray (ışın) oluşturma
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            // Ray ile görüş kontrolü
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

                    // Oyuncu görüldüğünde durum değişimi
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

        // Oyuncu görünmüyorsa ve şu an kovalama veya saldırı durumunda isek
        if (
            playerVisible == false
            && (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        )
        {
            // Oyuncunun son görüldüğü konuma git
            if (currentState == EnemyState.Attack)
            {
                ChangeState(EnemyState.Chase);
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
            }

            // Son konum yakınına geldiysek veya yol bulunamadıysa devriyeye geri dön
            if (
                Vector3.Distance(transform.position, lastKnownPlayerPosition) < waypointStopDistance
                || !navMeshAgent.hasPath
            )
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    private void UpdateIdleState()
    {
        // Bekleme süresini kontrol et
        if (!isWaiting)
        {
            StartCoroutine(WaitAndTransition(waitTime, EnemyState.Patrol));
        }
    }

    private void UpdatePatrolState()
    {
        if (patrolPoints.Length == 0)
            return;

        // Eğer hedef noktaya yeterince yaklaştıysak
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= waypointStopDistance)
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndTransition(waitTime, EnemyState.Patrol));
            }
        }
    }

    private void UpdateChaseState()
    {
        if (playerVisible)
        {
            navMeshAgent.SetDestination(playerTransform.position);

            // Eğer oyuncu atak menzilinde ise
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyState.Attack);
            }
        }
    }

    private void UpdateAttackState()
    {
        if (playerVisible)
        {
            // Düşmanı oyuncuya doğru döndür
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );

            // Ateş etme
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

    private void ChangeState(EnemyState newState)
    {
        // Önceki durum temizliği
        if (currentState == EnemyState.Attack)
        {
            animator.SetBool(_animIDFire, false);
        }

        // Yeni durum ayarları
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

    private void SetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private IEnumerator WaitAndTransition(float time, EnemyState nextState)
    {
        isWaiting = true;
        yield return new WaitForSeconds(time);

        isWaiting = false;

        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
        {
            ChangeState(nextState);
        }
    }

    private IEnumerator FireRoutine()
    {
        canFire = false;

        // Ateş etme animasyonu tetiklenir (animator'da Fire parametresi true yapıldı)

        // Mermi oluşturma
        if (bulletPrefab != null && weaponMuzzle != null)
        {
            // Mermi oluşturma işlemi
            GameObject bullet = Instantiate(
                bulletPrefab,
                weaponMuzzle.position,
                weaponMuzzle.rotation
            );
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bullet.transform.forward * bulletSpeed;
            }

            // Belirli süre sonra yok et
            PlayShootSound();
            Destroy(bullet, 3f);
        }

        // Ateş hızı kadar bekle
        yield return new WaitForSeconds(1f / fireRate);

        canFire = true;
    }

    private void PlayShootSound()
    {
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, shootAudioVolume);
        }
    }

    // Gizmo ile görüş ve ateş menzillerini görselleştirme
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
