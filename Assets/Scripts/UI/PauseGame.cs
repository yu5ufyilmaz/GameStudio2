using UnityEngine;

public class PauseGame : MonoBehaviour
{
    private GameObject pauseMenu; // Pause menüsünü tutan GameObject
    private Animator pauseMenuAnimator; // Pause menüsünün Animator bileşeni
    private GameObject sinMenu;
    private Animator sinMenuAnimator;
    private bool isPaused = false; // Oyun duraklatma durumu

    void Start()
    {
        pauseMenu = GameObject.Find("PauseMenu");
        sinMenu = GameObject.Find("SinMenu");
        sinMenuAnimator = sinMenu.GetComponent<Animator>();
        pauseMenuAnimator = pauseMenu.GetComponent<Animator>();
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // ESC tuşuna basıldığında duraklatma işlemi
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGameMenu();
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
            {
                ResumeGame2();
            }
            else
            {
                PauseGameMenu2();
            }
        }
    }

    void PauseGameMenu()
    {
        // Zamanı durdur
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        // Pause menüsünü aç
        //pauseMenu.SetActive(true);
        pauseMenuAnimator.SetTrigger("Open"); // Açılma animasyonunu başlat
    }

    void ResumeGame2()
    {
        // Zamanı devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü kapat
        Animator[] childs = sinMenu.GetComponentsInChildren<Animator>();
        foreach (Animator child in childs)
        {
            child.ResetTrigger("Selected");
        }
        sinMenuAnimator.SetTrigger("Close"); // Kapatma animasyonunu başlat
    }

    void PauseGameMenu2()
    {
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        // Pause menüsünü aç
        //pauseMenu.SetActive(true);
        Animator[] childs = sinMenu.GetComponentsInChildren<Animator>();

        foreach (Animator child in childs)
        {
            if (child != null)
                child.SetTrigger("Normal");
        }
        sinMenuAnimator.SetTrigger("Open"); // Açılma animasyonunu başlat
    }

    public void ResumeGame()
    {
        // Zamanı devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü kapat
        pauseMenuAnimator.SetTrigger("Close"); // Kapatma animasyonunu başlat
        StartCoroutine(DisableMenuAfterAnimation());
    }

    private System.Collections.IEnumerator DisableMenuAfterAnimation()
    {
        // Animasyon süresince bekle
        yield return new WaitForSeconds(pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length);
        //pauseMenu.SetActive(false); // Menü kapat
    }
}
