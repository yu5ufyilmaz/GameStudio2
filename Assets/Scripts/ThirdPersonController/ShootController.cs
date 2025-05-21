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
        public GameObject aimTargetInstance; // Oluşturulacak nesne
        private Rig rig1;
        private Rig rig2;
        public float targetWeight = 0f;
        private float weightChangeSpeed = 5f; // Ağırlığın değişim hızı
        public float orbitRadius = 10f;
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

            aimImage.enabled = true; // Crosshair başlangıçta görünür
            Cursor.visible = false;
        }

        void Update()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (ShouldAutoReload() || ShouldManuelReload())
            {
                //IKWeight(rig2, targetWeight);
                GunSelector.ActiveGun.StartReloading();
                isReloading = true;

                animator.SetTrigger("Reload");
                StartCoroutine(HandleReloadAnimation(GetReloadAnimationLength()));
            }
            else
            {
                if (isAiming && thirdPersonController.isRunning == false)
                {
                    IKWeight(rig1, targetWeight);
                    if (GunSelector != null && !isReloading)
                    {
                        GunSelector.ActiveGun.Tick(isShooting, aimTargetInstance.transform); // Sürekli ateşleme işlemi
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

        // Diğer değişkenler...
        public float reloadElapsedTime; // Reload süresi boyunca geçen zaman
        private float reloadDuration; // Reload süresi

        private IEnumerator HandleReloadAnimation(float duration)
        {
            reloadDuration = duration; // Reload süresini ayarla
            reloadElapsedTime = 0f; // Geçen zamanı sıfırla
            isReloading = true; // Reload işlemi başladığını belirt
            while (reloadElapsedTime < reloadDuration)
            {
                reloadElapsedTime += Time.deltaTime; // Geçen zamanı güncelle
                yield return null; // Bir frame bekle
            }
            isReloading = false; // Reload işlemi tamamlandı
            reloadElapsedTime = 0f; // Geçen zamanı sıfırla
        }

        public float GetReloadAnimationLength()
        {
            // Animator Controller'ı al
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            // Tüm animasyon kliplerini kontrol et
            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip.name == "Reloading") // "Reload" animasyonunu kontrol et
                {
                    return clip.length; // Animasyonun süresini döndür
                }
            }
            // Eğer "Reload" animasyonu bulunamazsa, 0 döndür
            Debug.LogWarning("Reload animation not found!");
            return 0f;
        }

        public float _lastAimAngle;

        private void Aim()
        {
            // Koşarken nişan almayı engelle
            if (isAiming && thirdPersonController._input.sprint)
            {
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

            // Kamera pozisyonundan ray oluştur
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            // Maksimum menzil değeri - çok uzaktaki nesnelere nişan almayı sınırla
            float maxAimDistance = 100f;

            // Düşman ve nesne katmanları için raycast
            RaycastHit interactableHit;
            bool hitInteractable = Physics.Raycast(
                ray,
                out interactableHit,
                maxAimDistance,
                layerMask
            );

            // Zemin için raycast
            RaycastHit groundHit;
            bool hitGround = Physics.Raycast(ray, out groundHit, maxAimDistance, groundMask);

            if (aimTargetInstance != null)
            {
                aimTargetInstance.SetActive(true);

                Vector3 targetPosition;

                if (hitInteractable)
                {
                    // Düşman veya nesneye isabet - doğrudan o noktaya nişan al
                    targetPosition = interactableHit.point;
                }
                else if (hitGround)
                {
                    // Zemine isabet durumunda basitçe işaretçiyi tam ray doğrultusunda konumlandır
                    // Ray'in uzunluğunu hesapla (kamera ile isabet noktası arasındaki mesafe)
                    float rayLength = Vector3.Distance(ray.origin, groundHit.point);

                    // O mesafede ray üzerinde bir nokta bul - bu kamerayla aynı açıda olacaktır
                    targetPosition = ray.GetPoint(rayLength);
                }
                else
                {
                    // Herhangi bir şeye isabet yok - rayın doğrultusunda ilerle
                    targetPosition = ray.origin + ray.direction * maxAimDistance;

                    // Eğer hedef çok yukarıdaysa (gökyüzüne doğru ateş) uygun bir yükseklik sınırlaması ekle
                    float maxYPosition = mainCamera.transform.position.y + 10f; // Makul bir yükseklik sınırı
                    if (targetPosition.y > maxYPosition)
                    {
                        // Gökyüzüne ateş etmeyi sınırla, hedefi yatay düzlemde tut
                        targetPosition.y = maxYPosition;
                    }

                    // Zemin kontrolü yap - bu hedef noktasından aşağıya doğru ray gönder
                    RaycastHit floorHit;
                    if (
                        Physics.Raycast(
                            new Vector3(targetPosition.x, targetPosition.y + 10f, targetPosition.z),
                            Vector3.down,
                            out floorHit,
                            100f,
                            groundMask
                        )
                    )
                    {
                        // Ray'in uzunluğunu hesapla
                        float rayLength = Vector3.Distance(
                            ray.origin,
                            new Vector3(floorHit.point.x, floorHit.point.y, floorHit.point.z)
                        );

                        // Aynı ray doğrultusunda hedef pozisyonu belirle
                        targetPosition = ray.GetPoint(rayLength);
                    }
                }

                // Hedef konumunu güncelle
                aimTargetInstance.transform.position = targetPosition;
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
