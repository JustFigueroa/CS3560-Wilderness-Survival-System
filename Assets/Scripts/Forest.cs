namespace WildernessGameplay
{
    // Forest terrain uses its configured entry costs and grants Cautious vision.
    public sealed class Forest : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Forest;
        public override Vision CreateVision() => new Cautious();
    }
}
