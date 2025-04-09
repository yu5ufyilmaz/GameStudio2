using UnityEngine;
namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 1)]
    public class AmmoScriptableObject : ScriptableObject, System.ICloneable
    {
        public int MaxAmmo;
        public int ClipSize;

        public int CurrentAmmo = 120;
        public int CurrentClipAmmo = 30;

        public void Reload()
        {
            int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
            int availableBulletsInCurrentClip = ClipSize - CurrentClipAmmo;
            int reloadAmount = Mathf.Min(maxReloadAmount, availableBulletsInCurrentClip);

            CurrentClipAmmo = CurrentClipAmmo + reloadAmount;
            CurrentAmmo -= reloadAmount;
        }
        public void AddAmmo(int Amount)
        {
            if(CurrentAmmo+ Amount>MaxAmmo) CurrentAmmo = MaxAmmo;
            else CurrentAmmo += Amount;
        }
        public bool CanReload()
        {
            return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
        }

        public object Clone()
        {
            AmmoScriptableObject config = CreateInstance<AmmoScriptableObject>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}
