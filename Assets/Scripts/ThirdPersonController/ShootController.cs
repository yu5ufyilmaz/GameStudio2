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

        [SerializeField]
        private LayerMask groundMask;

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

        private (bool success, Vector3 position) GetMousePosition()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
            {
                // The Raycast hit something, return with the position.
                return (success: true, position: hitInfo.point);
            }
            else
            {
                // The Raycast did not hit anything.
                return (success: false, position: Vector3.zero);
            }
        }

        private void Aim()
        {
            // Fare pozisyonunu doğrudan al
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Fareden bir ışın oluştur
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // Dünya üzerinde bir düzlem tanımla (Y ekseni yönünde)
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * fixedAimHeight);

            // Işının düzlemle kesişim noktasını bul
            if (groundPlane.Raycast(ray, out float hitDistance))
            {
                // Kesişim noktasını hesapla
                Vector3 hitPoint = ray.GetPoint(hitDistance);

                // Debug ile kontrol et
                Debug.DrawLine(mainCamera.transform.position, hitPoint, Color.red);

                // UI hedef imajını fare pozisyonuna ayarla
                if (aimImage != null)
                {
                    aimImage.rectTransform.position = mousePos;
                }

                // Karakterden hedef noktaya yön vektörü hesapla
                Vector3 targetDirection = hitPoint - transform.position;
                targetDirection.y = 0; // Y eksenini sıfırla (düz bakması için)

                if (targetDirection.magnitude < 0.1f)
                    return; // Çok küçük değerler için işlem yapma

                // Nişan açısını hesapla
                _lastAimAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                // Karakter nişan alıyorsa ve koşmuyorsa
                if (isAiming && !thirdPersonController._input.sprint)
                {
                    // Karakterin bakış yönünü güncelle (yumuşak dönüş için)
                    transform.forward = Vector3.Lerp(
                        transform.forward,
                        targetDirection.normalized,
                        Time.deltaTime * 10f
                    );

                    // AimTarget nesnesini güncelle
                    if (aimTargetInstance != null)
                    {
                        aimTargetInstance.SetActive(true);

                        // Minimum mesafe tanımla (bu değeri ihtiyacınıza göre ayarlayabilirsiniz)
                        float minDistance = 2.0f;

                        // Hedef nesnesini konumlandır
                        // Eğer hedef karaktere çok yakınsa, minimum mesafede konumlandır
                        float actualDistance = targetDirection.magnitude;
                        float useDistance =
                            actualDistance < minDistance ? minDistance : actualDistance;

                        // Hedef pozisyonunu hesapla - artık maksimum sınır yok
                        Vector3 targetPos =
                            transform.position + targetDirection.normalized * useDistance;
                        targetPos.y = transform.position.y + fixedAimHeight; // Sabit yükseklik

                        aimTargetInstance.transform.position = targetPos;

                        // Kontrol için debug çizgisi
                        Debug.DrawLine(transform.position, targetPos, Color.green);
                    }
                }
                else if (aimTargetInstance != null)
                {
                    aimTargetInstance.SetActive(false);
                }
            }
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
            /*if (thirdPersonController._input.sprint)
            {
                return;
            }*/

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
            // Koşarken nişan almayı engelle
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
