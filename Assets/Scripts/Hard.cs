namespace WildernessGameplay
{
    // Concrete Hard difficulty; optional scene settings override the default preset.
    public sealed class Hard : Difficulty
    {
        public Hard(DifficultyPreset settings = null) : base(settings ?? DifficultyPreset.Create(DifficultyLevel.Hard)) { }
    }
}
