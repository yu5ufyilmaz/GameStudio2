using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VignetteController : MonoBehaviour
{
    public static VignetteController Instance { get; private set; }

    private Vignette vignette;
    private float defaultIntensity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // PostProcessVolume'dan vignette efektini bul
        PostProcessVolume ppVolume = FindObjectOfType<PostProcessVolume>();
        if (ppVolume == null)
        {
            Debug.LogError("PostProcessVolume bulunamadı!");
            return;
        }

        if (!ppVolume.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette efekti bulunamadı!");
        }
        else
        {
            defaultIntensity = vignette.intensity.value;
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Clamp01(intensity);
        }
    }

    public void ResetVignette()
    {
        if (vignette != null)
        {
            vignette.intensity.value = defaultIntensity;
        }
    }
}
