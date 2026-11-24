using UnityEngine;
using UnityEngine.UI;

// Handles menu button actions and passes the selected settings to the persistent GameManager.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private DifficultyLevel selectedDifficulty = DifficultyLevel.Easy;
    [SerializeField] private BrainMode selectedBrain = BrainMode.ResourceAware;
    [SerializeField] private Text difficultyLabel;
    [SerializeField] private Text brainLabel;

    // Restore the previous menu selection when returning from a game.
    private void Start()
    {
        var settings = GameManager.EnsureInstance().settings;
        if (settings != null)
        {
            selectedDifficulty = settings.difficulty;
            selectedBrain = settings.difficultyValues.brain;
        }
        UpdateLabels();
    }

    // Start the selected difficulty with a new random seed for board generation.
    public void StartGame()
    {
        GameManager.Instance.StartGame(selectedDifficulty, Random.Range(0, int.MaxValue), selectedBrain);
    }

    public void PlayButtonPressed() => StartGame();

    // Cycle the original Easy/Medium/Hard menu choice and update the button label.
    public void DifficultyButtonPressed()
    {
        selectedDifficulty = (DifficultyLevel)((int)selectedDifficulty % 3 + 1);
        UpdateLabels();
    }

    // Switch between the two brain strategies; vision is picked at random per game and terrain adjusts its range.
    public void BrainButtonPressed()
    {
        selectedBrain = selectedBrain == BrainMode.ResourceAware ? BrainMode.EastFirst : BrainMode.ResourceAware;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        difficultyLabel.text = "Difficulty: " + selectedDifficulty;
        brainLabel.text = "Brain: " + (selectedBrain == BrainMode.ResourceAware ? "Resource Aware" : "East First");
    }
}
