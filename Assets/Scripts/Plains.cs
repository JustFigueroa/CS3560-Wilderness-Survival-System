namespace WildernessGameplay
{
    // Plains terrain uses its configured entry costs. Open ground: no change to vision range.
    public sealed class Plains : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Plains;
        public override int VisionModifier => 0;
    }
}
