using UnityEngine;
using UnityEngine.SceneManagement;

// Persists between scenes and stores the settings used to generate a game.
public class GameManager : MonoBehaviour
{
    // Singleton reference used by the menu and board systems.
    public static GameManager Instance { get; private set; }

    // Settings shared between the menu scene and the game scene.
    public GameSettings settings;

    // Creates the singleton and initializes the default settings.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        settings = new GameSettings();
        settings.difficulty = 1;
    }

    // Saves the selected seed, determines the board dimensions from the
    // difficulty, and loads the game scene.
    public void StartGame(int difficulty, int seed)
    {
        settings.seed = seed;

        if (settings.difficulty == 1)
        {
            Debug.Log("difficulty: " + settings.difficulty);
            settings.boardWidth = 10;
            settings.boardHeight = 10;
        }
        else if (difficulty == 2)
        {
            Debug.Log("difficulty: " + settings.difficulty);
            settings.boardWidth = 25;
            settings.boardHeight = 25;
        }
        else if (difficulty == 3)
        {
            Debug.Log("difficulty: " + settings.difficulty);
            settings.boardWidth = 50;
            settings.boardHeight = 50;
        }

        SceneManager.LoadScene("Game");
    }

    // Reserved for future game-wide update logic.
    void Update()
    {
    }
}
