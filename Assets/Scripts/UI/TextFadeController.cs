using System.Collections;
using TMPro;
using UnityEngine;

public class TextFadeController : MonoBehaviour
{
    public GameObject textParent; // TextMeshPro yazılarını içeren ana GameObject (tüm yazılar buranın altında olmalı)
    public BoxCollider triggerArea; // Oyuncunun izleneceği Box Collider (isTrigger olarak ayarlanmalı)
    public float fadeDuration = 2f; // Fade out süresi
    private Coroutine fadeCoroutine;

    private TextMeshProUGUI[] texts;

    void Start()
    {
        if (textParent == null)
        {
            Debug.LogError("Text parent GameObject referansı atanmadı.");
            return;
        }
        texts = textParent.GetComponentsInChildren<TextMeshProUGUI>(true);
        // Açık kalması için alfa 1 yap
        SetTextsAlpha(1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Fade varsa iptal et ve yazıları görünür yap
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            SetTextsAlpha(1f);
            if (textParent != null && !textParent.activeSelf)
                textParent.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(FadeOutAndDestroy());
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetTextsAlpha(alpha);
            yield return null;
        }
        SetTextsAlpha(0f);

        // Yazıları yok et (textParent GameObject dahil)
        Destroy(textParent);
        // Bu script nesnesini de yok et
        Destroy(this.gameObject);
    }

    void SetTextsAlpha(float alpha)
    {
        foreach (var text in texts)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }
    }
}
