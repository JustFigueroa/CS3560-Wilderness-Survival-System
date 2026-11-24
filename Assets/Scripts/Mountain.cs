namespace WildernessGameplay
{
    // Mountain terrain uses its configured entry costs. High ground extends vision range by two.
    public sealed class Mountain : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Mountain;
        public override int VisionModifier => 2;
    }
}
