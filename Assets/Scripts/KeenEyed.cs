namespace WildernessGameplay
{
    // Swamp vision: a square extending two tiles in every direction.
    public sealed class KeenEyed : Vision
    {
        public override VisionMode Mode => VisionMode.KeenEyed;
        protected override int Radius => 2;
        protected override bool Contains(int dx, int dy) => true;
    }
}
