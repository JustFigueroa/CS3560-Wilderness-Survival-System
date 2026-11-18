using UnityEngine;

// Creates the board, stores each tile's data, and places the player.
public class BoardManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject[] tilePrefabs;

    [Header("Player")]
    public GameObject playerPrefab;
    public Transform playerParent;
    public float playerYPosition = 1f;

    [Header("Board Organization")]
    public Transform boardParent;

    [Header("Tile Size")]
    public float tileWidth = 5.11f;
    public float tileHeight = 5.11f;

    public int BoardWidth { get; private set; }
    public int BoardHeight { get; private set; }

    // Stores tile data using the tile's grid coordinates:
    // tileDataGrid[x, y]
    private TileData[,] tileDataGrid;

    // Reference to the player created by this manager.
    private GameObject spawnedPlayer;

    // Allows other scripts, such as BoardCamera, to access the spawned player.
    public Transform PlayerTransform
    {
        get
        {
            if (spawnedPlayer == null)
            {
                return null;
            }

            return spawnedPlayer.transform;
        }
    }

    // Initializes the board using the settings created by GameManager.
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager was not found.");
            return;
        }

        if (GameManager.Instance.settings == null)
        {
            Debug.LogError("GameSettings was not initialized.");
            return;
        }

        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogError("No tile prefabs have been assigned.");
            return;
        }

        GameSettings settings = GameManager.Instance.settings;

        BoardWidth = settings.boardWidth;
        BoardHeight = settings.boardHeight;

        // Using the game seed makes the board generation repeatable for the
        // same game settings.
        Random.InitState(settings.seed);

        SpawnBoard(BoardWidth, BoardHeight);
        SpawnPlayer(BoardHeight);
    }

    // Creates every tile, generates its resources, and stores its TileData in
    // the grid so the player and other systems can query it later.
    private void SpawnBoard(int width, int height)
    {
        tileDataGrid = new TileData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Choose one of the available biome/tile prefabs.
                int randomTileIndex = Random.Range(0, tilePrefabs.Length);

                GameObject tilePrefab =
                    tilePrefabs[randomTileIndex];

                Vector3 spawnPosition = new Vector3(x * tileWidth, y * tileHeight, 0f);

                // Create the tile at its grid-based world position.
                GameObject spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, boardParent);

                TileData tileData =
                    spawnedTile.GetComponent<TileData>();

                if (tileData == null)
                {
                    Debug.LogError(
                        "Tile prefab is missing a TileData component.",
                        spawnedTile
                    );

                    continue;
                }

                // Generate the resources and activate the appropriate token
                // objects on this tile.
                tileData.GenerateResources();

                // Store this tile's data at its grid position.
                tileDataGrid[x, y] = tileData;
            }
        }
    }

    // Places the player on a randomly selected tile in the leftmost column.
    private void SpawnPlayer(int boardHeight)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab has not been assigned.");
            return;
        }

        // The leftmost column is x = 0. Only the y coordinate is randomized.
        int randomY = Random.Range(0, boardHeight);

        Vector3 playerPosition = new Vector3(0f, randomY * tileHeight + playerYPosition, 0f);

        // Store the created player so other systems can access its Transform.
        spawnedPlayer = Instantiate(playerPrefab, playerPosition, Quaternion.identity, playerParent);
    }

    // Returns the TileData at the requested grid position.
    // Returns null when the position is outside the board or the grid has not
    // been initialized yet.
    public TileData GetTileData(int x, int y)
    {
        if (tileDataGrid == null)
        {
            return null;
        }

        if (x < 0 || x >= BoardWidth ||
            y < 0 || y >= BoardHeight)
        {
            return null;
        }

        return tileDataGrid[x, y];
    }
}
