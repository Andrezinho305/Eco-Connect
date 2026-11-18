using System.Collections.Generic;
using UnityEngine;

// Conduz o tutorial etapa por etapa: mostra as instruções, destaca
// elementos, libera as ferramentas aos poucos e avança quando o
// jogador cumpre o que foi pedido.
public class TutorialManager : MonoBehaviour
{
    public const string CompletedKey = "TutorialCompleted";

    public static bool IsTutorialCompleted =>
        PlayerPrefs.GetInt(CompletedKey, 0) == 1;

    [Header("Referências do jogo")]
    [SerializeField] private NetworkBuilder networkBuilder;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NetworkConnectivity connectivity;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PauseManager pauseManager;

    [Header("Tutorial")]
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private TutorialHighlighter highlighter;

    [Header("Alvos de destaque na interface")]
    [SerializeField] private RectTransform waterButton;
    [SerializeField] private RectTransform electricityButton;
    [SerializeField] private RectTransform sewageButton;
    [SerializeField] private RectTransform eraserButton;
    [SerializeField] private RectTransform budgetPanel;
    [SerializeField] private RectTransform progressPanel;
    [SerializeField] private RectTransform starPanel;

    [Header("Etapas")]
    [SerializeField]
    private List<TutorialStep> steps = TutorialStep.CreateDefaultSteps();

    private int currentIndex = -1;
    private bool isFinished;
    private bool isVisible = true;

    // Evita que eventos disparados durante a troca de etapa
    // (ex.: SetAllowedTools) avancem o tutorial de novo
    private bool isChangingStep;

    private TutorialStep CurrentStep =>
        currentIndex >= 0 && currentIndex < steps.Count
            ? steps[currentIndex]
            : null;


    private void OnEnable()
    {
        if (networkBuilder != null)
        {
            networkBuilder.OnToolChanged += HandleToolChanged;
            networkBuilder.OnNetworkBuilt += HandleNetworkBuilt;
            networkBuilder.OnNetworkErased += HandleNetworkErased;
        }

        if (levelManager != null)
            levelManager.OnLevelCompleted += HandleLevelCompleted;

        if (tutorialUI != null)
        {
            tutorialUI.OnContinueClicked += HandleContinue;
            tutorialUI.OnSkipClicked += SkipTutorial;
        }
    }

    private void OnDisable()
    {
        if (networkBuilder != null)
        {
            networkBuilder.OnToolChanged -= HandleToolChanged;
            networkBuilder.OnNetworkBuilt -= HandleNetworkBuilt;
            networkBuilder.OnNetworkErased -= HandleNetworkErased;
        }

        if (levelManager != null)
            levelManager.OnLevelCompleted -= HandleLevelCompleted;

        if (tutorialUI != null)
        {
            tutorialUI.OnContinueClicked -= HandleContinue;
            tutorialUI.OnSkipClicked -= SkipTutorial;
        }
    }

    private void Start()
    {
        if (steps.Count == 0)
        {
            Finish();
            return;
        }

        // O jogador começa sem ferramenta e vai liberando aos poucos
        networkBuilder.DeselectTool();

        GoToStep(0);
    }

