using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConstructionCostPreview : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NetworkBuilder networkBuilder;
    [SerializeField] private BudgetManager budgetManager;

    [SerializeField] private TMP_Text costText;

    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color insufficientColor = Color.red;

    private void Update()
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        costText.enabled = false;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            gridManager.transform.position.z -
            mainCamera.transform.position.z;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2Int gridPosition =
            gridManager.WorldToGridPosition(worldPosition);

        GridCell cell = gridManager.GetCell(gridPosition);

        if (cell == null || !cell.CanBuildNetwork)
            return;

        int cost;

        if (networkBuilder.IsEraseMode)
        {
            if (cell.Networks == NetworkType.None)
                return;

            cost = budgetManager.GetCellCost(cell.Networks);

            costText.text = "+ R$ " + cost;
            costText.color = availableColor;
        }
        else
        {
            if (cell.HasNetwork(networkBuilder.SelectedNetwork))
                return;

            cost = budgetManager.GetConstructionCost(
                cell,
                networkBuilder.SelectedNetwork
            );

            costText.text = "- R$ " + cost;

            costText.color =
                cost <= budgetManager.CurrentBudget
                ? availableColor
                : insufficientColor;
        }

        costText.enabled = true;

        costText.transform.position =
            Input.mousePosition + new Vector3(20, 20, 0);
    }
}

