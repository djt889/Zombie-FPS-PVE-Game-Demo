using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [Header("音量控制")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("显示文本")]
    [SerializeField] private Text masterVolumeText;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Text sfxVolumeText;

    private void Start()
    {
        // 初始化滑块值
        masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
        
        // 更新文本显示
        UpdateVolumeText();
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        UpdateVolumeText();
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        UpdateVolumeText();
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        UpdateVolumeText();
    }

    private void UpdateVolumeText()
    {
        if (masterVolumeText != null)
            masterVolumeText.text = $"主音量: {Mathf.Round(AudioManager.Instance.GetMasterVolume() * 100)}%";
            
        if (musicVolumeText != null)
            musicVolumeText.text = $"音乐音量: {Mathf.Round(AudioManager.Instance.GetMusicVolume() * 100)}%";
            
        if (sfxVolumeText != null)
            sfxVolumeText.text = $"音效音量: {Mathf.Round(AudioManager.Instance.GetSFXVolume() * 100)}%";
    }
}
