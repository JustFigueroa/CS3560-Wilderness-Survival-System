using WildernessGameplay;
using UnityEngine;
using UnityEngine.UI;

// Updates the standard Canvas UI from the current player's game state.
public class GameHUD : MonoBehaviour
{
    // These references must be serialized so the pre-placed Canvas objects can be assigned in the scene.
    [SerializeField] private Text strengthText;
    [SerializeField] private Text foodText;
    [SerializeField] private Text waterText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text visionText;
    [SerializeField] private Text autoplayText;
    [SerializeField] private Button autoplayButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private Text messageText;
    [SerializeField] private GameObject outcomePanel;
    [SerializeField] private Text outcomeText;

    [SerializeField] private Slider zoomSlider;
    [SerializeField] private BoardCamera boardCamera;

    private void Start()
    {
        zoomSlider.SetValueWithoutNotify(boardCamera.playerZoom);
    }

    // Read the model and update existing labels/buttons; no UI objects are built by this script.
    private void Update()
    {
        var manager = GameManager.Instance;
        if (manager == null || manager.Session == null) return;

        zoomSlider.interactable = manager.Board != null && !manager.Board.OverviewActive;
        var game = manager.Session;
        var player = game.Player;
        // Resource names are printed on Player_HUD_1; these scene labels contain only the live numbers.
        strengthText.text = player.currentStrength.ToString();
        foodText.text = player.currentFood.ToString();
        waterText.text = player.currentWater.ToString();
        goldText.text = player.currentGold.ToString();
        visionText.text = "Vision: " + game.CurrentVision;
        messageText.text = game.Message;
        bool ended = game.Phase != SessionPhase.Running;
        outcomePanel.SetActive(ended);
        if (ended) outcomeText.text = game.Phase == SessionPhase.Won ? "You win!" : "You lose!";
        autoplayText.text = manager.paused ? "Autoplay" : "Pause";
        autoplayButton.interactable = game.Phase == SessionPhase.Running;
        nextButton.interactable = manager.CanStep;
    }

    public void ToggleAutoplay()
    {
        var manager = GameManager.Instance;
        manager.SetPaused(!manager.paused);
    }

    public void Next() => GameManager.Instance.Step();
    public void Restart() => GameManager.Instance.Restart();
    public void Menu() => GameManager.Instance.ReturnToMenu();
}
