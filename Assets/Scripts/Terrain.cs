namespace WildernessGameplay
{
    // Base terrain type. Each concrete terrain adjusts the player's vision range; difficulty supplies the entry costs.
    public abstract class Terrain
    {
        public int MovementCost, FoodCost, WaterCost;
        public abstract TerrainKind Kind { get; }
        // Added to the player's base vision range while standing here (positive = see further).
        public abstract int VisionModifier { get; }
        public static Terrain Create(TerrainKind kind)
        {
            switch (kind)
            {
                case TerrainKind.Plains: return new Plains();
                case TerrainKind.Forest: return new Forest();
                case TerrainKind.Desert: return new Desert();
                case TerrainKind.Swamp: return new Swamp();
                case TerrainKind.Mountain: return new Mountain();
                default: throw new System.ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
