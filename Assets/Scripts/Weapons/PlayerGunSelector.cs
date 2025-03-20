using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField]
    private GunType Gun;
    [SerializeField]
    private Transform GunParent;
    [SerializeField]
    private List<GunScriptableObject> Guns;



    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    void Start()
    {
        GunScriptableObject gun = Guns.Find(gun => gun.Type == Gun);
        if (gun == null)
        {
            Debug.LogError($"No GunScriptableObject found for type {gun}");
        }

        ActiveGun = gun;
        gun.Spawn(GunParent, this);


    }
}
