using UnityEngine;
using UnityEngine.UI;

// Handles menu button actions and passes the selected game settings to the
// persistent GameManager.
public class MainMenuController : MonoBehaviour
{
    // Current difficulty selected in the menu.
    int difficulty = 1;

    // Starts a game using the currently selected difficulty and a new random
    // seed for board generation.
    public void StartGame()
    {
        int seed = Random.Range(0, 100000);
        GameManager.Instance.StartGame(difficulty, seed);
    }

    // Starts a game using difficulty level 3 and a new random seed.
    public void PlayButtonPressed()
    {
        GameManager.Instance.StartGame(3, Random.Range(0, 100000));
    }

    // Cycles through the available difficulty levels and updates the button
    // label to display the current selection.
    public void DifficultyButtonPressed()
    {
        if (difficulty < 3)
        {
            GameManager.Instance.settings.difficulty++;
            difficulty++;
        }
        else if (difficulty == 3)
        {
            GameManager.Instance.settings.difficulty = 1;
            difficulty = 1;
        }

        GameObject.Find("DifficultyButton").GetComponentInChildren<Text>().text = "Difficulty " + difficulty;
    }
}
