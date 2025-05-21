using DotGalacticos.Guns.Demo;
using UnityEngine;
using UnityEngine.UI;

public class ReloadSlider : MonoBehaviour
{
    [SerializeField]
    private Slider reloadSlider; // Slider referansı

    private ShootController shootController; // ShootController referansı

    private void Start()
    {
        shootController = GetComponent<ShootController>();
        // Başlangıçta slider'ı gizle
        reloadSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Eğer reload işlemi devam ediyorsa slider'ı göster
        if (shootController.isReloading)
        {
            reloadSlider.gameObject.SetActive(true);
            UpdateSlider();
        }
        else
        {
            reloadSlider.gameObject.SetActive(false);
        }
    }

    private void UpdateSlider()
    {
        float reloadDuration = shootController.GetReloadAnimationLength();
        float elapsedTime = shootController.reloadElapsedTime;
        if (reloadDuration > 0)
        {
            // 0'dan 1'e doğru dolan slider
            float fillAmount = Mathf.Clamp01(elapsedTime / reloadDuration);
            reloadSlider.value = fillAmount;
        }
    }
}
