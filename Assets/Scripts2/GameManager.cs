using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public GameSettings settings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    }

    public void StartGame(int boardWidth, int boardHeight, int difficulty, int seed)
    {
        settings.boardWidth = boardWidth;
        settings.boardHeight = boardHeight;
        settings.difficulty = difficulty;
        settings.seed = seed;

        SceneManager.LoadScene("Game");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
