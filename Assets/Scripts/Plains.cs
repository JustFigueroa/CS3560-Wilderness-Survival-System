namespace WildernessGameplay
{
    // Plains terrain uses its configured entry costs and grants FarSight vision.
    public sealed class Plains : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Plains;
        public override Vision CreateVision() => new FarSight();
    }
}
