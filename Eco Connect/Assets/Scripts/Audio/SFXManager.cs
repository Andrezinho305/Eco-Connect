using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Todos os efeitos sonoros do jogo
public enum Sfx
{
    BuildWater,
    BuildElectricity,
    BuildSewage,
    Erase,
    NoMoney,
    ServiceConnected,
    HouseComplete,
    Victory,
    Star1,
    Star2,
    Star3,
    Click,
    ToolSwitch,
    PanelOpen,
    PanelClose,
    TutorialStep,
    VolumePreview,
    Stamp
}

// Toca os efeitos sonoros e o som ambiente.
// Cria a si mesmo ao iniciar o jogo (não precisa estar na cena)
// e carrega os sons de Assets/Resources/SFX.
// Uso: SFXManager.Play(Sfx.BuildWater);
public class SFXManager : MonoBehaviour
{
    private const string ResourceFolder = "SFX/";
    private const string MainMenuScene = "MainMenu";

    private const string AmbientFile = "ambient_city_birds";
    private const float AmbientFadeTime = 1.5f;

    private const int VoiceCount = 10;

    private const float FirstStarDelay = 0.75f;
    private const float StarInterval = 0.3f;

    // Arquivo, variação de tom (+/-), intervalo mínimo entre repetições e tom base
    private struct SfxDefinition
    {
        public string file;
        public float pitchVariation;
        public float cooldown;
        public float basePitch;

        public SfxDefinition(string file, float pitchVariation, float cooldown, float basePitch = 1f)
        {
            this.file = file;
            this.pitchVariation = pitchVariation;
            this.cooldown = cooldown;
            this.basePitch = basePitch;
        }
    }

    private static readonly Dictionary<Sfx, SfxDefinition> Definitions =
        new Dictionary<Sfx, SfxDefinition>
        {
            { Sfx.BuildWater,       new SfxDefinition("build_water",       0.06f, 0.04f) },
            { Sfx.BuildElectricity, new SfxDefinition("build_electric",    0.06f, 0.04f) },
            { Sfx.BuildSewage,      new SfxDefinition("build_sewage",      0.06f, 0.04f) },
            { Sfx.Erase,            new SfxDefinition("erase",             0.05f, 0.07f) },
            { Sfx.NoMoney,          new SfxDefinition("no_money",          0f,    0.25f) },
            { Sfx.ServiceConnected, new SfxDefinition("service_connected", 0f,    0.08f) },
            { Sfx.HouseComplete,    new SfxDefinition("house_complete",    0f,    0.1f) },
            { Sfx.Victory,          new SfxDefinition("victory",           0f,    0.5f) },
            { Sfx.Star1,            new SfxDefinition("star_1",            0f,    0.05f) },
            { Sfx.Star2,            new SfxDefinition("star_2",            0f,    0.05f) },
            { Sfx.Star3,            new SfxDefinition("star_3",            0f,    0.05f) },
            { Sfx.Click,            new SfxDefinition("click",             0.03f, 0.03f) },
            { Sfx.ToolSwitch,       new SfxDefinition("tool_switch",       0.03f, 0.03f) },
            { Sfx.PanelOpen,        new SfxDefinition("panel_open",        0f,    0.05f) },
            { Sfx.PanelClose,       new SfxDefinition("panel_close",       0f,    0.05f) },
            { Sfx.TutorialStep,     new SfxDefinition("tutorial_step",     0f,    0.1f) },
            { Sfx.VolumePreview,    new SfxDefinition("service_connected", 0f,    0.15f) },
            // Carimbo "Aprovado": o baque do esgoto, mais grave
            { Sfx.Stamp,            new SfxDefinition("build_sewage",      0f,    0.3f, 0.8f) },
        };

    private static SFXManager instance;

    private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    private readonly Dictionary<Sfx, float> lastPlayedTime = new Dictionary<Sfx, float>();

    // Pedidos do frame atual (resolvidos no LateUpdate)
    private readonly HashSet<Sfx> pending = new HashSet<Sfx>();

    private readonly HashSet<Button> hookedButtons = new HashSet<Button>();

    private AudioSource[] voices;
    private int nextVoice;

    private AudioSource ambientSource;
    private float ambientTarget;

    private AudioMixerGroup sfxGroup;


    // ---------- API pública ----------

    public static void Play(Sfx sfx)
    {
        if (instance != null)
            instance.pending.Add(sfx);
    }

    // Nota de uma estrela (0, 1, 2 → notas subindo)
    public static void PlayStar(int index)
    {
        Sfx[] stars = { Sfx.Star1, Sfx.Star2, Sfx.Star3 };

        Play(stars[Mathf.Clamp(index, 0, stars.Length - 1)]);
    }

    // Uma nota por estrela ganha, subindo (1, 2, 3)
    public static void PlayStars(int count)
    {
        if (instance != null && count > 0)
            instance.StartCoroutine(instance.PlayStarsRoutine(count));
    }


