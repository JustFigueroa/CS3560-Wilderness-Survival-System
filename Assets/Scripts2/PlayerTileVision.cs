using UnityEngine;

// Allows the player to inspect the TileData for the tile it occupies and the
// four directly adjacent tiles.
public class PlayerTileVision : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;

    // Grid coordinates of the tile currently occupied by the player.
    private int currentTileX;
    private int currentTileY;

    // Finds the BoardManager, determines the player's starting tile, and
    // reads the surrounding tiles once at startup.
    private void Start()
    {
        if (boardManager == null)
        {
            boardManager =
                FindObjectOfType<BoardManager>();
        }

        if (boardManager == null)
        {
            Debug.LogError("BoardManager was not found.");
            return;
        }

        UpdateCurrentTilePosition();
        ReadNearbyTiles();
    }

    // Converts the player's world position into board grid coordinates.
    // This should be called again whenever the player moves to another tile.
    public void UpdateCurrentTilePosition()
    {
        currentTileX = Mathf.RoundToInt(transform.position.x / boardManager.tileWidth);

        currentTileY = Mathf.RoundToInt(transform.position.y / boardManager.tileHeight);
    }

    // Reads the current tile plus the four cardinal neighboring tiles.
    // Diagonal tiles are not included in the current implementation.
    public void ReadNearbyTiles()
    {
        Debug.Log("Current tile:");

        ReadTile(currentTileX, currentTileY);

        Debug.Log("Left tile:");

        ReadTile(currentTileX - 1, currentTileY);

        Debug.Log("Right tile:");

        ReadTile(currentTileX + 1, currentTileY);

        Debug.Log("Tile above:");

        ReadTile(currentTileX, currentTileY + 1);

        Debug.Log("Tile below:");

        ReadTile(currentTileX, currentTileY - 1);
    }

    // Retrieves and displays one tile's stored resource information.
    private void ReadTile(int x, int y)
    {
        TileData tile =
            boardManager.GetTileData(x, y);

        if (tile == null)
        {
            Debug.Log("No tile exists at position (" + x + ", " + y + ").");

            return;
        }

        Debug.Log("Tile (" + x + ", " + y + ")" + " | Available Food: " + tile.availableFoodAmount + " | Available Water: " + tile.availableWaterAmount + " | Available Gold: " + tile.availableGoldAmount + " | Food Cost: " + tile.costFoodAmount + " | Water Cost: " + tile.costWaterAmount);
    }

    // Returns the data for the tile currently occupied by the player.
    public TileData GetCurrentTile()
    {
        return boardManager.GetTileData(currentTileX, currentTileY);
    }

    // Returns the data for the tile immediately to the player's left.
    public TileData GetTileToLeft()
    {
        return boardManager.GetTileData(currentTileX - 1, currentTileY);
    }

    // Returns the data for the tile immediately to the player's right.
    public TileData GetTileToRight()
    {
        return boardManager.GetTileData(currentTileX + 1, currentTileY);
    }

    // Returns the data for the tile immediately above the player.
    public TileData GetTileAbove()
    {
        return boardManager.GetTileData(currentTileX, currentTileY + 1);
    }

    // Returns the data for the tile immediately below the player.
    public TileData GetTileBelow()
    {
        return boardManager.GetTileData(currentTileX, currentTileY - 1);
    }
}
