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
        private Transform SecondHandTargetParent;

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
        private Rig rig2;
        private TwoBoneIKConstraint ik;
        private int activeGunIndex = 1;

        private class AmmoState
        {
            public int CurrentClipAmmo;
            public int CurrentAmmo;

            public AmmoState(int clipAmmo, int totalAmmo)
            {
                CurrentClipAmmo = clipAmmo;
                CurrentAmmo = totalAmmo;
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
            SetupGun(ActiveBaseGun);
            SetupHandGuns(firstGun, secondGun);
            if (FirstHandGun != null)
                ammoStates[1] = new AmmoState(
                    FirstHandGun.AmmoConfig.CurrentClipAmmo,
                    FirstHandGun.AmmoConfig.CurrentAmmo
                );

            if (SecondHandGun != null)
                ammoStates[2] = new AmmoState(
                    SecondHandGun.AmmoConfig.CurrentClipAmmo,
                    SecondHandGun.AmmoConfig.CurrentAmmo
                );
        }

        private void SetupHandGuns(
            GunScriptableObject firstHandGun,
            GunScriptableObject secondHandGun
        )
        {
            FirstHandBaseGun = firstHandGun;
            SecondHandBaseGun = secondHandGun;

            FirstHandGun = firstHandGun.Clone() as GunScriptableObject; // Birinci el silahını klonla
            SecondHandGun = secondHandGun.Clone() as GunScriptableObject; // İkinci el silahını klonla
        }

        private void SetupGun(GunScriptableObject gun)
        {
            SaveAmmoAmount(gun);

            ActiveBaseGun = gun;
            ActiveGun = gun.Clone() as GunScriptableObject; // Aktif silahı klonla
            ActiveGun.Spawn(GunParent, this); // Silahı sahneye yerleştier

            ik.data.target = ActiveGun.secondHandTarget;

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
            // İlk önce mevcut silahın ammo durumunu kaydet
            if (activeGunIndex != 0 && ActiveGun != null)
            {
                ammoStates[activeGunIndex] = new AmmoState(
                    ActiveGun.AmmoConfig.CurrentClipAmmo,
                    ActiveGun.AmmoConfig.CurrentAmmo
                );
            }

            if (gunNumber == 1 && FirstHandGun != null)
            {
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
                GunPickup gunPickupScript = hitCollider.GetComponent<GunPickup>();
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
            GunScriptableObject newGun = gun;
            newGun.AmmoConfig.CurrentClipAmmo = gun.AmmoConfig.CurrentClipAmmo;
            newGun.AmmoConfig.CurrentAmmo = gun.AmmoConfig.CurrentAmmo;
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
