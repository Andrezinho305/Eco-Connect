
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

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color =
                i < earnedStars
                ? earnedStarColor
                : lockedStarColor;
        }

        spentBudgetText.text =
            "Orçamento utilizado: R$ " +
            scoreCalculator.GetSpentBudget().ToString("N0");

        remainingBudgetText.text =
            "Saldo restante: R$ " +
            budgetManager.CurrentBudget.ToString("N0");

        victoryPanel.SetActive(true);
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

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
