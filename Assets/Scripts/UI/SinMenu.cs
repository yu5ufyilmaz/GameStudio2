using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SinMenu : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer; // VideoPlayer bileşeni

    [SerializeField]
    private string creditsSceneName; // Credits sahnesinin adı

    [SerializeField]
    private GameObject rawImage;

    [SerializeField]
    private GameObject credits;

    [SerializeField]
    private bool isFront;

    [SerializeField]
    private bool isOpen;

    [SerializeField]
    private GameObject[] cards; // Kart GameObjectleri
    private PauseGame pauseGame;

    [SerializeField]
    private TextMeshProUGUI _sinCountText;

    private void Start()
    {
        pauseGame = GetComponentInParent<PauseGame>();
        // Başlangıçta tüm kartları kapat (inaktif yap)
        foreach (var card in cards)
        {
            if (card != null)
                card.SetActive(false);
        }
        videoPlayer.gameObject.SetActive(false); // VideoPlayer nesnesini aktif yap
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            PauseGameAndPlayVideo();
        }
    }

    public void RevealCardForSin(Sin sin)
    {
        int index = GetIndexForSin(sin);
        if (index == -1)
        {
            Debug.LogWarning("Bilinmeyen Sin tipi: " + sin);
            return;
        }
        OpenCard(index);
    }

    private int GetIndexForSin(Sin sin)
    {
        // Günah enumuna göre index ataması
        switch (sin)
        {
            case Sin.Kibir:
                return 0;
            case Sin.Oburluk:
                return 1;
            case Sin.Tembellik:
                return 2;
            case Sin.Kıskançlık:
                return 3;
            case Sin.Açgözlülük:
                return 4;
            case Sin.Şehvet:
                return 5;
            default:
                return -1;
        }
    }

    private void OpenCard(int index)
    {
        if (index < 0 || index >= cards.Length)
            return;
        GameObject card = cards[index];
        if (card == null)
            return;

        StartCoroutine(PlayOpenAnimations(card));
    }

    private IEnumerator PlayOpenAnimations(GameObject card)
    {
        yield return new WaitForSeconds(1f); // OpenFirstTime animasyon süresine göre ayarlayın
        card.SetActive(true);
        SetSinCount();
        pauseGame.PauseGameMenu2();
    }

    public void ShowCard(GameObject selectedButton)
    {
        Animator anim = selectedButton.GetComponent<Animator>();
        if (isOpen == false && isFront == true)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject != selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Disabled");
                    }
                }
                else
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Show");
                        anim.SetBool("isOpen", true);
                        isOpen = anim.GetBool("isOpen");
                    }
                }
            }
        }
        else if (isOpen == true && isFront == true)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject == selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();

                    if (animator != null)
                    {
                        animator.SetTrigger("Turn");
                        anim.SetBool("isFront", false);
                        isFront = anim.GetBool("isFront");
                    }
                }
            }
        }
        else if (isOpen == true && isFront == false)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject != selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Normal");
                        animator.ResetTrigger("Disabled");
                    }
                }
                else
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Normal");
                        anim.SetBool("isOpen", false);
                        anim.SetBool("isFront", true);
                        isFront = anim.GetBool("isFront");
                        isOpen = anim.GetBool("isOpen");
                    }
                }
            }
        }
    }

    private void PauseGameAndPlayVideo()
    {
        Time.timeScale = 0f;
        pauseGame.AimImage.SetActive(false);

        pauseGame.TutorialImage.SetActive(false);

        rawImage.SetActive(true);
        // Oyunu durdur
        // Video oynatmayı başlat
        videoPlayer.gameObject.SetActive(true); // VideoPlayer nesnesini aktif yap
        videoPlayer.Play();
        // Video bittiğinde Credits sahnesine geç
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Credits sahnesine geçiş yap
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(creditsSceneName);
    }

    public int GetActiveCardCount()
    {
        int activeCount = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) // Eğer çocuk aktifse
            {
                activeCount++; // Sayacı artır
            }
        }
        return activeCount; // Aktif çocuk sayısını döndür
    }

    public void SetSinCount()
    {
        int sinCount = GetActiveCardCount();
        _sinCountText.SetText($"{sinCount}/" + $"{6}");
        // Eğer tüm kartlar toplandıysa
        if (sinCount >= 6)
        {
            PauseGameAndPlayVideo();
        }
    }
}
