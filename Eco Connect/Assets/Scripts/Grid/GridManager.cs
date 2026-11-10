
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;

    [SerializeField] private GridRenderer gridRenderer;

    public int Width => width;
    public int Height => height;

    private Vector2Int hoveredCell = new Vector2Int(-1, -1);

    private GridCell[,] cells;



    private void Awake()
    {
        cells = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new GridCell(
                    new Vector2Int(x, y)
                );
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellCenter = transform.position +
                    new Vector3(
                        x * cellSize + cellSize / 2f,
                        y * cellSize + cellSize / 2f,
                        0f
                    );

                Gizmos.DrawWireCube(
                    cellCenter,
                    new Vector3(cellSize, cellSize, 0f)
                );
            }
        }

        if (IsInsideGrid(hoveredCell))
        {
            Gizmos.color = Color.green;

            Vector3 hoveredCenter = transform.position +
                new Vector3(
                    hoveredCell.x * cellSize + cellSize / 2f,
                    hoveredCell.y * cellSize + cellSize / 2f,
                    0f
                );

            Gizmos.DrawCube(
                hoveredCenter,
                new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f)
            );
        }

    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;

        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int y = Mathf.FloorToInt(localPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    public bool IsInsideGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 &&
               gridPosition.x < width &&
               gridPosition.y >= 0 &&
               gridPosition.y < height;
    }

    public GridCell GetNeighbor(Vector2Int position, Vector2Int direction) //retorna a célula vizinha em determinada direção
    {
        Vector2Int neighborPosition = position + direction;

        return GetCell(neighborPosition);
    }

    public bool AreConnected(Vector2Int position, Vector2Int direction, NetworkType network) //verifica se duas células vizinhas possuem o mesmo tipo de rede
    {
        GridCell currentCell = GetCell(position);
        GridCell neighborCell = GetNeighbor(position, direction);

        if (currentCell == null || neighborCell == null)
            return false;

        return currentCell.HasNetwork(network) &&
               neighborCell.HasNetwork(network);
    }


    private void Update()
    {
        if (Camera.main == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2Int gridPosition =
            WorldToGridPosition(worldPosition);



        if (!IsInsideGrid(gridPosition))
        {
            hoveredCell = new Vector2Int(-1, -1);
            return;
        }

        hoveredCell = gridPosition;
    }


    public GridCell GetCell(Vector2Int position)
    {
        if (!IsInsideGrid(position))
        {
            return null;
        }

        return cells[position.x, position.y];
    }


    public Vector3 GetCellWorldPosition(Vector2Int position)
    {
        return transform.position + new Vector3(
            (position.x + 0.5f) * cellSize,
            (position.y + 0.5f) * cellSize,
            0f
        );
    }


}

