using System.Collections;
using Cinemachine;
using UnityEngine;

public class VignetteController : MonoBehaviour
{
    static VignetteController Instance;
    public CinemachineImpulseSource impulseSource;
    public Material screenDamageMat;
    private Coroutine screenDamageTask;

    void Awake()
    {
        Instance = this;
    }

    public void SetIntensity(float intensity)
    {
        if (screenDamageTask != null)
            StopCoroutine(screenDamageTask);
        StartCoroutine(screenDamage(intensity));
    }

    public void ShakeCamera(float intensity, float duration)
    {
        var velocity = new Vector3(0, -0.2f, -0.2f); // Düzeltme: -05f yerine -0.5f
        velocity.Normalize();
        impulseSource.GenerateImpulse(velocity * intensity * 0.2f);
    }

    private IEnumerator CameraShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            var velocity = new Vector3(0, -0.2f, -0.2f); // Düzeltme: -05f yerine -0.5f
            velocity.Normalize();
            impulseSource.GenerateImpulse(velocity * intensity * 0.2f);
            elapsed += Time.deltaTime;
            yield return null; // Her frame'de bir sarsıntı uygula
        }
    }

    private IEnumerator screenDamage(float intensity)
    {
        // Camera Shake
        var velocity = new Vector3(0, -0.5f, -1); // Düzeltme: -05f yerine -0.5f
        velocity.Normalize();
        impulseSource.GenerateImpulse(velocity * intensity * 0.4f);
        // Screen Effect
        var targetRadius = Remap(intensity, 0, 1.2f, 0.35f, 0.2f);
        var curRadius = 1.2f;
        for (float t = 0; curRadius != targetRadius; t += Time.deltaTime * 3)
        {
            curRadius = Mathf.Lerp(1.2f, targetRadius, t);
            screenDamageMat.SetFloat("_vignette_radius", curRadius);
            yield return null;
        }
        for (float t = 0; curRadius < 1.2f; t += Time.deltaTime * 3)
        {
            curRadius = Mathf.Lerp(targetRadius, 1.2f, t);
            screenDamageMat.SetFloat("_vignette_radius", curRadius);
            yield return null;
        }
    }

    public float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    }

    public static class SpecialEffects
    {
        public static void SetIntensity(float intensity)
        {
            Instance.SetIntensity(intensity);
        }

        public static void ShakeCamera(float intensity, float duration)
        {
            Instance.ShakeCamera(intensity, duration);
        }
    }
}
