using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerr : MonoBehaviour
{
    public GameObject LoadingScreen; // Yükleme ekranı
    public UnityEngine.UI.Image LoadingBarFill; // Yükleme çubuğu

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Settings")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoadScene("StartMenu");
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Yükleme ekranını aç
        //LoadingScreen.SetActive(true);

        // Asenkron sahne yükleme işlemi
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Yükleme ilerlemesini güncelle
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            //  LoadingBarFill.fillAmount = progress;

            // Yükleme tamamlandığında sahne aktivasyonunu başlat
            if (operation.progress >= 0.9f)
            {
                // LoadingBarFill.fillAmount = 1f;
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Yükleme tamamlandı, loading ekranını gizle
        //LoadingScreen.SetActive(false);
    }

    // Bu metot, sahne geçişini belirli bir süre bekleyerek yapar
    public void LoadSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName, delay));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadScene(sceneName);
    }

    // Bu metot, mevcut sahneyi yeniden yükler
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Eğer oyun Unity Editor'da çalışıyorsa, oyunu durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