    private void Update()
    {
        if (isFinished)
            return;

        // Esconde o tutorial enquanto o jogo estiver pausado
        bool paused = pauseManager != null && pauseManager.IsPaused;
        SetVisible(!paused);

        // Enter também avança as etapas informativas
        if (!paused &&
            tutorialUI.IsContinueVisible &&
            (Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            HandleContinue();
        }
    }


    // ---------- Navegação ----------

    private void GoToStep(int index)
    {
        isChangingStep = true;

        try
        {
            // Pula as etapas que já estiverem cumpridas
            while (index < steps.Count)
            {
                currentIndex = index;

                TutorialStep step = steps[index];

                networkBuilder.SetAllowedTools(step.allowedTools);

                if (!ShouldSkip(step))
                {
                    ShowStep(step);
                    return;
                }

                index++;
            }

            Finish();
        }
        finally
        {
            isChangingStep = false;
        }
    }

    private void Advance()
    {
        GoToStep(currentIndex + 1);
    }

    private void ShowStep(TutorialStep step)
    {
        bool waitsForContinue =
            step.condition == TutorialCondition.PressContinue ||
            step.condition == TutorialCondition.OverlapInfo;

        // Depois de concluir a fase não faz sentido pular
        bool showSkip = !levelManager.IsLevelCompleted;

        tutorialUI.ShowStep(
            step.message,
            currentIndex + 1,
            steps.Count,
            waitsForContinue,
            step.continueLabel,
            showSkip
        );

        UpdateHighlights(step);
        SetVisible(true);

        SFXManager.Play(Sfx.TutorialStep);
    }

    private void Finish()
    {
        isFinished = true;
        currentIndex = steps.Count;

        highlighter.Clear();
        tutorialUI.SetVisible(false);

        networkBuilder.SetAllowedTools(ToolMask.All);
    }

    public void SkipTutorial()
    {
        MarkCompleted();

        Finish();

        if (!networkBuilder.HasToolSelected)
            networkBuilder.SelectWater();
    }


    // ---------- Condições ----------

    // Verifica, ao entrar numa etapa, se ela já pode ser pulada
    private bool ShouldSkip(TutorialStep step)
    {
        switch (step.condition)
        {
            case TutorialCondition.SelectTool:
            case TutorialCondition.BuildingSupplied:
            case TutorialCondition.LevelCompleted:
                return IsConditionMet(step);

            case TutorialCondition.OverlapInfo:
                return GetOverlapCells().Count == 0;

            default:
                return false;
        }
    }

    private bool IsConditionMet(TutorialStep step)
    {
        switch (step.condition)
        {
            case TutorialCondition.SelectTool:
                return networkBuilder.HasToolSelected &&
                       networkBuilder.CurrentTool == step.requiredTool;

            case TutorialCondition.BuildingSupplied:
                return connectivity.IsBuildingSupplied(
                    step.targetBuilding,
                    step.requiredNetwork
                );

            case TutorialCondition.LevelCompleted:
                return levelManager.IsLevelCompleted;

            default:
                return false;
        }
    }

    private void TryAdvanceByState()
    {
        TutorialStep step = CurrentStep;

        if (isFinished || step == null)
            return;

        if (IsConditionMet(step))
            Advance();
    }


    // ---------- Eventos do jogo ----------

    private void HandleToolChanged()
    {
        if (isChangingStep)
            return;

        TutorialStep step = CurrentStep;

        if (step != null &&
            step.condition == TutorialCondition.SelectTool)
        {
            TryAdvanceByState();
        }
    }

    private void HandleNetworkBuilt(Vector2Int position, NetworkType network)
    {
        TryAdvanceByState();
    }

    private void HandleNetworkErased(Vector2Int position)
    {
        TutorialStep step = CurrentStep;

        if (isFinished || step == null)
            return;

        if (step.condition == TutorialCondition.EraseAnyNetwork)
        {
            Advance();
            return;
        }

        TryAdvanceByState();
    }

    private void HandleLevelCompleted()
    {
        MarkCompleted();

        TryAdvanceByState();
    }

    private void HandleContinue()
    {
        TutorialStep step = CurrentStep;

        if (isFinished || step == null)
            return;

        if (step.condition == TutorialCondition.PressContinue ||
            step.condition == TutorialCondition.OverlapInfo)
        {
            Advance();
        }
    }


    // ---------- Destaques ----------

    private void UpdateHighlights(TutorialStep step)
    {
        highlighter.Clear();

        List<Vector2Int> cells = step.condition == TutorialCondition.OverlapInfo
            ? GetOverlapCells()
            : step.highlightCells;

        foreach (Vector2Int cell in cells)
        {
            highlighter.HighlightCell(
                gridManager.GetCellWorldPosition(cell),
                gridManager.CellSize
            );
        }

        foreach (TutorialUITarget target in step.highlightUI)
        {
            highlighter.HighlightUI(GetUITarget(target));
        }
    }

    private RectTransform GetUITarget(TutorialUITarget target)
    {
        switch (target)
        {
            case TutorialUITarget.WaterButton: return waterButton;
            case TutorialUITarget.ElectricityButton: return electricityButton;
            case TutorialUITarget.SewageButton: return sewageButton;
            case TutorialUITarget.EraserButton: return eraserButton;
            case TutorialUITarget.BudgetPanel: return budgetPanel;
            case TutorialUITarget.ProgressPanel: return progressPanel;
            case TutorialUITarget.StarPanel: return starPanel;
            default: return null;
        }
    }

    // Células com duas ou mais redes construídas
    private List<Vector2Int> GetOverlapCells()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GridCell cell = gridManager.GetCell(position);

                if (cell == null)
                    continue;

                int networkCount = 0;

                if (cell.HasNetwork(NetworkType.Water)) networkCount++;
                if (cell.HasNetwork(NetworkType.Electricity)) networkCount++;
                if (cell.HasNetwork(NetworkType.Sewage)) networkCount++;

                if (networkCount >= 2)
                    result.Add(position);
            }
        }

        return result;
    }


    // ---------- Utilidades ----------

    private void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;

        tutorialUI.SetVisible(visible);
        highlighter.SetVisible(visible);
    }

    private static void MarkCompleted()
    {
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
    }

    [ContextMenu("Resetar tutorial concluído")]
    private void ResetCompleted()
    {
        PlayerPrefs.DeleteKey(CompletedKey);
        PlayerPrefs.Save();

        Debug.Log("Tutorial marcado como não concluído.");
    }
}
