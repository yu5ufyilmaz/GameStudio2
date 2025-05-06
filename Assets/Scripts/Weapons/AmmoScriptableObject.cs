using UnityEngine;

namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 1)]
    public class AmmoScriptableObject : ScriptableObject, System.ICloneable
    {
        public int OriginalMaxAmmo;
        public int OriginalClipSize;

        [Header("Değiştirilebilir Değerler")]
        public int MaxAmmo;
        public int ClipSize;

        public int CurrentAmmo = 120;
        public int CurrentClipAmmo = 30;

        private void OnEnable()
        {
            // Oyun başladığında veya sahne yüklendiğinde orijinal değerlere geri yükle
            ResetAmmoValues();
        }

        public void ResetAmmoValues()
        {
            MaxAmmo = OriginalMaxAmmo;
            ClipSize = OriginalClipSize;

            // Mevcut mermi sayısını orijinal değerlere göre sınırla
            CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MaxAmmo);
            CurrentClipAmmo = Mathf.Clamp(CurrentClipAmmo, 0, ClipSize);
        }

        public void Reload()
        {
            int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
            int availableBulletsInCurrentClip = ClipSize - CurrentClipAmmo;
            int reloadAmount = Mathf.Min(maxReloadAmount, availableBulletsInCurrentClip);

            CurrentClipAmmo += reloadAmount;
            CurrentAmmo -= reloadAmount;
        }

        public void AddAmmo(int Amount)
        {
            if (CurrentAmmo + Amount > MaxAmmo)
                CurrentAmmo = MaxAmmo;
            else
                CurrentAmmo += Amount;
        }

        public bool CanReload()
        {
            return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
        }

        private void OnValidate()
        {
            CurrentClipAmmo = Mathf.Clamp(CurrentClipAmmo, 0, ClipSize);
            CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MaxAmmo);
        }

        public object Clone()
        {
            AmmoScriptableObject config = CreateInstance<AmmoScriptableObject>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}
