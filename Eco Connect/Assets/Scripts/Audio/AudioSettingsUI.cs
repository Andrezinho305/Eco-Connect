using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void OnEnable()
    {
        if (AudioSettingsManager.Instance == null)
            return;

        musicVolumeSlider.SetValueWithoutNotify(
            AudioSettingsManager.Instance.MusicVolume
        );

        sfxVolumeSlider.SetValueWithoutNotify(
            AudioSettingsManager.Instance.SFXVolume
        );

        musicVolumeSlider.onValueChanged.AddListener(
            OnMusicVolumeChanged
        );

        sfxVolumeSlider.onValueChanged.AddListener(
            OnSFXVolumeChanged
        );
    }

    private void OnDisable()
    {
        musicVolumeSlider.onValueChanged.RemoveListener(
            OnMusicVolumeChanged
        );

        sfxVolumeSlider.onValueChanged.RemoveListener(
            OnSFXVolumeChanged
        );
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSFXVolume(value);
    }
}

