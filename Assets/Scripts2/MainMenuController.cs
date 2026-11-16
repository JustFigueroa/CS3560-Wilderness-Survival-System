using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        int boardWidth = 10;
        int boardHeight = 10;
        int difficulty = 1;
        int seed = Random.Range(0, 100000);

        GameManager.Instance.StartGame(
            boardWidth,
            boardHeight,
            difficulty,
            seed);
    }
    public void PlayButtonPressed()
    {
        Debug.Log("Play button pressed.");

        GameManager.Instance.StartGame(
            10,
            10,
            1,
            Random.Range(0, 100000));
    }
}
