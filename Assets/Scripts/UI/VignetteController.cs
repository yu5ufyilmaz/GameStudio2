using System.Collections;
using Cinemachine;
using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteController : MonoBehaviour
{
    public static VignetteController Instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineVolumeSettings cinemachineVolumeSettings;
    private Vignette vignette;

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Post-processing bileşenini al
        cinemachineVolumeSettings = virtualCamera.GetComponent<CinemachineVolumeSettings>();
        if (cinemachineVolumeSettings != null)
        {
            // Vignette bileşenini al
            cinemachineVolumeSettings.m_Profile.TryGet<Vignette>(out vignette);
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.intensity.value = intensity; // Vignette yoğunluğunu ayarlayın
        }
    }

    public void ResetVignette()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f; // Varsayılan yoğunluk
        }
    }

    public void ShakeVignette(float intensity, float duration)
    {
        if (vignette != null)
        {
            StartCoroutine(ShakeVignetteCoroutine(intensity, duration));
        }
    }

    private IEnumerator ShakeVignetteCoroutine(float intensity, float duration)
    {
        float timer = 0f;
        float originalIntensity = vignette.intensity.value;

        while (timer < duration)
        {
            float t = timer / duration;
            vignette.intensity.value = Mathf.Lerp(originalIntensity, intensity, t);
            timer += Time.deltaTime;
            yield return null;
        }

        vignette.intensity.value = originalIntensity; // Sarsıntı sonrası orijinal yoğunluğa döner
    }
}
