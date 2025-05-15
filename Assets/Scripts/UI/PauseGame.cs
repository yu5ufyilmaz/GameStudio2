using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public GameObject TutorialImage;

    public GameObject AimImage;
    private GameObject pauseMenu; // Pause menüsünü tutan GameObject
    private Animator pauseMenuAnimator; // Pause menüsünün Animator bileşeni
    private Animator settingsMenuAnimator;
    private GameObject sinMenu;
    private Animator sinMenuAnimator;
    private bool isSettingsMenuOpen = false;
    private bool isPaused = false; // Oyun duraklatma durumu
    private bool isTab = false;

    void Start()
    {
        AimImage = GameObject.Find("AimImage");
        pauseMenu = GameObject.Find("PauseMenu");
        sinMenu = GameObject.Find("SinMenu");
        settingsMenuAnimator = GameObject.Find("SettingsMenu").GetComponent<Animator>();
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
            if (isPaused == false && isTab == true)
            {
                ResumeGame2();
            }
            else if (isPaused == false && isTab == false)
            {
                PauseGameMenu2();
            }
        }
    }

    void PauseGameMenu()
    {
        // Zamanı durdur
        AimImage.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        // Pause menüsünü aç
        //pauseMenu.SetActive(true);
        pauseMenuAnimator.SetTrigger("Open"); // Açılma animasyonunu başlat
    }

    public void ResumeGame()
    {
        AimImage.SetActive(true);
        // Zamanı devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü kapat
        pauseMenuAnimator.SetTrigger("Close"); // Kapatma animasyonunu başlat
        StartCoroutine(DisableMenuAfterAnimation());
    }

    void ResumeGame2()
    {
        AimImage.SetActive(true);
        // Zamanı devam ettir
        Time.timeScale = 1f;
        isTab = false;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü kapat
        Animator[] childs = sinMenu.GetComponentsInChildren<Animator>();

        sinMenuAnimator.SetTrigger("Close"); // Kapatma animasyonunu başlat
    }

    public void PauseGameMenu2()
    {
        AimImage.SetActive(false);
        TutorialImage.SetActive(false);
        Time.timeScale = 0f;
        isTab = true;

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

    public void OpenSettings()
    {
        if (settingsMenuAnimator != null)
        {
            if (isSettingsMenuOpen == false)
            {
                settingsMenuAnimator.SetTrigger("Open");
                isSettingsMenuOpen = true;
            }
            else
            {
                settingsMenuAnimator.SetTrigger("Close");
                isSettingsMenuOpen = false;
            }
        }
    }

    private System.Collections.IEnumerator DisableMenuAfterAnimation()
    {
        // Animasyon süresince bekle
        yield return new WaitForSeconds(pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length);
        //pauseMenu.SetActive(false); // Menü kapat
    }
}
