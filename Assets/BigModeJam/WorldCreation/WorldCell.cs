using System.Collections.Generic;
using UnityEngine;

public class WorldCell : MonoBehaviour
{
    public int OptionNumber;

    public bool Collapsed;
    public WorldTile[] TileOptions;
    public bool ContainsEmptyTile;

    public void CreateCell(bool collapseState, WorldTile[] tiles)
    {
        Collapsed = collapseState;
        TileOptions = tiles;
    }

    public void RecreateCell(WorldTile[] tiles)
    {
        TileOptions = tiles;
        OptionNumber = TileOptions.Length;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, .8f);
    }
}
