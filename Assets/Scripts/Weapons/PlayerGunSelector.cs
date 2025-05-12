using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotGalacticos.Guns.Modifiers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
        private GameObject SecondHandTarget;

        public List<GunScriptableObject> Guns;

        [SerializeField]
        private GameObject[] gunUIs;

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
        private Rig rig2;
        private TwoBoneIKConstraint ik;
        private int activeGunIndex = 1;

        public class AmmoState
        {
            public int CurrentClipAmmo;
            public int CurrentAmmo;
            public int MaxAmmo;
            public int ClipSize;

            public AmmoState(int clipAmmo, int totalAmmo, int maxAmmo, int clipSize)
            {
                CurrentClipAmmo = clipAmmo;
                CurrentAmmo = totalAmmo;
                MaxAmmo = maxAmmo;
                ClipSize = clipSize;
            }
        }

        private Dictionary<int, AmmoState> ammoStates = new Dictionary<int, AmmoState>();

        private void Awake()
        {
            GunScriptableObject firstGun = Guns.Find(gun => gun.Type == Gun);
            GunScriptableObject secondGun = Guns.Find(gun => gun.Type == SecondGun);

            if (Guns == null || Guns.Count == 0)
            {
                Debug.LogError("No GunScriptableObject found.");
                return;
            }

            rig2 = GameObject.Find("Left Hand Rig").GetComponent<Rig>();
            ik = rig2.GetComponentInChildren<TwoBoneIKConstraint>();
            animator = GetComponent<Animator>();

            ActiveBaseGun = firstGun; // Başlangıçta aktif silahı birinci el silahı olarak ayarla
            SetOriginalAmmoValues();
            SetupGun(ActiveBaseGun);
            SetupHandGuns(firstGun, secondGun);
        }

        private void SetupHandGuns(
            GunScriptableObject firstHandGun,
            GunScriptableObject secondHandGun
        )
        {
            FirstHandBaseGun = firstHandGun;
            SecondHandBaseGun = secondHandGun;

            FirstHandGun = firstHandGun.Clone() as GunScriptableObject;

            SecondHandGun = secondHandGun.Clone() as GunScriptableObject;
        }

        private void SetupGun(GunScriptableObject gun)
        {
            // Mevcut silahın ammo durumunu kaydet
            // SaveAmmoAmount(ActiveGun);
            ActiveBaseGun = gun;
            ActiveGun = gun.Clone() as GunScriptableObject; // Aktif silahı klonla
            ActiveGun.Spawn(GunParent, this); // Silahı sahneye yerleştir
            foreach (var gunUI in gunUIs)
            {
                gunUI.SetActive(gunUI.name == gun.name);
            }
            // Yeni silahın mermi durumunu ayarla
            ActiveGun.AmmoConfig.CurrentClipAmmo = gun.GetClipAmmo(gun.name);
            ActiveGun.AmmoConfig.CurrentAmmo = gun.GetTotalAmmo(gun.name);
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

        private void SetOriginalAmmoValues()
        {
            foreach (var gun in Guns)
            {
                if (gun != null && gun.AmmoConfig != null)
                {
                    // Orijinal değerleri geri yükle
                    gun.AmmoConfig.MaxAmmo = gun.AmmoConfig.OriginalMaxAmmo;
                    gun.AmmoConfig.ClipSize = gun.AmmoConfig.OriginalClipSize;

                    // Mevcut mermi sayısını orijinal değerlere göre ayarla
                    gun.AmmoConfig.CurrentAmmo = gun.AmmoConfig.MaxAmmo; // Başlangıçta tam mermi
                    gun.AmmoConfig.CurrentClipAmmo = gun.AmmoConfig.ClipSize; // Başlangıçta tam şarjör

                    if (gun == ActiveGun)
                    { // Orijinal değerleri sakla
                        ActiveGun.AmmoConfig.CurrentAmmo = gun.AmmoConfig.OriginalMaxAmmo;
                        ActiveGun.AmmoConfig.CurrentClipAmmo = gun.AmmoConfig.OriginalClipSize;
                    }
                    if (gun == SecondHandGun)
                    {
                        SecondHandGun.AmmoConfig.CurrentAmmo = gun.AmmoConfig.OriginalMaxAmmo;
                        SecondHandGun.AmmoConfig.CurrentClipAmmo = gun.AmmoConfig.OriginalClipSize;
                    }
                }
            }
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
            animator.SetTrigger("Switch");
            if (ActiveGun != null)
                ActiveGun.canShoot = false;

            yield return new WaitForSeconds(1.1f);

            animator.ResetTrigger("Switch");

            SwitchGun(gunNumber);

            if (ActiveGun != null)
                ActiveGun.canShoot = true;
        }

        private void SwitchGun(int gunNumber)
        {
            // Mevcut silahın ammo durumunu kaydet
            SaveAmmoAmount(ActiveGun);

            if (gunNumber == 1 && FirstHandGun != null)
            {
                //SaveAmmoAmount(SecondHandGun);
                activeGunIndex = 1;

                DespawnActiveGun();
                ActiveBaseGun = FirstHandGun;

                SetupGun(FirstHandGun);

                // Ammo durumunu yükle
                if (ammoStates.ContainsKey(1))
                {
                    ActiveGun.AmmoConfig.CurrentClipAmmo = ammoStates[1].CurrentClipAmmo;
                    ActiveGun.AmmoConfig.CurrentAmmo = ammoStates[1].CurrentAmmo;
                }
            }
            else if (gunNumber == 2 && SecondHandGun != null)
            {
                //SaveAmmoAmount(FirstHandGun);
                activeGunIndex = 2;

                DespawnActiveGun();
                ActiveBaseGun = SecondHandGun;

                SetupGun(SecondHandGun);

                // Ammo durumunu yükle
                if (ammoStates.ContainsKey(2))
                {
                    ActiveGun.AmmoConfig.CurrentClipAmmo = ammoStates[2].CurrentClipAmmo;
                    ActiveGun.AmmoConfig.CurrentAmmo = ammoStates[2].CurrentAmmo;
                }
            }
        }

        private void TryPickupGroundGun()
        {
            // Yerdeki silahları kontrol et
            Collider[] hitColliders = Physics.OverlapSphere(
                transform.position,
                2f,
                1 << LayerMask.NameToLayer("Pickups")
            );

            // En yakın silahı bulmak için değişkenler
            GunScriptableObject closestGun = null;
            GunPickup gunPickup = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider != null)
                {
                    GunPickup gunPickupScript = hitCollider.GetComponent<GunPickup>();
                    if (gunPickupScript != null)
                    {
                        GunScriptableObject gunToPickup = gunPickupScript.Gun; // Silahın verisini al

                        if (gunToPickup != null && (ActiveGun.Place == gunToPickup.Place))
                        {
                            // Silahın pozisyonu ile oyuncunun pozisyonu arasındaki mesafeyi hesapla
                            float distance = Vector3.Distance(
                                transform.position,
                                hitCollider.transform.position
                            );

                            // Eğer bu silah, şu ana kadar bulduğumuz en yakın silah ise, güncelle
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestGun = gunToPickup;
                                gunPickup = gunPickupScript;
                            }
                        }
                    }
                }
            }

            // Eğer en yakın silah bulunduysa, onu al
            if (closestGun != null)
            {
                if (closestGun.Place == GunPlace.FirstHand)
                {
                    DropGun(FirstHandGun);
                    gunPickup.SetWeaponAmmoAmount(closestGun);
                    PickupGun(closestGun, SecondHandGun);
                    SetupGun(closestGun);
                }
                else
                {
                    DropGun(SecondHandGun);
                    gunPickup.SetWeaponAmmoAmount(closestGun);
                    PickupGun(ActiveGun, closestGun); // Yeni silahı al
                    SetupGun(closestGun);
                }

                // En yakın silahın bulunduğu collider'ı yok et
                Collider closestCollider = hitColliders.FirstOrDefault(c =>
                    c.GetComponent<GunPickup>().Gun == closestGun
                );
                if (closestCollider != null)
                {
                    Destroy(closestCollider.gameObject); // Yerdeki silahı yok et
                }
            }
        }

        private void SaveAmmoAmount(GunScriptableObject gun)
        {
            if (gun != null)
            {
                if (ammoStates.ContainsKey(activeGunIndex))
                {
                    // Mevcut silahın ammo durumunu güncelle
                    ammoStates[activeGunIndex].CurrentClipAmmo = gun.AmmoConfig.CurrentClipAmmo;
                    ammoStates[activeGunIndex].CurrentAmmo = gun.AmmoConfig.CurrentAmmo;
                }
                else
                {
                    // Yeni bir AmmoState oluştur ve kaydet
                    ammoStates[activeGunIndex] = new AmmoState(
                        gun.AmmoConfig.CurrentClipAmmo,
                        gun.AmmoConfig.CurrentAmmo,
                        gun.AmmoConfig.MaxAmmo,
                        gun.AmmoConfig.ClipSize
                    );
                }
            }
        }

        private void DropGun(GunScriptableObject gunToDrop)
        {
            if (gunToDrop != null)
            {
                GameObject PickupObject = Instantiate(
                    gunToDrop.PickupPrefab,
                    GunParent.position,
                    Quaternion.identity
                );
                GunPickup gunPickups = PickupObject.GetComponent<GunPickup>();
                gunPickups.SetPickupAmmoAmount(this);
            }
        }
    }
}
