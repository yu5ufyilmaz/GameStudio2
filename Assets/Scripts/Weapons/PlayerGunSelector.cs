using System.Collections.Generic;
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
            animator.SetBool(IsOneHanded, gun.Place != GunPlace.FirstHand);
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
                SwitchGun(1); // 1 tuşuna basıldığında ilk silahı seç
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchGun(2); // 2 tuşuna basıldığında ikinci silahı seç
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                // Yerdeki silahı almak için F tuşuna basıldığında
                TryPickupGroundGun();
            }
        }

     private void SwitchGun(int gunNumber)
        {
            if (gunNumber == 1)
            {
                // Birinci el silahını aktif et
                if (FirstHandGun != null)
                {
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
                    DespawnActiveGun(); // Mevcut silahı yok et
                    SetupGun( SecondHandGun); // İkinci el silahını kur
                    ActiveBaseGun = SecondHandGun; // Aktif silahı ikinci el silahı olarak ayarla
                }
            }
        }

         private void TryPickupGroundGun()
        {
            // Yerdeki silahı kontrol et ve al
             Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, 1 << LayerMask.NameToLayer("Pickups"));
             foreach (var hitCollider in hitColliders)
            {  
                    GunScriptableObject gunToPickup = hitCollider.GetComponent<GunPickup>().Gun; // Silahın verisini al
                    if (gunToPickup != null && (ActiveGun.Place == gunToPickup.Place))
                    {
                        if (gunToPickup.Place == GunPlace.FirstHand)
                        {
                            DropGun(FirstHandGun);
                            PickupGun(gunToPickup, SecondHandGun);
                            SetupGun(gunToPickup);
                            
                        }
                        else
                        {
                            DropGun(SecondHandGun);
                            PickupGun(ActiveGun, gunToPickup); // Yeni silahı al
                            SetupGun(gunToPickup);
                        }
                    }
                    Destroy(hitCollider.gameObject); // Yerdeki silahı yok et
                
            }
        }
        private void DropGun(GunScriptableObject gunToDrop)
        {
            if (gunToDrop != null)
            {
                Instantiate(gunToDrop.PickupPrefab, transform.position, Quaternion.identity);
            }
        }
       
    }
}
