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


}
