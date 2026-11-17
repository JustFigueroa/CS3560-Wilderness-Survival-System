using UnityEngine;

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

    public int BoardWidth { get; private set; }
    public int BoardHeight { get; private set; }
    private GameObject spawnedPlayer;

    //Do not change pls
    [Header("Tile Size")]
    public float tileWidth = 5.11f;
    public float tileHeight = 5.11f;

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

        Random.InitState(settings.seed);

        SpawnBoard(
            settings.boardWidth,
            settings.boardHeight
        );
        SpawnPlayer(settings.boardHeight);
        BoardWidth = settings.boardWidth;
        BoardHeight = settings.boardHeight;

    }

    private void SpawnBoard(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int randomTileIndex =
                    Random.Range(0, tilePrefabs.Length);

                GameObject tilePrefab =
                    tilePrefabs[randomTileIndex];

                Vector3 spawnPosition = new Vector3(
                    x * tileWidth,
                    y * tileHeight,
                    0f
                );

                GameObject spawnedTile = Instantiate(
                    tilePrefab,
                    spawnPosition,
                    Quaternion.identity,
                    boardParent
                );

                TileData tileData =
                    spawnedTile.GetComponent<TileData>();

                if (tileData == null)
                {
                    Debug.LogError(
                        "Tile prefab is missing the TileData component.",
                        spawnedTile
                    );

                    continue;
                }

                tileData.GenerateResources();
            }
        }
    }
        private void SpawnPlayer(int boardHeight)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab has not been assigned.");
            return;
        }

        int randomY = Random.Range(0, boardHeight);

        Vector3 playerPosition = new Vector3(
            .5f,
            randomY * tileHeight + playerYPosition,
            -.1f
        );

        spawnedPlayer = Instantiate(
            playerPrefab,
            playerPosition,
            Quaternion.identity,
            playerParent
);
    }
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
}