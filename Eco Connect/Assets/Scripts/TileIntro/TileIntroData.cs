using UnityEngine;

// Descreve um elemento do tabuleiro (fonte, casa, obstáculo...)
// para o card "Novo elemento!" e para a Legenda.
[CreateAssetMenu(
    fileName = "NewTileIntro",
    menuName = "EcoConnect/Tile Intro"
)]
public class TileIntroData : ScriptableObject
{
    [Header("Texto")]
    public string title = "Novo elemento";

    [TextArea(3, 6)]
    public string description;

    [Tooltip("Opcional. Se vazio, usa o sprite do StructureVisualDatabase.")]
    public Sprite iconOverride;

    [Header("Quais células representam este elemento")]
    public StructureType structureType = StructureType.Obstacle;

    [Tooltip("Se marcado, só considera células com a variante visual abaixo")]
    public bool matchVariant;

    [Min(0)]
    public int visualVariant;

    [Tooltip("Só considera casas que precisam de 2 ou mais serviços")]
    public bool onlyMultiServiceBuildings;


    public bool Matches(GridCell cell)
    {
        if (cell == null || cell.Structure != structureType)
            return false;

        if (matchVariant && cell.VisualVariant != visualVariant)
            return false;

        if (onlyMultiServiceBuildings &&
            CountServices(cell.RequiredServices) < 2)
            return false;

        return true;
    }

    public Sprite GetIcon(StructureVisualDatabase database)
    {
        if (iconOverride != null)
            return iconOverride;

        if (database == null)
            return null;

        return database.GetSprite(structureType, visualVariant);
    }

    private static int CountServices(NetworkType services)
    {
        int count = 0;

        if ((services & NetworkType.Water) != 0) count++;
        if ((services & NetworkType.Electricity) != 0) count++;
        if ((services & NetworkType.Sewage) != 0) count++;

        return count;
    }
}
