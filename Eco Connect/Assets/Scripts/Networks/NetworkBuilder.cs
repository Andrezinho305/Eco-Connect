using UnityEngine;
using UnityEngine.EventSystems;

public class NetworkBuilder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridRenderer gridRenderer;

    [SerializeField] private BudgetManager budgetManager;

    private NetworkType selectedNetwork = NetworkType.Water;

    private bool eraseMode = false;

    private bool pointerStartedOverUI;


    private void Update()
    {
        HandleToolSelection();
        HandleConstruction();
    }



    private void HandleToolSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectWater();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectElectricity();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectSewage();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SelectEraser();
    }




    private void HandleConstruction()
    {
        // Verifica se o clique começou sobre a interface
        if (Input.GetMouseButtonDown(0))
        {
            pointerStartedOverUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
        }

        // Impede construções iniciadas sobre a interface
        if (pointerStartedOverUI)
            return;

        // Impede construir enquanto o mouse estiver sobre a UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Só permite construir enquanto o botão estiver pressionado
        if (!Input.GetMouseButton(0))
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        // Converte a posição do mouse para o mundo
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            gridManager.transform.position.z -
            mainCamera.transform.position.z;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        // Identifica a célula selecionada
        Vector2Int gridPosition =
            gridManager.WorldToGridPosition(worldPosition);

        GridCell cell = gridManager.GetCell(gridPosition);

        if (cell == null)
            return;

        // Impede construir sobre estruturas e obstáculos
        if (!cell.CanBuildNetwork)
            return;

        if (eraseMode)
        {
            if (cell.Networks == NetworkType.None)
                return;

            // Calcula o reembolso
            int refund = budgetManager.GetCellCost(cell.Networks);

            // Remove todas as redes
            cell.ClearNetworks();

            // Devolve o dinheiro
            budgetManager.Refund(refund);
        }
        else
        {
            // Impede construir a mesma rede duas vezes
            if (cell.HasNetwork(selectedNetwork))
                return;

            // Calcula o custo da construção
            int cost = budgetManager.GetConstructionCost(
                cell,
                selectedNetwork
            );

            // Verifica o orçamento disponível
            if (!budgetManager.TrySpend(cost))
            {
                Debug.Log("Orçamento insuficiente!");
                return;
            }

            // Adiciona a rede
            cell.AddNetwork(selectedNetwork);
        }

        // Atualiza os elementos visuais
        gridRenderer.RefreshCell(gridPosition);
        gridRenderer.RefreshAllBuildings();
    }



    public void SelectWater()
    {
        selectedNetwork = NetworkType.Water;
        eraseMode = false;
    }

    public void SelectElectricity()
    {
        selectedNetwork = NetworkType.Electricity;
        eraseMode = false;
    }

    public void SelectSewage()
    {
        selectedNetwork = NetworkType.Sewage;
        eraseMode = false;
    }

    public void SelectEraser()
    {
        eraseMode = true;
    }


}
