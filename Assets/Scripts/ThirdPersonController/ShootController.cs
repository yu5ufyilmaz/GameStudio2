using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using System.Linq;
using Unity.VisualScripting;

public class ShootController : MonoBehaviour
{
    [SerializeField]
    private PlayerGunSelector GunSelector;
    public Transform _leftHandReferans;
    public Transform _leftElbowReferans;

    public Transform leftElbowTarget;
    public Transform leftHandTarget;

    public Transform leftIdleHandTarget;
    public Transform leftIdleElbowTarget;
    [Range(0, 1f)]
    public float HandIKAmount = 1f;

    [Range(0, 1f)]
    public float ElbowIKAmount = 1f;

    // Animasyon parametreleri
    private static readonly int IsShooting = Animator.StringToHash("isShooting");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private Animator animator;
    private bool isShooting;
    private bool isAiming;
    private bool isShootingInProgress;
    private bool canShoot = true;

    // Ses bileşeni
    private AudioSource audioSource;

    // INPUT PARAMETRELERI
    private InputAction shootAction;
    private InputAction aimAction;
    private PlayerInput playerInput;
    private Camera mainCamera;
    private ThirdPersonController thirdPersonController;

    [Header("Referanslar")]
    [SerializeField]
    private Image aimImage;
    [SerializeField]
    private GameObject bulletPrefab;

    //private Transform muzzlePoint;
    [SerializeField]
    private AudioClip shootSound;
    [Range(0, 1)] public float shootAudioVolume = 0.5f;

    [Header("Ateş Parametreleri")]
    [SerializeField]
    private float bulletSpeed = 20f;
    [SerializeField]
    private float recoilAmount = 0.1f;
    [SerializeField]
    private float bulletSpread = 0.05f;
    [SerializeField]
    private float shootCooldown = 0.5f;

    [Header("Nişan Alma Nesnesi")]
    [SerializeField]
    private GameObject aimTargetInstance; // Oluşturulacak nesne
    private Rig rig;
    private TwoBoneIKConstraint leftHandIKConstraint;
    public float targetWeight = 0f;
    private float weightChangeSpeed = 10f; // Ağırlığın değişim hızı
    void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        thirdPersonController = GetComponent<ThirdPersonController>();
        audioSource = gameObject.AddComponent<AudioSource>();
        rig = GameObject.Find("Rig 1").GetComponent<Rig>();
        shootAction = playerInput.actions["Shoot"];
        aimAction = playerInput.actions["Aim"];

        shootAction.Enable();
        aimAction.Enable();

        shootAction.performed += OnShootPerformed;
        aimAction.performed += OnAimPerformed;
        aimAction.canceled += OnAimCanceled;

        aimImage.enabled = false;
        Cursor.visible = false;
        DoIKMagic();
        // Nişan alma nesnesini oluştur
    }
    void DoIKMagic()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        leftHandTarget = allChildren.FirstOrDefault(child => child.name == "LeftHandTarget");
        leftElbowTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbowTarget");
        leftIdleHandTarget = allChildren.FirstOrDefault(child => child.name == "LeftIdleHandTarget");
        leftIdleElbowTarget = allChildren.FirstOrDefault(child => child.name == "LeftIdleElbowTarget");
    }
    void Update()
    {
        if (isAiming)
        {
            Aim();
        }
        IKWeight();
    }
    void Aim()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        aimImage.transform.position = mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("TransparentWalls")))
        {
            Vector3 targetPosition = hit.point;

            // Nişan alma nesnesini güncelle
            if (aimTargetInstance != null)
            {
                aimTargetInstance.transform.position = targetPosition;
            }

            // Karakterin nişan aldığı yöne bakmasını sağla
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Y eksenini sıfırla, sadece X ve Z düzleminde döndür
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            // Çizgi çizme
            //Debug.DrawLine(GunSelector.ActiveGun.position, targetPosition, Color.red);
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (GunSelector != null)
        {
            GunSelector.ActiveGun.Shoot();
            PlayShootSound();
        }

        /* if (!isShootingInProgress && canShoot)
         {
             isShootingInProgress = true;
             isShooting = true;
             animator.SetBool(IsShooting, true);
             float animationDuration = GetAnimationDuration("isShooting");
             StartCoroutine(ResetShooting(animationDuration));
             ApplyRecoil();
              ApplyBulletSpread();
             Shoot();  // Ateş etme fonksiyonunu çağır


              StartCoroutine(ShootCooldown());

         }*/
    }
    /*
        private void Shoot()
        {
            if (bulletPrefab != null && muzzlePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    // Fare konumunu kullanarak hedef pozisyonunu belirle
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 targetPosition = hit.point;
                        Vector3 direction = (targetPosition - muzzlePoint.position).normalized;

                        // Apply bullet spread
                        direction += new Vector3(
                            Random.Range(-bulletSpread, bulletSpread),
                            Random.Range(-bulletSpread, bulletSpread),
                            Random.Range(-bulletSpread, bulletSpread));

                        // Merminin yönünü ayarla
                        rb.linearVelocity = direction * bulletSpeed; // Doğru yöne ateş et
                    }
                }
            }
        }*/

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

    private void PlayShootSound()
    {
        if (shootSound != null && audioSource != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, GunSelector.transform.position, shootAudioVolume);
        }
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


    void IKWeight()
    {
        // Ağırlığı yavaşça hedef ağırlığa doğru değiştir
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * weightChangeSpeed);
    }

    private void OnAimPerformed(InputAction.CallbackContext context)
    {

        _leftHandReferans.position = leftHandTarget.position;
        _leftHandReferans.rotation = leftHandTarget.rotation;
        _leftElbowReferans.position = leftElbowTarget.position;

        isAiming = true;
        aimImage.enabled = true;
        if (thirdPersonController != null)
        {
            thirdPersonController.IsAiming = true;
        }

        // Nişan alındığında hedef ağırlığı 1'e ayarla
        targetWeight = 1f;
        animator.SetBool(IsAiming, true);

    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {

        _leftHandReferans.position = leftIdleHandTarget.position;
        _leftHandReferans.rotation = leftIdleHandTarget.rotation;
        _leftElbowReferans.position = leftIdleElbowTarget.position;

        isAiming = false;
        aimImage.enabled = false;
        if (thirdPersonController != null)
        {
            thirdPersonController.IsAiming = false;
        }
        // Nişan alma sona erdiğinde hedef ağırlığı 0'a ayarla


        targetWeight = 0f;
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