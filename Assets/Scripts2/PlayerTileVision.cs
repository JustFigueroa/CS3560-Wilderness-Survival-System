using WildernessGameplay;
using System.Collections.Generic;
using UnityEngine;

// Queries the session's terrain-based vision and authoritative grid position.
public class PlayerTileVision : MonoBehaviour
{
    public BoardManager boardManager;
    private Game Session => GameManager.Instance?.Session;

    // Refresh model vision instead of converting the offset player sprite back into grid coordinates.
    public void UpdateCurrentTilePosition()
    {
        Session?.RefreshVision();
    }

    // Return tile views selected by the player's current terrain-based vision.
    public List<TileData> GetVisibleTiles()
    {
        var result = new List<TileData>();
        var s = Session;
        if (s == null || boardManager == null)
        {
            return result;
        }

        for (int x = 0; x < s.Rules.boardWidth; x++)
        {
            for (int y = 0; y < s.Rules.boardHeight; y++)
            {
                if (s.Visible[x, y])
                {
                    result.Add(boardManager.GetTileData(x, y));
                }
            }
        }

        return result;
    }

    // Keep the original optional tile-reading helper; automatic action logs are produced by Game.
    public void ReadNearbyTiles()
    {
        foreach (var tile in GetVisibleTiles())
        {
            Debug.Log(tile.State.Terrain + " (" + tile.State.X + ", " + tile.State.Y + ") food " + tile.availableFoodAmount + ", water " + tile.availableWaterAmount);
        }
    }

    // Neighbor queries use the authoritative grid position and respect current visibility.
    private TileData At(int dx, int dy)
    {
        var s = Session;
        if (s == null || boardManager == null)
        {
            return null;
        }

        int x = s.Player.X + dx, y = s.Player.Y + dy;
        return s.IsInside(x, y) && s.Visible[x, y] ? boardManager.GetTileData(x, y) : null;
    }

    public TileData GetCurrentTile() => At(0, 0);
    public TileData GetTileToLeft() => At(-1, 0);
    public TileData GetTileToRight() => At(1, 0);
    public TileData GetTileAbove() => At(0, 1);
    public TileData GetTileBelow() => At(0, -1);
}
