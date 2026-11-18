using System;

[Serializable]
// Stores the settings passed from the menu into the game scene.
public class GameSettings
{
    // Number of tiles generated horizontally and vertically.
    public int boardWidth;
    public int boardHeight;

    // Selected difficulty level.
    public int difficulty;

    // Seed used to reproduce the same randomly generated board.
    public int seed;
}
