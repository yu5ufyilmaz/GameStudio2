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

        [Range(0, 1f)]
        public float HandIKAmount = 1f;

        [Range(0, 1f)]
        public float ElbowIKAmount = 1f;

        // Animasyon parametreleri
        private static readonly int IsShooting = Animator.StringToHash("isShooting");
        private static readonly int IsAiming = Animator.StringToHash("isAiming");
        private Animator animator;
        public bool isAiming;
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

        [Header("Nişan Alma Nesnesi")]
        [SerializeField]
        private GameObject aimTargetInstance; // Oluşturulacak nesne
        private Rig rig1;
        private Rig rig2;
        public float targetWeight = 0f;
        private float weightChangeSpeed = 5f; // Ağırlığın değişim hızı
        public float orbitRadius = 5f;
        public float fixedAimHeight = 1f; // Sabit nişan yüksekliği, ihtiyaç halinde değiştirilebilir

        void Start()
        {
            animator = GetComponent<Animator>();
            playerInput = GetComponent<PlayerInput>();
            mainCamera = Camera.main;
            thirdPersonController = GetComponent<ThirdPersonController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            rig1 = GameObject.Find("Aim Rig").GetComponent<Rig>();
            rig2 = GameObject.Find("Left Hand Rig").GetComponent<Rig>();
            shootAction = playerInput.actions["Shoot"];
            aimAction = playerInput.actions["Aim"];
            reloadAction = playerInput.actions["Reload"];

            shootAction.Enable();
            aimAction.Enable();
            reloadAction.Enable();

            shootAction.performed += OnShootPerformed;
            shootAction.canceled += OnShootCanceled;
            aimAction.performed += OnAimPerformed;
            reloadAction.performed += OnReloadPerformed;
            reloadAction.canceled += OnReloadCanceled;
            aimAction.canceled += OnAimCanceled;

            layerMask = LayerMask.GetMask("Default", "Obstacle", "Ground", "Enemy");
            aimImage.enabled = true; // Crosshair başlangıçta görünür
            Cursor.visible = false;
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
                if (isAiming && thirdPersonController.isRunning == false)
                {
                    IKWeight(rig1, targetWeight);
                    if (GunSelector != null)
                    {
                        GunSelector.ActiveGun.Tick(isShooting); // Sürekli ateşleme işlemi
                    }
                }

                // Her zaman Aim metodunu çağır - karakter koşarken yönünü Aim metodu içinde kontrol edeceğiz
                Aim();
            }
        }

        private void EndReload()
        {
            GunSelector.ActiveGun.EndReload();
            isReloading = false;
        }

        public float _lastAimAngle;

        private void Aim()
        {
            // İlk koşul: Koşarken nişan almaya çalışırsa
            /*   if (thirdPersonController._input.sprint && !isAiming)
               {
                   return;
               }*/

            // İkinci koşul: Nişan almaya başladıysa ve daha sonra koşmaya çalışırsa
            if (isAiming && thirdPersonController._input.sprint)
            {
                // Koşmayı engelle
                thirdPersonController._input.sprint = false;
            }

            // Fare pozisyonunu al
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // UI hedef imajını güncelle
            if (aimImage != null)
            {
                RectTransform rectTransform = aimImage.GetComponent<RectTransform>();
                rectTransform.position = mousePosition;
            }

            // Ray oluştur ve düzlemle kesişim noktasını bul
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, fixedAimHeight, 0));

            if (horizontalPlane.Raycast(ray, out float enter))
            {
                // İlk nişan alma durumu
                /*if (!isAiming)
                {
                    isAiming = true;
                }*/

                // Hedef noktayı hesapla
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 direction = hitPoint - transform.position;
                direction.y = 0;

                if (direction.sqrMagnitude < 0.001f)
                    return;

                // Nişan açısını hesapla ve sakla
                _lastAimAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // Nişan hedefini yerleştir
                if (aimTargetInstance != null)
                {
                    aimTargetInstance.SetActive(true);

                    // Sabit mesafede hedef pozisyonu hesapla
                    float targetDistance = Mathf.Min(direction.magnitude, orbitRadius);
                    Vector3 targetPosition =
                        transform.position + direction.normalized * targetDistance;
                    targetPosition.y = fixedAimHeight;

                    aimTargetInstance.transform.position = targetPosition;
                }
            }
            /* else
             {
                 // Nişan bırakıldığında
                 isAiming = false;
                 if (aimTargetInstance != null)
                 {
                     aimTargetInstance.SetActive(false);
                 }
             }*/
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
            // Koşarken nişan almayı engelle
            if (thirdPersonController._input.sprint)
            {
                return;
            }

            isAiming = true;
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
            isAiming = false;
            if (thirdPersonController != null)
            {
                thirdPersonController.IsAiming = false;
            }

            // Nişan alma sona erdiğinde hedef ağırlığı 0'a ayarla
            targetWeight = 0f;
            IKWeight(rig1, targetWeight);
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
