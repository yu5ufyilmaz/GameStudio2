using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider musicSlider; // Müzik ses seviyesi için slider
    public Slider sfxSlider; // SFX ses seviyesi için slider
    public Toggle fullscreenToggle; // Tam ekran için toggle
    public TMP_Dropdown resolutionDropdown; // Çözünürlük için dropdown
    public TMP_Dropdown qualityDropdown; // Kalite için dropdown

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

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        // Çözünürlük ve kalite ayarlarını ayarla
        PopulateResolutionDropdown();
        PopulateQualityDropdown();
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

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0); // Değeri kaydet
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution selectedResolution = resolutions[resolutionIndex];
        Screen.SetResolution(
            selectedResolution.width,
            selectedResolution.height,
            Screen.fullScreen
        );
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex); // Değeri kaydet
    }

    private void PopulateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        Resolution[] resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int defaultResolutionIndex = -1; // 1920x1080 çözünürlüğünün indeksini bulmak için
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);

            // Eğer çözünürlük 1920x1080 ise, indeksini kaydet
            if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
            {
                defaultResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        // Eğer 1920x1080 çözünürlüğü mevcutsa, onu varsayılan olarak ayarla
        if (defaultResolutionIndex != -1)
        {
            resolutionDropdown.value = defaultResolutionIndex;
        }
        else
        {
            // Eğer mevcut değilse, PlayerPrefs'ten kaydedilmiş çözünürlüğü al
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        }
        resolutionDropdown.RefreshShownValue();
    }

    private void PopulateQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        List<string> options = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 0);
        qualityDropdown.RefreshShownValue();
    }
}
