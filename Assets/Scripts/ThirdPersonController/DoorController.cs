using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform door; // Kapının Transform bileşeni
    public float openAngle = 90f; // Kapının açılacağı açı
    public float openSpeed = 2f; // Kapının açılma hızı
    private bool isOpen = false; // Kapının açık mı kapalı mı olduğunu kontrol eder
    private Quaternion closedRotation; // Kapının kapalı pozisyonu
    private Quaternion openRotation; // Kapının açık pozisyonu

    public AudioClip openSound; // Kapı açılma sesi
    private AudioSource audioSource; // AudioSource bileşeni

    private void Start()
    {
        closedRotation = door.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, 0, openAngle); // Y ekseninde açılacak

        // AudioSource bileşenini ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = openSound; // Ses kaynağını ayarla
    }

    private void Update()
    {
        // Kapının açılma ve kapanma animasyonu
        if (isOpen)
        {
            door.rotation = Quaternion.Slerp(
                door.rotation,
                openRotation,
                Time.deltaTime * openSpeed
            );
        }
        else
        {
            door.rotation = Quaternion.Slerp(
                door.rotation,
                closedRotation,
                Time.deltaTime * openSpeed
            );
        }
    }

    public void ToggleDoor(Vector3 playerPosition)
    {
        audioSource.volume = PlayerPrefs.GetFloat("SFXVolume");
        // Oyuncunun kapının hangi tarafında olduğunu kontrol et
        Vector3 doorPosition = door.position;
        if (playerPosition.x < doorPosition.x) // Oyuncu kapının sol tarafındaysa
        {
            openRotation = closedRotation * Quaternion.Euler(0, 0, -openAngle); // Kapı sola açılacak
        }
        else // Oyuncu kapının sağ tarafındaysa
        {
            openRotation = closedRotation * Quaternion.Euler(0, 0, openAngle); // Kapı sağa açılacak
        }

        isOpen = !isOpen; // Kapının durumunu değiştir

        // Kapı açıldığında ses çal
        if (isOpen && openSound != null)
        {
            audioSource.Play();
        }
    }
}
