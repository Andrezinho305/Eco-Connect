using UnityEngine;

public class LevelScoreCalculator : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BudgetManager budgetManager;

    [Header("Limites de avaliação")]
    [Range(0f, 1f)]
    [SerializeField] private float twoStarThreshold = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float threeStarThreshold = 0.35f;

    // Valores calculados a partir do orçamento inicial
    public float TwoStarBudget =>
        budgetManager.ConfiguredInitialBudget * twoStarThreshold;

    public float ThreeStarBudget =>
        budgetManager.ConfiguredInitialBudget * threeStarThreshold;

    public bool HasBudgetManager => budgetManager != null;

    public int CalculateStars()
    {
        int initial = budgetManager.InitialBudget;
        int remaining = budgetManager.CurrentBudget;

        if (initial <= 0)
            return 1;

        float remainingPercentage =
            (float)remaining / initial;

        if (remainingPercentage >= threeStarThreshold)
            return 3;

        if (remainingPercentage >= twoStarThreshold)
            return 2;

        return 1;
    }

    public int GetSpentBudget()
    {
        return budgetManager.InitialBudget -
               budgetManager.CurrentBudget;
    }
}

