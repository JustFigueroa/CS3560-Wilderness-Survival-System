namespace WildernessGameplay
{
    // Diamond with a base range of four squares.
    public sealed class FarSight : Vision
    {
        public override VisionMode Mode => VisionMode.FarSight;
        public override int BaseRadius => 4;
        protected override bool Contains(int dx, int dy) => System.Math.Abs(dx) + System.Math.Abs(dy) <= Radius;
    }
}
