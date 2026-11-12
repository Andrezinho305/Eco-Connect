
using UnityEngine;

public class GridCell
{
    public Vector2Int Position { get; private set; }
    public NetworkType Networks { get; private set; }

    public StructureType Structure { get; private set; }
    public NetworkType RequiredServices { get; private set; }


    public bool CanBuildNetwork => Structure == StructureType.None;


    public GridCell(Vector2Int position) //construtor
    {
        Position = position;
        Networks = NetworkType.None;
        Structure = StructureType.None;
        RequiredServices = NetworkType.None;
    }

    public bool HasNetwork(NetworkType type)
    {
        return (Networks & type) == type;
    }

    public void AddNetwork(NetworkType type)
    {
        if (!CanBuildNetwork)
            return;

        Networks |= type;
    }

    public void RemoveNetwork(NetworkType type)
    {
        Networks &= ~type;
    }

    public void ClearNetworks()
    {
        Networks = NetworkType.None;
    }

    public void SetStructure(StructureType structure)
    {
        Structure = structure;
        RequiredServices = NetworkType.None;

        if (structure != StructureType.None)
        {
            ClearNetworks();
        }
    }

    public void SetRequiredServices(NetworkType services)
    {
        if (Structure != StructureType.Building)
            return;

        RequiredServices = services;
    }


}
