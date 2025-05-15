using UnityEngine;
using UnityEngine.Video;

public class VideoSkipper : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoCanvas; // Atlama butonu

    // Video oynatıldıktan sonra atlama için bayrak
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

        // Video başladığında bayrağı ayarla
        videoPlayer.started += (vp) => { videoStarted = true; };

        // Video bittiğinde bayrağı sıfırla
        videoPlayer.loopPointReached += (vp) => { videoStarted = false; };
    }

    void Update()
    {
        // Eğer video oynatılıyorsa ve Space tuşuna basıldıysa
        if (videoStarted && Input.GetKeyDown(KeyCode.Space))
        {
            SkipVideo();
        }
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
    }
}
    
    