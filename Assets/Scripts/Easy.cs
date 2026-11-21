namespace WildernessGameplay
{
    // Concrete Easy difficulty; optional scene settings override the default preset.
    public sealed class Easy : Difficulty
    {
        public Easy(DifficultyPreset settings = null) : base(settings ?? DifficultyPreset.Create(DifficultyLevel.Easy)) { }
    }
}
