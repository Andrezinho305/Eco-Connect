using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class NetworkBuilder : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridRenderer gridRenderer;

    [SerializeField] private BudgetManager budgetManager;

    [SerializeField] private PauseManager pauseManager;

    private NetworkType selectedNetwork = NetworkType.Water;

    private bool eraseMode = false;

    private bool pointerStartedOverUI;

    // Ferramentas liberadas (o tutorial libera aos poucos)
    private ToolMask allowedTools = ToolMask.All;

    // Falso quando nenhuma ferramenta está selecionada
    private bool hasToolSelected = true;

    public NetworkType SelectedNetwork => selectedNetwork;

    public bool IsEraseMode => eraseMode;

    public bool HasToolSelected => hasToolSelected;

    public ToolMask AllowedTools => allowedTools;

    public ToolMask CurrentTool =>
        eraseMode ? ToolMask.Eraser : (ToolMask)(int)selectedNetwork;

    // Verdadeiro se a ferramenta atual pode ser usada para construir/apagar
    public bool CanUseCurrentTool =>
        hasToolSelected && IsToolAllowed(CurrentTool);

    public event Action OnToolChanged;

    // Disparado após construir uma rede (posição, rede construída)
    public event Action<Vector2Int, NetworkType> OnNetworkBuilt;

    // Disparado após apagar as redes de uma célula
    public event Action<Vector2Int> OnNetworkErased;


    private void Update()
    {
        if (pauseManager != null && pauseManager.IsPaused)
            return;

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
        if (levelManager.IsLevelCompleted)
            return;

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

        // Impede usar ferramentas não selecionadas ou bloqueadas
        if (!CanUseCurrentTool)
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

        levelManager.CheckLevelCompletion();

        // Notifica quem estiver acompanhando (ex.: tutorial)
        if (eraseMode)
            OnNetworkErased?.Invoke(gridPosition);
        else
            OnNetworkBuilt?.Invoke(gridPosition, selectedNetwork);
    }




    public void SelectWater()
    {
        SelectNetwork(NetworkType.Water);
    }

    public void SelectElectricity()
    {
        SelectNetwork(NetworkType.Electricity);
    }

    public void SelectSewage()
    {
        SelectNetwork(NetworkType.Sewage);
    }

    public void SelectEraser()
    {
        if (!IsToolAllowed(ToolMask.Eraser))
            return;

        eraseMode = true;
        hasToolSelected = true;

        OnToolChanged?.Invoke();
    }

    private void SelectNetwork(NetworkType network)
    {
        if (!IsToolAllowed((ToolMask)(int)network))
            return;

        selectedNetwork = network;
        eraseMode = false;
        hasToolSelected = true;

        OnToolChanged?.Invoke();
    }


    public bool IsToolAllowed(ToolMask tool)
    {
        return tool != ToolMask.None &&
               (allowedTools & tool) == tool;
    }

    // Define quais ferramentas podem ser usadas.
    // Se a ferramenta atual for bloqueada, ela é desmarcada.
    public void SetAllowedTools(ToolMask tools)
    {
        allowedTools = tools;

        if (hasToolSelected && !IsToolAllowed(CurrentTool))
            hasToolSelected = false;

        OnToolChanged?.Invoke();
    }

    // Deixa o jogador sem nenhuma ferramenta selecionada
    public void DeselectTool()
    {
        hasToolSelected = false;

        OnToolChanged?.Invoke();
    }



}
