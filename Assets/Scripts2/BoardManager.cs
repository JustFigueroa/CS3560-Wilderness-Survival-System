using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject[] tilePrefabs;

    [Header("Board Organization")]
    public Transform boardParent;

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
}