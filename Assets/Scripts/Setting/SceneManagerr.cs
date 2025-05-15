using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerr : MonoBehaviour
{
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenObject;
    [SerializeField] private Image fillPentagramImage; // Kırmızı pentagram (Image.Type.Filled)
    [SerializeField] private Text percentageText;     // Yüzde göstergesi (opsiyonel)
    [SerializeField] private GameObject playCanvas; 
    
    [Header("Yükleme Ayarları")]
    [SerializeField] private float minimumLoadingTime = 0.5f; // Minimum yükleme süresi
    [SerializeField] private float additionalDelayAfterLoading = 2.0f; // Yükleme sonrası ek bekleme süresi
    [SerializeField] private float fillCompletionSpeed = 1.0f; // Dolum tamamlama hızı (1 = normal hız)
    
    // Singleton instance
    private static SceneManagerr instance;
    public static SceneManagerr Instance { get { return instance; } }
    
    private void Awake()
    {
        // Singleton yapısı
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Bir sahneyi asenkron olarak pentagram filling ile yükle
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    }
    
    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        // Loading ekranını göster
        if (loadingScreenObject != null)
        {
            
            loadingScreenObject.SetActive(true);
            playCanvas.SetActive(false);
            
            // Pentagram fill amount'ı sıfırla
            if (fillPentagramImage != null)
            {
                fillPentagramImage.fillAmount = 0f;
            }
            
            // Yüzde metni varsa sıfırla
            if (percentageText != null)
            {
                percentageText.text = "0%";
            }
        }
        
        // Yükleme başlangıç zamanını kaydet
        float startTime = Time.time;
        
        // Sahneyi yüklemeye başla
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        
        // Sahne yüklemeden önce minimum bekleme süresi
        yield return new WaitForSeconds(minimumLoadingTime);
        
        // Maksimum ilerleme %90'dır (Unity'de async yükleme böyle çalışır)
        float progressTarget = 0.9f;
        
        // İlerleme çubuğunu güncelle
        while (asyncOperation.progress < progressTarget)
        {
            // İlerleme yüzdesini 0-1 aralığına normalize et (90% tamamlandığında)
            float progress = Mathf.Clamp01(asyncOperation.progress / progressTarget);
            
            // Pentagram fill amount'ı güncelle
            if (fillPentagramImage != null)
            {
                fillPentagramImage.fillAmount = progress;
            }
            
            // Yüzde metnini güncelle
            if (percentageText != null)
            {
                percentageText.text = Mathf.Round(progress * 100) + "%";
            }
            
            yield return null;
        }
        
        // Sahne teknik olarak hazır, ancak daha fazla bekleyeceğiz
        // Bu süre boyunca pentagramı %90'dan %100'e doğru doldur
        float completionProgress = 0f;
        float currentFill = fillPentagramImage.fillAmount;
        
        while (completionProgress < 1.0f)
        {
            completionProgress += Time.deltaTime * fillCompletionSpeed;
            
            // Pentagram dolumunu %90'dan %100'e doğru yavaşça artır
            if (fillPentagramImage != null)
            {
                float newFill = Mathf.Lerp(currentFill, 1.0f, completionProgress);
                fillPentagramImage.fillAmount = newFill;
            }
            
            // Yüzde metnini güncelle
            if (percentageText != null)
            {
                int displayPercent = Mathf.RoundToInt(Mathf.Lerp(90, 100, completionProgress));
                percentageText.text = displayPercent + "%";
            }
            
            yield return null;
        }
        
        // Tüm dolum tamamlandı, ek bir gecikme ile bekle
        yield return new WaitForSeconds(additionalDelayAfterLoading);
        
        // Artık sahneyi aktifleştirebiliriz
        asyncOperation.allowSceneActivation = true;
        
        // Sahne tamamen yüklendikten sonra da kısa bir süre bekle
        // Bu, tüm nesnelerin yerleşmesi için zaman tanır
        yield return new WaitForSeconds(0.2f);
        
        // Yükleme tamamlandığında loading ekranını gizle
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Bir sahneyi belirli bir gecikme ile yükle
    /// </summary>
    public void LoadSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(LoadSceneWithDelayRoutine(sceneName, delay));
    }
    
    private IEnumerator LoadSceneWithDelayRoutine(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadSceneAsync(sceneName);
    }
    
    /// <summary>
    /// Mevcut sahneyi yeniden yükle
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadSceneAsync(currentSceneName);
    }
    
    /// <summary>
    /// Oyundan çık
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Editor'da, oynatma modunu durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Build'de, uygulamadan çık
        Application.Quit();
#endif
    }
}