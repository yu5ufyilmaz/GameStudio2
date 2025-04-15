using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotGalacticos.Guns.Modifiers;
using Unity.VisualScripting;
using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    [DisallowMultipleComponent]
    public class PlayerGunSelector : MonoBehaviour
    {
        private Animator animator;

        private static readonly int IsOneHanded = Animator.StringToHash("Is1HandedGun");
        private static readonly int IsSecondHanded = Animator.StringToHash("Is2HandedGun");
        [SerializeField]
        private GunType Gun;
        [SerializeField]
        private GunType SecondGun;

        [SerializeField]
        private Transform GunParent;

        [SerializeField]
        private List<GunScriptableObject> Guns;

        [Space]
        [Header("Runtime Filled")]
        public GunScriptableObject ActiveGun;
        [SerializeField]
        public GunScriptableObject ActiveBaseGun;

        public GunScriptableObject FirstHandGun; // İkinci el silahı
        [SerializeField]
        public GunScriptableObject FirstHandBaseGun;

        public GunScriptableObject SecondHandGun; // İkinci el silahı
        [SerializeField]
        public GunScriptableObject SecondHandBaseGun;

        private int activeGunIndex = 1;

        private void Awake()
        {
            GunScriptableObject firstGun = Guns.Find(gun => gun.Type == Gun);
            GunScriptableObject secondGun = Guns.Find(gun => gun.Type == SecondGun);
            if (Guns == null || Guns.Count == 0)
            {
                Debug.LogError("No GunScriptableObject found.");
                return;
            }

            animator = GetComponent<Animator>();
            ActiveBaseGun = firstGun; // Başlangıçta aktif silahı birinci el silahı olarak ayarla
            SetupGun(ActiveBaseGun);
            SetupHandGuns(firstGun, secondGun);
        }

        private void SetupHandGuns(GunScriptableObject firstHandGun, GunScriptableObject secondHandGun)
        {
            FirstHandBaseGun = firstHandGun;
            SecondHandBaseGun = secondHandGun;

            FirstHandGun = firstHandGun.Clone() as GunScriptableObject; // Birinci el silahını klonla
            SecondHandGun = secondHandGun.Clone() as GunScriptableObject; // İkinci el silahını klonla
        }

        private void SetupGun(GunScriptableObject gun)
        {
            ActiveBaseGun = gun;
            ActiveGun = gun.Clone() as GunScriptableObject; // Aktif silahı klonla
            ActiveGun.Spawn(GunParent, this); // Silahı sahneye yerleştir
            UpdateAnimator(gun); // Animatörü güncelle
        }

        private void UpdateAnimator(GunScriptableObject gun)
        {
            bool isOneHanded = gun.Place != GunPlace.FirstHand;
            bool isTwoHanded = gun.Place != GunPlace.SecondHand;

            animator.SetBool(IsSecondHanded, isTwoHanded);
            animator.SetBool(IsOneHanded, isOneHanded);
        }

        public void DespawnActiveGun()
        {
            if (ActiveGun != null)
            {
                ActiveGun.Despawn();
                Destroy(ActiveGun);
            }
        }

        public void PickupGun(GunScriptableObject firstHandGun, GunScriptableObject secondHandGun)
        {
            DespawnActiveGun();
            SetupHandGuns(firstHandGun, secondHandGun);
        }

        public void ApplyModifier(IModifiers[] modifier)
        {
            DespawnActiveGun();
            SetupGun(ActiveBaseGun); // Temel silahı kur

            foreach (IModifiers mod in modifier)
            {
                mod.Apply(ActiveGun);
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (activeGunIndex != 1)
                    StartCoroutine(SwitchGunCoroutine(1)); // 1 tuşuna basıldığında birinci silahı seç
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (activeGunIndex != 2)
                    StartCoroutine(SwitchGunCoroutine(2)); // 2 tuşuna basıldığında ikinci silahı seç
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                // Yerdeki silahı almak için F tuşuna basıldığında
                TryPickupGroundGun();
            }
        }

        private IEnumerator SwitchGunCoroutine(int gunNumber)
        {
            animator.SetTrigger("Switch"); // Animasyonu tetikle
            ActiveGun.canShoot = false;
            // Animasyonun bitmesini bekle
            yield return new WaitForSeconds(1.1f);

            // Animasyon bittiğinde silahı değiştir
            animator.ResetTrigger("Switch");
            SwitchGun(gunNumber);
            ActiveGun.canShoot = true;

        }

        private void SwitchGun(int gunNumber)
        {
            if (gunNumber == 1)
            {
                // Birinci el silahını aktif et
                if (FirstHandGun != null)
                {
                    activeGunIndex = 1;
                    DespawnActiveGun(); // Mevcut silahı yok et
                    SetupGun(FirstHandGun); // Birinci el silahını kur
                    ActiveBaseGun = FirstHandGun; // Aktif silahı birinci el silahı olarak ayarla
                }
            }
            else if (gunNumber == 2)
            {
                // İkinci el silahını aktif et
                if (SecondHandGun != null)
                {
                    activeGunIndex = 2;
                    DespawnActiveGun(); // Mevcut silahı yok et
                    SetupGun(SecondHandGun); // İkinci el silahını kur
                    ActiveBaseGun = SecondHandGun; // Aktif silahı ikinci el silahı olarak ayarla
                }
            }
        }
        private void TryPickupGroundGun()
        {
            // Yerdeki silahları kontrol et
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, 1 << LayerMask.NameToLayer("Pickups"));

            // En yakın silahı bulmak için değişkenler
            GunScriptableObject closestGun = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hitCollider in hitColliders)
            {
                GunScriptableObject gunToPickup = hitCollider.GetComponent<GunPickup>().Gun; // Silahın verisini al
                if (gunToPickup != null && (ActiveGun.Place == gunToPickup.Place))
                {
                    // Silahın pozisyonu ile oyuncunun pozisyonu arasındaki mesafeyi hesapla
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                    // Eğer bu silah, şu ana kadar bulduğumuz en yakın silah ise, güncelle
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestGun = gunToPickup;
                    }
                }
            }

            // Eğer en yakın silah bulunduysa, onu al
            if (closestGun != null)
            {
                if (closestGun.Place == GunPlace.FirstHand)
                {
                    DropGun(FirstHandGun);
                    PickupGun(closestGun, SecondHandGun);
                    SetupGun(closestGun);
                }
                else
                {
                    DropGun(SecondHandGun);
                    PickupGun(ActiveGun, closestGun); // Yeni silahı al
                    SetupGun(closestGun);
                }

                // En yakın silahın bulunduğu collider'ı yok et
                Collider closestCollider = hitColliders.FirstOrDefault(c => c.GetComponent<GunPickup>().Gun == closestGun);
                if (closestCollider != null)
                {
                    Destroy(closestCollider.gameObject); // Yerdeki silahı yok et
                }
            }
        }
        private void DropGun(GunScriptableObject gunToDrop)
        {
            if (gunToDrop != null)
            {
                Instantiate(gunToDrop.PickupPrefab, GunParent.position, Quaternion.identity);
            }
        }

    }
}
