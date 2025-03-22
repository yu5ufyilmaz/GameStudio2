using UnityEngine;
[CreateAssetMenu(fileName = "Audio Config", menuName = "Guns/Audio Config", order = 5)]
public class AudioScriptableObject : ScriptableObject
{
    [Range(0f, 1f)]
    public float Volume = 1f;

    public AudioClip[] FireClips;
    public AudioClip EmptyClip;
    public AudioClip ReloadClip;
    public AudioClip LastBulletClip;

    public void PlayShotingClip(AudioSource audioSource, bool IsLastBullet = false)
    {
        if (IsLastBullet && LastBulletClip != null)
        {
            AudioSource.PlayClipAtPoint(LastBulletClip, audioSource.transform.position, Volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(FireClips[Random.Range(0, FireClips.Length)], audioSource.transform.position, Volume);
        }
    }

    public void PLayOutOfAmmoClip(AudioSource audioSource)
    {
        AudioSource.PlayClipAtPoint(EmptyClip, audioSource.transform.position, Volume);
    }

    public void PlayReloadClip(AudioSource audioSource)
    {
        AudioSource.PlayClipAtPoint(ReloadClip, audioSource.transform.position, Volume);
    }
}
