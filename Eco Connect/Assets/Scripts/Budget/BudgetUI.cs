using TMPro;
using UnityEngine;

public class BudgetUI : MonoBehaviour
{
    [SerializeField] private BudgetManager budgetManager;
    [SerializeField] private TMP_Text budgetText;

    private void Start()
    {
        UpdateBudgetText(budgetManager.CurrentBudget);
    }

    private void OnEnable()
    {
        if (budgetManager != null)
        {
            budgetManager.OnBudgetChanged += UpdateBudgetText;
        }
    }

    private void OnDisable()
    {
        if (budgetManager != null)
        {
            budgetManager.OnBudgetChanged -= UpdateBudgetText;
        }
    }

    private void UpdateBudgetText(int remainingBudget)
    {
        budgetText.text = "R$ " + remainingBudget.ToString("N0");
    }
}
