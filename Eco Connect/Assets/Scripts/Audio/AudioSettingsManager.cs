using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    public float MusicVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    public AudioMixer Mixer => audioMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Com o modo "Normal" o mixer congela quando Time.timeScale = 0
        // (menu de pausa) e os sliders de volume não fazem efeito.
        if (audioMixer != null)
            audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
    }

    private void Start()
    {
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, 0.7f);
        SFXVolume = PlayerPrefs.GetFloat(SFXKey, 1f);

        ApplyVolume("MusicVolume", MusicVolume);
        ApplyVolume("SFXVolume", SFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);

        ApplyVolume("MusicVolume", MusicVolume);

        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);

        ApplyVolume("SFXVolume", SFXVolume);

        PlayerPrefs.SetFloat(SFXKey, SFXVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null)
            return;

        // Converte volume linear para decibéis.
        // Zero corresponde a silêncio.
        float decibels = value <= 0f
            ? -80f
            : Mathf.Log10(value) * 20f;

        if (!audioMixer.SetFloat(parameter, decibels))
        {
            Debug.LogWarning(
                "Parâmetro não encontrado no AudioMixer: " +
                parameter
            );
        }
    }
}

