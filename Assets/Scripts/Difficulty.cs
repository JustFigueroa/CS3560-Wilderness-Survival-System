namespace WildernessGameplay
{
    // Owns one copied preset and creates terrain with difficulty-adjusted entry costs.
    public abstract class Difficulty
    {
        public DifficultyPreset Settings { get; }
        protected Difficulty(DifficultyPreset settings) { Settings = settings.Copy(); }
        public static Difficulty Create(DifficultyPreset settings)
        {
            switch (settings.level)
            {
                case DifficultyLevel.Easy: return new Easy(settings);
                case DifficultyLevel.Medium: return new Medium(settings);
                case DifficultyLevel.Hard: return new Hard(settings);
                default: throw new System.ArgumentOutOfRangeException(nameof(settings));
            }
        }
        public Terrain CreateTerrain(TerrainRule rule)
        {
            var terrain = Terrain.Create(rule.terrain);
            terrain.MovementCost = Cost(rule.strengthCost, Settings.movementCostMultiplier);
            terrain.FoodCost = Cost(rule.foodCost, Settings.foodCostMultiplier);
            terrain.WaterCost = Cost(rule.waterCost, Settings.waterCostMultiplier);
            return terrain;
        }
        private static int Cost(int value, float multiplier) => (int)System.Math.Ceiling(value * (double)multiplier);
    }
}
