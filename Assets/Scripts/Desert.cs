namespace WildernessGameplay
{
    // Desert terrain uses its configured entry costs and grants FarSight vision.
    public sealed class Desert : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Desert;
        public override Vision CreateVision() => new FarSight();
    }
}
