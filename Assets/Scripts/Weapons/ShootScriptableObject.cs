using System;
using System.Linq;
using UnityEngine;
namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 1)]
    public class ShootScriptableObject : ScriptableObject, System.ICloneable
    {
        public LayerMask HitMask;

        public float FireRate = 0.25f;
        public float RecoilRecoverySpeed = 1f;
        public float MaxSpreadTime = 1f;
        public float BulletPerShoot = 1;

        public BulletSpreadType SpreadType = BulletSpreadType.Simple;

        [Header("Simple Spread")]
        public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 MinSpread = Vector3.zero;

        [Header("Texture Based Spread")]
        [Range(0.001f, 5f)]
        public float SpreadMultiplier = 0.1f;
        public Texture2D SpreadTexture;



        public Vector3 GetSpread(float ShootTime = 0)
        {
            Vector3 spread = Vector3.zero;
            if (SpreadType == BulletSpreadType.Simple)
            {
                spread = Vector3.Lerp(
                    new Vector3(
                    UnityEngine.Random.Range(-MinSpread.x, MinSpread.x),
                    UnityEngine.Random.Range(-MinSpread.y, MinSpread.y),
                    UnityEngine.Random.Range(-MinSpread.z, MinSpread.z)
                    ),
                    new Vector3(
                    UnityEngine.Random.Range(-Spread.x, Spread.x),
                    UnityEngine.Random.Range(-Spread.y, Spread.y),
                    UnityEngine.Random.Range(-Spread.z, Spread.z)), Mathf.Clamp01(ShootTime / MaxSpreadTime)
                );
            }
            else if (SpreadType == BulletSpreadType.TextureBased)
            {
                spread = GetTextureDirection(ShootTime);
                spread *= SpreadMultiplier;
            }


            return spread;
        }

        private Vector3 GetTextureDirection(float ShootTime)
        {
            Vector2 halfSize = new Vector2(SpreadTexture.width / 2, SpreadTexture.height / 2);
            int halfSquareExtents = Mathf.CeilToInt(
                Mathf.Lerp(
                    1,
                    halfSize.x,
                    Mathf.Clamp01(ShootTime / MaxSpreadTime)

            ));
            int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
            int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

            Color[] sampleColors = SpreadTexture.GetPixels(
               minX,
               minY,
               halfSquareExtents * 2,
               halfSquareExtents * 2
            );

            float[] colorAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
            float totalGreyValue = colorAsGrey.Sum();

            float grey = UnityEngine.Random.Range(0, totalGreyValue);
            int i = 0;
            for (; i < colorAsGrey.Length; i++)
            {
                grey -= colorAsGrey[i];
                if (grey < 0)
                {
                    break;
                }
            }
            int x = minX + i % (halfSquareExtents * 2);
            int y = minY + i / (halfSquareExtents * 2);

            Vector2 targetPosition = new Vector2(x, y);
            Vector2 direction = (targetPosition- halfSize)/halfSize.x;
            return direction;
        }
        public object Clone()
        {
            ShootScriptableObject config = CreateInstance<ShootScriptableObject>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}
