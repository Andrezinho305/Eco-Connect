using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Porcentagem ao lado (opcional)")]
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text sfxValueText;

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

        UpdateValueText(musicValueText, musicVolumeSlider.value);
        UpdateValueText(sfxValueText, sfxVolumeSlider.value);

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

        UpdateValueText(musicValueText, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSFXVolume(value);

        UpdateValueText(sfxValueText, value);

        // Deixa ouvir o volume escolhido enquanto arrasta
        SFXManager.Play(Sfx.VolumePreview);
    }

    private static void UpdateValueText(TMP_Text label, float value)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
