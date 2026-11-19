
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject victoryPanel;

    [Header("Navegação")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    [SerializeField] private LevelScoreCalculator scoreCalculator;
    [SerializeField] private BudgetManager budgetManager;


    [Header("Resultado da fase")]
    [SerializeField] private TMPro.TMP_Text spentBudgetText;
    [SerializeField] private TMPro.TMP_Text remainingBudgetText;
    [SerializeField] private UnityEngine.UI.Image[] stars;

    [SerializeField] private Color earnedStarColor = Color.yellow;
    [SerializeField] private Color lockedStarColor = Color.gray;

    [Header("Carimbo \"Aprovado\" (opcional)")]
    [SerializeField] private RectTransform stamp;
    [SerializeField] private TMPro.TMP_Text stampDetailText;

    [Header("Tempos da sequência (segundos)")]
    [SerializeField] private float firstStarDelay = 0.55f;
    [SerializeField] private float starInterval = 0.3f;
    [SerializeField] private float stampDelay = 0.2f;

    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    private void OnEnable()
    {
        if (levelManager != null)
            levelManager.OnLevelCompleted += ShowVictory;
    }

    private void Start()
    {
        victoryPanel.SetActive(false);

        if (levelManager.IsLevelCompleted)
            ShowVictory();
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.OnLevelCompleted -= ShowVictory;
    }


    private void ShowVictory()
    {
        int earnedStars = scoreCalculator.CalculateStars();

        spentBudgetText.text =
            "<color=#5B6B60>Gasto</color>  R$ " +
            scoreCalculator.GetSpentBudget().ToString("N0", PtBr);

        remainingBudgetText.text =
            "<color=#5B6B60>Saldo</color>  R$ " +
            budgetManager.CurrentBudget.ToString("N0", PtBr);

        // Estrelas e carimbo começam escondidos e entram em sequência
        foreach (UnityEngine.UI.Image star in stars)
        {
            star.color = lockedStarColor;
            star.rectTransform.localScale = Vector3.zero;
        }

        if (stamp != null)
            stamp.gameObject.SetActive(false);

        victoryPanel.SetActive(true);

        SFXManager.Play(Sfx.Victory);

        StopAllCoroutines();
        StartCoroutine(VictorySequence(earnedStars));
    }

    private IEnumerator VictorySequence(int earnedStars)
    {
        yield return new WaitForSecondsRealtime(firstStarDelay);

        for (int i = 0; i < stars.Length; i++)
        {
            bool earned = i < earnedStars;

            stars[i].color = earned ? earnedStarColor : lockedStarColor;

            if (earned)
                SFXManager.PlayStar(i);

            StartCoroutine(Pop(stars[i].rectTransform, earned ? 1.25f : 1f));

            yield return new WaitForSecondsRealtime(earned ? starInterval : starInterval * 0.4f);
        }

        if (stamp == null)
            yield break;

        yield return new WaitForSecondsRealtime(stampDelay);

        if (stampDetailText != null)
            stampDetailText.text = earnedStars == 1 ? "1 ESTRELA" : earnedStars + " ESTRELAS";

        stamp.gameObject.SetActive(true);
        SFXManager.Play(Sfx.Stamp);

        yield return Slam(stamp);
    }

    // Cresce de 0 até "overshoot" e assenta em 1
    private static IEnumerator Pop(RectTransform target, float overshoot)
    {
        const float up = 0.14f;
        const float down = 0.1f;

        for (float time = 0f; time < up; time += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * Mathf.Lerp(0f, overshoot, time / up);
            yield return null;
        }

        for (float time = 0f; time < down; time += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * Mathf.Lerp(overshoot, 1f, time / down);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    // Carimbo: começa grande e transparente e "bate" no papel
    private static IEnumerator Slam(RectTransform target)
    {
        const float duration = 0.16f;

        CanvasGroup group = target.GetComponent<CanvasGroup>();

        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            float t = time / duration;
            float eased = t * t;

            target.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, eased);

            if (group != null)
                group.alpha = Mathf.Clamp01(t * 1.5f);

            yield return null;
        }

        target.localScale = Vector3.one;

        if (group != null)
            group.alpha = 1f;
    }

    public void NextLevel()
    {
        int currentSceneIndex =
            SceneManager.GetActiveScene().buildIndex;

        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Todas as fases foram concluídas!");

            ReturnToMainMenu();
        }
    }


    public void RetryLevel()
    {
        // Obtém a cena do nível atual
        Scene currentScene = SceneManager.GetActiveScene();

        // Recarrega a mesma cena
        SceneManager.LoadScene(currentScene.buildIndex);
    }


    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
