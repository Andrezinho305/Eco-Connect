using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private NetworkConnectivity connectivity;

    public event Action OnLevelCompleted;
    public event Action<int, int> OnProgressChanged;

    public bool IsLevelCompleted { get; private set; }

    public int TotalBuildings { get; private set; }
    public int SuppliedBuildings { get; private set; }


    private void Start()
    {
        CheckLevelCompletion();
    }


    public void CheckLevelCompletion()
    {
        if (IsLevelCompleted)
            return;

        int total = 0;
        int supplied = 0;

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                GridCell cell = gridManager.GetCell(position);

                if (cell == null)
                    continue;

                if (cell.Structure != StructureType.Building)
                    continue;

                total++;

                if (connectivity.IsBuildingFullySupplied(position))
                    supplied++;
            }
        }

        TotalBuildings = total;
        SuppliedBuildings = supplied;

        OnProgressChanged?.Invoke(supplied, total);

        if (total > 0 && supplied == total)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        IsLevelCompleted = true;

        Debug.Log("Fase concluída!");

        OnLevelCompleted?.Invoke();
    }
}

