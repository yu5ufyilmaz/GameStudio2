using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider musicSlider; // Müzik ses seviyesi için slider
    public Slider sfxSlider; // SFX ses seviyesi için slider

    void Start()
    {
        //musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        //sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        // Slider değerlerini ayarla
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        // Slider değerlerini değiştirildiğinde SoundManager'a bildir
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        SoundManager.instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume); // Değeri kaydet
    }

    public void SetSFXVolume(float volume)
    {
        SoundManager.instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume); // Değeri kaydet
    }
}
