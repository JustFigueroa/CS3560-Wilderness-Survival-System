using WildernessGameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persists between scenes and stores the settings used to generate a game.
public class GameManager : MonoBehaviour
{
    // Singleton reference used by the menu and board systems.
    public static GameManager Instance { get; private set; }

    [Header("Difficulty Presets")]
    [SerializeField]
    private DifficultyPreset easyPreset = DifficultyPreset.Create(DifficultyLevel.Easy);
    [SerializeField]
    private DifficultyPreset mediumPreset = DifficultyPreset.Create(DifficultyLevel.Medium);
    [SerializeField]
    private DifficultyPreset hardPreset = DifficultyPreset.Create(DifficultyLevel.Hard);
    private const float TurnInterval = 2f;
    public bool paused { get; private set; } = true;
    // Settings shared between the menu scene and the game scene.
    public GameSettings settings { get; private set; }
    public Game Session { get; private set; }
    public BoardManager Board { get; private set; }

    private float nextTurn;
    private bool ReadyToAct => Board != null && Session != null && Session.Phase == SessionPhase.Running &&
        !Board.OverviewActive;
    public bool CanStep => paused && ReadyToAct;

    // Keep the original persistent manager; discard the duplicate already placed in the next scene.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static GameManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        throw new System.InvalidOperationException("The scene needs an enabled GameManager object.");
    }

    // Copy the selected Inspector preset and optional brain choice without starting gameplay.
    public bool Configure(DifficultyLevel difficulty, int seed, BrainMode? brain = null)
    {
        var preset = GetPreset(difficulty);
        if (!ValidatePreset(preset, difficulty, out string error))
        {
            Debug.LogError(error, this);
            return false;
        }

        settings = new GameSettings(preset, seed);
        if (brain.HasValue)
        {
            settings.difficultyValues.brain = brain.Value;
        }


        Session = null;
        Board = null;
        paused = true;
        return true;
    }

    // Save the selected settings and seed, then load the existing Game scene.
    public void StartGame(DifficultyLevel difficulty, int seed, BrainMode? brain = null)
    {
        if (Configure(difficulty, seed, brain))
        {
            SceneManager.LoadScene("Game");
        }
    }

    // Create the plain C# game and connect its event messages to Unity's console.
    public void BeginBoard(BoardManager board)
    {
        if (settings == null && !Configure(DifficultyLevel.Easy, Random.Range(0, int.MaxValue)))
        {
            return;
        }

        Session = WildernessGameplay.GameFactory.CreateSession(settings);
        Session.EventReported += LogGameEvent;
        Board = board;
        paused = true;
        nextTurn = Time.unscaledTime + TurnInterval;
    }

    private void LogGameEvent(string message) => Debug.Log(message);

    public void FinishOverview()
    {
        nextTurn = Time.unscaledTime + TurnInterval;
    }

    private void Update()
    {
        if (!paused && ReadyToAct && Time.unscaledTime >= nextTurn)
        {
            AdvanceTurn();
        }
    }

    public void SetPaused(bool value)
    {
        paused = value || Session == null || Session.Phase != SessionPhase.Running;
        if (!paused)
        {
            nextTurn = Time.unscaledTime + TurnInterval;
        }
    }

    // The HUD's Next button advances exactly one action while autoplay is paused.
    public void Step()
    {
        if (CanStep)
        {
            AdvanceTurn();
        }
    }

    // Run one model action and then refresh the already-existing scene views.
    private void AdvanceTurn()
    {
        bool acted = Session.PlayTurn();
        if (!acted || Session.Phase != SessionPhase.Running) paused = true;
        Board.RefreshViews();
        nextTurn = Time.unscaledTime + TurnInterval;
    }

    // Restart with the same difficulty/brain and a fresh random seed.
    public void Restart()
    {
        if (settings == null)
        {
            return;
        }

        settings.seed = Random.Range(0, int.MaxValue);
        Session = null;
        Board = null;
        paused = true;
        SceneManager.LoadScene("Game");
    }

    // Release the running game while keeping the user's selection for the menu.
    public void ReturnToMenu()
    {
        Session = null;
        Board = null;
        paused = true;
        SceneManager.LoadScene("MainMenu");
    }

    private DifficultyPreset GetPreset(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                return easyPreset;
            case DifficultyLevel.Medium:
                return mediumPreset;
            case DifficultyLevel.Hard:
                return hardPreset;
            default:
                return null;
        }
    }

    private static bool ValidatePreset(DifficultyPreset preset, DifficultyLevel expected, out string error)
    {
        if (preset == null || preset.level != expected)
        {
            error = "Missing or mismatched " + expected + " preset.";
            return false;
        }

        return preset.TryValidate(out error);
    }

}