    // ---------- Inicialização ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("SFX Manager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SFXManager>();
    }

    private void Awake()
    {
        voices = new AudioSource[VoiceCount];

        for (int i = 0; i < VoiceCount; i++)
            voices[i] = CreateSource(false);

        ambientSource = CreateSource(true);
        ambientSource.volume = 0f;

        foreach (KeyValuePair<Sfx, SfxDefinition> pair in Definitions)
        {
            AudioClip clip = Resources.Load<AudioClip>(ResourceFolder + pair.Value.file);

            if (clip == null)
                Debug.LogWarning("Som não encontrado: Resources/" + ResourceFolder + pair.Value.file);
            else
                clips[pair.Key] = clip;
        }

        ambientSource.clip = Resources.Load<AudioClip>(ResourceFolder + AmbientFile);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (instance == this)
            instance = null;
    }

    private AudioSource CreateSource(bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryRouteToMixer();
        HookButtons();

        // Ambiente só nas fases
        ambientTarget = scene.name == MainMenuScene ? 0f : 1f;

        if (ambientTarget > 0f && ambientSource.clip != null && !ambientSource.isPlaying)
            ambientSource.Play();
    }

    // Usa o grupo SFX do MainMixer (o mesmo do volume de efeitos)
    private void TryRouteToMixer()
    {
        if (sfxGroup != null)
            return;

        AudioSettingsManager settings = AudioSettingsManager.Instance;

        if (settings == null || settings.Mixer == null)
            return;

        AudioMixerGroup[] groups = settings.Mixer.FindMatchingGroups("SFX");

        if (groups.Length == 0)
            return;

        sfxGroup = groups[0];

        foreach (AudioSource voice in voices)
            voice.outputAudioMixerGroup = sfxGroup;

        ambientSource.outputAudioMixerGroup = sfxGroup;
    }

    // Todos os botões da cena tocam o clique
    private void HookButtons()
    {
        hookedButtons.RemoveWhere(button => button == null);

        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (Button button in buttons)
        {
            if (hookedButtons.Add(button))
                button.onClick.AddListener(PlayClick);
        }
    }

    private static void PlayClick()
    {
        Play(Sfx.Click);
    }


    // ---------- Reprodução ----------

    private void Update()
    {
        // Fade do ambiente (funciona mesmo com o jogo pausado)
        float step = Time.unscaledDeltaTime / AmbientFadeTime;

        ambientSource.volume = Mathf.MoveTowards(ambientSource.volume, ambientTarget, step);

        if (ambientTarget <= 0f && ambientSource.volume <= 0f && ambientSource.isPlaying)
            ambientSource.Stop();
    }

    private void LateUpdate()
    {
        if (pending.Count == 0)
            return;

        // Um som mais específico substitui o clique genérico do botão
        if (pending.Contains(Sfx.ToolSwitch) ||
            pending.Contains(Sfx.PanelOpen) ||
            pending.Contains(Sfx.PanelClose) ||
            pending.Contains(Sfx.TutorialStep) ||
            pending.Contains(Sfx.Victory) ||
            pending.Contains(Sfx.VolumePreview))
        {
            pending.Remove(Sfx.Click);
        }

        if (pending.Contains(Sfx.PanelOpen))
            pending.Remove(Sfx.PanelClose);

        // Casa completa já inclui o "serviço conectado"
        if (pending.Contains(Sfx.HouseComplete))
            pending.Remove(Sfx.ServiceConnected);

        // Na vitória, só a vitória
        if (pending.Contains(Sfx.Victory))
        {
            pending.Remove(Sfx.HouseComplete);
            pending.Remove(Sfx.ServiceConnected);
            pending.Remove(Sfx.TutorialStep);
        }

        foreach (Sfx sfx in pending)
            PlayNow(sfx);

        pending.Clear();
    }

    private void PlayNow(Sfx sfx)
    {
        if (!clips.TryGetValue(sfx, out AudioClip clip))
            return;

        SfxDefinition definition = Definitions[sfx];

        float now = Time.unscaledTime;

        if (lastPlayedTime.TryGetValue(sfx, out float last) &&
            now - last < definition.cooldown)
            return;

        lastPlayedTime[sfx] = now;

        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        voice.clip = clip;
        voice.pitch = definition.basePitch +
                      Random.Range(-definition.pitchVariation, definition.pitchVariation);
        voice.Play();
    }

    private IEnumerator PlayStarsRoutine(int count)
    {
        yield return new WaitForSecondsRealtime(FirstStarDelay);

        Sfx[] stars = { Sfx.Star1, Sfx.Star2, Sfx.Star3 };

        for (int i = 0; i < Mathf.Min(count, stars.Length); i++)
        {
            PlayNow(stars[i]);

            yield return new WaitForSecondsRealtime(StarInterval);
        }
    }
}
