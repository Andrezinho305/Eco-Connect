using System.Collections.Generic;
using UnityEngine;

// Lista de todos os elementos do jogo, na ordem em que aparecem na Legenda.
[CreateAssetMenu(
    fileName = "TileIntroCatalog",
    menuName = "EcoConnect/Tile Intro Catalog"
)]
public class TileIntroCatalog : ScriptableObject
{
    [SerializeField] private StructureVisualDatabase visualDatabase;

    [SerializeField]
    private List<TileIntroData> elements = new List<TileIntroData>();

    public StructureVisualDatabase VisualDatabase => visualDatabase;

    public Sprite GetIcon(TileIntroData element)
    {
        return element != null
            ? element.GetIcon(visualDatabase)
            : null;
    }

    // Elementos que existem no tabuleiro atual
    public List<TileIntroData> GetElementsPresent(GridManager grid)
    {
        List<TileIntroData> result = new List<TileIntroData>();

        foreach (TileIntroData element in elements)
        {
            if (element != null && HasAnyMatch(grid, element))
                result.Add(element);
        }

        return result;
    }

    public static bool HasAnyMatch(GridManager grid, TileIntroData element)
    {
        return GetMatchingCells(grid, element).Count > 0;
    }

    public static List<Vector2Int> GetMatchingCells(
        GridManager grid,
        TileIntroData element)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (grid == null || element == null)
            return cells;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (element.Matches(grid.GetCell(position)))
                    cells.Add(position);
            }
        }

        return cells;
    }
}
