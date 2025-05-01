using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DotGalacticos.Guns.Demo
{
    public class ShootController : MonoBehaviour
    {
        [SerializeField]
        private Image crosshair;

        [SerializeField]
        Transform aimTarget;

        [SerializeField]
        private LayerMask layerMask;

        [Space(10)]
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
        private bool isAiming;
        private bool isShooting = false;

        //Reload
        [SerializeField]
        private bool autoReload = true;
        private bool shouldManuelReload = false;

        [SerializeField]
        public bool isReloading = false;

        // Ses bileşeni
        private AudioSource audioSource;

        // INPUT PARAMETRELERI
        private InputAction shootAction;
        private InputAction aimAction;
        private InputAction reloadAction;
        private PlayerInput playerInput;
        private Camera mainCamera;
        private ThirdPersonController thirdPersonController;

        [Header("Referanslar")]
        [SerializeField]
        private Image aimImage;

        //private Transform muzzlePoint;
        [SerializeField]
        private AudioClip shootSound;

        [SerializeField]
        private CameraTargetSwitcher cameraTargetSwitcher;

        [Range(0, 1)]
        public float shootAudioVolume = 0.5f;

        [Header("Nişan Alma Nesnesi")]
        [SerializeField]
        private GameObject aimTargetInstance; // Oluşturulacak nesne
        private Rig rig1;
        private Rig rig2;
        public float targetWeight = 0f;
        private float weightChangeSpeed = 5f; // Ağırlığın değişim hızı
        private MultiAimConstraint ikConstraint;

        void Start()
        {
            animator = GetComponent<Animator>();
            playerInput = GetComponent<PlayerInput>();
            mainCamera = Camera.main;
            thirdPersonController = GetComponent<ThirdPersonController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            rig1 = GameObject.Find("Rig 1").GetComponent<Rig>();
            rig2 = GameObject.Find("Rig 2").GetComponent<Rig>();
            shootAction = playerInput.actions["Shoot"];
            aimAction = playerInput.actions["Aim"];
            reloadAction = playerInput.actions["Reload"];

            shootAction.Enable();
            aimAction.Enable();

            shootAction.performed += OnShootPerformed;
            shootAction.canceled += OnShootCanceled;
            aimAction.performed += OnAimPerformed;
            reloadAction.performed += OnReloadPerformed;
            reloadAction.canceled += OnReloadCanceled;
            aimAction.canceled += OnAimCanceled;

            layerMask = LayerMask.GetMask("Default", "Obstacle", "Ground", "Enemy");
            aimImage.enabled = true; // Crosshair başlangıçta görünür
            Cursor.visible = false;

            // DoIKMagic();
        }

        void DoIKMagic()
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            leftHandTarget = allChildren.FirstOrDefault(child => child.name == "LeftHandTarget");
            leftElbowTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbowTarget");
            leftIdleHandTarget = allChildren.FirstOrDefault(child =>
                child.name == "LeftIdleHandTarget"
            );
            leftIdleElbowTarget = allChildren.FirstOrDefault(child =>
                child.name == "LeftIdleElbowTarget"
            );
        }

        void Update()
        {
            if (ShouldAutoReload() || ShouldManuelReload())
            {
                //IKWeight(rig2, targetWeight);
                GunSelector.ActiveGun.StartReloading();
                isReloading = true;
                animator.SetTrigger("Reload");
            }
            else
            {
                if (isAiming)
                {
                    IKWeight(rig1, targetWeight);
                }
                else
                {
                    //IKWeight(rig2, targetWeight);
                }
                Aim();
            }

            if (GunSelector != null)
            {
                GunSelector.ActiveGun.Tick(isShooting); // Sürekli ateşleme işlemi
            }
        }

        private void EndReload()
        {
            GunSelector.ActiveGun.EndReload();
            isReloading = false;
        }

        public float orbitRadius = 5f;
        public float fixedAimHeight = 1f; // Sabit nişan yüksekliği, ihtiyaç halinde değiştirilebilir

        private void Aim()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            RectTransform rectTransform = aimImage.GetComponent<RectTransform>();
            rectTransform.position = mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            // Yatay düzlemde karakter konumuna göre mouse ışınının açılarını hesaplayacağız
            // Öncelikle yatay düzlemde mouse pozisyonunun karaktere göre konumunu bulalım

            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, fixedAimHeight, 0)); // Y sabit düzlem

            if (horizontalPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                // Karakter pozisyonundan hitPoint'e yatay yön vektörü (Y 0)
                Vector3 direction = hitPoint - transform.position;
                direction.y = 0;

                // Eğer fare karakterin tam üstünde ise (direction sıfırsa), işlem yapma
                if (direction.sqrMagnitude < 0.001f)
                    return;

                // Açıyı hesapla (0-360 derece)
                float angle = Mathf.Atan2(direction.z, direction.x); // radyan cinsinden

                // Hedef pozisyonunu sabit yarıçap ve hesaplanan açıya göre hesapla
                // Unity yatayda Z ekseni ileri, X ekseni sağdır.
                float targetX = transform.position.x + orbitRadius * Mathf.Cos(angle);
                float targetZ = transform.position.z + orbitRadius * Mathf.Sin(angle);
                float targetY = fixedAimHeight;

                Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

                // aimTargetInstance pozisyonunu güncelle
                if (aimTargetInstance != null)
                {
                    aimTargetInstance.transform.position = targetPosition;
                }

                // Karakter sadece yatay düzlemde hedefe dönsün
                Vector3 lookDirection = targetPosition - transform.position;
                lookDirection.y = 0;

                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * 5f
                    );
                }
            }
        }

        IEnumerator MoveToTargetRoutine(Transform aimTarget, float duration)
        {
            Vector3 startPosition = aimTargetInstance.transform.position; // Başlangıç pozisyonu

            float elapsedTime = 0f; // Geçen süre

            while (elapsedTime < duration)
            {
                // Geçen süreyi normalize et
                float t = elapsedTime / duration;

                // Lerp ile yeni pozisyonu hesapla
                aimTargetInstance.transform.position = Vector3.Lerp(
                    startPosition,
                    aimTarget.position,
                    t
                );

                // Geçen süreyi güncelle
                elapsedTime += Time.deltaTime;

                // Bir sonraki frame'e geç
                yield return null;
            }

            // Hedef pozisyona tam olarak ulaş
            aimTargetInstance.transform.position = aimTarget.position;
        }

        private bool ShouldAutoReload()
        {
            return !isReloading
                && autoReload
                && GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0
                && GunSelector.ActiveGun.AmmoConfig.CanReload();
        }

        private bool ShouldManuelReload()
        {
            return !isReloading
                && shouldManuelReload
                && GunSelector.ActiveGun.AmmoConfig.CanReload();
        }

        void IKWeight(Rig rig, float targetWeight)
        {
            rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * weightChangeSpeed);
            //animator.SetLayerWeight(1, rig.weight);
        }

        public void DodgeIK(float weight)
        {
            rig1.weight = weight;
            rig2.weight = weight;
        }

        private void OnShootPerformed(InputAction.CallbackContext context)
        {
            if (GunSelector != null)
            {
                isShooting = true;
            }
        }

        private void OnShootCanceled(InputAction.CallbackContext context)
        {
            if (GunSelector != null)
            {
                isShooting = false;
            }
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            shouldManuelReload = true;
        }

        private void OnReloadCanceled(InputAction.CallbackContext context)
        {
            shouldManuelReload = false;
        }

        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            /* _leftHandReferans.position = leftHandTarget.position;
             _leftHandReferans.rotation = leftHandTarget.rotation;
             _leftElbowReferans.position = leftElbowTarget.position;*/

            isAiming = true;
            if (thirdPersonController != null)
            {
                thirdPersonController.IsAiming = true;
            }
            //cameraTargetSwitcher.SwitchTargets();

            // Nişan alındığında hedef ağırlığı 1'e ayarla
            targetWeight = 1f;
            animator.SetBool(IsAiming, true);
        }

        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            /* _leftHandReferans.position = leftIdleHandTarget.position;
             _leftHandReferans.rotation = leftIdleHandTarget.rotation;
             _leftElbowReferans.position = leftIdleElbowTarget.position;*/

            isAiming = false;

            if (gameObject.activeInHierarchy)
            {
                //StartCoroutine(MoveToTargetRoutine(aimTarget, 1.0f)); // 1 saniyede hedefe ulaşır
            }

            if (thirdPersonController != null)
            {
                thirdPersonController.IsAiming = false;
            }
            //cameraTargetSwitcher.FixTarget();
            // Nişan alma sona erdiğinde hedef ağırlığı 0'a ayarla
            targetWeight = 1f;
            animator.SetBool(IsAiming, false);
        }

        private void OnDestroy()
        {
            // InputAction'ları devre dışı bırak
            if (shootAction != null)
            {
                shootAction.performed -= OnShootPerformed;
                shootAction.canceled -= OnShootCanceled;
                shootAction.Disable();
            }

            if (reloadAction != null)
            {
                reloadAction.performed -= OnReloadPerformed;
                reloadAction.canceled -= OnReloadCanceled;
                reloadAction.Disable();
            }

            if (aimAction != null)
            {
                aimAction.performed -= OnAimPerformed;
                aimAction.canceled -= OnAimCanceled;
                aimAction.Disable();
            }
        }
    }
}
