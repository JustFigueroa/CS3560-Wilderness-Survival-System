namespace WildernessGameplay
{
    // Diamond with a base range of two squares.
    public sealed class Cautious : Vision
    {
        public override VisionMode Mode => VisionMode.Cautious;
        public override int BaseRadius => 2;
        protected override bool Contains(int dx, int dy) => System.Math.Abs(dx) + System.Math.Abs(dy) <= Radius;
    }
}
