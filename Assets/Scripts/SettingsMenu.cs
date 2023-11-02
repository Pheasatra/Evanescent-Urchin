using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------

public class SettingsMenu : MonoBehaviour
{
    public MusicPlayer musicPlayer;
    public Manager manager; 
    
    [Space(10)]

    public Slider volumeSlider;
    public Slider bloomSlider;
    public Slider screenShake;

    [Space(10)]

    [HideInInspector]
    public Bloom bloom;
    public Volume volume;

    [HideInInspector]
    public float currentVolume;
    
    private float initalBloom;

    //-----------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out bloom);

        initalBloom = bloom.intensity.value;
        bloomSlider.maxValue = initalBloom;
        bloomSlider.value = initalBloom;
        screenShake.value = manager.screenShakeMultiplyer;

        volumeSlider.value = 1.0f;

        // Sometimes you have to reset the settings
        SetVolume(1.0f);

        LoadSettings();
        bloomSlider.value = 10;
    }

    //-----------------------------------------------------------------------

    public void SetBloom(float bloomIntensity)
    {
        bloom.intensity.value = bloomIntensity;

        PlayerPrefs.SetFloat("BloomPreference", bloom.intensity.value);
    }

    //-----------------------------------------------------------------------

    public void SetVolume(float volume)
    {
        // Its broken so yeah
        /*
        AudioManager.audioManager.volumeRange = volume;
        musicPlayer.volume = volume;

        currentVolume = volume;*/

        PlayerPrefs.SetFloat("VolumePreference", currentVolume);
    }

    //-----------------------------------------------------------------------

    public void SetScreenShake(float shakeMultiplyer)
    {
        manager.screenShakeMultiplyer = shakeMultiplyer;

        PlayerPrefs.SetFloat("ScreenShakePreference", currentVolume);
    }

    //-----------------------------------------------------------------------

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FullscreenPreference", Convert.ToInt32(Screen.fullScreen));
    }

    //-----------------------------------------------------------------------

    public void LoadSettings()
    {
        switch (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            case true:
                Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
                break;

            case false:
                Screen.fullScreen = true;
                break;
        }

        switch (PlayerPrefs.HasKey("VolumePreference"))
        {
            case true:
                volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
                SetVolume(volumeSlider.value);
                break;

            case false:
                SetVolume(1);
                break;
        }

        switch (PlayerPrefs.HasKey("BloomPreference"))
        {
            case true:
                bloomSlider.value = PlayerPrefs.GetFloat("BloomPreference");
                SetBloom(bloomSlider.value);
                break;

            case false:
                SetBloom(1);
                break;
        }
    }
}
