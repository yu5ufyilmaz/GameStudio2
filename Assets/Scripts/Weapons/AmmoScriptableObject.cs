using UnityEngine;
[CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 1)]
public class AmmoScriptableObject : ScriptableObject
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

    public bool CanReload()
    {
        return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
    }
     
    }
