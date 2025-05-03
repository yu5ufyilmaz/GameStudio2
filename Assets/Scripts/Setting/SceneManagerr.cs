using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerr : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        // Sahneyi yükle
        SceneManager.LoadScene(sceneName);
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
