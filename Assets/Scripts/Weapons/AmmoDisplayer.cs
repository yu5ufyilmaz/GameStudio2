using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

namespace DotGalacticos.Guns.Demo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class AmmoDisplayer : MonoBehaviour
    {
        [SerializeField]
        private PlayerGunSelector GunSelector;
        private TextMeshProUGUI _ammoText;

        void Awake()
        {
            _ammoText = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            _ammoText.SetText(
                $"{GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo}/"
                    + $"{GunSelector.ActiveGun.AmmoConfig.CurrentAmmo}"
            );
        }
    }
}
