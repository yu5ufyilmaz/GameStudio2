using UnityEngine;

// Attach this script to any building GameObject with a Box Collider marked as "Is Trigger".
public class BuildingMusicTrigger : MonoBehaviour
{
    [Tooltip("The background music to play when player enters this building.")]
    public AudioClip buildingMusic;

    private void Reset()
    {
        // Ensure the collider is set to trigger automatically on adding the script
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Assuming the player has the tag "Player"
        if (other.CompareTag("Player"))
        {
            if (BackgroundMusicManager.Instance != null && buildingMusic != null)
            {
                BackgroundMusicManager.Instance.PlayMusic(buildingMusic);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When player leaves the building trigger, optionally revert to default music
        if (other.CompareTag("Player"))
        {
            if (BackgroundMusicManager.Instance != null)
            {
                BackgroundMusicManager.Instance.PlayDefaultMusic();
            }
        }
    }
}
