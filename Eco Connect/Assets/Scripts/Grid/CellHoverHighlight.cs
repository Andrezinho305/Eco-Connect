using UnityEngine;
using UnityEngine.EventSystems;

// Contorno na célula sob o mouse, por cima do tabuleiro.
// Não altera os sprites do tabuleiro: é um SpriteRenderer próprio.
// Branco = pode construir/apagar; vermelho = falta dinheiro.
public class CellHoverHighlight : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NetworkBuilder networkBuilder;
    [SerializeField] private BudgetManager budgetManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PauseManager pauseManager;

    [SerializeField] private Sprite sprite;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color insufficientColor = new Color(0.851f, 0.282f, 0.231f);

    [Tooltip("Acima das construções e indicadores do tabuleiro (que vão até 10)")]
    [SerializeField] private int sortingOrder = 20;

    private SpriteRenderer highlight;

    private void Awake()
    {
        GameObject go = new GameObject("Highlight");
        go.transform.SetParent(transform, false);

        highlight = go.AddComponent<SpriteRenderer>();
        highlight.sprite = sprite;
        highlight.sortingOrder = sortingOrder;
        highlight.enabled = false;
    }

    private void Update()
    {
        highlight.enabled = false;

        if (sprite == null || gridManager == null || networkBuilder == null)
            return;

        if (pauseManager != null && pauseManager.IsPaused)
            return;

        if (levelManager != null && levelManager.IsLevelCompleted)
            return;

        if (!networkBuilder.CanUseCurrentTool)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = gridManager.transform.position.z - mainCamera.transform.position.z;

        Vector2Int position = gridManager.WorldToGridPosition(mainCamera.ScreenToWorldPoint(mousePosition));
        GridCell cell = gridManager.GetCell(position);

        if (cell == null || !cell.CanBuildNetwork)
            return;

        Color color = availableColor;

        if (networkBuilder.IsEraseMode)
        {
            if (cell.Networks == NetworkType.None)
                return;
        }
        else
        {
            if (cell.HasNetwork(networkBuilder.SelectedNetwork))
                return;

            int cost = budgetManager.GetConstructionCost(cell, networkBuilder.SelectedNetwork);

            if (cost > budgetManager.CurrentBudget)
                color = insufficientColor;
        }

        // Pulsa de leve para chamar atenção sem esconder a arte
        color.a *= 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 5f);

        float spriteSize = sprite.bounds.size.x;
        float scale = spriteSize > 0f ? gridManager.CellSize / spriteSize : 1f;

        Vector3 world = gridManager.GetCellWorldPosition(position);

        highlight.transform.position = new Vector3(world.x, world.y, gridManager.transform.position.z);
        highlight.transform.localScale = Vector3.one * scale;
        highlight.color = color;
        highlight.enabled = true;
    }
}
