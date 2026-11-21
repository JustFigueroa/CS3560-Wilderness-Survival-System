namespace WildernessGameplay
{
    // Mountain terrain uses its configured entry costs and grants Focused vision.
    public sealed class Mountain : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Mountain;
        public override Vision CreateVision() => new Focused();
    }
}
