using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoSkipper : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField]
    private GameObject videoCanvas; // Atlama butonu

    // Video oynatıldıktan sonra atlama için bayrak
    [SerializeField]
    private bool videoStarted = false;

    void Start()
    {
        // VideoPlayer atanmamışsa, bu nesnenin üzerindeki VideoPlayer'ı almaya çalış
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // VideoPlayer hala null ise hata ver
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer komponenti bulunamadı! Lütfen inspector'dan atayın.");
            return;
        }

        // Oyunun ilk açılışını kontrol et
        if (PlayerPrefs.GetInt("HasPlayedBefore", 0) == 0)
        {
            // Video başladığında bayrağı ayarla
            videoPlayer.started += (vp) =>
            {
                videoStarted = true;
            };

            // Video bittiğinde bayrağı sıfırla
            videoPlayer.loopPointReached += (vp) =>
            {
                videoStarted = false;
                OnVideoFinished(vp);
            };

            // Video oynat
            videoPlayer.Play();
        }
        else
        {
            // Eğer daha önce oynatıldıysa, sahneye geç
            // LoadNextScene();
        }
    }

    void Update()
    {
        // Eğer video oynatılıyorsa ve Space tuşuna basıldıysa
        if (videoStarted && Input.GetKeyDown(KeyCode.Space))
        {
            SkipVideo();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Credits sahnesine geçiş yap
        Cursor.visible = true;
        SkipVideo();
    }

    void SkipVideo()
    {
        // Videoyu durdur
        videoPlayer.Stop();

        // Video oynatıcıyı devre dışı bırak veya gizle
        videoPlayer.gameObject.SetActive(false);
        videoCanvas.SetActive(false);

        // Bayrağı sıfırla
        videoStarted = false;

        // İlk açılış bayrağını ayarla
        PlayerPrefs.SetInt("HasPlayedBefore", 1);
        PlayerPrefs.Save();

        // Sonraki sahneye geç
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Burada sahne adını değiştirin
        SceneManager.LoadScene("StartMenu"); // "NextSceneName" yerine geçmek istediğiniz sahnenin adını yazın
    }
}
