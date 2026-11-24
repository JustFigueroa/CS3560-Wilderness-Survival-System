namespace WildernessGameplay
{
    // Swamp terrain uses its configured entry costs. Mist and reeds reduce vision range by one.
    public sealed class Swamp : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Swamp;
        public override int VisionModifier => -1;
    }
}
