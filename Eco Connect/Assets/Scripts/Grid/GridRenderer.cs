using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridCellView cellPrefab;

    [SerializeField] private NetworkConnectivity connectivity;

    private GridCellView[,] cellViews;

    private void Start()
    {
        CreateGridVisual();
    }

    private void CreateGridVisual()
    {
        cellViews = new GridCellView[
            gridManager.Width,
            gridManager.Height
        ];

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                Vector3 worldPosition =
                    gridManager.GetCellWorldPosition(position);

                GridCellView view = Instantiate(
                    cellPrefab,
                    worldPosition,
                    Quaternion.identity,
                    transform
                );

                cellViews[x, y] = view;

                view.UpdateVisual(
                    gridManager.GetCell(position),
                    gridManager
                );

                view.UpdateBuildingStatus(
                gridManager.GetCell(position),
                connectivity
                );
            }
        }
    }


    public void RefreshCell(Vector2Int position)
    {
        RefreshSingleCell(position);

        RefreshSingleCell(position + Vector2Int.up);
        RefreshSingleCell(position + Vector2Int.down);
        RefreshSingleCell(position + Vector2Int.left);
        RefreshSingleCell(position + Vector2Int.right);
    }

    private void RefreshSingleCell(Vector2Int position)
    {
        GridCell cell = gridManager.GetCell(position);

        if (cell == null)
            return;

        cellViews[position.x, position.y].UpdateVisual(
            cell,
            gridManager
        );
    }


    public void RefreshAllBuildings()
    {
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                GridCell cell = gridManager.GetCell(position);

                if (cell.Structure != StructureType.Building)
                    continue;

                cellViews[x, y].UpdateBuildingStatus(
                    cell,
                    connectivity
                );
            }
        }
    }


}
