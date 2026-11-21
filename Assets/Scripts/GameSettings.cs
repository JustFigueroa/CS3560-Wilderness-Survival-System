using System;

// Configuration for one game. Current resources belong to the player's runtime state.
public class GameSettings
{
    // Seed used to reproduce the same board; Play and Restart supply a new random seed.
    public int seed;
    public DifficultyPreset difficultyValues;
    // Read-only access to the original difficulty and boardWidth/boardHeight names.
    public DifficultyLevel difficulty => difficultyValues.level;
    public int boardWidth => difficultyValues.boardWidth;
    public int boardHeight => difficultyValues.boardHeight;
    // Preserve the existing resource names without duplicating preset values.
    public int startingStrength => difficultyValues.startingStrength;
    public int startingFood => difficultyValues.startingFood;
    public int startingWater => difficultyValues.startingWater;
    public int startingGold => difficultyValues.startingGold;
    public int maxStrength => difficultyValues.maximumStrength;
    public int maxFood => difficultyValues.maximumFood;
    public int maxWater => difficultyValues.maximumWater;

    public GameSettings(DifficultyPreset preset, int seed)
    {
        if (preset == null)
        {
            throw new ArgumentNullException(nameof(preset));
        }

        this.seed = seed;
        difficultyValues = preset.Copy();
    }
}
