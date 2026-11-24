namespace WildernessGameplay
{
    // Forest terrain uses its configured entry costs. Tree cover reduces vision range by two.
    public sealed class Forest : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Forest;
        public override int VisionModifier => -2;
    }
}
