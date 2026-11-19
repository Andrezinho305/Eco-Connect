using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConstructionCostPreview : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NetworkBuilder networkBuilder;
    [SerializeField] private BudgetManager budgetManager;

    [SerializeField] private TMP_Text costText;

    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color insufficientColor = Color.red;

    [Header("Tema (opcional)")]
    [Tooltip("Pílula de fundo que acompanha o cursor (o texto fica dentro dela)")]
    [SerializeField] private RectTransform pill;
    [SerializeField] private Color refundColor = new Color(0.12f, 0.48f, 0.23f);

    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    private Image pillImage;

    private void Awake()
    {
        if (pill != null)
            pillImage = pill.GetComponent<Image>();
    }

    private void Update()
    {
        UpdatePreview();
    }

    private void SetVisible(bool visible)
    {
        costText.enabled = visible;

        if (pillImage != null)
            pillImage.enabled = visible;
    }

    private void UpdatePreview()
    {
        SetVisible(false);

        if (!networkBuilder.CanUseCurrentTool)
            return;

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

            costText.text = "+ R$ " + cost.ToString("N0", PtBr);
            costText.color = pill != null ? refundColor : availableColor;
        }
        else
        {
            if (cell.HasNetwork(networkBuilder.SelectedNetwork))
                return;

            cost = budgetManager.GetConstructionCost(
                cell,
                networkBuilder.SelectedNetwork
            );

            costText.text = "− R$ " + cost.ToString("N0", PtBr);

            costText.color =
                cost <= budgetManager.CurrentBudget
                ? availableColor
                : insufficientColor;
        }

        SetVisible(true);

        Transform follow = pill != null ? pill : costText.transform;

        follow.position =
            Input.mousePosition + new Vector3(20, 20, 0);
    }
}
