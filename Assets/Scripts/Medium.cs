namespace WildernessGameplay
{
    // Concrete Medium difficulty; optional scene settings override the default preset.
    public sealed class Medium : Difficulty
    {
        public Medium(DifficultyPreset settings = null) : base(settings ?? DifficultyPreset.Create(DifficultyLevel.Medium)) { }
    }
}
