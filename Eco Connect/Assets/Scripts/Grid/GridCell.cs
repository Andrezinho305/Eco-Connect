
using UnityEngine;

public class GridCell
{
    public Vector2Int Position { get; private set; }
    public NetworkType Networks { get; private set; }


    public GridCell(Vector2Int position)
    {
        Position = position;
        Networks = NetworkType.None;
    }

    public bool HasNetwork(NetworkType type)
    {
        return (Networks & type) == type;
    }

    public void AddNetwork(NetworkType type)
    {
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



}
