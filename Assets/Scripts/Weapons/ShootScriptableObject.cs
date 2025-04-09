using System;
using UnityEngine;
namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 1)]
    public class ShootScriptableObject : ScriptableObject, System.ICloneable
    {
        public LayerMask HitMask;

        [Header("Spread")]
        public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 MinSpread= Vector3.zero;
        public float BulletPerShoot=1;
        public float FireRate = 0.25f;

        public object Clone()
        {
            ShootScriptableObject config = CreateInstance<ShootScriptableObject>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}
