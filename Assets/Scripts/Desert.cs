namespace WildernessGameplay
{
    // Desert terrain uses its configured entry costs. Open ground: no change to vision range.
    public sealed class Desert : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Desert;
        public override int VisionModifier => 0;
    }
}
