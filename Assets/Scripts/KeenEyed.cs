namespace WildernessGameplay
{
    // Square with a base range of two tiles in every direction.
    public sealed class KeenEyed : Vision
    {
        public override VisionMode Mode => VisionMode.KeenEyed;
        public override int BaseRadius => 2;
        protected override bool Contains(int dx, int dy) => true;
    }
}
