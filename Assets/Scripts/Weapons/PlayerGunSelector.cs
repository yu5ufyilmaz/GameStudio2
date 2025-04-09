using System.Collections.Generic;
using DotGalacticos.Guns.Modifiers;
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
        private Transform GunParent;

        [SerializeField]
        private List<GunScriptableObject> Guns;

        [Space]
        [Header("Runtime Filled")]
        public GunScriptableObject ActiveGun;
        [SerializeField]
        public GunScriptableObject ActiveBaseGun;

        private void Awake()
        {
            GunScriptableObject gun = Guns.Find(gun => gun.Type == Gun);
            if (gun == null)
            {
                Debug.LogError($"No GunScriptableObject found for type {gun}");
            }
            
            animator = GetComponent<Animator>();
            SetupGun(gun);
           
        }

        private void SetupGun(GunScriptableObject Gun)
        {
            ActiveBaseGun = Gun;
            ActiveGun = Gun.Clone() as GunScriptableObject;
            ActiveGun.Spawn(GunParent, this);
            if(ActiveGun.Type ==GunType.Pistol)
            {
                animator.SetBool(IsOneHanded, true);
            }
            else
            {
                animator.SetBool(IsOneHanded, false);
            }
        }
        public void DespawnActiveGun()
        {
            ActiveGun.Despawn();
            Destroy(ActiveGun);
        }

        public void PickupGun(GunScriptableObject Gun)
        {
            DespawnActiveGun();
            SetupGun(Gun);
        }

        public void ApplyModifier(IModifiers[] Modifier)
        {
            
            DespawnActiveGun();
            SetupGun(ActiveBaseGun);
            
            foreach (IModifiers mod in Modifier)
            {
                mod.Apply(ActiveGun);
            }
        }
    }
}
