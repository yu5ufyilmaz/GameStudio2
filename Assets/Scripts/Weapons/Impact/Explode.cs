using UnityEngine;

namespace DotGalacticos.Guns.ImpactEffects
{
    public class Explode : ICollisionHandler
    {
        public float Radius = 1;
        public AnimationCurve DamageFalloff;
        public int BaseDamage = 10;
        public int MaxEnemiesAffected = 10;

        private Collider[] HitObjects;

        public Explode(float Radius, AnimationCurve DamageFalloff, int BaseDamage, int MaxEnemiesAffected)
        {
            this.Radius = Radius;
            this.DamageFalloff = DamageFalloff;
            this.BaseDamage = BaseDamage;
            this.MaxEnemiesAffected = MaxEnemiesAffected;
            HitObjects = new Collider[MaxEnemiesAffected];
        }

        public void HandleImpact(Collider ImpactedObjects, Vector3 HitPosition, Vector3 HitNormal, GunScriptableObject Gun)
        {
           int hits = Physics.OverlapSphereNonAlloc(
            HitPosition,
            Radius,
            HitObjects,
            Gun.ShootConfig.HitMask
           );
           for(int i =0;i<hits;i++)
           {
            if(HitObjects[i].TryGetComponent(out IDamageable damageable))
            {
                float distance = Vector3.Distance(HitPosition,HitObjects[i].ClosestPoint(HitPosition));
                damageable.TakeDamage(
                    Mathf.CeilToInt(BaseDamage* DamageFalloff.Evaluate(distance/ Radius)),HitPosition
                );
            }
           }
        }
    }
}
