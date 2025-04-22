using UnityEngine;
using DotGalacticos.Guns.Modifiers;
namespace DotGalacticos.Guns.Demo
{
    public class GunModifierApplier : MonoBehaviour
    {
        [SerializeField]
        PlayerGunSelector GunSelector;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DamageModifier damageModifier = new()
            {
                Amount = 1.5f,
                AttributeName = "DamageConfig/DamageCurve"
            };
            damageModifier.Apply(GunSelector.ActiveGun);

            Vector3Modifier spreadModifier = new()
            {
                Amount = Vector3.zero,
                AttributeName = "ShootConfig/Spread"
            };
            spreadModifier.Apply(GunSelector.ActiveGun);


        }


    }
}
