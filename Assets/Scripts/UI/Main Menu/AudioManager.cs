using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mainMixer;
    
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Text Volume Display")]
    public TMP_Text masterText;
    public TMP_Text musicText;
    public TMP_Text sfxText;

    private void Start()
    {
        // Load saved volume or default to 1
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music  = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx    = PlayerPrefs.GetFloat("SFXVolume", 1f);

        masterSlider.value = master;
        musicSlider.value  = music;
        sfxSlider.value    = sfx;

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMasterVolume(float value)
    {
        Debug.Log("Master slider value = " + value);
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(value <= 0 ? 0.001f : value) * 20);
        masterText.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        Debug.Log("music value = " + value);
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(value <= 0 ? 0.001f : value) * 20);
        musicText.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(value <= 0 ? 0.001f : value) * 20);
        sfxText.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
