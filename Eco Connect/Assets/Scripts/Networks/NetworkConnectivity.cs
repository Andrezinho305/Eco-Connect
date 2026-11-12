using System.Collections.Generic;
using UnityEngine;

public class NetworkConnectivity : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    [SerializeField] private Vector2Int testStartPosition = new Vector2Int(0, 0);
    [SerializeField] private NetworkType testNetwork = NetworkType.Water;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestBuildingSupply();
        }
    }


    private readonly Vector2Int[] directions = //array contém as quatro direções válidas de movimentação pelo tabuleiro
    {
    Vector2Int.up,
    Vector2Int.down,
    Vector2Int.left,
    Vector2Int.right
    };


    public HashSet<Vector2Int> FindConnectedCells(
        Vector2Int startPosition,
        NetworkType network)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        GridCell startCell = gridManager.GetCell(startPosition);

        if (startCell == null || !startCell.HasNetwork(network))
            return visited;

        visited.Add(startPosition);
        queue.Enqueue(startPosition);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPosition = current + direction;

                GridCell neighbor = gridManager.GetCell(neighborPosition);

                if (neighbor == null)
                    continue;

                if (!neighbor.HasNetwork(network))
                    continue;

                if (visited.Contains(neighborPosition))
                    continue;

                visited.Add(neighborPosition);
                queue.Enqueue(neighborPosition);
            }
        }

        return visited;
    }


    private StructureType GetSourceType(NetworkType network)
    {
        switch (network)
        {
            case NetworkType.Water:
                return StructureType.WaterSource;

            case NetworkType.Electricity:
                return StructureType.PowerSource;

            case NetworkType.Sewage:
                return StructureType.SewageTreatment;

            default:
                return StructureType.None;
        }
    }


    public HashSet<Vector2Int> FindSuppliedCells(NetworkType network)
    {
        HashSet<Vector2Int> suppliedCells = new HashSet<Vector2Int>();

        StructureType sourceType = GetSourceType(network);

        if (sourceType == StructureType.None)
            return suppliedCells;

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GridCell cell = gridManager.GetCell(position);

                if (cell.Structure != sourceType)
                    continue;

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int neighborPosition = position + direction;
                    GridCell neighbor = gridManager.GetCell(neighborPosition);

                    if (neighbor == null || !neighbor.HasNetwork(network))
                        continue;

                    HashSet<Vector2Int> connected =
                        FindConnectedCells(neighborPosition, network);

                    suppliedCells.UnionWith(connected);
                }
            }
        }

        return suppliedCells;
    }


    public bool IsBuildingSupplied(Vector2Int buildingPosition, NetworkType network)
    {
        GridCell building = gridManager.GetCell(buildingPosition);

        if (building == null ||
            building.Structure != StructureType.Building)
        {
            return false;
        }

        HashSet<Vector2Int> suppliedCells =
            FindSuppliedCells(network);

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition =
                buildingPosition + direction;

            if (suppliedCells.Contains(neighborPosition))
            {
                return true;
            }
        }

        return false;
    }


    public bool IsBuildingFullySupplied(Vector2Int buildingPosition)
    {
        GridCell building = gridManager.GetCell(buildingPosition);

        if (building == null ||
            building.Structure != StructureType.Building)
        {
            return false;
        }

        NetworkType required = building.RequiredServices;

        if (required == NetworkType.None)
            return false;

        if ((required & NetworkType.Water) != 0 &&
            !IsBuildingSupplied(buildingPosition, NetworkType.Water))
        {
            return false;
        }

        if ((required & NetworkType.Electricity) != 0 &&
            !IsBuildingSupplied(buildingPosition, NetworkType.Electricity))
        {
            return false;
        }

        if ((required & NetworkType.Sewage) != 0 &&
            !IsBuildingSupplied(buildingPosition, NetworkType.Sewage))
        {
            return false;
        }

        return true;
    }



    [ContextMenu("Testar Conectividade")] //permite executar um método diretamente pelo inspector
    private void TestConnectivity()
    {
        HashSet<Vector2Int> connectedCells =
            FindConnectedCells(testStartPosition, testNetwork);

        Debug.Log(
            "Quantidade de células conectadas: " +
            connectedCells.Count
        );

        foreach (Vector2Int position in connectedCells)
        {
            Debug.Log("Célula conectada: " + position);
        }
    }


    [ContextMenu("Testar Abastecimento")]
    private void TestBuildingSupply()
    {
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GridCell cell = gridManager.GetCell(position);

                if (cell.Structure != StructureType.Building)
                    continue;

                bool supplied = IsBuildingFullySupplied(position);

                Debug.Log(
                    "Edificação " + position +
                    " | Serviços necessários: " + cell.RequiredServices +
                    " | Totalmente atendida: " + supplied
                );
            }
        }
    }


}

