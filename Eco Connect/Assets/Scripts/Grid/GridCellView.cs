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

        bool hasNetwork = cell.HasNetwork(network);

        bool up = grid.AreConnected(
            position, Vector2Int.up, network);

        bool down = grid.AreConnected(
            position, Vector2Int.down, network);

        bool left = grid.AreConnected(
            position, Vector2Int.left, network);

        bool right = grid.AreConnected(
            position, Vector2Int.right, network);

        connectionView.SetConnections(
            hasNetwork, up, down, left, right
        );
    }


    private void UpdateStructureVisual(GridCell cell)
    {
        if (cell.Structure == StructureType.None)
        {
            structureRenderer.enabled = false;
            return;
        }

        structureRenderer.enabled = true;

        switch (cell.Structure)
        {
            case StructureType.WaterSource:
                structureRenderer.color = Color.blue;
                break;

            case StructureType.PowerSource:
                structureRenderer.color = Color.yellow;
                break;

            case StructureType.SewageTreatment:
                structureRenderer.color = Color.magenta;
                break;

            case StructureType.Building:
                structureRenderer.color = Color.white;
                break;

            case StructureType.Obstacle:
                structureRenderer.color = Color.black;
                break;
        }
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
