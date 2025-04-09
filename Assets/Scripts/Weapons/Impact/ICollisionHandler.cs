using UnityEngine;

namespace DotGalacticos.Guns.ImpactEffects
{
    public interface ICollisionHandler
    {
        void HandleImpact(
            Collider ImpactedObjects,
            Vector3 HitPosition,
            Vector3 HitNormal,
            GunScriptableObject Gun
        );
    }
}
