namespace WildernessGameplay
{
    // Swamp terrain uses its configured entry costs and grants KeenEyed vision.
    public sealed class Swamp : Terrain
    {
        public override TerrainKind Kind => TerrainKind.Swamp;
        public override Vision CreateVision() => new KeenEyed();
    }
}
