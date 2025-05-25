using UnityEngine;

public class ReSpawnPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform teleportTarget; // Işınlanacak hedef konum

    private void OnTriggerEnter(Collider other)
    {
        // Eğer tetikleyici alanına giren nesne "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // Player nesnesinin konumunu teleportTarget pozisyonuna ayarla
            other.transform.position = teleportTarget.position;
            // İstersen rotasyonu da aynen set edebilirsin:
            // other.transform.rotation = teleportTarget.rotation;
        }
    }
}
