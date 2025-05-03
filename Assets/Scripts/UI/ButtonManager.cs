using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public GameObject pauseMenu; // Pause menüsünü tutan GameObject
    public Animator pauseMenuAnimator; // Pause menüsünün Animator bileşeni
    public Animator bradasAnimator;

    // Bu metot, belirtilen sahneye geçiş yapar
    public void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void FindSource()
    {
        SoundManager.instance.FindAllAudioSources();
    }

    public void Options(string eventName)
    {
        pauseMenuAnimator.SetTrigger(eventName); // Kapatma animasyonunu başlat
        if (bradasAnimator != null)
            bradasAnimator.SetTrigger(eventName);
    }

    // Bu metot, oyunu kapatır
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Oyun editördeyse, oyunu durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Oyun build'deyse, oyundan çık
        Application.Quit();
#endif
    }
}
