using UnityEngine;

public class GridCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    //[SerializeField] private SpriteRenderer waterIndicator; //indicador teste
    //[SerializeField] private SpriteRenderer electricityIndicator; //indicador teste
    //[SerializeField] private SpriteRenderer sewageIndicator; //indicador teste


    [SerializeField] private NetworkConnectionView waterConnections;
    [SerializeField] private NetworkConnectionView electricityConnections;
    [SerializeField] private NetworkConnectionView sewageConnections;

    [SerializeField] private SpriteRenderer structureRenderer;
    [SerializeField] private BuildingStatusView buildingStatusView;

    [Header("Artes das estruturas")]
    [SerializeField]
    private StructureVisualDatabase visualDatabase;

    public void UpdateVisual(GridCell cell, GridManager grid)
    {
        UpdateNetworkConnections(
            cell, grid, NetworkType.Water, waterConnections
        );

        UpdateNetworkConnections(
            cell, grid, NetworkType.Electricity, electricityConnections
        );

        UpdateNetworkConnections(
            cell, grid, NetworkType.Sewage, sewageConnections
        );

        UpdateStructureVisual(cell);
    }



    private void UpdateNetworkConnections(
        GridCell cell,
        GridManager grid,
        NetworkType network,
        NetworkConnectionView connectionView)
    {
        Vector2Int position = cell.Position;

        // Verifica se existe uma rede construída nesta célula
        bool hasNetwork = cell.HasNetwork(network);

        // Verifica se os vizinhos aceitam conexão
        GridCell upCell = grid.GetCell(
            position + Vector2Int.up
        );

        GridCell downCell = grid.GetCell(
            position + Vector2Int.down
        );

        GridCell leftCell = grid.GetCell(
            position + Vector2Int.left
        );

        GridCell rightCell = grid.GetCell(
            position + Vector2Int.right
        );

        bool up = upCell != null &&
                  upCell.AcceptsNetworkConnection(network);

        bool down = downCell != null &&
                    downCell.AcceptsNetworkConnection(network);

        bool left = leftCell != null &&
                    leftCell.AcceptsNetworkConnection(network);

        bool right = rightCell != null &&
                     rightCell.AcceptsNetworkConnection(network);

        // Atualiza os segmentos visuais
        connectionView.SetConnections(
            hasNetwork,
            up,
            down,
            left,
            right
        );
    }




    private void UpdateStructureVisual(GridCell cell)
    {
        if (cell.Structure == StructureType.None)
        {
            structureRenderer.enabled = false;
            return;
        }

        if (visualDatabase == null)
        {
            Debug.LogWarning(
                "StructureVisualDatabase não configurado!"
            );

            structureRenderer.enabled = false;
            return;
        }

        Sprite sprite = visualDatabase.GetSprite(
            cell.Structure,
            cell.VisualVariant
        );

        if (sprite == null)
        {
            structureRenderer.enabled = false;
            return;
        }

        structureRenderer.enabled = true;
        structureRenderer.sprite = sprite;
        structureRenderer.color = Color.white;
    }



    public void UpdateBuildingStatus(
        GridCell cell,
        NetworkConnectivity connectivity)
    {
        if (cell.Structure != StructureType.Building)
        {
            buildingStatusView.gameObject.SetActive(false);
            return;
        }

        buildingStatusView.gameObject.SetActive(true);

        buildingStatusView.UpdateStatus(cell, connectivity);
    }


}
